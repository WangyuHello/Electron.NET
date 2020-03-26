using Newtonsoft.Json.Linq;
using Socket.Io.Client.Core;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ElectronNET.API
{
    internal static class BridgeConnector
    {
        private static SocketIoWrapper _socket;
        private static object _syncRoot = new object();

        public static SocketIoWrapper Socket
        {
            get
            {
                if(_socket == null && HybridSupport.IsElectronActive)
                {
                    lock (_syncRoot)
                    {
                        if (_socket == null && HybridSupport.IsElectronActive)
                        {
                            _socket = new SocketIoWrapper("http://localhost:" + BridgeSettings.SocketPort);
                            _socket.OnConnected(() =>
                            {
                                Console.WriteLine("BridgeConnector connected!");
                            });
                            _socket.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                    }
                }
                else if(_socket == null && !HybridSupport.IsElectronActive)
                {
                    lock (_syncRoot)
                    {
                        if (_socket == null && !HybridSupport.IsElectronActive)
                        {
                            _socket = new SocketIoWrapper("http://localhost");
                        }
                    }
                }

                return _socket;
            }
        }

        public class SocketIoWrapper
        {
            SocketIoClient _client;
            string url;

            public void OnConnected(Action callback)
            {
                _client.Events.OnConnect.Subscribe((_) => callback());
            }

            public SocketIoWrapper(string url)
            {
                _client = new SocketIoClient();
                this.url = url;
                events = new ConcurrentDictionary<string, IDisposable>();
            }

            public async Task OpenAsync()
            {
                await _client.OpenAsync(new Uri(url));
            }

            private ConcurrentDictionary<string, IDisposable> events;

            public void On(string eventName, Action callback)
            {
                var sub = _client.On(eventName).Subscribe((_) =>
                {
                    callback();
                });
                events.AddOrUpdate(eventName, sub, (_ , _2) => sub);
            }

            public void On(string eventName, Action<object> callback)
            {
                var sub = _client.On(eventName).Subscribe((arg) =>
                {
                    callback(arg.FirstData);
                });

                events.AddOrUpdate(eventName, sub, (_, _2) => sub);
            }

            public void Off(string eventName)
            {
                events.TryRemove(eventName, out var sub);
                sub?.Dispose();
            }

            public void Emit(string eventName)
            {
                _client.Emit(eventName);
            }

            public void Emit(string eventName, params object[] data1)
            {
                _client.Emit(eventName, data1);
            }

            //public void Emit(string eventName, object data1, object data2)
            //{
            //    _client.Emit(eventName, new { data1, data2 });
            //}

            //public void Emit(string eventName, object data1, object data2, object data3)
            //{
            //    _client.Emit(eventName, new { data1, data2, data3 });
            //}

            //public void Emit(string eventName, object data1, object data2, object data3, object data4)
            //{
            //    _client.Emit(eventName, new { data1, data2, data3, data4 });
            //}

            //public void Emit(string eventName, object data1, object data2, object data3, object data4, object data5)
            //{
            //    _client.Emit(eventName, new { data1, data2, data3, data4, data5 });
            //}

            //public void Emit(string eventName, object data1, object data2, object data3, object data4, object data5, object data6)
            //{
            //    _client.Emit(eventName, new { data1, data2, data3, data4, data5, data6 });
            //}
        }
    }
}
