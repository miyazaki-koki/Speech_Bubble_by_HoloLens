using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.IO;
#else
using Windows.UI.Core;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
#endif
using System.Text;
using UnityEngine;
using HoloLensModule.Network;
using UnityEngine.UI;
using HoloToolkit.Unity.InputModule;
using UnityEngine.Windows.Speech;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class Sound_Mic : MonoBehaviour
{
    //New!
    private DictationRecognizer m_DictationRecognizer;

    [SerializeField]
    private GameObject Sound_ON = null;
    [SerializeField]
    private GameObject Sound_OFF = null;

    [SerializeField]
    private GameObject Speaker_Img = null;

    public MicStream.StreamCategory StreamType = MicStream.StreamCategory.HIGH_QUALITY_VOICE;
    private List<short> _data = new List<short>();
    public float InputGain = 1;
    public bool PlaybackMicrophoneAudioSource = true;

    private float averageAmplitude;

    [SerializeField]
    private float minObjectScale = .3f;

    private bool isRunning;
    public bool IsRunning
    {
        get { return isRunning; }
        private set
        {
            isRunning = value;
            CheckForErrorOnCall(!isRunning ? MicStream.MicPause() : MicStream.MicResume());
        }
    }

    private void OnAudioFilterRead(float[] buffer, int numChannels)
    {
        if (!IsRunning) return;
        CheckForErrorOnCall(MicStream.MicGetFrame(buffer, buffer.Length, numChannels));
        /*
        float sumOfValues = 0;
        for (int i = 0; i < buffer.Length; i++)
        {

            if (float.IsNaN(buffer[i]))
            {
                buffer[i] = 0;
            }

            buffer[i] = Mathf.Clamp(buffer[i], -1f, 1f);
            sumOfValues += Mathf.Clamp01(Mathf.Abs(buffer[i]));
        }

        averageAmplitude = sumOfValues / buffer.Length;

        //if (averageAmplitude > 0.075) { flg = true; }//閾値決定(使用していない)
        */

        lock (this)
        {
            foreach (var f in buffer)
            {
                _data.Add(F_To_I16(f));
            }
        }
    }

    private short F_To_I16(float value)
    {
        var f = value * short.MaxValue * 0.9;
        if (f > short.MaxValue) f = short.MaxValue * 0.98f;
        if (f < short.MinValue) f = short.MinValue * 1.02f;
        return (short)f;
    }

    private void Awake()
    {
        CheckForErrorOnCall(MicStream.MicInitializeCustomRate((int)StreamType, 16000)); //AudioSettings.outputSampleRate [Edit ->project setting -> Audio]
        CheckForErrorOnCall(MicStream.MicSetGain(InputGain));

        if (!PlaybackMicrophoneAudioSource)
        {
            gameObject.GetComponent<AudioSource>().volume = 0; // can set to zero to mute mic monitoring
        }
    }

    void Start()
    {
        IsRunning = false;
        /*
        //開始タイミング
        m_DictationRecognizer = new DictationRecognizer();
        m_DictationRecognizer.InitialSilenceTimeoutSeconds = 10;

        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            Speaker_Img.SetActive(false);
            IsRunning = false;
        };

        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            Speaker_Img.SetActive(true);
            IsRunning = true;
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause == DictationCompletionCause.TimeoutExceeded)
            {
                m_DictationRecognizer.Start();
            }
        };
        */
    }

    public void Start_OnClick()
    {
        Sound_OFF.SetActive(false);
        Sound_ON.SetActive(true);
        //m_DictationRecognizer.Start();
        IsRunning = true;
    }

    public void Stop_OnClick()
    {
        Sound_OFF.SetActive(true);
        Sound_ON.SetActive(false);
        //m_DictationRecognizer.Stop();
        IsRunning = false;
        Write_Data(_data);
    }

    private void Update()
    {
        gameObject.transform.localScale = new Vector3(minObjectScale, minObjectScale, minObjectScale);
    }

    private static void CheckForErrorOnCall(int returnCode)
    {
        MicStream.CheckForErrorOnCall(returnCode);
    }


    private void Write_Data(List<short> _data)
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

        Task task =Task.Run(async () =>
        {
#if UNITY_UWP
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
#endif
            _data.Clear();
        });
    }
}
