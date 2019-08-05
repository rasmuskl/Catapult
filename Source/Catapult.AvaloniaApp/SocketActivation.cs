using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Catapult.AvaloniaApp
{
    public class SocketActivation
    {
        static string SocketPath = Path.Combine(Path.GetTempPath(), "catapultsocket");
        const string SocketToken = "CATAPULT";

        public static Action Activator { get; internal set; }

        public static bool TryConnect()
        {
            try
            {
                var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                listener.Connect(new UnixDomainSocketEndPoint("/tmp/testsocket"));
                listener.Send(Encoding.UTF8.GetBytes(SocketToken));
                listener.Disconnect(false);
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine($"Unable to connect: {e.Message}");
                return false;
            }
        }

        public static void CreateServer()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return;
            }

            var thread = new Thread(() =>
            {
                File.Delete(SocketPath);

                var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                listener.Bind(new UnixDomainSocketEndPoint(SocketPath));
                listener.Listen(1);

                byte[] bytes = new Byte[1024];

                // Start listening for connections.
                while (true)
                {
                    Socket handler = listener.Accept();
                    string data = null;

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                        if (data.IndexOf(SocketToken) > -1)
                        {
                            break;
                        }

                        Activator?.Invoke();
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }) { IsBackground = true };

            thread.Start();
        }
    }
}