using UnityEngine;
using System;// Needed for Math

public class PlayWave : MonoBehaviour
{
    const double PI = System.Math.PI;
    const double PI2 = PI * 2.0;
    const double PI_2 = PI / 2.0;
    const double sampling_frequency = 48000;
    const double PI2_SR = PI2 / sampling_frequency;
    private enum PlayState
    {
        Stop,
        SineWave,
        SquareWave,
        TriangleWave,
        SawtoothWave
    }

    const double frequency = 440;
    public double gain = 0.05;
    private double increment;
    private double time;
    private PlayState playState = PlayState.Stop;

    void SineWave(float[] data, int channels)
    {
        increment = frequency * 2 * PI / sampling_frequency;
        for (var i = 0; i < data.Length; i = i + channels)
        {
            time = time + increment;

            data[i] = (float)(gain * Math.Sin(time));
            if (channels == 2)
                data[i + 1] = data[i];
            if (time > 2 * Math.PI)
                time = 0;
        }
    }

    void SquareWave(float[] data, int channels)
    {
        increment = frequency * 2 * PI / sampling_frequency;
        for (var i = 0; i < data.Length; i = i + channels)
        {
            time = time + increment;

            data[i] = (float)(gain * ((time % PI2) < PI2 * 0.5 ? 1.0 : -1.0));
            if (channels == 2)
                data[i + 1] = data[i];
            if (time > 2 * Math.PI)
                time = 0;
        }
    }

    void TriangleWave(float[] data, int channels)
    {
        increment = frequency * 2 * PI / sampling_frequency;
        for (var i = 0; i < data.Length; i = i + channels)
        {
            time = time + increment;
            if (time > 2 * Math.PI)
                time = 0;

            double t = (time + PI_2) % PI2;
            data[i] = (float)(gain * ((t < PI ? t - PI : PI - t) / PI_2 + 1.0));
            if (channels == 2)
                data[i + 1] = data[i];
        }
    }

    void SawtoothWave(float[] data, int channels)
    {
        increment = frequency * 2 * PI / sampling_frequency;
        for (var i = 0; i < data.Length; i = i + channels)
        {
            time = time + increment;

            data[i] = (float)(gain * ((time + PI) % PI2) / PI - 1.0);
            if (channels == 2)
                data[i + 1] = data[i];
            if (time > 2 * Math.PI)
                time = 0;
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        switch (playState)
        {
            case PlayState.SineWave:
                SineWave(data, channels);
                break;
            case PlayState.SquareWave:
                SquareWave(data, channels);
                break;
            case PlayState.TriangleWave:
                TriangleWave(data, channels);
                break;
            case PlayState.SawtoothWave:
                SawtoothWave(data, channels);
                break;
        }
    }

    void OnGUI()
    {
        int y = 10;
        if (GUI.Button(new Rect(10, y, 100, 30), "サイン波"))
        {
            playState = PlayState.SineWave;
        }
        y += 40;
        if (GUI.Button(new Rect(10, y, 100, 30), "矩形波"))
        {
            playState = PlayState.SquareWave;
        }
        y += 40;
        if (GUI.Button(new Rect(10, y, 100, 30), "三角波"))
        {
            playState = PlayState.TriangleWave;
        }
        y += 40;
        if (GUI.Button(new Rect(10, y, 100, 30), "ノコギリ波"))
        {
            playState = PlayState.SawtoothWave;
        }
        y += 40;
        if (GUI.Button(new Rect(10, y, 100, 30), "Stop"))
        {
            playState = PlayState.Stop;
        }
    }
}