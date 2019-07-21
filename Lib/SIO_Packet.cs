using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace SocketIO {
    // https://github.com/socketio/socket.io-parser/blob/master/index.js
    class SIO_Packet {
        public enum PacketType {
            CONNECT = 0,
            DISCONNECT = 1,
            EVENT = 2,
            ACK = 3,
            ERROR = 4,
            BINARY_EVENT = 5,
            BINARY_ACK = 6
        }

        public PacketType Type {
            get; set;
        }

        public string Namespace {
            get; set;
        }

        public int? ID {
            get; set;
        }

        public IJsonValue Data {
            get; set;
        }

        public SIO_Packet(PacketType type) {
            Type = type;
            Namespace = null;
            ID = null;
            Data = null;
        }

        private static SIO_Packet Error(string errorMessage) {
            return new SIO_Packet(PacketType.ERROR) {
                Data = JsonValue.CreateStringValue(errorMessage)
            };
        }

        public string Encode() {
            if(Type == PacketType.BINARY_EVENT || Type == PacketType.BINARY_ACK) {
                return "TODO";
            } else {
                var sb = new StringBuilder();
                sb.Append((int)Type);

                if(Namespace != null && !Namespace.Equals("/")) {
                    sb.Append(Namespace);
                    sb.Append(',');
                }

                if(ID != null) {
                    sb.Append(ID);
                }

                if(Data != null) {
                    sb.Append(Data.Stringify());
                }

                return sb.ToString();
            }
        }

        private static Regex packetRegex = new Regex(@"(?:(\/[^,]+),)?(\d+)?(.*)");

        public static SIO_Packet Decode(string s) {
            if(s.Length == 0 || !char.IsNumber(s.ElementAt(0))) {
                return Error("Malformed packet");
            }

            int typeID = int.Parse(s.ElementAt(0).ToString());
            if(!Enum.IsDefined(typeof(PacketType), typeID)) {
                return Error("Unknown packet type " + typeID);
            }

            var p = new SIO_Packet((PacketType)typeID);
            if(p.Type == PacketType.BINARY_EVENT || p.Type == PacketType.BINARY_ACK) {
                // ignore for now
            }

            var match = packetRegex.Match(s.Substring(1));
            if(!match.Success) {
                return Error("Couldn't parse packet");
            }

            if(match.Groups[1].Success) {
                p.Namespace = match.Groups[1].Value;
            } else {
                p.Namespace = "/";
            }

            if(match.Groups[2].Success) {
                p.ID = int.Parse(match.Groups[2].Value);
            }

            var payload = match.Groups[3].Value;
            if(JsonValue.TryParse(payload, out var jsonPayload)) {
                p.Data = jsonPayload;
            }

            return p;
        }

    }
}
