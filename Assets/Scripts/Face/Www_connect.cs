using HoloToolkit.Unity.SpatialMapping;
using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.XR.WSA.WebCam;


public class Www_connect : MonoBehaviour {

    private string url = "http://192.168.10.36:8080/";
    List<GameObject> face_list = new List<GameObject>();

    public GameObject target = null;
    [SerializeField]
    private GameObject face_dc = null;
    [SerializeField]
    private GameObject _mic = null;
    GameObject obj = null;

    private Quaternion rot;
    Vector3 s_pos,b_pos;
    public RaycastHit hit;

    private Transform _tf;

    void Start()
    {
        Regex re = new Regex(@"[^0-9.]");
        url= "http://"+re.Replace(GameObject.Find("Key_I").GetComponent<ButtonSource>().webIP,"")+":8080/";
    }

    public byte[] ReadPngFile(string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryReader bin = new BinaryReader(fileStream);
        byte[] values = bin.ReadBytes((int)bin.BaseStream.Length);

        bin.Dispose();

        return values;
    }

    public IEnumerator ConnectionStart(byte[] data)
    {
        face_dc.SetActive(true);
        _tf = Camera.main.transform;
        WWWForm form = new WWWForm();
        form.AddBinaryData("post_data", data, "test.jpg", "image/jpg");
        var www = new WWW(url, form);

        yield return www; //get data
        face_dc.SetActive(false);
        foreach (var face_l in face_list)
        {
            Destroy(face_l);
        }
        Set_Object(Json.Deserialize(www.text) as Dictionary<string, object>);
        //_mic.SetActive(true);
    }

    public IEnumerator ConnectionStart(byte[] data, PhotoCaptureFrame photoCaptureFrame, int imageWidth, int imageHeight)
    {
        face_dc.SetActive(true);
        WWWForm form = new WWWForm();
        form.AddBinaryData("post_data", data, "test.jpg", "image/jpg");
        var www = new WWW(url, form);

        yield return www; //get data
        face_dc.SetActive(false);
        foreach (var face_l in face_list)
        {
            Destroy(face_l);
        }

        Matrix4x4 cameraToWorldMatrix;
        photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
        Matrix4x4 projectionMatrix;
        photoCaptureFrame.TryGetProjectionMatrix(out projectionMatrix);

        Dictionary<string, object> _data = Json.Deserialize(www.text) as Dictionary<string, object>;
        for (int i = 0; i < Convert.ToInt32(_data["num"]); i++)
        {
            var pixelPos = new Vector2(Convert.ToSingle(_data["center_position_x" + i]), Convert.ToSingle(_data["center_position_y" + i]));
            var imagePosZeroToOne = new Vector2(pixelPos.x / imageWidth, 1 - (pixelPos.y / imageHeight));
            var imagePosProjected = (imagePosZeroToOne * 2) - new Vector2(1, 1);    // -1 to 1 space

            var cameraSpacePos = UnProjectVector(projectionMatrix, new Vector3(imagePosProjected.x, imagePosProjected.y, 1));
            var worldSpaceCameraPos = cameraToWorldMatrix.MultiplyPoint(Vector3.zero);     // camera location in world space
            var worldSpaceBoxPos = cameraToWorldMatrix.MultiplyPoint(cameraSpacePos);   // ray point in world space

            RaycastHit hit;
            bool hitToMap = Physics.Raycast(worldSpaceCameraPos, worldSpaceBoxPos - worldSpaceCameraPos, out hit, 5, SpatialMappingManager.Instance.LayerMask);

            obj = Instantiate(target) as GameObject;
            foreach (Transform childTransform in obj.GetComponentInChildren<Transform>())
            {
                foreach (Transform grandchildTransform in childTransform.GetComponentInChildren<Transform>())
                {
                    if (grandchildTransform.tag == "Bubble_box")
                    {
                        pixelPos = new Vector2(Convert.ToSingle(_data["rect_center_position_x" + i]), Convert.ToSingle(_data["rect_center_position_y" + i]));
                        imagePosZeroToOne = new Vector2(pixelPos.x / imageWidth, 1 - (pixelPos.y / imageHeight));
                        imagePosProjected = (imagePosZeroToOne * 2) - new Vector2(1, 1);    // -1 to 1 space
                        cameraSpacePos = UnProjectVector(projectionMatrix, new Vector3(imagePosProjected.x, imagePosProjected.y, 1));
                        worldSpaceBoxPos = cameraToWorldMatrix.MultiplyPoint(cameraSpacePos);   // ray point in world space

                        grandchildTransform.position = new Vector3(worldSpaceBoxPos.x, worldSpaceBoxPos.y, hit.point.z);
                        grandchildTransform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
                    }
                    else
                    {
                        //grandchildTransform.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                        grandchildTransform.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Vector3.up);
                        grandchildTransform.transform.position = hit.point;
                    }
                }
            }
            face_list.Add(obj);
        }
    }

    public static Vector3 UnProjectVector(Matrix4x4 proj, Vector3 to)
    {
        Vector3 from = new Vector3(0, 0, 0);
        var axsX = proj.GetRow(0);
        var axsY = proj.GetRow(1);
        var axsZ = proj.GetRow(2);
        from.z = to.z / axsZ.z;
        from.y = (to.y - (from.z * axsY.z)) / axsY.y;
        from.x = (to.x - (from.z * axsX.z)) / axsX.x;
        return from;
    }

    private void Set_Object(Dictionary<string, object> _data)
    {
        for (int i = 0; i < Convert.ToInt32(_data["num"]); i++)
        {//(0.0254f / 72.0f) * 3.22f=0.001135944f
            /*s_pos = Relative_pos_x(
                Relative_pos_y(
                    new Vector3(Convert.ToSingle(_data["center_position_x" + i]) * 0.00125f, 
                    Convert.ToSingle(_data["center_position_y" + i]) * 0.00125f + 0.05f , 2),
                    rot_y(Camera.main.transform.localRotation.eulerAngles.y)
                    ),
                rot_x(Camera.main.transform.localRotation.eulerAngles.x)
                );

            b_pos = Relative_pos_x(
    Relative_pos_y(
        new Vector3(Convert.ToSingle(_data["rect_center_position_x" + i]) * 0.00925f,
        Convert.ToSingle(_data["rect_center_position_y" + i]) * 0.00925f, 0),
        rot_y(Camera.main.transform.localRotation.eulerAngles.y)
        ),
    rot_x(Camera.main.transform.localRotation.eulerAngles.x)
    );
    */
            obj = Instantiate(target, _tf) as GameObject;
            obj.transform.SetParent(_tf);
            obj.transform.localScale = new Vector3(Convert.ToSingle(_data["x_length" + i]) * 0.0008f,
                Convert.ToSingle(_data["y_length" + i]) * 0.0008f, 1);
            obj.transform.localPosition = new Vector3(Convert.ToSingle(_data["center_position_x" + i]) * 0.001135944f,
                    Convert.ToSingle(_data["center_position_y" + i]) * 0.001135944f + 0.05f, 2);//s_pos
            obj.transform.rotation = Quaternion.LookRotation(obj.transform.localPosition - _tf.localPosition, Vector3.up);
            obj.transform.parent = null;
            foreach (Transform childTransform in obj.GetComponentInChildren<Transform>())
            {
                if(childTransform.tag == "Bubble_box")
                {
                    childTransform.localPosition = new Vector3(Convert.ToSingle(_data["rect_center_position_x" + i]) * 0.00925f,
        Convert.ToSingle(_data["rect_center_position_y" + i]) * 0.00925f, 0);
                }
            }
            face_list.Add(obj);
        }
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