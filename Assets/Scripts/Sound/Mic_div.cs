using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine.UI;
#if UNITY_UWP
using Windows.Storage;
using System.Text;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
#endif

public class Mic_div : MonoBehaviour {
    public MicStream.StreamCategory _streamtp = MicStream.StreamCategory.HIGH_QUALITY_VOICE;
    public int _gain = 1;
    private bool keepAllData=false;
    public bool _ini = true;
    private List<short> _data = new List<short>();
    private float _time = 0;

#if UNITY_UWP
    private Task task;
#endif

    // Use this for initialization
    void Start () {
        gameObject.GetComponent<Text>().text = ":DATA:";
        /*
        gameObject.GetComponent<Text>().text ="";
        AudioConfiguration config = AudioSettings.GetConfiguration();
        config.numRealVoices = 4;
        config.speakerMode = AudioSpeakerMode.Quad;
        AudioSettings.Reset(config);

        foreach (string d in Microphone.devices)
        {

            gameObject.GetComponent<Text>().text += d.ToString() + "\n";
        }
        gameObject.GetComponent<Text>().text += AudioSettings.driverCapabilities.ToString() + "\n";
        gameObject.GetComponent<Text>().text += AudioSettings.speakerMode.ToString() + "\n";

    */
    }

    private void Awake()
    {
        MicStream.CheckForErrorOnCall(MicStream.MicInitializeCustomRate((int)_streamtp, AudioSettings.outputSampleRate));
        MicStream.CheckForErrorOnCall(MicStream.MicStartStream(keepAllData, false));
        MicStream.CheckForErrorOnCall(MicStream.MicSetGain(_gain));
    }
	
	// Update is called once per frame
	void Update () {
        if (_time >= 10)
        {
            _ini = false;
            CheckForErrorOnCall(MicStream.MicStopStream());
            Write_Data();
            gameObject.GetComponent<Text>().text += "End" + "\n";
        }
        else
        {
            _time += Time.deltaTime;
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        gameObject.GetComponent<Text>().text += "channels:"+ channels.ToString() + "\n";
        if (!_ini) return;
        lock (this) {
            MicStream.CheckForErrorOnCall(MicStream.MicGetFrame(data, data.Length, channels));
            foreach (var f in data)
            {
                _data.Add(F_To_I16(f));
            }
        }
    }

    private short F_To_I16(float value)
    {
        var f = value * short.MaxValue;
        if (f > short.MaxValue) f = short.MaxValue;
        if (f < short.MinValue) f = short.MinValue;
        return (short)f;
    }
    private void OnDestroy()
    {
        MicStream.CheckForErrorOnCall(MicStream.MicStopStream());
    }
    private void CheckForErrorOnCall(int returnCode)
    {
        MicStream.CheckForErrorOnCall(returnCode);
    }

    private void Write_Data()
    {
        string filename = "StreamingData.wav";
        int header = 46;
        short expantion = 0;

        short bitsparsample = 16;
        short _channels = 2;
        int sample_rate = AudioSettings.outputSampleRate;
        var block_size = (short)(_channels * (bitsparsample / 8));
        var ave_bytes_per_second = sample_rate * block_size;

        var data_count = _data.Count;
        var data_bytesize = data_count * block_size * _channels;

#if UNITY_UWP
        task = Task.Run(async () =>
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            using (var outputStrm = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var bytes = Encoding.UTF8.GetBytes("RIFF");

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = BitConverter.GetBytes(header + data_bytesize - 8);

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = Encoding.UTF8.GetBytes("WAVE");

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = Encoding.UTF8.GetBytes("fmt ");

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = BitConverter.GetBytes(18);

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = BitConverter.GetBytes((short)1);

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = BitConverter.GetBytes(_channels);

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = BitConverter.GetBytes(sample_rate);

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = BitConverter.GetBytes(ave_bytes_per_second);

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = BitConverter.GetBytes(block_size);

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = BitConverter.GetBytes(bitsparsample);

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = BitConverter.GetBytes(expantion);

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = Encoding.UTF8.GetBytes("data");

                await outputStrm.WriteAsync(bytes.AsBuffer());

                bytes = BitConverter.GetBytes(data_bytesize);

                await outputStrm.WriteAsync(bytes.AsBuffer());

                foreach (var d in _data)
                {
                    var dat = BitConverter.GetBytes(d);

                    await outputStrm.WriteAsync(dat.AsBuffer());
                }
            }
        });
        task.Wait();
#endif

    }
}
