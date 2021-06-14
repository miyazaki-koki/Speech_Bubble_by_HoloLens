using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axis_cre : MonoBehaviour {

    // Use this for initialization
    public LineRenderer lr2;

    // Use this for initialization
    void Start()
    {
        Vector3 startVec = new Vector3(-300f, 0.0f, 0.0f);
        Vector3 endVec = new Vector3(750f, 0.0f, 0.0f);
        lr2.SetPosition(0, startVec);
        lr2.SetPosition(1, endVec);
    }
        // Update is called once per frame
        void Update () {
		
	}
}
