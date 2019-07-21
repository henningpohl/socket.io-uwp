# UWP Client for socket.io

Quite barebones implementation of the [socket.io](https://socket.io/) protocol for UWP applications. Currently binary messages and acknowledgements are not supported. There also is no way to leave a namespace once connected or stop listening to messages. I might get around to doing that, but probably only if the project using this library requires it.

## How to use
```C#
using SocketIO;
using Windows.Data.Json;

var client = new Client();
client.Connect(new Uri("ws://localhost:3150/socket.io/?EIO=4&transport=websocket"));

client.On("testfast", async (data) => {
    var num = data.GetArray().GetNumberAt(0);
});

client.NS("/asd").On("chat message", async (data) => {
    var msg = data.GetArray().GetStringAt(0);
});

var msg = JsonValue.CreateStringValue(MessageText.Text);
client.NS("/asd").Emit("chat message", msg);
```