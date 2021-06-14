using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeText : MonoBehaviour
{
    /// <summary>
    /// 文字列を反映するテキストフィールド
    /// </summary>
    [SerializeField, Tooltip("文字列を反映するテキストフィールド")]
    private Text TargetTextField;

    /// <summary>
    /// byte列を文字列に変換してテキストフィールドに反映する
    /// </summary>
    /// <param name="message"></param>
    public void SetASCIIBytes(byte[] bytes)
    {
        // データを文字列に変換
        string getMessage = System.Text.Encoding.ASCII.GetString(bytes);
        TargetTextField.text = getMessage;
    }
}