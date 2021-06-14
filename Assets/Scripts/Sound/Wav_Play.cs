using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using System.Text.RegularExpressions;
#if UNITY_UWP
using HoloLensModule.Network;
using System.Threading.Tasks;
using System;
#endif


namespace HoloToolkit.Unity.InputModule.Tests
{
    [RequireComponent(typeof(AudioSource))]
public class Wav_Play : MonoBehaviour {
#if UNITY_UWP
        UdpVet_C_M client;
#endif
    public int port;
    private string adress;

    //New!
    private DictationRecognizer m_DictationRecognizer;

    private List<short> _data = new List<short>();
    public float InputGain = 1;
    public bool PlaybackMicrophoneAudioSource = true;

    private float averageAmplitude;
    private bool flg;

    [SerializeField]
    private float minObjectScale = .3f;

    [SerializeField]
    private GameObject Sound_ON = null;
    [SerializeField]
    private GameObject Sound_OFF = null;
    private bool isRunning;
    public bool IsRunning
    {
        get { return isRunning; }
        private set
        {
            isRunning = value;
        }
    }

    private void OnAudioFilterRead(float[] buffer, int numChannels)
    {
        if (!IsRunning) return;

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

        if (averageAmplitude > 0.075) { flg = true; }


        lock (this)
        {
            if (flg)
            {
                foreach (var f in buffer)
                {
                    _data.Add(F_To_I16(f));
                }
            }
        }
    }

    private short F_To_I16(float value)
    {
        var f = value * short.MaxValue * 0.9;
        if (f > short.MaxValue) f = short.MaxValue * 0.98f;
        if (f < short.MinValue) f = short.MinValue * 0.98f;
        return (short)f;
    }

    private void OnEnable()
    {
        IsRunning = true;
    }

    private void Awake()
    {
        Sound_ON.SetActive(true);
        Sound_OFF.SetActive(false);
#if UNITY_UWP
        client = new UdpVet_C_M(port, adress);
#endif

        if (!PlaybackMicrophoneAudioSource)
        {
            gameObject.GetComponent<AudioSource>().volume = 0; // can set to zero to mute mic monitoring
        }
    }

    void Start()
    {
        flg = false;
        OnEnable();

        m_DictationRecognizer = new DictationRecognizer();
        m_DictationRecognizer.InitialSilenceTimeoutSeconds = 10;

        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            flg = false;
            _WriteWavAsync(_data.ToArray());
            _data.Clear();
        };

        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            _data.Clear();
            if (completionCause == DictationCompletionCause.TimeoutExceeded)
            {
                m_DictationRecognizer.Start();
            }
        };
        m_DictationRecognizer.Start();

    }

    private void Update()
    {
        gameObject.transform.localScale = new Vector3(minObjectScale, minObjectScale, minObjectScale);
    }

    private void OnApplicationPause(bool pause)
    {
        IsRunning = !pause;
    }

    private void OnDisable()
    {
        IsRunning = false;
    }

#if !UNITY_EDITOR
        private void OnApplicationFocus(bool focused)
        {
            IsRunning = focused;
        }
#endif


    public void _WriteWavAsync(short[] _dat)
    {
#if UNITY_UWP
            int index = 0;
            short _seq = 0;
            int _size = _data.Count;
            int pac_size = 200;
            byte[] data_pac = new byte[pac_size];
            Task.Run(() =>
            {

                foreach (short d in _dat)
            {
                var _d = BitConverter.GetBytes(d);
                Array.Copy(_d, 0, data_pac, index * 2, _d.Length);
                index++;
                _size--;

                if(index * 2 >= pac_size)
                {
                    byte[] br = BitConverter.GetBytes(_seq);
                    //client.SBytes(cordinate.Concat((br.Concat(data_pac)).ToArray());
                    client.SBytes(br.Concat(data_pac).ToArray());
                    _seq++;
                    index = 0;
                    Array.Clear(data_pac,0,data_pac.Length);
                }else if(_size == 0)
                {
                    byte[] br = BitConverter.GetBytes(_seq);
                    //client.SBytes(cordinate.Concat((br.Concat(data_pac)).ToArray());
                    client.SBytes(br.Concat(data_pac).ToArray());
                    _seq++;
                }
            }
                client.SShort(short.MinValue);
                _seq = 0;
            });

            //byte[] _lim = BitConverter.GetBytes(short.MinValue);

#endif

        }
    }
}
