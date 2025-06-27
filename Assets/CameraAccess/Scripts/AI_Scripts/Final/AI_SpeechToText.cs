using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using static AI_Model;

public class AI_SpeechToText : AI_Base
{
    public StringEvent AudioWasRecorded;

    // The maximum length of the recording in seconds.
    [SerializeField]
    private int maxRecordingLength = 10;
    // The language used for transcription.
    [SerializeField]
    private string language = "en";

    [SerializeField]
    private Image ButtonImage;

    // The sample rate used for recording.
    private int sampleRate = 16000;
    // Flag indicating whether recording is currently in progress.
    private bool isRecording = false;
    private DateTime lastToggleTime = DateTime.MinValue;
    // Definiere, wie viel Zeit mindestens zwischen den Aufrufen liegen muss
    private readonly TimeSpan debounceTime = TimeSpan.FromSeconds(2);


    private static AudioClip _clip;


    // Called when recording was finished
    private async Task<string> ReturnTextFromAudio()
    {
        int length = Microphone.GetPosition(null);
        float[] samples = new float[length];
        _clip.GetData(samples, 0);
        StopRecording();

        Task<string> transcriptionTask = RequestAudioTranscription(samples, Model.FromAudioModel(AudioModel.Whisper), Temperature, language);
        await transcriptionTask;

        Debug.Log("Input is " + transcriptionTask.Result);

        return transcriptionTask.Result;
    }
    public async Task<string> RequestAudioTranscription(float[] pcmSamples16k1Ch, Model model, double temperature, string language = null)
    {
        byte[] audioData = ConvertAudioClipToWav(pcmSamples16k1Ch, 1, 16000);
        return await RequestAudioTranscription(audioData, model, temperature, language);
    }
    public async Task<string> RequestAudioTranscription(byte[] audioData, Model model, double temperature, string language = null)
    {
        using var formData = new MultipartFormDataContent();

        formData.Add(new StringContent(model.ModelName), "model");
        formData.Add(new StringContent(temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)), "temperature");

        if (language != null)
        {
            formData.Add(new StringContent(language), "language");
        }

        var audioContent = new ByteArrayContent(audioData);
        audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
        formData.Add(audioContent, "file", "audio.wav");

        var result = await DoRequest<AudioTranscriptionResponse>(urlSpeechToText, HttpMethod.Post, formData);
        return result.text;
    }
    private async Task<T> DoRequest<T>(string url, HttpMethod method, HttpContent content, IProgress<double> progress = null, CancellationToken token = default) where T : class
    {

        using (var request = new UnityWebRequest(url, method.ToString().ToUpperInvariant()))
        {
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.SetRequestHeader("Accept", "application/json");
            if (content != null)
            {
                byte[] contentBytes = await content.ReadAsByteArrayAsync();
                request.uploadHandler = new UploadHandlerRaw(contentBytes);
                request.SetRequestHeader("Content-Type", content.Headers.ContentType.ToString());
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            UnityWebRequest answer = await SendWebRequestAsync(request, progress, token);

            if (answer.result != UnityWebRequest.Result.Success && !token.IsCancellationRequested)
            {
                Debug.LogError("error stt");
            }

            var responseJson = answer.downloadHandler.text;
            return JsonUtility.FromJson<T>(responseJson);
        }
    }
    public async Task<UnityWebRequest> SendWebRequestAsync(UnityWebRequest request, IProgress<double> progress, CancellationToken token = default)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();
        var webRequestAsyncOperation = request.SendWebRequest();
        if (Application.isEditor)
        {
            var i = 0;
            while (!webRequestAsyncOperation.isDone)
            {
                i = (i + 1) % 100;
                progress?.Report(i / 100f);
                //progress.Report(webRequestAsyncOperation.progress);
                if (token.IsCancellationRequested)
                    webRequestAsyncOperation.webRequest.Abort();
                await Task.Delay(10);
            }
        }

        webRequestAsyncOperation.completed += _ =>
        {
            tcs.SetResult(request);
        };
        return await tcs.Task;
    }

    public void Debugger(string message)
    {
        // Überprüfe, ob die letzte Umschaltung weniger als 'debounceTime' zurückliegt
        if ((DateTime.Now - lastToggleTime) < debounceTime)
            return; // Wenn ja, ignoriere den aktuellen Aufruf

        // Aktualisiere die Zeit des letzten erfolgreichen Aufrufs
        lastToggleTime = DateTime.Now;

        Debug.LogError(message);
    }

    public void ToggleMaterial()
    {
        if (ButtonImage)
            ButtonImage.material.color = ButtonImage.material.color == Color.red ? Color.blue : Color.red;
    }

    public async void ToggleRecording()
    {

        // Überprüfe, ob die letzte Umschaltung weniger als 'debounceTime' zurückliegt
        if ((DateTime.Now - lastToggleTime) < debounceTime)
            return; // Wenn ja, ignoriere den aktuellen Aufruf

        // Aktualisiere die Zeit des letzten erfolgreichen Aufrufs
        lastToggleTime = DateTime.Now;

        ToggleMaterial();

        Debug.LogError("entered ToggleRecording with isRecording " + isRecording);

        if (!isRecording)
            StartRecording();
        else
        {
            // Internally calls StopRecording
            string text = await ReturnTextFromAudio();
            if (AudioWasRecorded != null)
                AudioWasRecorded.Invoke(text);
        }
    }
    private void StartRecording()
    {
        isRecording = true;
        _clip = Microphone.Start(null, false, maxRecordingLength, sampleRate);
    }
    private void StopRecording()
    {
        isRecording = false;
        Microphone.End(null);
    }

    /// <summary>
    /// Converts an audio clip to a wav byte array
    /// </summary>
    private byte[] ConvertAudioClipToWav(float[] samples, int channels, int frequency)
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);

        int totalBytes = samples.Length * 2;

        writer.Write(0x46464952); // "RIFF"
        writer.Write(36 + totalBytes);
        writer.Write(0x45564157); // "WAVE"
        writer.Write(0x20746D66); // "fmt "
        writer.Write(16);
        writer.Write((short)1); // PCM format
        writer.Write((short)channels);
        writer.Write(frequency);
        writer.Write(frequency * channels * 2); // Byte rate
        writer.Write((short)(channels * 2)); // Block align
        writer.Write((short)16); // Bits per sample
        writer.Write(0x61746164); // "data"
        writer.Write(totalBytes);

        for (int i = 0; i < samples.Length; i++)
        {
            writer.Write((short)(samples[i] * 32767));
        }

        writer.Flush();
        byte[] wavData = stream.ToArray();
        writer.Close();
        stream.Close();

        return wavData;
    }

}
