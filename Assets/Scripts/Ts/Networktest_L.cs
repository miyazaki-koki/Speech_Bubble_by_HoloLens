using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Networktest_L : MonoBehaviour
{

    public Text sentence;

    [SerializeField]
    private int UDPReceivePort;

#if WINDOWS_UWP
    Windows.Networking.Sockets.DatagramSocket p_Socket;
    object p_LockObject = new object();
    const int MAX_BUFFER_SIZE = 1024;
#endif

    string W = null;

    // Use this for initialization
    void Start()
    {
        sentence.text = "Nodata";
#if WINDOWS_UWP
        UDPClientReceiver_Init();
#endif
    }

#if WINDOWS_UWP
    public void UDPClientReceiver_Init()
    {
        p_Socket = new Windows.Networking.Sockets.DatagramSocket();
        p_Socket.MessageReceived += OnMessageAsync;
        p_Socket.BindServiceNameAsync(UDPReceivePort.ToString());
    }
#endif

#if WINDOWS_UWP
    async void OnMessageAsync(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        StreamReader reader = new StreamReader(args.GetDataStream().AsStreamForRead());
        W = await reader.ReadLineAsync();
        W += Environment.NewLine;
        W += System.DateTime.Now.ToString("hh:mm:ss:ffffff");
    }
#endif
    void Update()
    {
        sentence.text = W;
    }
}
