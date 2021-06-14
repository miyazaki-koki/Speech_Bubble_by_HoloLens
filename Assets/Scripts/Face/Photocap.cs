using UnityEngine;
using System.Linq;
using UnityEngine.XR.WSA.WebCam;
using System.Collections.Generic;
using System.IO;
#if UNITY_UWP
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Text;
#endif

public class Photocap : MonoBehaviour
{

    string filePath = null;
    string filename = null;

    [SerializeField]
    private GameObject Camera_Image = null;
    [SerializeField]
    private int inter = 20;

    PhotoCapture photo_obj = null;
    public CameraParameters cam = new CameraParameters();
    Www_connect web_con;

    private float time = 0;


    void Start()
    {
        web_con = GetComponent<Www_connect>();
        Camera_Image.SetActive(true);
        PhotoCapture.CreateAsync(false, OnCreatedCallback);
    }

    public byte[] ReadPngFile(string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryReader bin = new BinaryReader(fileStream);
        byte[] values = bin.ReadBytes((int)bin.BaseStream.Length);

        bin.Dispose();

        return values;
    }

    void OnCreatedCallback(PhotoCapture capture_obj)
    {
        photo_obj = capture_obj;

        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).Last();
        cam.hologramOpacity = 0.0f;
        cam.cameraResolutionWidth = cameraResolution.width;
        cam.cameraResolutionHeight = cameraResolution.height;
        cam.pixelFormat = CapturePixelFormat.BGRA32;

        capture_obj.StartPhotoModeAsync(cam, OnPhotoModeStarted);
    }

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Camera_Image.SetActive(true);
            photo_obj.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            photo_obj.StopPhotoModeAsync(OnStoppedPhotoMode);
            PhotoCapture.CreateAsync(false, OnCreatedCallback);
        }
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).Last();
            Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            StartCoroutine(web_con.ConnectionStart(ImageConversion.EncodeToJPG(targetTexture, 50),
                                                    photoCaptureFrame,
                                                    cameraResolution.width,
                                                    cameraResolution.height));
            Camera_Image.SetActive(false);
        }
        else
        {
            Camera_Image.SetActive(false);
            photo_obj.StopPhotoModeAsync(OnStoppedPhotoMode);
            PhotoCapture.CreateAsync(false, OnCreatedCallback);
        }
    }

    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            StartCoroutine(web_con.ConnectionStart(ReadPngFile(filePath)));
            photo_obj.StopPhotoModeAsync(OnStoppedPhotoMode);
            Camera_Image.SetActive(false);
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
            Camera_Image.SetActive(true);
            photo_obj.TakePhotoAsync(OnCapturedPhotoToMemory);
            time = 0;
        }
        else
        {
            time += Time.deltaTime;
        }
    }
}