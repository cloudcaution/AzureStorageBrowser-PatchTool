using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputfieldPage : InputfieldBasic
{
    public SelectedFileInfo info;

    protected override bool ValidateFileName(string str)
    {
        Regex rx = new Regex(@"([a-z]|[0-9])*",
          RegexOptions.Compiled | RegexOptions.IgnoreCase);

        return rx.Matches(str).Count == 2 ? true : false;
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        if (!string.IsNullOrEmpty(inputField.text) && text != inputField.text && ValidateFileName(inputField.text))
            info.NewContainer = inputField.text;
        else
            inputField.text = text;
    }

    IEnumerator SetDelay(float time)
    {
        yield return new WaitForSecondsRealtime(0f);

        
    }
}
