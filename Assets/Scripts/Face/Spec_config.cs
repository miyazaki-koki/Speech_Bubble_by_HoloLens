using MiniJSON;
using UnityEngine;
using System.Linq;
using UnityEngine.XR.WSA.WebCam;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
#if UNITY_UWP
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Text;
#endif

public class Spec_config : MonoBehaviour {

    string filePath = null;
    string filename = null;

    TextMesh textmesh;

    int _count = 0;

    [SerializeField]
    private GameObject Camera_Image = null;
    [SerializeField]
    private float inter = 20;

    PhotoCapture photo_obj = null;
    public CameraParameters cam = new CameraParameters();

    private float time = 0;

    private string url = "http://192.168.10.21:8080/test/";

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
        WWWForm form = new WWWForm();
        form.AddBinaryData("post_data", data, string.Format(@"CapturedImage{0}_n.jpg", Time.time), "image/jpg");
        var www = new WWW(url, form);

        yield return www; //get data

        Set_Object(Json.Deserialize(www.text) as Dictionary<string, object>);
    }
    /*
    public IEnumerator ConnectionStart_http(byte[] data)
    {
        WebRequest request = HttpWebRequest.Create(url);
        request.Method = "POST";
        .AddBinaryData("post_data", data, string.Format(@"CapturedImage{0}_n.jpg", Time.time), "image/jpg");
        var www = new WWW(url, form);

        yield return www; //get data

        Set_Object(Json.Deserialize(www.text) as Dictionary<string, object>);
}
*/

    private void Set_Object(Dictionary<string, object> _data)
    {
        textmesh.text = Convert.ToString(_data["filename"]);
    }

    private void Count_Object(int _data)
    {
        textmesh.text = _data.ToString();
    }

    void Start()
    {
        textmesh = GetComponent<TextMesh>();
        textmesh.text = "TEST";
        Camera_Image.SetActive(true);
        PhotoCapture.CreateAsync(false, OnCreatedCallback);
    }

    void OnCreatedCallback(PhotoCapture capture_obj)
    {
        photo_obj = capture_obj;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).Last();
        Resolution cameraResolution_frame = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.refreshRate).Last();
        cam.hologramOpacity = 0.0f;
        cam.frameRate = cameraResolution_frame.refreshRate;
        cam.cameraResolutionWidth = cameraResolution.width;
        cam.cameraResolutionHeight = cameraResolution.height;
        cam.pixelFormat = CapturePixelFormat.BGRA32;

        capture_obj.StartPhotoModeAsync(cam, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            TakePhoto();
        }
        else
        {
            photo_obj.StopPhotoModeAsync(OnStoppedPhotoMode);
            PhotoCapture.CreateAsync(false, OnCreatedCallback);
        }
    }

    private void TakePhoto()
    {
        filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
        filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
        _count++;
        Camera_Image.SetActive(true);
        photo_obj.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            //StartCoroutine(ConnectionStart(ReadPngFile(filePath)));
            //photo_obj.StopPhotoModeAsync(OnStoppedPhotoMode);
            Camera_Image.SetActive(false);
            TakePhoto();
        }
    }

    private string PictureFileDirectoryPath()
    {
        string directorypath = "";
#if WINDOWS_UWP
    directorypath = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
        directorypath = Application.streamingAssetsPath;
#endif
        return directorypath;
    }


    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photo_obj.Dispose();
        photo_obj = null;
    }

    void Update()
    {
        if (time >= inter)
        {
            Count_Object(_count);
            _count = 0;
            time = 0;
        }
        else
        {
            time += Time.deltaTime;
        }
    }
}
