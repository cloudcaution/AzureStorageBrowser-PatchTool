using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: Define click event over blob directory for blob storage browser.
//////////////////////////////////////
public class DirectoryBasic : FileBasic
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 1)
        {
            image.color = Color.cyan;
            info.Directory = inputField.text;
            inputField.interactable = true;
            inputField.Select();
        }

        if (eventData.clickCount == 2)
        {
            info.AbsolutePath = path;
            fileEvent.Raise();
        }
    }
}
