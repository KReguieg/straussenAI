using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class APIKeyManager
{
    private static string apiKey;
    private static bool isInitialized = false;

    public static async Task<string> GetAPIKeyAsync()
    {
        await InitializeAsync();
        return apiKey;
    }

    private static async Task InitializeAsync()
    {
        // Der relative Pfad zur Textdatei im StreamingAssets Unterordner
        string relativePath = "APIKey.txt";
        string path = Path.Combine(Application.streamingAssetsPath, relativePath);
        apiKey = await GetRequestAsync(path);
        //isInitialized = true;
    }

    public static async Task<string> GetRequestAsync(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            var operation = webRequest.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                // Handle error
                throw new Exception(webRequest.error);
            }
            else
            {
                // Successfully received response
                return webRequest.downloadHandler.text;
            }
        }
    }
}
