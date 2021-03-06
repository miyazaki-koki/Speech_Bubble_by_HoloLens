// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;

namespace HoloToolkit.UI.Keyboard
{
    /// <summary>
    /// A simple general use keyboard that is ideal for AR/VR applications.
    /// </summary>
    /// 
    /// NOTE: This keyboard will not automatically appear when you select an InputField in your
    ///       Canvas. In order for the keyboard to appear you must call Keyboard.Instance.PresentKeyboard(string).
    ///       To retrieve the input from the Keyboard, subscribe to the textEntered event. Note that
    ///       tapping 'Close' on the Keyboard will not fire the textEntered event. You must tap 'Enter' to
    ///       get the textEntered event.
    public class Keyboard : Singleton<Keyboard>
    {
        /// <summary>
        /// Layout type enum for the type of keyboard layout to use.  
        /// This is used when spawning to enable the correct keys based on layout type.
        /// </summary>
        public enum LayoutType
        {
            Symbol,
        }

        #region Callbacks

        /// <summary>
        /// Sent when the 'Enter' button is pressed. To retrieve the text from the event,
        /// cast the sender to 'Keyboard' and get the text from the TextInput field.
        /// (Cleared when keyboard is closed.)
        /// </summary>
        public event EventHandler OnTextSubmitted = delegate { };

        /// <summary>
        /// Fired every time the text in the InputField changes.
        /// (Cleared when keyboard is closed.)
        /// </summary>
        public event Action<string> OnTextUpdated = delegate { };

        /// <summary>
        /// Fired every time the Close button is pressed.
        /// (Cleared when keyboard is closed.)
        /// </summary>
        public event EventHandler OnClosed = delegate { };

        /// <summary>
        /// Sent when the 'Previous' button is pressed. Ideally you would use this event
        /// to set your targeted text input to the previous text field in your document.
        /// (Cleared when keyboard is closed.)
        /// </summary>
        public event EventHandler OnPrevious = delegate { };

        /// <summary>
        /// Sent when the 'Next' button is pressed. Ideally you would use this event
        /// to set your targeted text input to the next text field in your document.
        /// (Cleared when keyboard is closed.)
        /// </summary>
        public event EventHandler OnNext = delegate { };

        /// <summary>
        /// Sent when the keyboard is placed.  This allows listener to know when someone else is co-opting the keyboard.
        /// </summary>
        public event EventHandler OnPlacement = delegate { };

        #endregion Callbacks

        /// <summary>
        /// The InputField that the keyboard uses to show the currently edited text.
        /// If you are using the Keyboard prefab you can ignore this field as it will
        /// be already assigned.
        /// </summary>
        public InputField InputField = null;

        /// <summary>
        /// Move the axis slider based on the camera forward and the keyboard plane projection.
        /// </summary>
        public AxisSlider InputFieldSlide = null;

        /// <summary>
        /// Bool for toggling the slider being enabled.
        /// </summary>
        public bool SliderEnabled = true;

        /// <summary>
        /// Bool to flag submitting on enter
        /// </summary>
        public bool SubmitOnEnter = true;

        /// <summary>
        /// The panel that contains the number and symbol keys.
        /// </summary>
        public Image SymbolKeyboard = null;


        private LayoutType m_LastKeyboardLayout = LayoutType.Symbol;

        /// <summary>
        /// The scale the keyboard should be at its maximum distance.
        /// </summary>
        [Header("Positioning")]
        [SerializeField]
        private float m_MaxScale = 1.0f;

        /// <summary>
        /// The scale the keyboard should be at its minimum distance.
        /// </summary>
        [SerializeField]
        private float m_MinScale = 1.0f;

        /// <summary>
        /// The maximum distance the keyboard should be from the user.
        /// </summary>
        [SerializeField]
        private float m_MaxDistance = 3.5f;

        /// <summary>
        /// The minimum distance the keyboard needs to be away from the user.
        /// </summary>
        [SerializeField]
        private float m_MinDistance = 0.25f;

        /// <summary>
        /// Make the keyboard disappear automatically after a timeout
        /// </summary>
        public bool CloseOnInactivity = true;

        /// <summary>
        /// Inactivity time that makes the keyboard disappear automatically.
        /// </summary>
        public float CloseOnInactivityTime = 15;

        /// <summary>
        /// Time on which the keyboard should close on inactivity
        /// </summary>
        private float _closingTime;

        /// <summary>
        /// The position of the caret in the text field.
        /// </summary>
        private int m_CaretPosition = 0;

