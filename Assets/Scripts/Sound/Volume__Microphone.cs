// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using HoloLensModule.Network;
using System;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;
#if UNITY_UWP
using System.Threading.Tasks;
#endif

namespace HoloToolkit.Unity.InputModule.Tests
{
    [RequireComponent(typeof(AudioSource))]
    public class Volume__Microphone : MonoBehaviour
    {
        UdpVet_C_M client;
        public int port;
        public string adress;
        private Vector3 lastPos = Vector3.zero;
        private bool _flg = false;

        public MicStream.StreamCategory StreamType = MicStream.StreamCategory.HIGH_QUALITY_VOICE;
        private List<short> _data = new List<short>();
        public float InputGain = 1;
        public bool PlaybackMicrophoneAudioSource = true;
        private float averageAmplitude;
        private float _Volume=0;
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
            lock (this)
            {
                    foreach(float b in buffer)
                    {
                    _data.Add(F_To_I16(b));
                }
            }
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
            */
        }

        private short F_To_I16(float value)
        {
            var f = value * short.MaxValue;
            if (f > short.MaxValue) f = short.MaxValue;
            if (f < short.MinValue) f = short.MinValue;
            return (short)f;
        }

        private void OnEnable()
        {
            IsRunning = true;
        }

        private void Awake()
        {
            InteractionManager.InteractionSourceDetected += SourceDetected;
            InteractionManager.InteractionSourceUpdated += SourceUpdated;
            InteractionManager.InteractionSourceLost += SourceLost;
            InteractionManager.InteractionSourcePressed += SourcePressed;
            InteractionManager.InteractionSourceReleased += SourceReleased;

            _Volume = gameObject.GetComponent<Slider>().value;
#if UNITY_UWP
        client = new UdpVet_C_M(port, adress);
#endif
            CheckForErrorOnCall(MicStream.MicInitializeCustomRate((int)StreamType, 16000)); //AudioSettings.outputSampleRate [Edit ->project setting -> Audio]
            CheckForErrorOnCall(MicStream.MicSetGain(InputGain));

            if (!PlaybackMicrophoneAudioSource)
            {
                gameObject.GetComponent<AudioSource>().volume = 0; // can set to zero to mute mic monitoring
            }

            isRunning = true;
        }
        private float _time = 0;
        private bool flg = true;

        private void Update()
        {
            CheckForErrorOnCall(MicStream.MicSetGain(_Volume));

            if (_time >= 15)
            {
                isRunning = false;
            }
            else
            {
                _time += Time.deltaTime;
            }

            if (!isRunning && flg)
            {
                flg = false;
                _WriteWavAsync();
                _time = 0;
                //CheckForErrorOnCall(MicStream.MicStopStream());

            }

            gameObject.transform.localScale = new Vector3(minObjectScale + averageAmplitude, minObjectScale + averageAmplitude, minObjectScale + averageAmplitude);
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

        public void Enable()
        {
            IsRunning = true;
        }

        public void Disable()
        {
            IsRunning = false;
        }

        void SourceDetected(InteractionSourceDetectedEventArgs state)
        {
            Vector3 pos;
            if (state.state.sourcePose.TryGetPosition(out pos))
            {
                lastPos = pos;
            }
        }

        void SourceUpdated(InteractionSourceUpdatedEventArgs state)
        {
            if (_flg)
            {
                Vector3 pos;
                if (state.state.sourcePose.TryGetPosition(out pos))
                {
                    // 手の移動量
                    _Volume = (pos - lastPos).y * 5;
                }
            }
        }

        void SourceLost(InteractionSourceLostEventArgs state)
        {
            _flg = false;
        }

        void SourcePressed(InteractionSourcePressedEventArgs state)
        {
            _flg = true;
        }

        void SourceReleased(InteractionSourceReleasedEventArgs state)
        {
            _flg = false;
        }

        public void _WriteWavAsync()
        {
#if UNITY_UWP
            int index = 0;
            int _size = _data.Count;
            int pac_size = 500;
            byte[] data_pac = new byte[pac_size];

            Task.Run(async () =>
            {
                foreach (short d in _data)
            {
                var _d = BitConverter.GetBytes(d);
                Array.Copy(_d, 0, data_pac, index * 2, _d.Length);
                index++;
                _size--;

                if(index * 2 >= pac_size)
                {
                    await client.SBytesAsync(data_pac);
                    index = 0;
                    Array.Clear(data_pac,0,data_pac.Length);
                }else if(_size == 0)
                {
                    await client.SBytesAsync(data_pac);
                }
            }
                await client.SShortAsync(short.MinValue);
            });
            
            //byte[] _lim = BitConverter.GetBytes(short.MinValue);

#endif

        }
    }
}
