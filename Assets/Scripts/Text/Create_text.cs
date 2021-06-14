using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class Create_text : MonoBehaviour {

    private float time = 0f;
    private int count=0;
#if UNITY_UWP
    TextMesh sentence;
    string[] w;
    Encoding Shift_JIS;
#endif

    void Start () {
#if UNITY_UWP
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
      Shift_JIS = Encoding.GetEncoding("shift_jis");

        sentence = GetComponent<TextMesh>();
        var filePath = Path.Combine(Application.streamingAssetsPath, "Sentence.txt");

        w = File.ReadAllLines(filePath, Shift_JIS);
#endif
    }
	
	// Update is called once per frame
	void Update () {
        if (time > 1.5f)
        {
            time = 0f;
            count += 1;
        }
        else
        {
#if UNITY_UWP
            sentence.text = w[count];
#endif
            time += Time.deltaTime;
        }
	}
}
