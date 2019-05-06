using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;

/// <summary>
/// The script used to test and demonstrate the usage of AzureStorageManager class
/// </summary>
public class TestAzureStorageManager : MonoBehaviour {

    // The name of the remote container(folder)
    //[SerializeField]
    //string container = "azure-storage-test";

    //// The name of the remote blob(file) for testing uploading file
    //[SerializeField]
    //string uploadToFileBlob = "test_video_1.mp4";
    //// The name of the local file for testing uploading file
    //[SerializeField]
    //string uploadFromFile = "test_video_1.mp4";

    //// The name of the remote blob(file) for testing downloading file
    //[SerializeField]
    //string downloadFromFileBlob = "test_video_1.mp4";
    //// The name of the local file for testing downloading file
    //[SerializeField]
    //string downloadToFile = "test_video_1.mp4";

    //// The name of the remote blob(file) for testing uploading text
    //[SerializeField]
    //string uploadToTextBlob = "hello_world.txt";
    //// The text string for testing uploading text
    //[SerializeField]
    //string uploadFromText = "Hello world! I'm Atom!";

    //// The name of the remote blob(file) for testing downloading text
    //[SerializeField]
    //string downloadFromTextBlob = "hello_world.txt";

    //// The name of the remote blob(file) for testing deleting blob
    //[SerializeField]
    //string blobToDelete = "hello_world.txt";

    /// <summary>
    /// Test creating a remote container on the Azure Cloud Storage
    /// </summary>
    public void TestCreatingContainer()
    {
        //bool creationResult = await AzureStorageManager.Instance.CreateContainer(
        //    container,
        //    (result) =>
        //    {
        //        if (result)
        //        {
        //            Debug.Log("Successfully created container " + container);
        //        }
        //        else
        //        {
        //            Debug.Log("Failed to create container " + container);
        //        }

        //    });
    }

    /// <summary>
    /// Test deleting a remote container on the Azure Cloud Storage
    /// </summary>
    public void TestDeletingContainer()
    {
        //bool deletionResult = await AzureStorageManager.Instance.DeleteContainer(
        //    container,
        //    (result) =>
        //    {
        //        if (result)
        //        {
        //            Debug.Log("Successfully deleted container " + container);
        //        }
        //        else
        //        {
        //            Debug.Log("Failed to delete container " + container);
        //        }

        //    });
    }

    /// <summary>
    /// Test uploading a local file as a whole to a remote blob on the Azure Cloud Storage
    /// </summary>
    public void TestUploadingFileAsAWhole()
    {
        //await AzureStorageManager.Instance.UploadFromFile(
        //    container,
        //    uploadToFileBlob,
        //    Path.Combine(Application.streamingAssetsPath, uploadFromFile),
        //    () =>
        //    {
        //        Debug.Log("Successfully uploaded " + Path.Combine(Application.streamingAssetsPath, uploadFromFile) + " to " + container + "/" + uploadToFileBlob);
        //    });
    }

    /// <summary>
    /// Test uploading a local file by blocks to a remote blob on the Azure Cloud Storage
    /// </summary>
    public void TestUploadingFileByBlocks()
    {
        //await AzureStorageManager.Instance.UploadFromFileProgressively(
        //    container,
        //    uploadToFileBlob,
        //    Path.Combine(Application.streamingAssetsPath, uploadFromFile),
        //    2 * 1024 * 1024,
        //    (double percent) =>
        //    {
        //        Debug.Log("Uploaded " + uploadFromFile + " progress: " + percent);
        //    },
        //    () =>
        //    {
        //        Debug.Log("Successfully uploaded " + Path.Combine(Application.streamingAssetsPath, uploadFromFile) + " to " + container + "/" + uploadToFileBlob);
        //    });
    }

    /// <summary>
    /// Test downloading a remote blob on the Azure Cloud Storage as a whole to a local file
    /// </summary>
    public void TestDownloadingFileAsAWhole()
    {
        //await AzureStorageManager.Instance.DownloadToFile(
        //    container,
        //    downloadFromFileBlob,
        //    Path.Combine(Application.dataPath, downloadToFile),
        //    () =>
        //    {
        //        Debug.Log("Successfully downloaded " + container + "/" + downloadFromFileBlob + " to " + Path.Combine(Application.dataPath, downloadToFile));
        //    });
    }

    /// <summary>
    /// Test downloading a remote blob on the Azure Cloud Storage by blocks to a local file
    /// </summary>
    public void TestDownloadingFileByBlocks()
    {
        //await AzureStorageManager.Instance.DownloadToFileProgressively(
        //    container,
        //    downloadFromFileBlob,
        //    Path.Combine(Application.dataPath, downloadToFile),
        //    2 * 1024 * 1024,
        //    (double percent) =>
        //    {
        //        Debug.Log("Downloaded " + downloadToFile + " progress: " + percent);
        //    },
        //    () =>
        //    {
        //        Debug.Log("Successfully downloaded " + container + "/" + downloadFromFileBlob + " to " + Path.Combine(Application.dataPath, downloadToFile));
        //    });
    }

    /// <summary>
    /// Test uploading a text string to a remote blob on the Azure Cloud Storage
    /// </summary>
    public void TestUploadingText()
    {
        //await AzureStorageManager.Instance.UploadFromText(
        //   container,
        //   uploadToTextBlob,
        //   uploadFromText,
        //   () =>
        //   {
        //       Debug.Log("Successfully uploaded '" + uploadFromText + "' to " + container + "/" + uploadToTextBlob);
        //   });
    }

    /// <summary>
    /// Test downloading a remote blob on the Azure Cloud Storage to a text string
    /// </summary>
    public void TestDownloadingText()
    {
        //string downloadedString = await AzureStorageManager.Instance.DownloadToText(
        // container,
        // downloadFromTextBlob,
        // (downloadedText) =>
        // {
        //     Debug.Log("Successfully downloaded " + container + "/" + downloadFromTextBlob + " to " + "'" + downloadedText + "'");
        // });
    }

    /// <summary>
    /// Test deleting a remote blob on the Azure Cloud Storage
    /// </summary>
    public void TestDeletingBlob()
    {
        //bool deletionResult = await AzureStorageManager.Instance.DeleteBlob(
        //    container,
        //    blobToDelete,
        //    (result) =>
        //    {
        //        if (result)
        //        {
        //            Debug.Log("Successfully deleted " + container + "/" + blobToDelete);
        //        }
        //        else
        //        {
        //            Debug.Log("Failed to delete " + container + "/" + blobToDelete);
        //        }

        //    });
    }
}

   
