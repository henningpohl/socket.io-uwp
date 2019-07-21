using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketIO {
    abstract class Transport {
        public enum TransportStatus {
            Connecting,
            Connected,
            Disconnected
        }

        public TransportStatus Status {
            get; protected set;
        }

        public bool PrintDebug {
            get; set;
        }

        protected Queue<string> sendQueue = new Queue<string>();

        public event EventHandler<string> Message;
        public event EventHandler Close;

        public Uri URI {
            get; private set;
        }

        public Transport(Uri uri) {
            URI = uri;
            Status = TransportStatus.Disconnected;
            PrintDebug = false;
        }

        public abstract void Connect();
        public abstract void Disconnect();

        public void Send(string message) {
            sendQueue.Enqueue(message);
        }

        protected void OnMessage(string msg) {
            if(PrintDebug) {
                System.Diagnostics.Debug.WriteLine("Received: " + msg);
            }

            if(Message != null) {
                Message(this, msg);
            }
        }

        protected void OnClose() {
            if(Close != null) {
                Close(this, null);
            }
        }
    }
}
