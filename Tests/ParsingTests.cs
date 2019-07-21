using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests {
    [TestClass]
    public class ParsingTests {
        [TestMethod]
        public void SocketIOPacketParsing0() {
            var p = SIO_Packet.Decode(@"2/test,213[0,1,2]");
            Assert.AreEqual(p.Type, SIO_Packet.PacketType.EVENT);
            Assert.AreEqual(p.Namespace, "/test");
            Assert.AreEqual(p.ID, 213);
        }

        [TestMethod]
        public void SocketIOPacketParsing1() {
            var p = SIO_Packet.Decode(@"2/test,[0,1,2]");
            Assert.AreEqual(p.Type, SIO_Packet.PacketType.EVENT);
            Assert.AreEqual(p.Namespace, "/test");
            Assert.AreEqual(p.ID, null);
        }

        [TestMethod]
        public void SocketIOPacketParsing2() {
            var p = SIO_Packet.Decode(@"2");
            Assert.AreEqual(p.Type, SIO_Packet.PacketType.EVENT);
            Assert.AreEqual(p.Namespace, "/");
            Assert.AreEqual(p.ID, null);
        }
    }
}
