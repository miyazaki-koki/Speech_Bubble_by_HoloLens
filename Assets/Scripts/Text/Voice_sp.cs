using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.IO;
using System.Text;

public class Voice_sp : MonoBehaviour {
    [SerializeField]
    private TextMesh sentence = null;

    [SerializeField]
    private GameObject Sound_ON = null;
    [SerializeField]
    private GameObject Sound_OFF = null;

    private DictationRecognizer dictationRecognizer;
    private int num = 5;
    private List<string> w_d = new List<string>();
    // Use this for initialization
    void Start()
    {
        dictationRecognizer = new DictationRecognizer();
        //dictationRecognizer.InitialSilenceTimeoutSeconds = 10;
        dictationRecognizer.DictationResult += (text, config) =>
        {

            sentence.text = "";
            w_d.Insert(0,text+"\n");
            if (w_d.Count > num)
            {
                w_d.RemoveAt(num);
            }
            foreach (string t in w_d) {
                sentence.text +=t;
            }

        };
        dictationRecognizer.DictationComplete += (config) => {
            if (config != DictationCompletionCause.Complete)
            {
                Sound_OFF.SetActive(true);
                Sound_ON.SetActive(false);
            }
            else
            {
                Sound_OFF.SetActive(false);
                Sound_ON.SetActive(true);
                dictationRecognizer.Start();
            }
        };
        dictationRecognizer.Start();
        Sound_ON.SetActive(true);
    }

    // Update is called once per frame
}
