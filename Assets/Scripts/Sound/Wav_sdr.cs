using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.UI;
#if UNITY_UWP
using HoloLensModule.Network;
using System.Threading.Tasks;
using System;
#endif

namespace HoloToolkit.Unity.InputModule.Tests
{
    [RequireComponent(typeof(AudioSource))]
    public class Wav_sdr : MonoBehaviour
    {
#if UNITY_UWP
        UdpVet_C_M client;
#endif
        public int port;
        public string adress;
        short[] data;
        public Text obj = null;

        private void Awake()
        {
#if UNITY_UWP
        client = new UdpVet_C_M(port, adress);
#endif
        }

        public short[] LoadWAVData(string name)
        {
            // Load local folder
            var _data = File.ReadAllBytes(name);
            short[] d = new short[_data.Length / 2];
            for (int i = 0; i < _data.Length / 2; i++)
            {
                d[i] = BitConverter.ToInt16(_data, i * 2);
            }
            return d;
        }

        void Start()
        {
        }

        public void OnClick()
        {
            obj.text = DateTime.Now.ToString("hh.mm.ss.ffffff");
            var file = Path.Combine(Application.persistentDataPath, "StreamingData.wav");
            if (File.Exists(file))
            {
                //_WriteWavAsync(LoadWAVData(file));//データ一括
                StartCoroutine(DelayMethod(file));
            }
            else
            {
                obj.text = "File Not Found...";
            }
        }

        private IEnumerator DelayMethod(string file)
        {
            const int size = 2048;
            short[] vs = LoadWAVData(file);
            for (int i = 0; i < (int)(vs.Length / size); i++)
            {
                if (vs.Length > ((i + 1) * size))
                {
                    short[] d_p = new short[size];

                    Array.Copy(vs, i * size, d_p, 0, size);
                    _WriteWav_sep_Async(d_p);
                }
                else
                {
                    short[] d_p = new short[size];
                    Array.Copy(d_p, i * size, d_p, 0, d_p.Length - (i * size));
                    _WriteWav_sep_Async(d_p);
                }
                yield return null;
            }
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

        public void _WriteWav_sep_Async(short[] _dat)
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
                    //client.SBytes(cordinate.Concat((br.Concat(data_pac)).ToArray());
                    client.SBytes(br.Concat(data_pac).ToArray());
                    _seq++;
                    index = 0;
                    Array.Clear(data_pac, 0, data_pac.Length);
                }
                else if (_size == 0)
                {
                    byte[] br = BitConverter.GetBytes(_seq);
                    //client.SBytes(cordinate.Concat((br.Concat(data_pac)).ToArray());
                    client.SBytes(br.Concat(data_pac).ToArray());
                    _seq++;
                }
            }
            //client.SShort(short.MinValue);
            _seq = 0;
            });
            //byte[] _lim = BitConverter.GetBytes(short.MinValue);
#endif
        }
    }
}
