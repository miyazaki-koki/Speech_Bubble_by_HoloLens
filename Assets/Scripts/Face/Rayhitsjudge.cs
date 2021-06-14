using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rayhitsjudge : MonoBehaviour {

    Ray_obj hit_obj;
    public GameObject cam1;
    public GameObject _cam;
    int _width;
    int _height;

    void Start (){
        hit_obj = GameObject.Find("3DTextPrefab").GetComponent<Ray_obj>();

    }

    // Update is called once per frame
    void Update () {

        gameObject.GetComponent<Text>().text="Ray:"+hit_obj.hit.point.ToString() + " Pos:" + cam1.transform.position.ToString();
        gameObject.GetComponent<Text>().text += "Cam_Size(" + _cam.GetComponent<Photocap>().cam.cameraResolutionWidth.ToString() + " * "
            + _cam.GetComponent<Photocap>().cam.cameraResolutionHeight.ToString();
    }
}
