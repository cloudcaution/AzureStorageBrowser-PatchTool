using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// The singleton manager class for Azure Storage operations
/// </summary>
public class AzureStorageManager : MonoBehaviour {

    public static string connectionString = "DefaultEndpointsProtocol=https;AccountName=ultracloudstorage;AccountKey=DN75xqF4lXyq8PVZ7/ELW8B9/zZGAK7jCl+cfYUsKlU+jt7j5SJO8rDUEMTiyuetKzN4rzZDHYQAlUXaBYC7aA==;EndpointSuffix=core.windows.net";

    public static CloudStorageAccount storageAccount;

    public static CloudBlobClient blobClient;
    private static BlobRequestOptions timeoutRequestOptions = new BlobRequestOptions()
    {
        // Each REST operation will timeout after 5 seconds.
        ServerTimeout = TimeSpan.FromSeconds(5),

        // Allot 30 seconds for this API call, including retries
        MaximumExecutionTime = TimeSpan.FromSeconds(30)
    };

    /// <summary>
    /// Create a remote container on the Azure Cloud Storage
    /// </summary>
    /// <param name="containerName">The name of the remote container on the Azure Cloud Storage</param>
    /// <param name="onComplete">The callback function to call when the function gets completed</param>
    /// <returns>
    /// A boolean indicating whether the action is successful
    /// </returns>
    public static async Task<bool> CreateContainer(string containerName, Action<bool> onComplete = null)
    {
        CloudBlobContainer container;
        try
        {
            container = blobClient.GetContainerReference(containerName);
        }
        catch(ArgumentException ex)
        {
            return false;
            throw;
        }
        try
        {
            bool result = await container.CreateIfNotExistsAsync();

            onComplete?.Invoke(result);

            return result;
        }
        catch (StorageException)
        {
            throw;
            
        }
    }

    /// <summary>
    /// Delete a remote container on the Azure Cloud Storage
    /// </summary>
    /// <param name="containerName">The name of the remote container on the Azure Cloud Storage</param>
    /// <param name="onComplete">The callback function to call when the function gets completed</param>
    /// <returns>
    /// A boolean indicating whether the action is successful
    /// </returns>
    public static async Task<bool> DeleteContainer(string containerName, Action<bool> onComplete = null)
    {
        CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        try
        {
            bool result = await container.DeleteIfExistsAsync();

            onComplete?.Invoke(result);

            return result;
        }
        catch (StorageException)
        {
            throw;

        }
    }

    /// <summary>
    /// Upload a local file as a whole to a remote blob on the Azure Cloud Storage
    /// </summary>
    /// <param name="containerName">The name of the remote container containing the blob</param>
    /// <param name="blobName">The name of the remote blob</param>
    /// <param name="filePath">The full path of the local file</param>
    /// <param name="onComplete">The callback function to call when the function gets completed</param>
    public static async Task UploadFromFile(string containerName, string blobName, string filePath, Action onComplete=null)
    {
        CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

        await blockBlob.UploadFromFileAsync(filePath);

        onComplete?.Invoke();
    }

    /// <summary>
    /// Upload a local file by blocks to a remote blob on the Azure Cloud Storage. The uploading progress is tracked during the uploading process.
    /// </summary>
    /// <param name="containerName">The name of the remote container containing the blob</param>
    /// <param name="blobName">The name of the remote blob</param>
    /// <param name="filePath">The full path of the local file</param>
    /// <param name="blockSizeInBytes">The maximum amount of bytes of each uploaded block</param>
    /// <param name="onProgressChange">The callback function to call when the uploading progress changes</param>
    /// <param name="onComplete">The callback function to call when the function gets completed</param>
    public static async Task UploadFromFileProgressively(string containerName, string blobName, string filePath, int blockSizeInBytes, Action<double> onProgressChange = null, Action onComplete = null)
    {

        CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
        OperationContext operationContext = new OperationContext();
        blockBlob.StreamWriteSizeInBytes = blockSizeInBytes;
        var bytesToUploadRemaining = (new FileInfo(filePath)).Length;
        var bytesToUploadTotal = bytesToUploadRemaining;

        List<string> blockIds = new List<string>();
        int blockIndex = 1;
        long startPosition = 0;

        do
        {
            long bytesToRead = Math.Min(blockSizeInBytes, bytesToUploadRemaining);

            var blobContents = new byte[bytesToRead];

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                fs.Position = startPosition;
                fs.Read(blobContents, 0, (int)bytesToRead);
            }

            var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(blockIndex.ToString("d6")));

            blockIds.Add(blockId);

            await blockBlob.PutBlockAsync(blockId, new MemoryStream(blobContents), null, AccessCondition.GenerateEmptyCondition(), timeoutRequestOptions, operationContext);
            startPosition += bytesToRead;
            bytesToUploadRemaining -= bytesToRead;

            blockIndex++;
            double percentCompleted = startPosition / (double)(bytesToUploadTotal + 1);

