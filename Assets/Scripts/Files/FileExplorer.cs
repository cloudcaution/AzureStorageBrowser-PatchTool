using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using iMVR_Launcher;
using System;
using Microsoft.WindowsAzure.Storage.Blob;
using UnityEngine.UI;
using System.IO;
using UnityEngine.EventSystems;
using Microsoft.WindowsAzure.Storage;
using System.Text.RegularExpressions;

/// <summary>
/// Main fileExplorer controller
/// Including error checking, blob API calling, page refreshing
/// </summary>
public class FileExplorer : MonoBehaviour
{
    /// <summary>
    /// boolean value to control active state of right click menu
    /// </summary>
    public bool showRightClickMenu;

    /// <summary>
    /// string of request message
    /// </summary>
    private string requestMessage;

    /// <summary>
    /// Azure storage API wrapper class
    /// </summary>
    private AzureStorageAPI storageAPI;

    /// <summary>
    /// The main scriptable object that contains current selected file, its infomation,
    /// current GUI event, path of azure blob storage etc.
    /// </summary>
    public SelectedFileInfo info;

    /// <summary>
    /// prefeb for files on Azure blob storage
    /// </summary>
    public GameObject filePrefeb;

    /// <summary>
    /// prefeb for virtual directory (not container) on Azure blob storage
    /// </summary>
    public GameObject directoryPrefeb;

    /// <summary>
    /// Unity scrollview content container for storing files or directories
    /// </summary>
    public GameObject UIContainer;

    /// <summary>
    /// prefeb of right click menu for file right click event
    /// </summary>
    public GameObject rightClickMenuPrefeb;

    /// <summary>
    /// prefeb of right click menu for background right click event
    /// </summary>
    public GameObject rightClickMenuBGPrefeb;

    /// <summary>
    /// prefeb of pop-up Input page
    /// </summary>
    public GameObject InputPagePrefeb;

    /// <summary>
    /// prefeb of warning pop-up menu
    /// </summary>
    public GameObject WarningPopUpPrefeb;

    /// <summary>
    /// prefeb of container on Azure blob storage
    /// </summary>
    public GameObject containerPrefeb;

    /// <summary>
    /// prefeb of right click menu for container
    /// </summary>
    public GameObject rightClickContainerPrefeb;

    /// <summary>
    /// canvas of gui
    /// </summary>
    public Canvas Canvas;

    /// <summary>
    /// right click event queue. 
    /// </summary>
    public Queue<GameObject> rightClickMenuQueue = new Queue<GameObject>();

    /// <summary>
    /// return full path of selected file
    /// </summary>
    private string SelectedFile
    {
        get
        {
            return info.AbsolutePath + info.Filename;
        }
    }

