using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class Recieve_Array_data: MonoBehaviour
{

    private TextMesh sentence;
    Quaternion Rot;
    Vector3 toTarget;
    public GameObject _Plane = null;
    UDP_Array_Listen udp = null;

    // Use this for initialization
    void Start()
    {
        sentence = GetComponent<TextMesh>();
        sentence.text = "Nodata";
        udp = GameObject.Find("UDP_Recieve").GetComponent<UDP_Array_Listen>();

        Rot = new Quaternion(0, 0, 0, 0);
        toTarget = _Plane.transform.position - Camera.main.transform.position;
        toTarget.y = 0;
        Rot = Quaternion.FromToRotation(Camera.main.transform.right, toTarget);
        Rot.w = Rot.w * -1;

    }

    // Update is called once per frame
    void Update()
    {
        sentence.text = "";
        foreach(string tx in udp.W_s)
        {
            sentence.text += tx;
            //sentence.text += Contents(udp.W_s[i]); //分離用
            sentence.text += Environment.NewLine;
        }

    }

    //分離用ヘッダー確認メソッド
    string Contents(string _text)
    {
        string str = "";
        int i = int.Parse(Regex.Match(_text, @".+d*:").Value.Trim(':'));

        if (((Rot_y(Rot.eulerAngles.y) + Rot_main_cam(Camera.main.transform.rotation.eulerAngles.y) + 10) >= i)
                && (i >= (Rot_y(Rot.eulerAngles.y) + Rot_main_cam(Camera.main.transform.rotation.eulerAngles.y) - 10)))
        {
            str = Regex.Replace(_text, @".+\d*:", "");
        }

        return str;
    }

    private float Rot_y(float z)
    {

        return 180 - z;
    }

    private float Rot_main_cam(float z)
    {
        if (360 > z && z >= 270)
        {
            z -= 360;
        }

        return z;
    }

}
