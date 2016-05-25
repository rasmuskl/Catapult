using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Serilog;

namespace Catapult.Core.Indexes
{
    public class ClipboardIndexer : IDisposable
    {
        private readonly List<ClipboardEntry> _clipboardEntries = new List<ClipboardEntry>();
        public ClipboardEntry[] ClipboardEntries => _clipboardEntries.ToArray();

        public ClipboardIndexer()
        {
            TryRestore();

            ClipboardMonitor.Start();
            ClipboardMonitor.OnClipboardChange += ClipboardMonitor_OnClipboardChange;
        }

        private void ClipboardMonitor_OnClipboardChange()
        {
            try
            {
                var dataObject = Clipboard.GetDataObject();

                if (dataObject == null)
                {
                    return;
                }

                var text = dataObject.GetData("Text") as string ?? dataObject.GetData("UnicodeText") as string;

                if (text.IsNullOrWhiteSpace())
                {
                    return;
                }

                AddEntry(text, DateTime.UtcNow);
                Save();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ClipboardMonitor_OnClipboardChange failed.");
            }
        }

        private void AddEntry(string text, DateTime createdUtc)
        {
            _clipboardEntries.RemoveAll(x => x.Text == text);
            _clipboardEntries.Add(new ClipboardEntry { CreatedUtc = createdUtc, Text = text });

            while (_clipboardEntries.Count > 50)
            {
                var toRemove = _clipboardEntries.OrderBy(x => x.CreatedUtc).Take(_clipboardEntries.Count - 50).ToArray();

                foreach (var entry in toRemove)
                {
                    _clipboardEntries.Remove(entry);
                }
            }
        }

        private void TryRestore()
        {
            if (!File.Exists(CatapultPaths.ClipboardPath))
            {
                return;
            }

            var clipboardJson = File.ReadAllText(CatapultPaths.ClipboardPath);
            var clipboardHistory = JsonConvert.DeserializeObject<ClipboardEntry[]>(clipboardJson);

            foreach (var entry in clipboardHistory)
            {
                AddEntry(entry.Text, entry.CreatedUtc);
            }
        }

        private void Save()
        {
            var clipboardJson = JsonConvert.SerializeObject(ClipboardEntries);
            File.WriteAllText(CatapultPaths.ClipboardPath, clipboardJson);
        }

        // Inspiration: http://stackoverflow.com/a/12750325
        public void Dispose()
        {
            ClipboardMonitor.Stop();
        }

        private static class ClipboardMonitor
        {
            public static event Action OnClipboardChange;

            public static void Start()
            {
                ClipboardWatcher.StartWatcher();
                ClipboardWatcher.OnChange += WatcherOnChange;
            }

            private static void WatcherOnChange()
            {
                OnClipboardChange?.Invoke();
            }

            public static void Stop()
            {
                OnClipboardChange = null;
                ClipboardWatcher.StopWatcher();
            }

            private class ClipboardWatcher : Form
            {
                // static instance of this form
                private static ClipboardWatcher _instance;

                // needed to dispose this form
                private static IntPtr _nextClipboardViewer;

                public static event Action OnChange;

                // start listening
                public static void StartWatcher()
                {
                    // we can only have one instance if this class
                    if (_instance != null)
                    {
                        return;
                    }

                    var thread = new Thread(x =>
                    {
                        try
                        {
                            Application.Run(new ClipboardWatcher());
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "ClipboardWatcher failed.");
                        }
                    });

                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }

                // stop listening (dispose form)
                public static void StopWatcher()
                {
                    if (_instance == null)
                    {
                        return;
                    }

                    _instance.Invoke(new MethodInvoker(() =>
                    {
                        ChangeClipboardChain(_instance.Handle, _nextClipboardViewer);
                    }));

                    _instance.Invoke(new MethodInvoker(_instance.Close));

                    _instance.Dispose();
                    _instance = null;
                }

                // on load: (hide this window)
                protected override void SetVisibleCore(bool value)
                {
                    CreateHandle();

                    _instance = this;

                    _nextClipboardViewer = SetClipboardViewer(_instance.Handle);

                    base.SetVisibleCore(false);
                }

                [DllImport("User32.dll", CharSet = CharSet.Auto)]
                private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

                [DllImport("User32.dll", CharSet = CharSet.Auto)]
                private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

                [DllImport("user32.dll", CharSet = CharSet.Auto)]
                private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

                // defined in winuser.h
                private const int WmDrawclipboard = 0x308;
                private const int WmChangecbchain = 0x030D;

                protected override void WndProc(ref Message m)
                {
                    switch (m.Msg)
                    {
                        case WmDrawclipboard:
                            ClipChanged();
                            SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                            break;

                        case WmChangecbchain:
                            if (m.WParam == _nextClipboardViewer)
                                _nextClipboardViewer = m.LParam;
                            else
                                SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                            break;

                        default:
                            base.WndProc(ref m);
                            break;
                    }
                }

                private void ClipChanged()
                {
                    OnChange?.Invoke();
                }
            }
        }
    }

    public class ClipboardEntry
    {
        public DateTime CreatedUtc;
        public string Text;
    }
}