        /// <summary>
        /// The starting scale of the keyboard.
        /// </summary>
        private Vector3 m_StartingScale = Vector3.one;

        /// <summary>
        /// The default bounds of the keyboard.
        /// </summary>
        private Vector3 m_ObjectBounds;

        /// <summary>
        /// The default color of the mike key.
        /// </summary>        
        private Color _defaultColor;

        /// <summary>
        /// Deactivate on Awake.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_StartingScale = transform.localScale;
            Bounds canvasBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(transform);

            RectTransform rect = GetComponent<RectTransform>();
            m_ObjectBounds = new Vector3(canvasBounds.size.x * rect.localScale.x, canvasBounds.size.y * rect.localScale.y, canvasBounds.size.z * rect.localScale.z);

            // Keep keyboard deactivated until needed
            gameObject.SetActive(false);
        }


        /// <summary>
        /// Set up Dictation, CanvasEX, and automatically select the TextInput object.
        /// </summary>
        private void Start()
        {
            // Delegate Subscription
            InputField.onValueChanged.AddListener(DoTextUpdated);
        }

        /// <summary>
        /// Intermediary function for text update events.
        /// Workaround for strange leftover reference when unsubscribing.
        /// </summary>
        /// <param name="value">String value.</param>
        private void DoTextUpdated(string value)
        {
            if (OnTextUpdated != null)
            {
                OnTextUpdated(value);
            }
        }

        /// <summary>
        /// Makes sure the input field is always selected while the keyboard is up.
        /// </summary>
        private void LateUpdate()
        {
            // Axis Slider
            if (SliderEnabled)
            {
                Vector3 nearPoint = Vector3.ProjectOnPlane(CameraCache.Main.transform.forward, transform.forward);
                Vector3 relPos = transform.InverseTransformPoint(nearPoint);
                InputFieldSlide.TargetPoint = relPos;
            }
            CheckForCloseOnInactivityTimeExpired();
        }

        private void UpdateCaretPosition(int newPos)
        {
            InputField.caretPosition = newPos;
        }

        /// <summary>
        /// Called whenever the keyboard is disabled or deactivated.
        /// </summary>
        private void OnDisable()
        {
            m_LastKeyboardLayout = LayoutType.Symbol;
            Clear();
        }

        /// <summary>
        /// Destroy unmanaged memory links.
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #region Present Functions

        /// <summary>
        /// Present the default keyboard to the camera.
        /// </summary>
        public void PresentKeyboard()
        {
            ResetClosingTime();
            gameObject.SetActive(true);
            ActivateSpecificKeyboard(LayoutType.Symbol);

            OnPlacement(this, EventArgs.Empty);

            //This bring up the default keyboard in MR so the user is presented with TWO keyboards
            //InputField.ActivateInputField();
        }


        /// <summary>
        /// Presents the default keyboard to the camera, with start text.
        /// </summary>
        /// <param name="startText">The initial text to show in the Keyboard's InputField.</param>
        public void PresentKeyboard(string startText)
        {
            PresentKeyboard();
            Clear();
            InputField.text = startText;
        }

        /// <summary>
        /// Presents a specific keyboard to the camera.
        /// </summary>
        /// <param name="keyboardType">Specify the keyboard type.</param>
        public void PresentKeyboard(LayoutType keyboardType)
        {
            PresentKeyboard();
            ActivateSpecificKeyboard(keyboardType);
        }

        /// <summary>
        /// Presents a specific keyboard to the camera, with start text.
        /// </summary>
        /// <param name="startText">The initial text to show in the Keyboard's InputField.</param>
        /// <param name="keyboardType">Specify the keyboard type.</param>
        public void PresentKeyboard(string startText, LayoutType keyboardType)
        {
            PresentKeyboard(startText);
            ActivateSpecificKeyboard(keyboardType);
        }

        #endregion Present Functions
        /// <summary>
        /// Function to reposition the Keyboard based on target position and vertical offset 
        /// </summary>
        /// <param name="kbPos">World position for keyboard</param>
        /// <param name="verticalOffset">Optional vertical offset of keyboard</param>
        public void RepositionKeyboard(Vector3 kbPos, float verticalOffset = 0.0f)
        {
            transform.position = kbPos;
            ScaleToSize();
            LookAtTargetOrigin();
        }

