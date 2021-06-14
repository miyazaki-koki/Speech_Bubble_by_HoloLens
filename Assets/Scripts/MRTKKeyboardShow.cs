using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.UI.Keyboard;
using System;

public class MRTKKeyboardShow : MonoBehaviour, IInputClickHandler
{
    /// <summary>
    /// 文字列を反映するテキストフィールド
    /// </summary>
    [SerializeField, Tooltip("文字列を反映するテキストフィールド")]
    private Text TargetInputField;

    /// <summary>
    /// アタッチオブジェクトのタップイベント
    /// </summary>
    /// <param name="eventData"></param>
    public void OnInputClicked(InputClickedEventData eventData)
    {
        // キーボードを開いていなければ実行
        if (!Keyboard.Instance.gameObject.activeSelf)
        {
            // キーボードを表示する
            Keyboard.Instance.PresentKeyboard();
            // キーボードの位置をオブジェクトの近くに配置する
            Keyboard.Instance.RepositionKeyboard(transform, null, 0.4f);

            // キーボードの確定(Enter)イベントを設定する
            Keyboard.Instance.OnTextSubmitted += KeyboardOnTextSubmitted;
            // キーボードの更新イベントを設定する
            Keyboard.Instance.OnTextUpdated += KeyboardOnTextUpdated;
            // キーボードの終了イベントを設定する
            Keyboard.Instance.OnClosed += KeyboardOnClosed;
        }
    }

    /// <summary>
    /// 入力文字列の更新イベント
    /// </summary>
    /// <param name="text"></param>
    private void KeyboardOnTextUpdated(string text)
    {
        // text変数から入力した文字列が取得できる
        if (!string.IsNullOrEmpty(text))
        {
            // Textに文字列をセットする
            TargetInputField.text = text;
        }
    }

    /// <summary>
    /// キーボード入力の確定(Enter)イベント
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    private void KeyboardOnTextSubmitted(object sender, EventArgs eventArgs)
    {
        // InputField変数から入力した文字列が取得できる
        string text = ((Keyboard)sender).InputField.text;
        if (!string.IsNullOrEmpty(text))
        {
            // Textに文字列をセットする
            TargetInputField.text = text;
        }
    }

    /// <summary>
    /// キーボードの終了イベント
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    private void KeyboardOnClosed(object sender, EventArgs eventArgs)
    {
        // 全てのイベントを解除する
        Keyboard.Instance.OnTextSubmitted -= KeyboardOnTextSubmitted;
        Keyboard.Instance.OnTextUpdated -= KeyboardOnTextUpdated;
        Keyboard.Instance.OnClosed -= KeyboardOnClosed;
    }
}