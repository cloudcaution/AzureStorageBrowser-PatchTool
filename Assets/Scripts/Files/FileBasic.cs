using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
//////////////////////////////////////
/// <summary>
/// Base class for all different kinds of files including container, directory and blob(file)
/// </summary>
public class FileBasic : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    /// <summary>
    /// Icon of gameobject on GUI
    /// </summary>
    [HideInInspector]
    public Image image;

    /// <summary>
    /// Original color of image
    /// </summary>
    [HideInInspector]
    public Color color;

    /// <summary>
    /// Path of this file
    /// </summary>
    public string path;

    /// <summary>
    /// Dynamic File event that assign before load
    /// </summary>
    public FileEvent fileEvent;

    /// <summary>
    /// Dynamic File event but refer to right click event
    /// </summary>
    public FileEvent RightClickEvent;

    /// <summary>
    /// The main scriptable object that contains current selected file, its infomation,
    /// current GUI event, path of azure blob storage etc.
    /// </summary>
    public SelectedFileInfo info;

    /// <summary>
    /// Inputfield that contain the current filename
    /// </summary>
    public InputField inputField;

    /// <summary>
    /// get icon of current file onEnable
    /// </summary>
    private void OnEnable()
    {
        image = GetComponent<Image>();
        color = image.color;
    }

    /// <summary>
    /// Set icon color to grey if pointer is hover over 
    /// </summary>
    /// <param name="eventData">Current pointer event</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = Color.grey;
    }

    /// <summary>
    /// set icon color back to default if pointer is out
    /// </summary>
    /// <param name="eventData">Current pointer event</param>
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        image.color = color;
    }

    /// <summary>
    /// Set icon color to cyan if click.
    /// Right click may trigger right click menu poping-up
    /// </summary>
    /// <param name="eventData">Current pointer event</param>
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        image.color = Color.cyan;
        info.Filename = inputField.text;

        if (eventData.clickCount == 1 && eventData.button == PointerEventData.InputButton.Right)
        {
            info.SelectedInputField = inputField;
            RightClickEvent.Raise();
        }
    }

    /// <summary>
    /// Update filename on Azure Storage by given name
    /// </summary>
    /// <param name="name">new name of this file</param>
    public void SetNewFileName(string name)
    {
        info.NewFilename = name;
        fileEvent.Raise();
    } 
}
