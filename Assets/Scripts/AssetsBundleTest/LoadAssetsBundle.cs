using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadAssetsBundle : MonoBehaviour
{
    void Awake()
    {
        StartCoroutine(DownloadAndCacheAssetBundle("AssetBundles/StandaloneWindows/azurestoragemanager", "azurestoragemanager"));
        StartCoroutine(DownloadAndCacheAssetBundle("AssetBundles/StandaloneWindows/azureblobexplorer", "azureblobexplorer"));
    }

    IEnumerator DownloadAndCacheAssetBundle(string manifestBundlePath, string name)
    {
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(manifestBundlePath);
        AssetBundleManifest manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        var loadAsset = manifestBundle.LoadAssetAsync<GameObject>(name);
        yield return loadAsset;
        Instantiate(loadAsset.asset, this.transform);
    }
}
