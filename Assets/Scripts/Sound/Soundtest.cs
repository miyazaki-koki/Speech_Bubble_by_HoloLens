using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;
using System;
#if UNITY_UWP
using System.Threading.Tasks;
using System.Threading;
#endif

[RequireComponent(typeof(AudioSource))]
public class Soundtest : MonoBehaviour {
    //private int time=0;
#if A
    public LineRenderer lr;
    private int index;
    private int count;
#endif

    UdpNetworkClientManager client;
    public int port;
    public string adress;

    private AudioSource _audio;
    [HideInInspector]
    public float[] data;
    public int channel=4;

    internal float output;
#if A
    void Conduct(float[] data)
    {
        if (count > 1000) count = 0;
        if (index >= data.Length) index = 0;
        count++;
        lr.positionCount = count;
        lr.SetPosition(count - 1, new Vector3(-200 + count, data[index] * 1000, 0));
        index++;
    }
#endif

    float Mic_Volume(float[] data)
    {
        float a = 0;
        foreach (float i in data)
        {
            a += Math.Abs(i);
        }

        return a / 256.0f;
    }

    // Use this for initialization
    void Start()
    {
#if UNITY_UWP
        client = new UdpNetworkClientManager(port, adress);
#endif
#if A
        index = 0;
        count = 0;
        lr.positionCount = count;
#endif

#if DEVICE_NAME
        foreach (string device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }
#endif
        _audio = GetComponent<AudioSource>();

        _audio.clip = Microphone.Start(null, true, 999, 44100);

        while (Microphone.GetPosition(null) <= 0) {}

        _audio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (Microphone.IsRecording(null) == false)
        {
            for (int i = 0; i < channel; i++)
            {
                _audio.GetOutputData(data, i);
#if UNITY_UWP
            client.SMessage("ch" + i.ToString());
            client.SMessage(Mic_Volume(data).ToString());
#endif
            }
#if A
            Conduct(data);
#endif
        }
    }
}
