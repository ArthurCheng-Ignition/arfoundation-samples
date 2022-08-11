using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.XR.ARFoundation.Samples;

public class LoadModels : MonoBehaviour
{
    [SerializeField]
    string[] _URLs;

    [SerializeField]
    GameObject[] downloadedModels;

    [SerializeField]
    GameObject downloadingPanel;
    [SerializeField]
    TextMeshProUGUI percentageText;
    [SerializeField]
    Slider progressSlider;
    [SerializeField]
    TextMeshProUGUI downloadNumberText;

    [SerializeField]
    private AssetBundle[] assetBundles;

    bool clearCache = false;

    [SerializeField]
    private TrackedImageInfoManager _TrackedImageInfoManager;

    private void Awake()
    {
        downloadedModels = new GameObject[_URLs.Length];
        assetBundles = new AssetBundle[_URLs.Length];
        Caching.compressionEnabled = false;

        if (clearCache)
        {
            Caching.ClearCache();
        }
    }


    public void DownloadModelsButton()
    {
        downloadingPanel.SetActive(true);
        StartCoroutine(DownloadAndLoad());

    }

    private IEnumerator DownloadAndLoad()
    {
        while (!Caching.ready)
        {
            // wait for caching to be ready for use
            Debug.Log("waiting for cache to be ready");
            yield return null;
        }
        yield return GetBundles();
        if (assetBundles == null)
        {
            Debug.LogError("Bundle Failed to Load");
            yield break;
        }
        else
        {
            downloadingPanel.SetActive(false);
        }
        for (int i = 0; i < assetBundles.Length; i++)
        {
            string prefabname = "Promo" + (i + 1);
            Debug.Log(prefabname);
            downloadedModels[i] = assetBundles[i].LoadAsset<GameObject>(prefabname);
        }

        // pass new models to TrackedImageInfoManager to add to Dictionary
        _TrackedImageInfoManager.AddDownloadedModels(downloadedModels);
    }

    private IEnumerator GetBundles()
    {
        int index = 0;
        int downloadNumber = 1;
        foreach (string url in _URLs)
        {
            downloadNumberText.text = "Downloading asset " + downloadNumber + " of " + _URLs.Length;
            Debug.Log("URL: " + url);

            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url);

            request.SendWebRequest();

            while (request.isDone != true)
            {
                //Debug.Log(request.downloadProgress);
                progressSlider.value = request.downloadProgress;
                percentageText.text = (request.downloadProgress * 100).ToString() + "%";
                //Debug.Log(request.downloadProgress * 100);
            }
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Web request error: " + request.error);
            }
            else
            {
                Debug.Log("request success");
                assetBundles[index] = DownloadHandlerAssetBundle.GetContent(request);
            }

            index++;
            downloadNumber++;
        }
        yield return null;

    }
}
