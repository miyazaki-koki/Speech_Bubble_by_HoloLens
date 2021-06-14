using System.Net;
using UnityEngine;
using System;
#if UNITY_UWP
using System.Runtime.InteropServices.WindowsRuntime;
using System.IO;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Text;
using Windows.Storage.Streams;
#elif UNITY_EDITOR || UNITY_STANDALONE
using System.Threading;
using System.Net.Sockets;
using System.Text;
#endif

namespace HoloLensModule.Network
{
    public class UdpVet_C_M : MonoBehaviour
    {
#if UNITY_UWP
        IOutputStream writer;
#elif UNITY_EDITOR || UNITY_STANDALONE
        private Thread thread;
        private UdpClient udpclient;
        IPEndPoint localEP;
#endif

        public UdpVet_C_M(int port, string address)
        {
#if UNITY_UWP
            Task.Run(async () =>
             {
                 DatagramSocket socket = new DatagramSocket();
                 writer = await socket.GetOutputStreamAsync(new HostName(address), port.ToString());
             });
#elif UNITY_EDITOR || UNITY_STANDALONE
            IPAddress localAddress = IPAddress.Parse(address);
            localEP = new IPEndPoint(localAddress, port);
            udpclient = new UdpClient();

#endif
        }

        public void DeleteManager()
        {
#if UNITY_UWP
            writer.Dispose();
#elif UNITY_EDITOR || UNITY_STANDALONE
            udpclient.Close();
#endif
        }

        public void SMessage(string data)
        {
#if UNITY_UWP
            Task.Run(async () =>
             {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
                 await writer.WriteAsync(bytes.AsBuffer());
             });
#elif UNITY_EDITOR || UNITY_STANDALONE
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            udpclient.Send(bytes, bytes.Length, localEP);

#endif
        }
#if UNITY_UWP
        public async Task SShortAsync(short data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            await writer.WriteAsync(bytes.AsBuffer());
        }
#endif

#if UNITY_UWP
        public async void SShort(short data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            await writer.WriteAsync(bytes.AsBuffer());
        }
#endif

        #if UNITY_UWP
        public async Task SBytesAsync(byte[] data)
        {
            await writer.WriteAsync(data.AsBuffer());
        }
#endif

#if UNITY_UWP
        public async void SBytes(byte[] data)
        {
            await writer.WriteAsync(data.AsBuffer());
        }
#endif

    }
}