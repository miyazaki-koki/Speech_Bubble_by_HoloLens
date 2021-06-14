using UnityEngine;
using System.Linq;
using UnityEngine.XR.WSA.WebCam;
using UnityEngine.XR.WSA.Input;
using System.IO;
#if UNITY_UWP
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Text;
#endif

public class Photo_touch : MonoBehaviour
{

    string filePath = null;
    string filename = null;

    [SerializeField]
    private GameObject Camera_Image = null;

    PhotoCapture photo_obj = null;
    public CameraParameters cam = new CameraParameters();
    Www_connect web_con;

    private void SourcePressed(InteractionSourcePressedEventArgs obj)
    {
        Camera_Image.SetActive(true);
        PhotoCapture.CreateAsync(false, OnCreatedCallback);
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
            photo_obj.TakePhotoAsync(OnCapturedPhotoToMemory);
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
        }
        Camera_Image.SetActive(false);
        photo_obj.StopPhotoModeAsync(OnStoppedPhotoMode);
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

    Texture2D ResizeTexture(Texture2D input, int width, int height)
    {
        Color[] pix = input.GetPixels(0, input.height - height, width, height);
        input.Resize(width, height);
        input.SetPixels(pix);
        input.Apply();
        return input;
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        photo_obj.Dispose();
        photo_obj = null;
    }

    void Start()
    {
        web_con = GetComponent<Www_connect>();

        InteractionManager.InteractionSourcePressed += SourcePressed;
    }

}