using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace SocketIO {
    class Engine {

        private enum PacketType {
            Open = 0,
            Close = 1,
            Ping = 2,
            Pong = 3,
            Message = 4,
            Upgrade = 5,
            Noop = 6
        };

        public bool Connected {
            get; private set;
        }

        public bool Reconnection {
            get; set;
        }

        public string SessionID {
            get; private set;
        }

        public int PingTimeout {
            get; private set;
        }

        public int PingInterval {
            get; private set;
        }

        public event EventHandler<string> OnOpen;
        public event EventHandler<string> OnClose;
        public event EventHandler<string> OnPing;
        public event EventHandler<string> OnPong;
        public event EventHandler<string> OnMessage;
        public event EventHandler<string> OnUpgrade;
        public event EventHandler<string> OnNoop;

        private Transport transport;
        private Timer pingTimer, pongTimer;

        public Engine(Uri uri, bool reconnection = true) {
            Reconnection = reconnection;

            transport = new WebsocketTransport(uri);
            transport.Close += Transport_Close;
            transport.Message += Transport_Message;
            transport.PrintDebug = true;

            OnOpen += (s, e) => {
                var message = JsonObject.Parse(e);

                SessionID = message.GetNamedString("sid");
                PingTimeout = (int)message.GetNamedNumber("pingTimeout");
                PingInterval = (int)message.GetNamedNumber("pingInterval");

                pongTimer = new Timer((_) => {
                    Disconnect();
                }, null, Timeout.Infinite, Timeout.Infinite);
                pingTimer = new Timer((_) => {
                    Send(((int)PacketType.Ping).ToString());
                    pongTimer.Change(PingTimeout, Timeout.Infinite);
                }, null, 0, PingInterval);
            };

            OnPong += (s, e) => {
                pingTimer.Change(PingInterval, PingInterval);
                pongTimer.Change(Timeout.Infinite, Timeout.Infinite);
            };
        }

        private void Transport_Message(object sender, string rawMessage) {
            var packetIdentifier = rawMessage.ElementAt(0) - '0';
            if(!Enum.IsDefined(typeof(PacketType), packetIdentifier)) {
                return;
            }
            var packetType = (PacketType)packetIdentifier;
            var msg = rawMessage.Substring(1);

            switch(packetType) {
                case PacketType.Open:
                    OnOpen(this, msg);
                    break;
                case PacketType.Close:
                    OnClose(this, msg);
                    break;
                case PacketType.Ping:
                    OnPing(this, msg);
                    break;
                case PacketType.Pong:
                    OnPong(this, msg);
                    break;
                case PacketType.Message:
                    OnMessage(this, msg);
                    break;
                case PacketType.Upgrade:
                    OnUpgrade(this, msg);
                    break;
                case PacketType.Noop:
                    OnNoop(this, msg);
                    break;
            }
        }

        private void Transport_Close(object sender, EventArgs e) {
            Connected = false;
            System.Diagnostics.Debug.WriteLine("Transport was closed");
            if(Reconnection) {
                Connect();
            }
        }

        public void Connect() {
            if(Connected) {
                Disconnect();
            }
            transport.Connect();
            Connected = true;
        }

        public void Disconnect() {
            if(Connected) {
                pingTimer.Dispose();
                pingTimer = null;
                pongTimer.Dispose();
                pongTimer = null;

                transport.Disconnect();
                Connected = false;
            }
        }

        private void Send(string data) {
            transport.Send(data);
        }

        public void SendMessage(string message) {
            transport.Send(((int)PacketType.Message).ToString() + message);
        }
    }
}