    /// <summary>
    /// update file path by current path
    /// </summary>
    private bool UpdateUpOnPath
    {
        get
        {
            if (!string.IsNullOrEmpty(info.AbsolutePath))
            {
                // check if the last character is "/", strip that out in order to find the current name of directory
                if (info.AbsolutePath[info.AbsolutePath.Length - 1] == '/')
                    info.AbsolutePath = info.AbsolutePath.Substring(0, info.AbsolutePath.Length - 1);

                // Path.GetDirectoryName will return path which use \ as delimiter, replace that to /
                info.AbsolutePath = Path.GetDirectoryName(info.AbsolutePath).ToString().Replace(@"\", "/");
                return true;
            }
            else
            {
                info.Container = "";
                return false;
            }
        }
    }

    /// <summary>
    /// Check if current directory is root directory
    /// </summary>
    private bool IsRoot
    {
        get
        {
            if (string.IsNullOrEmpty(info.Container))
                return true;
            return false;
        }
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        storageAPI = new AzureStorageAPI(OnErrorMsg);
        GetDefaultDirectory();
    }

    private void OnErrorMsg(string str)
    {

    }

    #region blob storage references. Tons of codes can be reused, may optimize later in April
    /// <summary>
    /// Test function to get list of blob by given path
    /// </summary>
    public async void GetListOfBlob()
    {
        try
        {
            var container = AzureStorageManager.blobClient.GetContainerReference(info.Container);
            var directory = container.GetDirectoryReference(info.AbsolutePath);
            BlobContinuationToken token = null;
            do
            {
                var segments = await container.ListBlobsSegmentedAsync(prefix: null,
                                                currentToken: token);
                foreach (var seg in segments.Results)
                {
                    if (seg as CloudBlobDirectory != null)
                        CreateDirectory(seg.StorageUri.PrimaryUri.AbsolutePath);
                    else
                        CreateBlob(seg.StorageUri.PrimaryUri.AbsolutePath);
                }
                token = segments.ContinuationToken;
            } while (token != null);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        
    } // same as GetListOfBlobFromDirectory()

    /// <summary>
    /// Get list of blobs that under a specific container. There is no blobs can be at root directory
    /// </summary>
    public async void GetListOfBlobFromDirectory()
    {
        try
        {
            var container = AzureStorageManager.blobClient.GetContainerReference(info.Container);
            var directory = container.GetDirectoryReference(info.AbsolutePath);
            BlobContinuationToken token = null;
            do
            {
                var segments = await directory.ListBlobsSegmentedAsync(token);
                int count = 0;
                foreach (var seg in segments.Results)
                {
                    if (seg as CloudBlobDirectory != null)
                        CreateDirectory(seg.StorageUri.PrimaryUri.AbsolutePath);
                    else
                        CreateBlob(seg.StorageUri.PrimaryUri.AbsolutePath);
                    count++;
                }
                // check if no file or sub-directory found in current directory, and only if the current directory is not a container, go up to root
                if (count == 0 && !string.IsNullOrEmpty(info.AbsolutePath))
                    SetBackToDefault();

                token = segments.ContinuationToken;
            } while (token != null);

        }
        catch(Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    /// <summary>
    /// Get list of containers at root directory 
    /// </summary>
    public async void GetDefaultDirectory()
    {
        AzureStorageManager.blobClient.DefaultRequestOptions.ServerTimeout = new TimeSpan(0, 0, 3);
        var container = AzureStorageManager.blobClient.GetRootContainerReference();
        BlobContinuationToken token = null;
        try
        {
            do
            {
                var sege = await AzureStorageManager.blobClient.ListContainersSegmentedAsync(null, token);
                token = null;
                foreach (var seg in sege.Results)
                {
                    CreateContainer(seg.StorageUri.PrimaryUri.AbsolutePath.ToString().Replace("/", ""));
                }
            }
            while (token != null);
        }
        catch (Exception exc)
        {
            Debug.Log(exc.Message);
        }
    }

    /// <summary>
    /// Instantiate directory by name from the list of directory
    /// </summary>
    /// <param name="name">name of virtual directory</param>
    private void CreateDirectory(string name)
    {
        GameObject obj = InitialMenu(directoryPrefeb);
        obj.GetComponentInChildren<DirectoryBasic>().path = name.ToString().Replace("/" + info.Container + "/", "");
        obj.GetComponentInChildren<InputField>().text = new FileInfo(name).Directory.Name;
    }

    /// <summary>
    /// Instantiate container by name from the list of containers
    /// </summary>
    /// <param name="name">name of container</param>
    private void CreateContainer(string name)
    {
        GameObject obj = InitialMenu(containerPrefeb);
        ContainerBasic basic = obj.GetComponentInChildren<ContainerBasic>();
        basic.path = "";
        obj.GetComponentInChildren<InputField>().text = name;
    }

    /// <summary>
    /// Instantiate blob file by name from the list of blobs
    /// </summary>
    /// <param name="name">name of blob</param>
    private void CreateBlob(string name)
    {
        GameObject obj = InitialMenu(filePrefeb);
        obj.GetComponentInChildren<FileBasic>().path = name;
        obj.GetComponentInChildren<InputField>().text = Path.GetFileName(name);
    }

    /// <summary>
    /// instantiate the given prefeb and set transform to UI content container
    /// </summary>
    /// <param name="prefeb">prefeb of files</param>
    /// <returns></returns>
    private GameObject InitialMenu(GameObject prefeb)
    {
        return Instantiate(prefeb, UIContainer.transform);    
    }
    #endregion

    #region FileEvent invoking functions
    /// <summary>
    /// Delete current selected file
    /// </summary>
    public async void DeleteSelectedFile()
    {
        try
        {
            if (!info.CanModifyFile)
            {
                PopUpWarning("Please Select a file!");
                return;
            }
            requestMessage = "Send request: Delete selected file";
            if (await AzureStorageManager.DeleteBlob(info.Container, SelectedFile, LogEvent) == true)
            {
                PopUpWarning("Send request: Delete selected file\n" + "Delete successfully!");
                DeleteBlobOnDataBaseOnVersion(ParseFilenameToVersion(SelectedFile));
            }
            else
                PopUpWarning("Failed to delete!");
        }
        catch (StorageException ex)
        {
            PopUpWarning(ex.Message);
        }
        finally
        {
            ClearRightClickMenu();
            RefreshPage();
        }
    }

    /// <summary>
    /// delete current selected container
    /// </summary>
    public async void DeleteContainer()
    {
        try
        {
            requestMessage = "Send request: Delete selected Container";
            PopUpWarning("Send request: 'Delete selected Container'\n" + (await AzureStorageManager.DeleteContainer(info.CurrentSelectedContainer,
                LogEvent) == true ? "Delete sucessfully!" : "Failed to delete!"));
        }
        catch (StorageException ex)
        {
            PopUpWarning(ex.Message);
        }
        finally
        {
            info.CurrentSelectedContainer = "";
            RefreshPage();
            ResetNewContainer();
        }
    }

    /// <summary>
    /// UpateFile name by changing name in Inputfield
    /// </summary>
    public async void UpdateFileName()
    {
        try
        {
            requestMessage = "Send request: update filename";
            Debug.Log(await AzureStorageManager.UpdateBlob(info.Container, info.AbsolutePath + info.Filename, info.AbsolutePath + info.NewFilename,
                LogEvent) == true ? "Update sucessfully!" : "Failed to Update!");    
        }
        catch(StorageException ex)
        {
            PopUpWarning(ex.Message);
        }
        finally
        {
            RefreshPage();
        }
    }

    /// <summary>
    /// Create container from right click menu and pop-up window
    /// </summary>
    public async void CreateContainerOnAzure()
    {
        try
        {
            requestMessage = "Send request: create a container";
            if (await AzureStorageManager.CreateContainer(info.NewContainer, LogEvent) == true)
                OpenInputPage();
            else
                PopUpWarning("Name is invalid or already existed.");
        }
        catch(StorageException ex)
        {
            PopUpWarning(ex.Message);
        }
        finally
        {
            RefreshPageOnGoUp();
            ResetNewContainer();
        }
    }

    /// <summary>
    /// save file information onto main scriptable object info.
    /// </summary>
    public void CutFileInfo()
    {
        info.CopyedFileName = info.Filename;
        info.CopyedFilePath = info.AbsolutePath;
        info.CopyedContainer = info.Container;
        ClearRightClickMenu();
    }

    /// <summary>
    /// Paste the file according to the infomation in main scriptable object info to the current directory. 
    /// If current directory is container, the paste button is inactive.
    /// </summary>
    public async void PasteAfterCut()
    {
        try
        {
            requestMessage = "Send request: create a container";
            if (await AzureStorageManager.CopyAndPasteBlob(info.CopyedContainer, info.Container, info.CopyedFilePath + info.CopyedFileName, info.AbsolutePath + info.CopyedFileName, LogEvent) == true)
            {
                PopUpWarning("Paste file successfully. Notice: move files in Azure storage may lose track of file from database.");
            }
            else
                PopUpWarning("Name is invalid or already existed.");
        }
        catch (StorageException ex)
        {
            PopUpWarning(ex.Message);
        }
        finally
        {
            info.Pasted();
            ClearRightClickMenu();
            RefreshPage();
        }
    }

    /// <summary>
    /// Go to upper directory. If the current directory is root then return
    /// </summary>
    public void GoUp()
    {
        if (IsRoot)
            return;

        RefreshPageOnGoUp();
    }

    /// <summary>
    /// pop-up file right click menu
    /// </summary>
    public void RightClickOnfile()
    {
        ClearRightClickMenu();
        CreateMenu(rightClickMenuPrefeb);
    }

    /// <summary>
    /// pop-up directory click menu
    /// </summary>
    public void RightClickOnDirectory()
    {
        ClearRightClickMenu();
        CreateMenu(rightClickMenuBGPrefeb);
    }

    /// <summary>
    /// set inputfield to be interactable in order to manually change name
    /// </summary>
    public void Rename()
    {
        ClearRightClickMenu();
        info.SelectedInputField.interactable = true;
        info.SelectedInputField.Select();
    }

    /// <summary>
    /// Pop-up Input page
    /// </summary>
    public void OpenInputPage()
    {
        Active(InputPagePrefeb, true);
    }

    /// <summary>
    /// Pop-up container right click menu
    /// </summary>
    public void OpenContainerRightClickMenu()
    {
        Active(rightClickContainerPrefeb);
    }
    #endregion

    #region implement methods
    /// <summary>
    /// Delete blob on database by versions
    /// </summary>
    /// <param name="versions"></param>
    private void DeleteBlobOnDataBaseOnVersion(string[] versions)
    {
        ////I remove the connection to database, so this method should be disabled.
        //AzureRestPatchAPI patchAPI = new AzureRestPatchAPI(AzureRestClient.UrlEndpoint + AzureRestClient.deleteClientPatchData, AzureRestAPI.HttpVerb.POST);
        //Debug.Log(patchAPI.DeletePatchFileOnServer(versions[0], versions[1]));
    }

    /// <summary>
    /// parse out patch number from filename of patch
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    private string[] ParseFilenameToVersion(string filename)
    {
        var str = Regex.Matches(filename, @"\d*(\.*\d)*");
        string[] versions = new string[2];
        int counter = 0;
        foreach(Match match in str)
        {
            if (counter < 2 && match.Value != "")
            {
                versions[counter] = match.Value;
                counter++;
            }
        }
        return versions;
    }

    private void Active(GameObject obj)
    {
        ClearRightClickMenu();
        CreateMenu(obj);
    }

    private void Active(GameObject obj, bool MousePosition)
    {
        ClearRightClickMenu();
        obj.SetActive(!obj.activeSelf);
    }

    private void CreateMenu(GameObject obj)
    {
        rightClickMenuQueue.Enqueue(obj);
        obj.SetActive(true);
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Canvas.transform as RectTransform, Input.mousePosition, Canvas.worldCamera, out pos);
        RectTransform rect = obj.GetComponent<RectTransform>();
        pos += new Vector2(rect.sizeDelta.x / 2, -rect.sizeDelta.y / 2);
        rect.anchoredPosition = pos;
    }

    private void LogEvent(bool complete)
    {
        Debug.Log(requestMessage);
    }

    private void ResetNewContainer()
    {
        info.NewContainer = string.Empty;
    }

    public void PopUpWarning(string str)
    {
        WarningPopUpPrefeb.SetActive(!WarningPopUpPrefeb.activeSelf);
        WarningPopUpPrefeb.GetComponentInChildren<Text>().text = str;
    }
    #endregion

    #region Pop-up panel cleaner
    private void ClearRightClickMenu()
    {
        if (rightClickMenuQueue.Count <= 0)
            return;

        for (int i = 0; i < rightClickMenuQueue.Count; i++)
        {
            rightClickMenuQueue.Peek().SetActive(false);
            rightClickMenuQueue.Dequeue();
        }
    }
    #endregion

    #region Explorer Wiper, flush out gameobjects that are in content => update the file/directoy icons
    private void ClearFileOrDirectory()
    {
        foreach (Transform child in UIContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void SetBackToDefault()
    {
        info.Reset();
        GetDefaultDirectory();
    }

    public void RefreshPage()
    {
        ClearFileOrDirectory();
        ClearRightClickMenu();

        if (IsRoot)
            GetDefaultDirectory();
        else if (info.AbsolutePath == "")
            GetListOfBlobFromDirectory();
        else
            GetListOfBlobFromDirectory();
    }

    private void RefreshPageOnGoUp()
    {
        if (UpdateUpOnPath)
        {
            RefreshPage();
        }
        else
        {
            ClearFileOrDirectory();
            GetDefaultDirectory();
        }
    }
    #endregion

    #region clean up pop-up panels that are active if any click event occurs at current frame
    // Update is called once per frame
    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            if (!info.HoverOverRightClickMenu)
                ClearRightClickMenu();
        }
        return;
    }
    #endregion
}
