using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class AI_Base : MonoBehaviour
{
    protected string urlTextToSpeech = "https://api.openai.com/v1/audio/speech";
    public const string urlChatCompletion = "https://api.openai.com/v1/chat/completions";
    public const string urlSpeechToText = "https://api.openai.com/v1/audio/transcriptions";
    protected string apiKey;

    protected virtual async void Start()
    {
        await InitializeAsync();
    }

    protected virtual async Task InitializeAsync()
    {
        apiKey = await APIKeyManager.GetAPIKeyAsync();
    }


    public static double Temperature { get; set; } = 0.1;

    protected UnityWebRequest CreateRequest(object requestObject, string url)
    {
        string jsonRequest = JsonConvert.SerializeObject(requestObject);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonRequest);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        return request;
    }

    protected async Task<DownloadHandler> SendWebRequestAsync(UnityWebRequest request)
    {
        var operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError(request.error);
        }
        else
            return request.downloadHandler;

        return null;
    }
}


public enum SupportedVoice
{
    alloy,
    echo,
    fable,
    onyx,
    nova,
    shimmer
}

/// <summary>
/// Text to Speech Request. For voice see SupportedVoice
/// </summary>
[Serializable]
public class TTSRequest
{
    public string model;
    public string voice;
    public string response_format;
    public string input;
}

[Serializable]
public class ChatCompletionMessage
{
    /// <summary>
    /// The role of the message sender. Can be system (to give AI commands), user, assistant
    /// </summary>
    public string role;

    /// <summary>
    /// The content of the message. Can be a prompt or a response
    /// </summary>
    public string content;
}

/// <summary>
/// Represents a response for chat completion.
/// </summary>
[Serializable]
public class ChatCompletionResponse
{
    /// <summary>
    /// The completed text from the first choice.
    /// </summary>
    public string Text => choices[0].message.content;

    /// <summary>
    /// The choices for chat completion.
    /// </summary>
    public ChatCompletionChoice[] choices;
}

/// <summary>
/// Represents a choice for chat completion.
/// </summary>
[Serializable]
public class ChatCompletionChoice
{
    /// <summary>
    /// The completed message.
    /// </summary>
    public ChatCompletionMessage message;
}

/// <summary>
/// Represents a request for chat completion.
/// </summary>
[Serializable]
public class ChatCompletionRequest
{
    /// <summary>
    /// The name of the model to use for chat completion.
    /// </summary>
    public string model;

    /// <summary>
    /// The messages to use for chat completion.
    /// </summary>
    public ChatCompletionMessage[] messages;

    /// <summary>
    /// The temperature to use for chat completion. A higher temperature will result in more diverse responses.
    /// </summary>
    public double temperature;

    public bool stream;
}
/// <summary>
/// Represents a response for audio transcription.
/// </summary>
[Serializable]
class AudioTranscriptionResponse
{
    /// <summary>
    /// The transcribed text.
    /// </summary>
    public string text;
}

[Serializable]
public class StringEvent : UnityEvent<string> { }

