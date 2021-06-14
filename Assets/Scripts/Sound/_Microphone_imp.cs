// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Windows.Speech;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
#if UNITY_UWP
using HoloLensModule.Network;
using System.Threading.Tasks;
#endif

namespace HoloToolkit.Unity.InputModule.Tests
{
    [RequireComponent(typeof(AudioSource))]
    public class _Microphone_imp : MonoBehaviour
    {
#if UNITY_UWP
        UdpVet_C_M client;
#endif
        public int port;
        private string adress = "192.168.10.35";
        private List<short> _data = new List<short>();

        public MicStream.StreamCategory StreamType = MicStream.StreamCategory.HIGH_QUALITY_VOICE;
        public float InputGain = 1;
        public bool PlaybackMicrophoneAudioSource = true;

        private bool p_UDPSendFlg = false;

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
                CheckForErrorOnCall(!isRunning ? MicStream.MicPause() : MicStream.MicResume());
            }
        }

        private void OnAudioFilterRead(float[] buffer, int numChannels)
        {
            if (!IsRunning) return;
            CheckForErrorOnCall(MicStream.MicGetFrame(buffer, buffer.Length, numChannels));

            _data.Clear();
            
            lock (this)
            {
                foreach (var f in buffer)
                {
                    _data.Add(F_To_I16(f));
                }
                p_UDPSendFlg = true;
            }
            
        }

        private short F_To_I16(float value)
        {
            var f = value * short.MaxValue * 0.9;
            if (f > short.MaxValue) f = short.MaxValue * 0.98f;
            if (f < short.MinValue) f = short.MinValue * 1.02f;
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
            Regex re = new Regex(@"[^0-9.]");
            adress = re.Replace(GameObject.Find("Key_I").GetComponent<ButtonSource>().PCIP, "");
#if UNITY_UWP
        client = new UdpVet_C_M(port, adress);
#endif
            CheckForErrorOnCall(MicStream.MicInitializeCustomRate((int)StreamType, 16000));
            CheckForErrorOnCall(MicStream.MicSetGain(InputGain));

            if (!PlaybackMicrophoneAudioSource)
            {
                gameObject.GetComponent<AudioSource>().volume = 0; // can set to zero to mute mic monitoring
            }
        }

        void Start()
        {
            IsRunning = true;
        }

        private void Update()
        {
            if (p_UDPSendFlg)
            {
                _WriteWavAsync(_data.ToArray());

            }
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

        private void OnDestroy()
        {
            CheckForErrorOnCall(MicStream.MicDestroy());
        }

#if !UNITY_EDITOR
        private void OnApplicationFocus(bool focused)
        {
            IsRunning = focused;
        }
#endif

        private static void CheckForErrorOnCall(int returnCode)
        {
            MicStream.CheckForErrorOnCall(returnCode);
        }

        public void _WriteWavAsync(short[] _dat)
        {
#if UNITY_UWP
            int index = 0;
            short _seq = 0;
            int _size = _dat.Length;
            int pac_size = 512;
            byte[] data_pac = new byte[pac_size];
            Task.Run(() =>
            {
                foreach (short d in _dat)
            {
                var _d = BitConverter.GetBytes(d);
                Array.Copy(_d, 0, data_pac, index * 2, _d.Length);
                index++;
                _size--;

                if (index * 2 >= pac_size)
                {
                    byte[] br = BitConverter.GetBytes(_seq);
                    client.SBytes(br.Concat(data_pac).ToArray());
                    _seq++;
                    index = 0;
                    Array.Clear(data_pac, 0, data_pac.Length);
                }
                else if (_size == 0)
                {
                    byte[] br = BitConverter.GetBytes(_seq);
                    client.SBytes(br.Concat(data_pac).ToArray());
                    _seq++;
                }
                }
            _seq = 0;
            });
#endif
            p_UDPSendFlg = false;
        }
    }
}