        /// <summary>
        /// Function to reposition the Keyboard based on target transform and collider information 
        /// </summary>
        /// <param name="objectTransform">Transform of target object to remain relative to</param>
        /// <param name="aCollider">Optional collider information for offset placement</param>
        /// <param name="verticalOffset">Optional vertical offset from the target</param>
        public void RepositionKeyboard(Transform objectTransform, BoxCollider aCollider = null, float verticalOffset = 0.0f)
        {
            transform.position = objectTransform.position;

            if (aCollider != null)
            {
                float yTranslation = -((aCollider.bounds.size.y * 0.5f) + verticalOffset);
                transform.Translate(0.0f, yTranslation, -0.6f, objectTransform);
            }
            else
            {
                float yTranslation = -((m_ObjectBounds.y * 0.5f) + verticalOffset);
                transform.Translate(0.0f, yTranslation, -0.6f, objectTransform);
            }

            ScaleToSize();
            LookAtTargetOrigin();
        }

        /// <summary>
        /// Function to scale keyboard to the appropriate size based on distance
        /// </summary>
        private void ScaleToSize()
        {
            float distance = (transform.position - CameraCache.Main.transform.position).magnitude;
            float distancePercent = (distance - m_MinDistance) / (m_MaxDistance - m_MinDistance);
            float scale = m_MinScale + (m_MaxScale - m_MinScale) * distancePercent;

            scale = Mathf.Clamp(scale, m_MinScale, m_MaxScale);
            transform.localScale = m_StartingScale * scale;

            Debug.LogFormat("Setting scale: {0} for distance: {1}", scale, distance);
        }

        /// <summary>
        /// Look at function to have the keyboard face the user
        /// </summary>
        private void LookAtTargetOrigin()
        {
            transform.LookAt(CameraCache.Main.transform.position);
            transform.Rotate(Vector3.up, 180.0f);
        }

        /// <summary>
        /// Activates a specific keyboard layout, and any sub keys.
        /// </summary>
        /// <param name="keyboardType"></param>
        private void ActivateSpecificKeyboard(LayoutType keyboardType)
        {
            DisableAllKeyboards();
            ResetKeyboardState();

            switch (keyboardType)
            {
                case LayoutType.Symbol:
                    {
                        ShowSymbolKeyboard();
                        break;
                    }
            }
        }

        #region Keyboard Functions

        /// <summary>
        /// Primary method for typing individual characters to a text field.
        /// </summary>
        /// <param name="valueKey">The valueKey of the pressed key.</param>
        public void AppendValue(KeyboardValueKey valueKey)
        {
            IndicateActivity();
            string value = "";

            value = valueKey.Value;

            m_CaretPosition = InputField.caretPosition;

            InputField.text = InputField.text.Insert(m_CaretPosition, value);
            m_CaretPosition += value.Length;

            UpdateCaretPosition(m_CaretPosition);
        }

