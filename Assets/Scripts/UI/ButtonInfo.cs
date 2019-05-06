using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains information that controls the button behavior 
/// </summary>
public class ButtonInfo : MonoBehaviour
{
    /// <summary>
    /// The main scriptable object that contains current selected file, its infomation,
    /// current GUI event, path of azure blob storage etc.
    /// </summary>
    public SelectedFileInfo info;

    /// <summary>
    /// Current attached button gameobject;
    /// </summary>
    public Button button;

    /// <summary>
    /// Enum set of different type of buttons
    /// </summary>
    public enum ButtonType { Container, File, Paste }

    /// <summary>
    /// current type of attached button
    /// </summary>
    public ButtonType type;

    /// <summary>
    /// enable the button that is available at current state
    /// </summary>
    private void OnEnable()
    {
        switch (type)
        {
            case ButtonType.Container:
                SetButtonActive(IsContainerEmpty(), true);
                break;
            case ButtonType.File:
                SetButtonActive(IsContainerEmpty(), false);
                break;
            case ButtonType.Paste:
                SetButtonActive(IsContainerEmpty() || !IsCut(), false);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Check if current path is at root container of Azure Blob Storage
    /// </summary>
    /// <returns></returns>
    private bool IsContainerEmpty()
    {
        return string.IsNullOrEmpty(info.Container);
    }

    /// <summary>
    /// Check if theres a file got cut
    /// </summary>
    /// <returns></returns>
    private bool IsCut()
    {
        return info.CopyedContainer != "" && info.CopyedFileName != "";
    }

    /// <summary>
    /// set button to the given value of setter
    /// </summary>
    /// <param name="value">Checking boolean</param>
    /// <param name="setter">the value the button should be set</param>
    private void SetButtonActive(bool value, bool setter)
    {
        if (value)
            button.interactable = setter;
        else
            button.interactable = !setter;
    }
}
