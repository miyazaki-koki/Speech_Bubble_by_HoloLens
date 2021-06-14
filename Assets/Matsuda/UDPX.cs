//テキスト表示位置固定でUDP通信でテキストを受信し，表示までの処理にかかる時間の計測
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// UnityEvent を利用するため Events を追加
using UnityEngine.Events;
// usingでStreamを開くため System.IO を追加
using System.IO;

//----------------------------------------------------------------------------
using UnityEngine.UI;//追加
using System;//追加
//----------------------------------------------------------------------------

// 引数にByte列を受け取る UnityEvent<T0> の継承クラスを作成する
// UDP受信したバイト列を引数として渡す
// Inspector ビューに表示させるため、Serializable を設定する
//[System.Serializable]
//public class MyIntEvent : UnityEvent<byte[]> { }

public class UDPX : MonoBehaviour
{
    /// UDPメッセージ受信時実行処理
    //[SerializeField, Tooltip("UDPメッセージ受信時実行処理")]
    //private MyIntEvent UDPReceiveEventUnityEvent;

    /// UDP受信ポート
    [SerializeField, Tooltip("UDP受信ポート")]
    private int UDPReceivePort = 4602;

    /// UDP受信データ
    private byte[] p_UDPReceivedData;

    /// UDP受信イベント検出フラグ
    private bool p_UDPReceivedFlg;

    //----------------------------------------------------------------------------
    public GameObject ReceivedMessage_object = null; //Textオブジェクト(受信したメッセージ)の格納用

    //Text表示用
    //public GameObject Text_object = null; //Textオブジェクト
    //public int Number_num = 0;//処理回数カウント

    //計測用
    //private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//StopWatch型の宣言
    private double total;//合計値
    private int i;//変数

    public GameObject Result_object = null; //Textオブジェクト(result)の格納用

    //UNIXエポックを表すDateTimeオブジェクトを取得
    //private static DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);
    private double Shorizikan;//処理時間
    private double Result;//処理時間
    //----------------------------------------------------------------------------


    /// 起動時処理
    void Start()
    {
        // 検出フラグOFF
        p_UDPReceivedFlg = false;

        // 初期化処理
        UDPClientReceiver_Init();

        //----------------------------------------------------------------------------
        total = 0.0f;
        i = 1;
        //sw.Reset();//リセット
        //----------------------------------------------------------------------------
    }


    /// 定期実行
    void Update()
    {

        if (p_UDPReceivedFlg)
        {
            // UDP受信を検出すればUnityEvent実行
            // 受信データを引数として渡す
            //UDPReceiveEventUnityEvent.Invoke(p_UDPReceivedData);

            //----------------------------------------------------------------------------

            //sw.Start();//計測開始

            // データを文字列に変換
            string ReceivedMessage = System.Text.Encoding.ASCII.GetString(p_UDPReceivedData);
            //TargetTextField.text = (GetMessage + "testtest");//getMessage;

            //オブジェクトからTextコンポーネントを取得
            Text UDP_Message = ReceivedMessage_object.GetComponent<Text>();
            // テキストの表示を入れ替える
            UDP_Message.text = ("Received Message\n" + ReceivedMessage);

            //sw.Stop();//計測終了


            // 現在日時を表すDateTimeオブジェクトを取得
            //DateTime targetTime = DateTime.Now;
            // UTC時間に変換
            //targetTime = targetTime.ToUniversalTime();
            // UNIXエポックからの経過時間を取得
            //TimeSpan elapsedTime = targetTime - UNIX_EPOCH;

            //UNIX時間（表示終了の時刻）の取得
            var unixTimestamp = (double)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            //処理時間=送信時間-表示時間
            Result = unixTimestamp - double.Parse(ReceivedMessage);

            //total = sw.ElapsedTicks * 0.0001f;//タイマーはint型でカウントされているので、変換してから足す.
            //total += sw.ElapsedTicks * 0.0001f;//タイマーはint型でカウントされているので、変換してから足す.

            total += Result;
            //オブジェクトからTextコンポーネントを取得
            Text Result_text = Result_object.GetComponent<Text>();

            //ToString 変数やオブジェクトを文字列に変換する
            // テキストの表示を入れ替える
            //Result_text.text = "処理時間 ： " + total.ToString() + "[ms]";
            //Result_text.text = "処理の平均時間\n" + (total / i).ToString() + "[ms]";

            //処理時間を表示
            Result_text.text = "処理時間\n" + Result + "[s]" + "\n処理の平均時間\n" + (total / i).ToString() + "[s]";
            //sw.Reset();//リセット

            i++;

            //----------------------------------------------------------------------------

            // 検出フラグをOFF
            p_UDPReceivedFlg = false;
        }
    }
    /// UDP受信時処理
    private void UDPReceiveEvent(byte[] receiveData)
    {
        // 検出フラグONに変更する
        // UnityEventの実行はMainThreadで行う
        p_UDPReceivedFlg = true;

        // 受信データを記録する
        p_UDPReceivedData = receiveData;
    }


#if WINDOWS_UWP
    /// UDP通信サポート
    Windows.Networking.Sockets.DatagramSocket p_Socket;
    
    /// ロック用オブジェクト
    object p_LockObject = new object();
    
    /// バッファサイズ
    const int MAX_BUFFER_SIZE = 1024;
    
    /// UDP受信初期化
    private async void UDPClientReceiver_Init()
    {
        try {
            // UDP通信インスタンスの初期化
            p_Socket = new Windows.Networking.Sockets.DatagramSocket();
            // 受信時のコールバック関数を登録する
            p_Socket.MessageReceived += OnMessage;
            // 指定のポートで受信を開始する
            p_Socket.BindServiceNameAsync(UDPReceivePort.ToString());
        } catch (System.Exception e) {
            Debug.LogError(e.ToString());
        }
    }
    
    /// UDP受信時コールバック関数
    async void OnMessage
    (Windows.Networking.Sockets.DatagramSocket sender,
     Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        using (System.IO.Stream stream = args.GetDataStream().AsStreamForRead()) {
            // 受信データを取得
            byte[] receiveBytes = new byte[MAX_BUFFER_SIZE];
            await stream.ReadAsync(receiveBytes, 0, MAX_BUFFER_SIZE);
            lock (p_LockObject) {
                // 受信データを処理に引き渡す
                UDPReceiveEvent(receiveBytes);
            }
        }
    }
#else

    /// UDP受信初期化
    private void UDPClientReceiver_Init()
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

    /// UDP受信時コールバック関数
    private void OnReceived(System.IAsyncResult a_result)
    {
        //----------------------------------------------------------------------------
        //sw.Start();//計測開始
        //----------------------------------------------------------------------------

        // ステータスからUdpClientのインスタンスを取得する
        System.Net.Sockets.UdpClient udpClient =
        (System.Net.Sockets.UdpClient)a_result.AsyncState;

        // 受信データをバイト列として取得する
        System.Net.IPEndPoint endPoint = null;
        byte[] receiveBytes = udpClient.EndReceive(a_result, ref endPoint);

        // 受信データを受信時処理に引き渡す
        UDPReceiveEvent(receiveBytes);

        // 非同期受信を再開する
        udpClient.BeginReceive(OnReceived, udpClient);
    }
#endif
}