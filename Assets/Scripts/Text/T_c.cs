using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;

public class T_c : MonoBehaviour
{

    private float time = 0f;
    private int count = 0;
    TextMesh sentence;
    public int num = 5;
    string[] w;
    Encoding Shift_JIS;


    void Start()
    {
      Shift_JIS = Encoding.GetEncoding("shift_jis");

        sentence = GetComponent<TextMesh>();
        var filePath = Path.Combine(Application.streamingAssetsPath, "Sentence.txt");

        w = File.ReadAllLines(filePath, Shift_JIS);

    }
    
    // Update is called once per frame
    void Update()
    {
        if (time > 1.5f)
        {
            time = 0.0f;
            count += 1;
        }
        else
        {
            sentence.text = "";
                for (int i = 0; i < num && w.Length > count + i ; i++)
                {
                    if (w[count + i] != "\n" && w[count + i] != null)
                    {
                        sentence.text += w[count + i] + "\n";
                    }
                    else if (w[count + i] == null)
                    {
                        sentence.text += "null";
                    }
                    else
                    {
                        sentence.text += " ";
                    }
                }
            time += Time.deltaTime;
        }
    }
}
