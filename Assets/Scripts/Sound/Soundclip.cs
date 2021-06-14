using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Soundclip : MonoBehaviour
{
    private AudioSource _audio;

    // Use this for initialization
    void Start()
    {
        _audio = GetComponent<AudioSource>();

        _audio.clip = Microphone.Start(null, true, 999, 44100);

        while (Microphone.GetPosition(null) <= 0) { Debug.Log("Loading..."); }

        _audio.Play();
    }

    // Update is called once per frame
    void Update()
    {

    }
}