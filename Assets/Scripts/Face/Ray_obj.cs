using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ray_obj : MonoBehaviour {

    private GameObject gaze;

    private Vector3 pos;
    private Quaternion rot;
    public RaycastHit hit;
    private TextMesh sen = null;
    public int _layer = 0;
    private int layerMask;

    // Use this for initialization
    void Start()
    {
        layerMask = 1 << _layer;
        gaze = GameObject.Find("HoloLensCamera").gameObject;
        sen = GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        sen.text = "x:" + rot_x(Camera.main.transform.localRotation.eulerAngles.x);
        sen.text += "y:" + rot_y(Camera.main.transform.localRotation.eulerAngles.y);
        sen.text += "z:" + Camera.main.transform.localRotation.eulerAngles.z;
    }

    private Vector3 Relative_pos_y(Vector3 xyz, float _angle)
    {
        Vector3 _pos = new Vector3(
            xyz.x * Mathf.Cos(_angle * (Mathf.PI / 180)) - xyz.z * Mathf.Sin(_angle * (Mathf.PI / 180)),
            xyz.y,
            xyz.x * Mathf.Sin(_angle * (Mathf.PI / 180)) + xyz.z * Mathf.Cos(_angle * (Mathf.PI / 180))
            );

        return _pos;
    }

    private Vector3 Relative_pos_x(Vector3 xyz, float _angle)
    {
        Vector3 _pos = new Vector3(
            xyz.x,
            xyz.y * Mathf.Cos(_angle * (Mathf.PI / 180)) + xyz.z * Mathf.Sin(_angle * (Mathf.PI / 180)),
            -xyz.y * Mathf.Sin(_angle * (Mathf.PI / 180)) + xyz.z * Mathf.Cos(_angle * (Mathf.PI / 180))
            );

        return _pos;
    }

    private float rot_y(float z)
    {
        //z -= 90;

        if (z < 0)
        {
            z += 360;
        }

        if (180 < z && z < 360)
        {
            z -= 360;
        }

        //z *= -1;
        return z;
    }

    private float rot_x(float z)
    {
        z -= 90;

        if (z < 0)
        {
            z += 360;
        }

        if (180 < z && z < 360)
        {
            z -= 360;
        }

        z *= -1;
        z -= 90;
        return z;
    }
}
