using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloLensModule.Network;
#if UNITY_UWP
using System.Threading.Tasks;
using System.Threading;
#endif

public class Networktest : MonoBehaviour {
    UdpVet_C_M client1;

    public Text sentence;

    public int port;
    public string adress;
    void Start () {
#if UNITY_UWP
        client1 = new UdpVet_C_M(port, adress);
#endif
    }

    public void OnClick()
    {
        StartCoroutine(PingTest(adress));
        #if UNITY_UWP
        client1.SMessage("Test");
#endif
        sentence.text = System.DateTime.Now.ToString("hh:mm:ss:ffffff") + Environment.NewLine;
    }

    void Update () {
    }

    IEnumerator PingTest(string adress)
    {
        Ping ping = new Ping(adress);
        if (!ping.isDone) yield return new WaitForSeconds(.1f);
        sentence.text += "ping.ip : ";
        sentence.text += ping.ip;
        sentence.text += Environment.NewLine;
        sentence.text += "ping.time : ";
        sentence.text += ping.time;
        sentence.text += Environment.NewLine;
    }

}
