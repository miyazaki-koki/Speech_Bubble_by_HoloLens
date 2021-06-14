﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using UnityEngine.UI;
using System.Text;
using System;

[Serializable]

public class UDP_Files:MonoBehaviour
{
    [SerializeField]
    private int Save_pool;
    [SerializeField]
    private int UDPReceivePort;
    private int _index;

    //public string FilePath { get; set; }

    public List<string> W_s { get; set; }

    void Start()
    {
        W_s = new List<string>();
        _index = 0;
        UDPClientReceiver_Init();
    }

#if WINDOWS_UWP

    Windows.Networking.Sockets.DatagramSocket p_Socket;
    object p_LockObject = new object();
    const int MAX_BUFFER_SIZE = 1024;

    public void UDPClientReceiver_Init()
    {
        //FilePath = Path.Combine(Application.temporaryCachePath, "Sentence.txt");
        p_Socket = new Windows.Networking.Sockets.DatagramSocket();
        p_Socket.MessageReceived += OnMessage;
        p_Socket.BindServiceNameAsync(UDPReceivePort.ToString());
    }

    async void OnMessage(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        Stream stream = args.GetDataStream().AsStreamForRead();
        byte[] receiveBytes = new byte[MAX_BUFFER_SIZE];
        await stream.ReadAsync(receiveBytes, 0, MAX_BUFFER_SIZE);
        W_s.Add(Encoding.UTF8.GetString(receiveBytes));
        if (Save_pool < W_s.Count)
        {
            W_s.RemoveAt(0);
        }
    }
#else
    #region
    /// <summary>
    /// UDP受信初期化
    /// </summary>
    public void UDPClientReceiver_Init()
    {
        // UDP受信ポートに受信する全てのメッセージを取得する
        System.Net.IPEndPoint endPoint =
            new System.Net.IPEndPoint(System.Net.IPAddress.Any, UDPReceivePort);

        // UDPクライアントインスタンスを初期化
        System.Net.Sockets.UdpClient udpClient =
            new System.Net.Sockets.UdpClient(endPoint);

        // 非同期のデータ受信を開始する
        udpClient.BeginReceive(OnReceived, udpClient);
    }

    /// <summary>
    /// UDP受信時コールバック関数
    /// </summary>
    private void OnReceived(System.IAsyncResult a_result)
    {
        // ステータスからUdpClientのインスタンスを取得する
        System.Net.Sockets.UdpClient udpClient =
        (System.Net.Sockets.UdpClient)a_result.AsyncState;

        // 受信データをバイト列として取得する
        System.Net.IPEndPoint endPoint = null;
        byte[] receiveBytes = udpClient.EndReceive(a_result, ref endPoint);
        Debug.Log(Encoding.UTF8.GetString(receiveBytes));
        //File.AppendAllText(FilePath, C_data);
        W_s[_index] = Encoding.UTF8.GetString(receiveBytes);
        _index++;
        if (Save_pool < _index)
        {
            for (int i = 0; Save_pool > i; i++)
            {
                W_s[i] = W_s[i + 1];
            }
            _index--;
        }

        // 非同期受信を再開する
        udpClient.BeginReceive(OnReceived, udpClient);
    }
    #endregion
#endif
}