using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using UnityEngine.UI;
using System.Text;
using System;

[System.Serializable]

public class UDP : MonoBehaviour
{

    [SerializeField, Tooltip("UDP受信ポート")]
    private int UDPReceivePort = 3333;


    public bool _Flg { get; set; }
    public string C_data { get; private set; }

    // Update is called once per frame
    public UDP()
    {
    }

#if WINDOWS_UWP

    Windows.Networking.Sockets.DatagramSocket p_Socket;
    object p_LockObject = new object();
    const int MAX_BUFFER_SIZE = 1024;

    public void UDPClientReceiver_Init()
    {

            _Flg = false;
            p_Socket = new Windows.Networking.Sockets.DatagramSocket();
            p_Socket.MessageReceived += OnMessage;
            p_Socket.BindServiceNameAsync(UDPReceivePort.ToString());
    }

    void OnMessage(Windows.Networking.Sockets.DatagramSocket sender, Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        using (Stream stream = args.GetDataStream().AsStreamForRead())
        {
            byte[] receiveBytes = new byte[MAX_BUFFER_SIZE];
            stream.ReadAsync(receiveBytes, 0, MAX_BUFFER_SIZE);
            lock (p_LockObject)
            {
                C_data = Encoding.UTF8.GetString(receiveBytes) + Environment.NewLine;
                _Flg = true;
            }
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
            C_data = Encoding.UTF8.GetString(receiveBytes) + Environment.NewLine;
            _Flg = true;

        // 非同期受信を再開する
        udpClient.BeginReceive(OnReceived, udpClient);
    }
    #endregion
#endif
}