using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSource : MonoBehaviour {

    [SerializeField]
    private GameObject obj1;
    [SerializeField]
    private GameObject obj2;
    [SerializeField]
    private GameObject obj3;
    [SerializeField]
    private GameObject obj4;
    [SerializeField]
    private GameObject obj5;
    [SerializeField]
    private Text text1;
    [SerializeField]
    private Text text2;

    public string webIP { get; set; }
    public string PCIP { get; set; }

    // ボタンが押された場合、今回呼び出される関数
    public void OnJudgeClick()
    {
        if(text1.text =="" || text2.text == "" || text1.text == "X.X.X.X" || text2.text == "X.X.X.X")
        {
            obj4.SetActive(true);
        }
        else
        {
            webIP = text1.text;
            PCIP = text2.text;
            obj1.SetActive(true);
            obj2.SetActive(true);
            obj3.SetActive(true);
            obj4.SetActive(false);
            obj5.SetActive(false);
        }
    }

    public void OnSameClick()
    {
        if (text2.text.Length <text1.text.Length)
        {
            text2.text = text1.text;
        }else{
            text1.text = text2.text;
        }
    }

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
    }
}
