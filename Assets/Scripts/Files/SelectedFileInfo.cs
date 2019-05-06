using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///////////////////////////////////////
// Author: Cloud Zhou
// Date: 2/27/2019
//////////////////////////////////////

/// <summary> 
/// The main scriptable object that contains current selected file, its infomation,
/// current GUI event, path of azure blob storage etc.
/// </summary>
[CreateAssetMenu]
public class SelectedFileInfo : ScriptableObject
{
    /// <summary>
    /// the container that the current relative path refers to
    /// </summary>
    private string container;

    /// <summary>
    /// the virtual directory from relative path
    /// </summary>
    private string directory;

    /// <summary>
    /// absolutePath here is the relative path
    /// </summary>
    private string absolutePath;
    private string filename;
    private string newfilename;

    /// <summary>
    /// current selected Container for deleting container event
    /// </summary>
    [HideInInspector]
    public string CurrentSelectedContainer;
    /// <summary>
    /// the container that the current relative path refers to
    /// </summary>
    public string Container;

    /// <summary>
    /// the virtual directory from relative path
    /// </summary>
    public string Directory;
    /// <summary>
    /// absolutePath here is the relative path
    /// </summary>
    public string AbsolutePath;

    /// <summary>
    /// The name of new blobfile that user input in inputfield
    /// </summary>
    [HideInInspector]
    public string NewFilename;

    /// <summary>
    /// The name of new container that user input in inputfield
    /// </summary>
    [HideInInspector]
    public string NewContainer;

    /// <summary>
    /// result of mouse event to check whether the mouse is now hoverring over right click panel
    /// </summary>
    [HideInInspector]
    public bool HoverOverRightClickMenu;

    /// <summary>
    /// result of mouse event that checks whether the selected file could be modified.
    /// </summary>
    [HideInInspector]
    public bool CanModifyFile;

    /// <summary>
    /// result of mouse event to check whether the mouse is now hoverring over background of virtual directories panel
    /// </summary>
    [HideInInspector]
    public bool HoverOverDirectoryBackGround;

    /// <summary>
    /// name of copyed blob file
    /// </summary>
    [HideInInspector]
    public string CopyedFileName;

    /// <summary>
    /// path of copyed blob file
    /// </summary>
    [HideInInspector]
    public string CopyedFilePath;

    /// <summary>
    /// container of copyed blob file
    /// </summary>
    [HideInInspector]
    public string CopyedContainer;

    /// <summary>
    /// inputfield of selected file
    /// </summary>
    [HideInInspector]
    public InputField SelectedInputField;

    /// <summary>
    /// name of selected file
    /// </summary>
    public string Filename
    {
        set
        {
            filename = value;
            if (string.IsNullOrEmpty(filename))
                CanModifyFile = false;
            else
                CanModifyFile = true;
        }
        get
        {
            return filename;
        }
    }

    /// <summary>
    /// reset all string variable when this gameobject is initialized
    /// </summary>
    public void OnEnable()
    {
        Reset();
    }

    /// <summary>
    /// reset all string variable to empty string
    /// </summary>
    public void Reset()
    {
        Container = Directory = AbsolutePath = Filename = NewFilename = NewContainer = CopyedContainer = CopyedFileName = CopyedFilePath = "";
    }

    /// <summary>
    /// reset infomation of copied file to empty string 
    /// </summary>
    public void Pasted()
    {
        CopyedFilePath = CopyedFileName = CopyedContainer = "";
    }
}
