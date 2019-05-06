using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputfieldBasic : MonoBehaviour, IDeselectHandler, ISelectHandler
{
    protected InputField inputField;
    private FileBasic file;
    protected string text;
    protected float MagicNumberDelay = 0.2f;

    protected void OnEnable()
    {
        inputField = GetComponent<InputField>();
        file = GetComponentInParent<FileBasic>();
        inputField.onEndEdit.AddListener(delegate
        {
            if (string.IsNullOrEmpty(inputField.text))
                inputField.text = text;
        });
    }

    public void OnSelect(BaseEventData eventData)
    {
        text = inputField.text;
    }

    public virtual void OnDeselect(BaseEventData eventData)
    {
        StartCoroutine(SetDelay(MagicNumberDelay));
    }

    IEnumerator SetDelay(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        inputField.interactable = false;
        file.image.color = file.color;

        if (!string.IsNullOrEmpty(inputField.text) && text != inputField.text && ValidateFileName(inputField.text))
            file.SetNewFileName(inputField.text);
        else
            inputField.text = text;
        file.info.Filename = "";
    }

    protected virtual bool ValidateFileName(string str)
    {
        Regex rx = new Regex(@"([a-z]|[0-9]|\.|-)*",
          RegexOptions.Compiled | RegexOptions.IgnoreCase);

        return rx.Matches(str).Count == 2 ? true : false;
    }
}
