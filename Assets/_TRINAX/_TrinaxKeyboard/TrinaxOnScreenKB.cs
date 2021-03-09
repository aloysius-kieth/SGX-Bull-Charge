using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using System.Linq;

public enum INPUTFIELD
{
    NONE, 
    MOBILE,
    OTP,
}

public class TrinaxOnScreenKB : MonoBehaviour
{
    const int MAX_MOBILE_NO = 8;
    const int MAX_OTP_LENGTH = 6;
    //public static TrinaxOnScreenKB Instance;

    public TMP_InputField[] inputFields;
    public Color activeColor;
    public Color inactiveColor;

    TMP_InputField activeField;
    HashSet<int> targetedInputFields = new HashSet<int>();

    public void OnKeyDown(KeyCode code, string custom = "")
    {
        if (activeField != null)
        {
            string newChar = "";
            string current = activeField.text;

            if (custom.ToUpper() == "CLEAR")
            {
                activeField.text = "";
                return;
            }

            if (code == KeyCode.Backspace)
            {
                if (current.Length == 0)
                {
                    return;
                }

                current = current.Substring(0, current.Length - 1);
                activeField.text = current;
                return;
            }

            if (!string.IsNullOrEmpty(custom))
            {
                newChar = custom;
            }
            else
            {
                switch (code)
                {
                    case KeyCode.Alpha0:
                        newChar = "0";
                        break;
                    case KeyCode.Alpha1:
                        newChar = "1";
                        break;
                    case KeyCode.Alpha2:
                        newChar = "2";
                        break;
                    case KeyCode.Alpha3:
                        newChar = "3";
                        break;
                    case KeyCode.Alpha4:
                        newChar = "4";
                        break;
                    case KeyCode.Alpha5:
                        newChar = "5";
                        break;
                    case KeyCode.Alpha6:
                        newChar = "6";
                        break;
                    case KeyCode.Alpha7:
                        newChar = "7";
                        break;
                    case KeyCode.Alpha8:
                        newChar = "8";
                        break;
                    case KeyCode.Alpha9:
                        newChar = "9";
                        break;
                    case KeyCode.Space:
                        newChar = " ";
                        break;
                    case KeyCode.At:
                        newChar = "@";
                        break;
                    case KeyCode.Period:
                        newChar = ".";
                        break;
                    case KeyCode.Minus:
                        newChar = "-";
                        break;
                    case KeyCode.Underscore:
                        newChar = "_";
                        break;
                    default:
                        newChar = code.ToString();

                        if (string.IsNullOrEmpty(current))
                        {
                            newChar = newChar.ToUpper();
                        }
                        else
                        {
                            char lastChar = current[current.Length - 1];
                            if (char.IsWhiteSpace(lastChar))
                            {
                                newChar = newChar.ToUpper();
                            }
                            else
                            {
                                newChar = newChar.ToLower();
                            }
                        }

                        break;
                }
            }


            if (activeField.GetComponent<InputFieldID>().id == INPUTFIELD.MOBILE)
            {
                if (!newChar.Any(char.IsDigit) || activeField.text.Length >= MAX_MOBILE_NO) return;
            }

            if (activeField.GetComponent<InputFieldID>().id == INPUTFIELD.OTP)
            {
                if (activeField.text.Length >= MAX_OTP_LENGTH) return;
            }

            activeField.text += newChar;
        }
    }

    public void ActivateFirstInputField()
    {
        if (inputFields != null/* && inputFields.Length > 1*/)
        {
            inputFields[0].ActivateInputField();
            activeField = inputFields[0];
        }

        ColorActiveInputField();
    }

    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //    }
    //    else
    //    {
    //        Destroy(gameObject);
    //    }
    //}

    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < inputFields.Length; ++i)
        {
            targetedInputFields.Add(inputFields[i].GetHashCode());
        }

        ActivateFirstInputField();
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject)
        {
            TMP_InputField input = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
            if (input != null)
            {
                if (targetedInputFields.Contains(input.GetHashCode()))
                {
                    activeField = input;

                    ColorActiveInputField();
                }
                else
                {
                    // clicked on not a targeted input field.
                }
            }
            else
            {
                for (int i = 0; i < inputFields.Length; ++i)
                {
                    //inputFields[i].interactable = true;
                }
            }
        }
    }

    private void OnEnable()
    {
        ActivateFirstInputField();
    }

    void ColorActiveInputField()
    {
        for (int i = 0; i < inputFields.Length; ++i)
        {
            inputFields[i].image.color = inactiveColor;
            //inputFields[i].interactable = true;
        }

        if (activeField != null)
        {
            activeField.image.color = activeColor;
            //activeField.interactable = false;
        }

    }
}
