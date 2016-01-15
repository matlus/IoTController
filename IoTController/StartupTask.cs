using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using IoTController.Http;
using IoTController.Gpio;
using IoTController.Enums;
using Windows.System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace IoTController
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral backgroundTaskDeferral;
        HttpServer httpServer;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            backgroundTaskDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnCanceled;

            httpServer = new HttpServer(8000);
            IAsyncAction asyncAction = httpServer.StartServer();            
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            httpServer.Dispose();
        }
    }

    public sealed class HttpServer : IDisposable
    {
        private const uint BufferSize = 8192;
        private int port = 8000;
        private readonly StreamSocketListener listener;

        public HttpServer(int serverPort)
        {
            listener = new StreamSocketListener();
            port = serverPort;
            listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
        }

        public IAsyncAction StartServer()
        {
            return listener.BindServiceNameAsync(port.ToString());
        }

        public void Dispose()
        {
            listener.Dispose();
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            StringBuilder request = new StringBuilder();
            using (IInputStream input = socket.InputStream)
            {
                byte[] data = new byte[BufferSize];
                IBuffer buffer = data.AsBuffer();
                uint dataRead = BufferSize;
                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }

            var httpRequest = new HttpRequest(request.ToString());

            using (IOutputStream output = socket.OutputStream)
            {
                if (httpRequest.Method == "GET")
                    await WriteResponseAsync(httpRequest, output);
                else
                    throw new InvalidDataException("HTTP method: " + httpRequest.Method + ", not supported.");
            }
        }

        private async Task WriteResponseAsync(HttpRequest httpRequest, IOutputStream outputStream)
        {
            if (httpRequest.PathInfo == "/")
            {
                var localStorageFolderPath = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                IBuffer buffer = await PathIO.ReadBufferAsync(localStorageFolderPath + "\\default.htm");
                await ResponseWrite(buffer.ToArray(), outputStream);
            }
            else if (httpRequest.PathInfo == "/lights")
            {
                var lightColor = httpRequest.QueryStrings["color"] == "red" ? LightColor.Red : LightColor.Green;
                var lightState = httpRequest.QueryStrings["state"] == "true" ? LightState.On : LightState.Off;

                LightController.SetLightState(lightColor, lightState);

                byte[] httpContent = Encoding.UTF8.GetBytes(string.Format("The {0} light has been turned {1}", lightColor, lightState.ToString()));
                await ResponseWrite(httpContent, outputStream);
            }
            else
            {
            }
        }

        private async Task ResponseWrite(byte[] httpContent, IOutputStream outputStream)
        {
            using (Stream responseStream = outputStream.AsStreamForWrite())
            {
                string header = String.Format("HTTP/1.1 200 OK\r\n" +
                                  "Content-Length: {0}\r\n" +
                                  "Connection: close\r\n\r\n",
                                  httpContent.Length);
                byte[] headerArray = Encoding.UTF8.GetBytes(header);
                await responseStream.WriteAsync(headerArray, 0, headerArray.Length);
                await responseStream.WriteAsync(httpContent, 0, httpContent.Length);
                await responseStream.FlushAsync();
            }
        }
    }
}
