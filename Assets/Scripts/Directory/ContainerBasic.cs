using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: ContainerBasic defines the event clicks on blob container, it is different from Directory
//////////////////////////////////////
public class ContainerBasic : DirectoryBasic
{
    public override void OnPointerClick(PointerEventData eventData)
    {
        image.color = Color.cyan;

        if (eventData.clickCount == 1 && eventData.button == PointerEventData.InputButton.Right)
        {
            info.CurrentSelectedContainer = inputField.text;
            RightClickEvent.Raise();
        }

        if (eventData.clickCount == 2 && eventData.button == PointerEventData.InputButton.Left)
        {
            info.Container = inputField.text;
            info.AbsolutePath = path;
            fileEvent.Raise();
        }
    }
}
