using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace SocketIO {
    // https://docs.microsoft.com/en-us/windows/uwp/networking/websockets
    // https://docs.microsoft.com/en-us/uwp/api/windows.networking.sockets.messagewebsocket
    // https://github.com/microsoft/Windows-universal-samples/blob/master/Samples/WebSocket/cs/Scenario1_UTF8.xaml.cs
    class WebsocketTransport : Transport {

        private MessageWebSocket websocket;
        private DataWriter writer;
        private Task sendTask;
        private CancellationToken sendTaskCancel;

        public WebsocketTransport(Uri uri) : base(uri) {
            sendTaskCancel = new CancellationToken();
            sendTask = new Task(SendTask, sendTaskCancel);
            sendTask.Start();
        }

        private void Websocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args) {
            using(var reader = args.GetDataReader()) {
                string message = reader.ReadString(reader.UnconsumedBufferLength);
                OnMessage(message);
            }
        }

        private void Websocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args) {
            writer.DetachStream();
            writer.Dispose();
            writer = null;
            websocket = null;

            OnClose();
        }

        private async void SendTask() {
            while(true) {
                if(Status != TransportStatus.Connected || sendQueue.Count == 0) {
                    await Task.Delay(100);
                    continue;
                }

                while(sendQueue.Count > 0) {
                    var msg = sendQueue.Dequeue();
                    writer.WriteString(msg);
                    await writer.StoreAsync();
                    if(PrintDebug) {
                        System.Diagnostics.Debug.WriteLine("Sent: " + msg);
                    }
                }
            }
        }

        public async override void Connect() {
            Status = TransportStatus.Connecting;

            websocket = new MessageWebSocket();
            websocket.Closed += Websocket_Closed;
            websocket.MessageReceived += Websocket_MessageReceived;
            websocket.Control.MessageType = SocketMessageType.Utf8;
            writer = new DataWriter(websocket.OutputStream);

            await websocket.ConnectAsync(URI);

            Status = TransportStatus.Connected;
        }

        public override void Disconnect() {
            Status = TransportStatus.Disconnected;
            // https://www.iana.org/assignments/websocket/websocket.xml#close-code-number
            websocket.Close(1000, "Done");
        }
    }
}
