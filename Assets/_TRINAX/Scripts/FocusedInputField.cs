using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FocusedInputField : InputField {

    private const float cdTime = 0.175f;

    private bool countdown = false;
    private float timer = cdTime;

    protected override void OnDisable()
    {
        //base.OnDisable();
        onValueChanged.RemoveAllListeners();
        DeactivateInputField();
    }

    protected override void OnEnable()
    {
        //text = "";
        StartCoroutine("ClearText");

        StartCoroutine("ActivateInputFieldWithoutSelection");
        onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    public void ValueChangeCheck()
    {
        countdown = true;
        timer = cdTime;
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        StopCoroutine("ActivateInputFieldWithoutSelection");
        StartCoroutine("ActivateInputFieldWithoutSelection");
    }

    IEnumerator ActivateInputFieldWithoutSelection()
    {
        ActivateInputField();
        // wait for the activation to occur in a lateupdate
        yield return new WaitForEndOfFrame();
        // make sure we're still the active ui
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            // To remove hilight we'll just show the caret at the end of the line
            MoveTextEnd(false);
        }
    }

    private void Update()
    {
        if (countdown)
        {
            if ((timer -= Time.deltaTime) <= 0.0f)
            {
                OnReceivedText();

                StopCoroutine("ClearText");
                StartCoroutine("ClearText");
                countdown = false;
            }
        }
    }

    void OnReceivedText()
    {
        // Do code here 
        TrinaxAsyncServerManager.Instance.VerifyQrCodeAsync(text, (success, result) => 
        {
            if(success)
            {
                Debug.Log("QR CODE VALID!");
                Debug.Log(result);
            }
            else
            {
                Debug.Log("QR CODE INVALID!");
            }
        }).WrapErrors();
    }

    IEnumerator ClearText()
    {
        text = "";
        timer = float.MaxValue;
        yield return new WaitForEndOfFrame();
        countdown = false;
        timer = cdTime;
    }
}
