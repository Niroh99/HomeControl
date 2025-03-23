using HomeControl.Integrations.TPLink.JSON;
using Microsoft.CSharp.RuntimeBinder;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HomeControl.Integrations.TPLink
{
    public static class Discovery
    {
        private static bool _discoveryRunning = false;

        public static bool DiscoveryRunning { get => _discoveryRunning; }

        public static List<Device> Discover(int port = 9999, int timeout = 5000, string target = "255.255.255.255")
        {
            if (DiscoveryRunning) throw new InvalidOperationException("Discovery is already running.");

            _discoveryRunning = true;

            SendDiscoveryRequest(target, port);
            using (var udp = new UdpClient(port)
            {
                EnableBroadcast = true
            })
            {
                var devices = new List<Device>();

                Task.WhenAny(Task.Run(() =>
                {
                    Thread.Sleep(timeout);
                    _discoveryRunning = false;
                }), Receive(udp, port, devices)).Wait();

                udp.Close();

                return devices;
            }
        }

        private static async Task Receive(UdpClient udp, int port, List<Device> devices)
        {
            while (_discoveryRunning)
            {
                UdpReceiveResult udpReceiveResult = await udp.ReceiveAsync();

                IPEndPoint remoteEndPoint = udpReceiveResult.RemoteEndPoint;

                Device device = null;

                try
                {
                    var response = ProtocolMessage.ParseResponseData<SysInfo>(udpReceiveResult.Buffer, Device.ProtocolMessageSystem, Device.GetSysInfoCommand);

                    if (response == null) continue;

                    //if (response.Model.StartsWith("HS110"))
                    //{
                    //    device = await TPLinkSmartMeterPlug.Create(remoteEndPoint.Address.ToString()).ConfigureAwait(continueOnCapturedContext: false);
                    //}
                    //else if (response.Model.StartsWith("HS300") || response.Model.StartsWith("KP303") || response.Model.StartsWith("HS107"))
                    //{
                    //    device = await TPLinkSmartMultiPlug.Create(remoteEndPoint.Address.ToString()).ConfigureAwait(continueOnCapturedContext: false);
                    //}
                    //else if (response.Model.StartsWith("HS220"))
                    //{
                    //    device = await TPLinkSmartDimmer.Create(remoteEndPoint.Address.ToString()).ConfigureAwait(continueOnCapturedContext: false);
                    //}
                    //else
                    if (response.Model?.StartsWith("HS") == true)
                    {
                        device = new SmartPlug(remoteEndPoint.Address.ToString());
                    }
                    //else if (response.Model.StartsWith("KL") || response.Model.StartsWith("LB"))
                    //{
                    //    device = await TPLinkSmartBulb.Create(remoteEndPoint.Address.ToString()).ConfigureAwait(continueOnCapturedContext: false);
                    //}
                }
                catch (ProtocolErrorException) { }
                catch (RuntimeBinderException)
                {
                }

                if (device != null) devices.Add(device);
            }
        }


        private static void SendDiscoveryRequest(string target, int port)
        {
            using (var client = new UdpClient(port))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(target), port);

                byte[] array = ProtocolEncoder.Encrypt(ProtocolMessage.BuildCommandJson(Device.ProtocolMessageSystem, Device.GetSysInfoCommand)).ToArray().Skip(4).ToArray();

                client.EnableBroadcast = true;

                client.Send(array, array.Length, endPoint);

                client.Close();
            }
        }

    }
}