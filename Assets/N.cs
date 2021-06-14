using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class N : MonoBehaviour {

    public GameObject[] Target;

    Quaternion Rot;
    public int count = 0;
    Vector3[] toTarget;

    void Start () {
        toTarget = new Vector3[count];
        Rot = new Quaternion(0, 0, 0, 0);

        for(int i = 0; i < Target.Length; i++)
        {
            toTarget[i] = Target[i].transform.position  - Camera.main.transform.position;
            toTarget[i].y = 0;
        }

    }
	

	void Update () {

        for (int i = 0; i < Target.Length; i++)
        {
            Rot = Quaternion.FromToRotation(Camera.main.transform.right, toTarget[i]);
            Rot.w = Rot.w * -1;
            Debug.Log(Rot);
            Debug.Log(Rot.eulerAngles);
        }

    }
}
