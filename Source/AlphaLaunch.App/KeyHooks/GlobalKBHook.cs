using System.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AlphaLaunch.App.KeyHooks
{
    public enum KBKeyStatus
    {
        KeyUp = 0x101,
        SysKeyUp = 0x105,
        KeyDown = 0x100,
        SysKeyDown = 0x104,
    }

    public class KBHookEventArgs : EventArgs
    {
        public int HookCode;
        public IntPtr wParam;
        public KBDLLHookStruct lParam;
        public bool AbortKey = false;
        public KBKeyStatus KeyStatus;

        public bool IsKeyDown()
        {
            return KeyStatus == KBKeyStatus.KeyDown || KeyStatus == KBKeyStatus.SysKeyDown;
        }

        public bool IsKeyUp()
        {
            return KeyStatus == KBKeyStatus.KeyUp || KeyStatus == KBKeyStatus.SysKeyUp;
        }
    }

    public struct KBDLLHookStruct
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }

    public enum HookType : int
    {
        WH_KEYBOARD_LL = 13,
    }

    public class GlobalKBHook
    {
        //Callback for the hook 
        public delegate int KBHookProc(int code, IntPtr wParam, ref KBDLLHookStruct lParam);

        protected KBHookProc kbhook = null;
        protected IntPtr hhook = IntPtr.Zero;

        public delegate void KBHookEventHandler(object sender, KBHookEventArgs e);

        public event KBHookEventHandler KBHookInvoked;

        protected void OnKBHookInvoked(KBHookEventArgs e)
        {
            if (KBHookInvoked != null)
                KBHookInvoked(this, e);
        }

        public GlobalKBHook()
        {
            kbhook = this.CoreKBHook;
        }

        public int CoreKBHook(int code, IntPtr wParam, ref KBDLLHookStruct lParam)
        {
            if (code < 0)
                return CallNextHookEx(hhook, code, wParam, lParam);

            KBHookEventArgs e = new KBHookEventArgs();
            e.HookCode = code;
            e.wParam = wParam;
            e.lParam = lParam;

            e.KeyStatus = (KBKeyStatus)Enum.ToObject(typeof (KBKeyStatus), wParam.ToInt32());

            OnKBHookInvoked(e);

            // Abort the key
            if (e.AbortKey)
                return 1;

            // Yield to the next hook in the chain
            return CallNextHookEx(hhook, code, wParam, lParam);
        }

        public void Install()
        {
            int hInstance = LoadLibrary("User32");
            hhook = SetWindowsHookEx(
                HookType.WH_KEYBOARD_LL,
                kbhook,
                (IntPtr)hInstance, //IntPtr.Zero for local hooks
                0); //zero = global hook, otherwise use local thread ID
        }

        public void Uninstall()
        {
            UnhookWindowsHookEx(hhook);
        }

        //win32 api function for creating hooks
        [DllImport("user32.dll")]
        protected static extern IntPtr SetWindowsHookEx(HookType code,
                                                        KBHookProc func,
                                                        IntPtr hInstance,
                                                        int threadID);


        //win32 api function for unhooking 
        [DllImport("user32.dll")]
        protected static extern int UnhookWindowsHookEx(IntPtr hhook);

        //win32 api function for continuing the hook chain
        [DllImport("user32.dll")]
        protected static extern int CallNextHookEx(IntPtr hhook,
                                                   int code,
                                                   IntPtr wParam,
                                                   KBDLLHookStruct lParam);

        //Used to find HWND to user32.dll
        [DllImport("kernel32")]
        public extern static int LoadLibrary(string lpLibFileName);
    }
}