        /// <summary>
        /// Trigger specific keyboard functionality.
        /// </summary>
        /// <param name="functionKey">The functionKey of the pressed key.</param>
        public void FunctionKey(KeyboardKeyFunc functionKey)
        {
            IndicateActivity();
            switch (functionKey.m_ButtonFunction)
            {
                case KeyboardKeyFunc.Function.Enter:
                    {
                        Enter();
                        break;
                    }

                case KeyboardKeyFunc.Function.Symbol:
                    {
                        ActivateSpecificKeyboard(LayoutType.Symbol);
                        break;
                    }

                case KeyboardKeyFunc.Function.Previous:
                    {
                        MoveCaretLeft();
                        break;
                    }

                case KeyboardKeyFunc.Function.Next:
                    {
                        MoveCaretRight();
                        break;
                    }

                case KeyboardKeyFunc.Function.Close:
                    {
                        Close();
                        break;
                    }

                case KeyboardKeyFunc.Function.Backspace:
                    {
                        Backspace();
                        break;
                    }

                case KeyboardKeyFunc.Function.UNDEFINED:
                    {
                        Debug.LogErrorFormat("The {0} key on this keyboard hasn't been assigned a function.", functionKey.name);
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Delete the character before the caret.
        /// </summary>
        public void Backspace()
        {
            // check if text is selected
            if (InputField.selectionFocusPosition != InputField.caretPosition || InputField.selectionAnchorPosition != InputField.caretPosition)
            {
                if (InputField.selectionAnchorPosition > InputField.selectionFocusPosition) // right to left
                {
                    InputField.text = InputField.text.Substring(0, InputField.selectionFocusPosition) + InputField.text.Substring(InputField.selectionAnchorPosition);
                    InputField.caretPosition = InputField.selectionFocusPosition;
                }
                else // left to right
                {
                    InputField.text = InputField.text.Substring(0, InputField.selectionAnchorPosition) + InputField.text.Substring(InputField.selectionFocusPosition);
                    InputField.caretPosition = InputField.selectionAnchorPosition;
                }

                m_CaretPosition = InputField.caretPosition;
                InputField.selectionAnchorPosition = m_CaretPosition;
                InputField.selectionFocusPosition = m_CaretPosition;
            }
            else
            {
                m_CaretPosition = InputField.caretPosition;

                if (m_CaretPosition > 0)
                {
                    --m_CaretPosition;
                    InputField.text = InputField.text.Remove(m_CaretPosition, 1);
                    UpdateCaretPosition(m_CaretPosition);
                }
            }
        }

        /// <summary>
        /// Send the "previous" event.
        /// </summary>
        public void Previous()
        {
            OnPrevious(this, EventArgs.Empty);
        }

        /// <summary>
        /// Send the "next" event.
        /// </summary>
        public void Next()
        {
            OnNext(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fire the text entered event for objects listening to keyboard.
        /// Immediately closes keyboard.
        /// </summary>
        public void Enter()
        {
            if (SubmitOnEnter)
            {
                // Send text entered event and close the keyboard
                if (OnTextSubmitted != null)
                {
                    OnTextSubmitted(this, EventArgs.Empty);
                }

                Close();
            }
            else
            {
                string enterString = "\n";

                m_CaretPosition = InputField.caretPosition;

                InputField.text = InputField.text.Insert(m_CaretPosition, enterString);
                m_CaretPosition += enterString.Length;

                UpdateCaretPosition(m_CaretPosition);
            }

        }

        /// <summary>
        /// Move caret to the left.
        /// </summary>
        public void MoveCaretLeft()
        {
            m_CaretPosition = InputField.caretPosition;

            if (m_CaretPosition > 0)
            {
                --m_CaretPosition;
                UpdateCaretPosition(m_CaretPosition);
            }
        }

        /// <summary>
        /// Move caret to the right.
        /// </summary>
        public void MoveCaretRight()
        {
            m_CaretPosition = InputField.caretPosition;

            if (m_CaretPosition < InputField.text.Length)
            {
                ++m_CaretPosition;
                UpdateCaretPosition(m_CaretPosition);
            }
        }

        /// <summary>
        /// Close the keyboard.
        /// (Clears all event subscriptions.)
        /// </summary>
        public void Close()
        {
            OnClosed(this, EventArgs.Empty);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Clear the text input field.
        /// </summary>
        public void Clear()
        {
            ResetKeyboardState();
            InputField.MoveTextStart(false);
            InputField.text = "";
            m_CaretPosition = InputField.caretPosition;
        }

        #endregion

        /// <summary>
        /// Method to set the sizes by code, as the properties are private. 
        /// Useful for scaling 'from the outside', for instance taking care of differences between
        /// immersive headsets and HoloLens
        /// </summary>
        /// <param name="minScale">Min scale factor</param>
        /// <param name="maxScale">Max scale factor</param>
        /// <param name="minDistance">Min distance from camera</param>
        /// <param name="maxDistance">Max distance from camera</param>
        public void SetScaleSizeValues(float minScale, float maxScale, float minDistance, float maxDistance)
        {
            m_MinScale = minScale;
            m_MaxScale = maxScale;
            m_MinDistance = minDistance;
            m_MaxDistance = maxDistance;
        }

        #region Keyboard Layout Modes
        /// <summary>
        /// Enable the symbol keyboard.
        /// </summary>
        public void ShowSymbolKeyboard()
        {
            SymbolKeyboard.gameObject.SetActive(true);
        }

        /// <summary>
        /// Disable GameObjects for all keyboard elements.
        /// </summary>
        private void DisableAllKeyboards()
        {
            SymbolKeyboard.gameObject.SetActive(false);
        }

        /// <summary>
        /// Reset temporary states of keyboard.
        /// </summary>
        private void ResetKeyboardState()
        {
        }

        #endregion Keyboard Layout Modes

        /// <summary>
        /// Respond to keyboard activity: reset timeout timer, play sound
        /// </summary>
        private void IndicateActivity()
        {
            ResetClosingTime();
        }

        /// <summary>
        /// Reset inactivity closing timer
        /// </summary>
        private void ResetClosingTime()
        {
            if (CloseOnInactivity)
            {
                _closingTime = Time.time + CloseOnInactivityTime;
            }
        }

        /// <summary>
        /// Check if the keyboard has been left alone for too long and close
        /// </summary>
        private void CheckForCloseOnInactivityTimeExpired()
        {
            if (Time.time > _closingTime && CloseOnInactivity)
            {
                Close();
            }
        }
    }
}