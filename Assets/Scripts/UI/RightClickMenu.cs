using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RightClickMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SelectedFileInfo info;

    public void OnPointerEnter(PointerEventData eventData)
    {
        info.HoverOverRightClickMenu = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        info.HoverOverRightClickMenu = false;
    }
}
