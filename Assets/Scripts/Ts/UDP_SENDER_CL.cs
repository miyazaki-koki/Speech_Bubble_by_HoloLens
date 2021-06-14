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
    public class UDP_SENDER_CL : MonoBehaviour
    {

#if UNITY_UWP
        private StreamWriter writer = null;
        private Task task = null;
#elif UNITY_EDITOR || UNITY_STANDALONE
        private Thread thread;
        private UdpClient udpclient;
        IPEndPoint localEP;
#endif

        public UDP_SENDER_CL(int port, string address)
        {
#if UNITY_UWP
            task = Task.Run(async () =>
             {
                 DatagramSocket socket = new DatagramSocket();
                 var data_s = await socket.GetOutputStreamAsync(new HostName(address), port.ToString());
                writer = new StreamWriter(data_s.AsStreamForWrite());
                
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

        public void S_Short(byte[] data)
        {
#if UNITY_UWP
            if (writer != null)
            {
                if (task == null || task.IsCompleted == true)
                {
                    task = Task.Run(async () =>
                    {
                        await writer.BaseStream.WriteAsync(data, 0, data.Length);
                        await writer.FlushAsync();
                    });
                    
                }
            }
#endif
        }

    }
}