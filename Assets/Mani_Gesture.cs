using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;

public class Mani_Gesture : MonoBehaviour {

    private Vector3 lastPos=Vector3.zero;
    private bool _flg = false;

    void Start()
{
    InteractionManager.InteractionSourceDetected += SourceDetected;
    InteractionManager.InteractionSourceUpdated += SourceUpdated;
    InteractionManager.InteractionSourceLost += SourceLost;
    InteractionManager.InteractionSourcePressed += SourcePressed;
    InteractionManager.InteractionSourceReleased += SourceReleased;
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
                gameObject.GetComponent<Slider>().value = (pos - lastPos).y * 5;
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

}
