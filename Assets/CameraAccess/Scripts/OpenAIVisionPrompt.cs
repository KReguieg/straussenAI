using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Newtonsoft.Json.Linq;
using PassthroughCameraSamples;


public class OpenAIVisionPrompt : MonoBehaviour
{
    [SerializeField] private OpenAIPromptDefaults promptDefaults;
    [SerializeField] private GptModel selectedModel = GptModel.GPT4o;

    
    [SerializeField] private string userPrompt = "Was siehst du auf diesem Bild?";
    [SerializeField] private TextMeshProUGUI responseText;
    [SerializeField] private WebCamTextureManager m_webCamTextureManager;


    private WebCamTexture webCamTexture;
    private string openAIApiKey = "";
    private bool isRequestRunning = false;


    async void Start()
    {
        openAIApiKey = await APIKeyManager.GetAPIKeyAsync();

        // Start the webcam
        webCamTexture = new WebCamTexture();
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.mainTexture = webCamTexture;
        webCamTexture.Play();
        //StartCoroutine(GetWebcamTexture());
    }

    private IEnumerator GetWebcamTexture()
    {
        while (m_webCamTextureManager.WebCamTexture == null)
        {
            yield return null;
        }
        webCamTexture = m_webCamTextureManager.WebCamTexture;

        if (!webCamTexture.isPlaying)
            webCamTexture.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isRequestRunning)
        {
            RequestPictureAnalysation();
        }
    }

    public void RequestPictureAnalysation()
    {
        StartCoroutine(CaptureAndSendToOpenAI());
    }

    IEnumerator CaptureAndSendToOpenAI(float delaySeconds = 0)
    {
        isRequestRunning = true;

        yield return new WaitForSeconds(delaySeconds);

        // Convert WebCamTexture to Texture2D
        Texture2D image = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
        image.SetPixels(webCamTexture.GetPixels());
        image.Apply();

        // Encode to PNG
        byte[] pngBytes = image.EncodeToPNG();
        string base64Image = System.Convert.ToBase64String(pngBytes);
        Destroy(image); // optional

        // Build JSON
        string jsonPayload = BuildOpenAIJSON(userPrompt, base64Image);

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        // Send to OpenAI
        UnityWebRequest request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + openAIApiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseJson = request.downloadHandler.text;

            Debug.Log("OpenAI Antwort: " + responseJson);
            string content = ExtractContentFromResponse(responseJson);

            responseText.text = content;
        }
        else
        {
            Debug.LogError("Fehler: " + request.error);
            Debug.LogError(request.downloadHandler.text);
        }

        isRequestRunning = false;
    }

    private string BuildOpenAIJSON(string prompt, string base64Image)
    {
        return $@"
{{
  ""model"": ""gpt-4o"",
  ""messages"": [
    {{
      ""role"": ""user"",
      ""content"": [
        {{ ""type"": ""text"", ""text"": ""{EscapeJSON(prompt)}"" }},
        {{
          ""type"": ""image_url"",
          ""image_url"": {{
            ""url"": ""data:image/png;base64,{base64Image}""
          }}
        }}
      ]
    }}
  ]
}}";
    }


    private string EscapeJSON(string text)
    {
        return text.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private string ExtractContentFromResponse(string json)
    {
        JObject root = JObject.Parse(json);
        return root["choices"]?[0]?["message"]?["content"]?.ToString() ?? "[no content found]";
    }
    private string GetModelName()
    {
        switch (selectedModel)
        {
            case GptModel.GPT3_5_Turbo:
                return "gpt-3.5-turbo";
            case GptModel.GPT4:
                return "gpt-4";
            case GptModel.GPT4_Turbo:
                return "gpt-4-1106-preview";
            case GptModel.GPT4o:
            default:
                return "gpt-4o";
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (pause && webCamTexture != null && webCamTexture.isPlaying)
        {
            Debug.Log("Pausing App - stopping WebCamTexture");
            webCamTexture.Stop();
        }
    }

    void OnApplicationQuit()
    {
        if (webCamTexture != null && webCamTexture.isPlaying)
        {
            Debug.Log("Quitting App - stopping WebCamTexture");
            webCamTexture.Stop();
        }
    }

}

public enum GptModel
{
    GPT3_5_Turbo,
    GPT4,
    GPT4_Turbo,
    GPT4o
}
