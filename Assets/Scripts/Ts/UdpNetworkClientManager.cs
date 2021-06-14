using System.Net;
using UnityEngine;
using System;
#if UNITY_UWP
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
    public class UdpNetworkClientManager : MonoBehaviour
    {

#if UNITY_UWP
        private StreamWriter writer = null;
#elif UNITY_EDITOR || UNITY_STANDALONE
        private Thread thread;
        private UdpClient udpclient;
        IPEndPoint localEP;
#endif

        public UdpNetworkClientManager(int port, string address)
        {
#if UNITY_UWP
            Task.Run(async () =>
             {
                 DatagramSocket socket = new DatagramSocket();

                 var soc = await socket.GetOutputStreamAsync(new HostName(address), port.ToString());
                 writer = new StreamWriter(soc.AsStreamForWrite());
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
                 writer.BaseStream.Write(bytes,0,bytes.Length);
                 await writer.FlushAsync();

             });
#elif UNITY_EDITOR || UNITY_STANDALONE
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                udpclient.Send(bytes, bytes.Length,localEP);

#endif
        }
       
        public void SShort(short data)
        {
#if UNITY_UWP
            byte[] bytes = BitConverter.GetBytes(data);
            writer.BaseStream.Write(bytes,0,bytes.Length);

#elif UNITY_EDITOR || UNITY_STANDALONE
            byte[] bytes = BitConverter.GetBytes(data);
            udpclient.Send(bytes, bytes.Length, localEP);

#endif
        }

    }
}