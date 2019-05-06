using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
// Description: Define mutiple mouse events on background image. E.g.: right click pop-up menu, lose focus event.
//////////////////////////////////////
public class DirectoryBackGround : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public SelectedFileInfo info;
    public FileEvent RightClickEvent;

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 1 && eventData.button == PointerEventData.InputButton.Right)
        {
            RightClickEvent.Raise();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        info.HoverOverDirectoryBackGround = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        info.HoverOverDirectoryBackGround = false;
    }
}
