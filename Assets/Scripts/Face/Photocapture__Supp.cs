using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.WebCam;

public class Photocapture__Supp : MonoBehaviour {


	// Use this for initialization
	void Start () {

        gameObject.GetComponent <TextMesh > ().text = "Start"+"\n";

        Resolution[] cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).ToArray();
        foreach(Resolution _re in cameraResolution)
        {
            gameObject.GetComponent<TextMesh>().text += "Height" + _re.height.ToString() + "\n";
            gameObject.GetComponent<TextMesh>().text += "Width" + _re.width.ToString() + "\n";
        }

        gameObject.GetComponent<TextMesh>().text += "Resolution" + "\n";

        foreach (Resolution resolution in PhotoCapture.SupportedResolutions)
        {
            gameObject.GetComponent<TextMesh>().text += resolution.ToString() + "\n";
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
