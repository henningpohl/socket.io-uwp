using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Networking.Sockets;

namespace SocketIO {
    public sealed class Client {
        private Engine engine;

        private Dictionary<string, Namespace> namespaces = new Dictionary<string, Namespace>();
        private Namespace rootNamespace;

        public Client() {
            rootNamespace = new Namespace("/", this);
            namespaces["/"] = rootNamespace;
        }

        private void Engine_OnMessage(object sender, string message) {
            SIO_Packet p = SIO_Packet.Decode(message);
            if(namespaces.ContainsKey(p.Namespace)) {
                namespaces[p.Namespace].OnPacket(p);
            } else {
                rootNamespace.OnPacket(p);
            }
        }

        public void Connect(Uri uri) {
            engine = new Engine(uri);
            engine.OnMessage += Engine_OnMessage;
            engine.Connect();
        }

        public void On(string eventName, MessageCallback callback) {
            rootNamespace.On(eventName, callback);
        }

        public void Emit(string eventName, IJsonValue args) {
            rootNamespace.Emit(eventName, args);
        }

        public void Emit(string eventName, IJsonValue args, AckCallback ackCallback) {
            rootNamespace.Emit(eventName, args, ackCallback);
        }

        public Namespace NS(string name) {
            if(namespaces.ContainsKey(name)) {
                return namespaces[name];
            }

            var newNamespace = new Namespace(name, this);
            namespaces[name] = newNamespace;
            SIO_Packet p = new SIO_Packet(SIO_Packet.PacketType.CONNECT);
            p.Namespace = name;
            Send(p.Encode());
            return newNamespace;
        }

        internal void Send(string message) {
            engine.SendMessage(message);
        }
    }
}
