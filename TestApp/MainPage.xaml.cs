using SocketIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TestApp {
    public sealed partial class MainPage : Page {
        private Client client;

        public MainPage() {
            this.InitializeComponent();

            client = new Client();
            client.Connect(new Uri("ws://localhost:3150/socket.io/?EIO=4&transport=websocket"));

            client.On("testfast", async (data) => {
                var num = data.GetArray().GetNumberAt(0);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    TestMessages.Text = num.ToString() + "\n" + TestMessages.Text;
                });
            });

            client.NS("/asd").On("chat message", async (data) => {
                var msg = data.GetArray().GetStringAt(0);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                    ChatMessages.Text = msg + "\n" + ChatMessages.Text;
                });
            });

            MessageSend.Click += (s, e) => {
                var msg = JsonValue.CreateStringValue(MessageText.Text);
                client.NS("/asd").Emit("chat message", msg);
            };
        }
    }
}
