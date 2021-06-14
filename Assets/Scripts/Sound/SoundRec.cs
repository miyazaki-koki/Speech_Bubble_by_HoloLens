using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(LineRenderer))]
public class SoundRec: MonoBehaviour {
    //private int time=0;

    public LineRenderer lr;
    private int index;
    private int count;


    private AudioSource _audio;
    [HideInInspector]
    public float[] data; //=new float[1024]
    private int channel=1;

    internal float output;

    void Conduct(float data)
    {
        if (count > 1000) count = 0;
        count++;
        lr.positionCount = count;
        lr.SetPosition(count - 1, new Vector3(count*0.001f, data, 2));
        Debug.Log(data+"   "+index.ToString());
        index++;
    }

    float Mic_Volume(float[] data)
    {
        float a = 0;
        foreach(float i in data)
        {
            a += Math.Abs(i);
        }

        return a / 256.0f;
    }

    // Use this for initialization
    void Start()
    {

        index = 0;
        count = 0;
        lr.positionCount = count;

        _audio = GetComponent<AudioSource>();

        _audio.clip = Microphone.Start(null, true, 999, 44100);

        while (Microphone.GetPosition(null) <= 0) { Debug.Log("Loading..."); }
        Debug.Log("play");
        _audio.Play();

    }

	// Update is called once per frame
	void Update () {

            _audio.GetOutputData(data, channel);
            Conduct(Mic_Volume(data));
    }
}