            onProgressChange?.Invoke(percentCompleted);
        }
        while (bytesToUploadRemaining > 0);

        await blockBlob.PutBlockListAsync(blockIds);

        onComplete?.Invoke();
    }

    /// <summary>
    /// Download a remote blob on the Azure Cloud Storage as a whole to a local file
    /// </summary>
    /// <param name="containerName">The name of the remote container containing the blob</param>
    /// <param name="blobName">The name of the remote blob</param>
    /// <param name="filePath">The full path of the local file</param>
    /// <param name="onComplete">The callback function to call when the function gets completed</param>
    public static async Task DownloadToFile(string containerName, string blobName, string filePath, Action onComplete = null)
    {
        CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

        await blockBlob.DownloadToFileAsync(filePath, FileMode.Create);

        onComplete?.Invoke();
    }

    /// <summary>
    /// Download a remote blob on the Azure Cloud Storage by blocks to a local file. The downloading progress is tracked during the downloading process.
    /// </summary>
    /// <param name="containerName">The name of the remote container containing the blob</param>
    /// <param name="blobName">The name of the remote blob</param>
    /// <param name="filePath">The full path of the local file</param>
    /// <param name="blockSizeInBytes">The maximum amount of bytes of each downloaded block</param>
    /// <param name="onProgressChange">The callback function to call when the downloading progress changes</param>
    /// <param name="onComplete">The callback function to call when the function gets completed</param>
    public static async Task DownloadToFileProgressively(string containerName, string blobName, string filePath, int blockSizeInBytes, Action<double> onProgressChange = null, Action onComplete = null)
    {

        Debug.Log(containerName);
        Debug.Log(blobName);
        CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

        await blockBlob.FetchAttributesAsync();

        var bytesToDownloadTotal = blockBlob.Properties.Length;
        var bytesToDownloadRemaining = bytesToDownloadTotal;

        long startPosition = 0;
        
        do
        {

            long bytesToWrite = Math.Min(blockSizeInBytes, bytesToDownloadRemaining);

            byte[] blobContents = new byte[bytesToWrite];
            using (MemoryStream ms = new MemoryStream())
            {
                //blockBlob.DownloadRangeToStream(ms, startPosition, blockSize);
                await blockBlob.DownloadRangeToStreamAsync(ms, startPosition, bytesToWrite);
                ms.Position = 0;
                ms.Read(blobContents, 0, blobContents.Length);
                using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
                {
                    fs.Position = startPosition;
                    fs.Write(blobContents, 0, blobContents.Length);
                }
            }

            startPosition += bytesToWrite;
            bytesToDownloadRemaining -= bytesToWrite;

            double percentCompleted = startPosition / (double)bytesToDownloadTotal;

            if (onProgressChange != null)
            {
                onProgressChange(percentCompleted);
            }

        }
        while (bytesToDownloadRemaining > 0);

        onComplete?.Invoke();
    }

    /// <summary>
    /// Upload a text string as a whole to a remote blob on the Azure Cloud Storage
    /// </summary>
    /// <param name="containerName">The name of the remote container containing the blob</param>
    /// <param name="blobName">The name of the remote blob</param>
    /// <param name="text">The text string to upload</param>
    /// <param name="onComplete">The callback function to call when the function gets completed</param>
    public static async Task UploadFromText(string containerName, string blobName, string text, Action onComplete = null)
    {
        CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

        await blockBlob.UploadTextAsync(text);

        onComplete?.Invoke();
    }

    /// <summary>
    /// Download a remote blob on the Azure Cloud Storage as a whole to a text string
    /// </summary>
    /// <param name="containerName">The name of the remote container containing the blob</param>
    /// <param name="blobName">The name of the remote blob</param>
    /// <param name="onComplete">The callback function to call when the function gets completed</param>
    /// <returns>
    /// A string storing the downloaded text
    /// </returns>
    public static async Task<string> DownloadToText(string containerName, string blobName, Action<string> onComplete = null)
    {
        CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

        string downloadedText = await blockBlob.DownloadTextAsync();

        onComplete?.Invoke(downloadedText);

        return downloadedText;
    }

    /// <summary>
    /// Delete a remote blob on the Azure Cloud Storage
    /// </summary>
    /// <param name="containerName">The name of the remote container containing the blob</param>
    /// <param name="blobName">The name of the remote blob</param>
    /// <param name="onComplete">The callback function to call when the function gets completed</param>
    /// <returns>
    /// A boolean indicating whether the action is successful
    /// </returns>
    public static async Task<bool> DeleteBlob(string containerName, string blobName, Action<bool> onComplete = null)
    {
        CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

        try
        {
            bool result = await blockBlob.DeleteIfExistsAsync();

            onComplete?.Invoke(result);

            return result;
        }
        catch (StorageException)
        {
            throw;
        }
    }

    public static async Task<bool> UpdateBlob(string containerName, string OldBlobName, string NewBlobName, Action<bool> onComplete = null)
    {
        CloudBlobContainer container = blobClient.GetContainerReference(containerName);
        try
        {
            string fileName = OldBlobName;
            string newFileName = NewBlobName;
            await container.CreateIfNotExistsAsync();
            CloudBlockBlob blobCopy = container.GetBlockBlobReference(newFileName);
            if (!await blobCopy.ExistsAsync())
            {
                CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

                if (await blob.ExistsAsync())
                {
                    await blobCopy.StartCopyAsync(blob);
                    await blob.DeleteIfExistsAsync();
                }
            }
            return await blobCopy.ExistsAsync();
        }
        catch (StorageException)
        {
            throw;
        }
    }

    public static async Task<bool> CopyAndPasteBlob(string containerName1, string containerName2, string OldBlobName, string NewBlobName, Action<bool> onComplete = null)
    {   
        CloudBlobContainer container1 = blobClient.GetContainerReference(containerName1);
        CloudBlobContainer container2 = blobClient.GetContainerReference(containerName2);
        try
        {
            string fileName = OldBlobName;
            string newFileName = NewBlobName;
            await container2.CreateIfNotExistsAsync();
            CloudBlockBlob blobCopy = container2.GetBlockBlobReference(newFileName);
            if (!await blobCopy.ExistsAsync())
            {
                CloudBlockBlob blob = container1.GetBlockBlobReference(fileName);

                if (await blob.ExistsAsync())
                {
                    await blobCopy.StartCopyAsync(blob);
                    await blob.DeleteIfExistsAsync();
                }
            }
            return await blobCopy.ExistsAsync();
        }
        catch (StorageException)
        {
            throw;
        }
    }
}
