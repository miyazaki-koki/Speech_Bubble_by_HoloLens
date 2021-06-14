using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube_rot : MonoBehaviour {

    private Vector3 pos;

	// Use this for initialization
	void Start () {
        pos = gameObject.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {

        gameObject.transform.position = Relative_pos_x(
            Relative_pos_y(
                pos,
                rot_y(Camera.main.transform.localRotation.eulerAngles.y)
                ),
            rot_x(Camera.main.transform.localRotation.eulerAngles.x));
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
            xyz.y * Mathf.Cos(_angle*(Mathf.PI / 180)) + xyz.z * Mathf.Sin(_angle * (Mathf.PI / 180)),
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

        z *= -1;
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
