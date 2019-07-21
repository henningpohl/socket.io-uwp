using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SocketIO {

    public delegate void MessageCallback(IJsonValue msg);
    public delegate void AckCallback();

    public sealed class Namespace {
        private Dictionary<string, List<MessageCallback>> EventHandlers = new Dictionary<string, List<MessageCallback>>();

        public string Name {
            get; private set;
        }

        private Client client;

        internal Namespace(string name, Client client) { 
            Name = name;
            this.client = client;
        }

        internal void OnPacket(SIO_Packet p) {
            // https://socket.io/docs/client-api/#socket-on-eventName-callback
            switch(p.Type) {
                case SIO_Packet.PacketType.ACK:
                case SIO_Packet.PacketType.BINARY_ACK:
                case SIO_Packet.PacketType.DISCONNECT:
                case SIO_Packet.PacketType.ERROR:
                    // ignore for now
                    break;
                case SIO_Packet.PacketType.CONNECT:
                    // Indicates we are now properly connected (to this namespace)
                    break;
                case SIO_Packet.PacketType.EVENT:
                    OnEvent(p);
                    break;
                // maybe add support for:
                // CallHandler("connect_timeout", null)
                // CallHandler("connect_error", null)
                // CallHandler("reconnect", null)
                // CallHandler("reconnect_attempt", null)
                // CallHandler("reconnecting", null)
                // CallHandler("reconnect_error", null)
                // CallHandler("reconnect_failed", null)
                // CallHandler("ping", null)
                // CallHandler("pong", null)
            }
        }

        private void OnEvent(SIO_Packet p) {
            if(p.ID.HasValue) {
                //transport.Send(Ack)
            }

            var dataArray = p.Data.GetArray();
            var eventName = dataArray.GetStringAt(0);
            dataArray.RemoveAt(0);
            if(dataArray.Count == 0) {
                p.Data = null;
            }
            CallHandlers(eventName, p.Data);
        }

        private void CallHandlers(string eventName, IJsonValue data) {
            if(EventHandlers.ContainsKey(eventName)) {
                foreach(var handler in EventHandlers[eventName]) {
                    handler(data);
                }
            }
        }

        public void On(string eventName, MessageCallback callback) {
            if(!EventHandlers.ContainsKey(eventName)) {
                EventHandlers[eventName] = new List<MessageCallback>();
            }
            EventHandlers[eventName].Add(callback);
        }

        public void Emit(string eventName, IJsonValue args) {
            Emit(eventName, args, null);
        }

        public void Emit(string eventName, IJsonValue args, AckCallback ackCallback) {
            SIO_Packet p = new SIO_Packet(SIO_Packet.PacketType.EVENT);
            p.Namespace = Name;

            if(args.ValueType == JsonValueType.Array) {
                args.GetArray().Insert(0, JsonValue.CreateStringValue(eventName));
                p.Data = args;
            } else {
                var data = new JsonArray();
                data.Add(JsonValue.CreateStringValue(eventName));
                data.Add(args);
                p.Data = data;
            }            

            client.Send(p.Encode());
        }
    }
}
