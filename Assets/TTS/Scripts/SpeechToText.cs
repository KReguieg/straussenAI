using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using static AI_Model;

public class SpeechToText : AI_Base
{
    [Header("Speech Detection")]
    public float startThreshold = 0.06f;   // Loudness to start speech
    public float stopThreshold = 0.04f;    // Loudness to end speech
    public float silenceTimeout = 1.0f;    // Time in seconds of silence to stop recording
    public int sampleWindow = 64;          // Number of samples for loudness check

    [Header("Transcription")]
    [SerializeField] private string language = "en";
    [SerializeField] private UnityEvent<string> onSpeak;

    private static AudioClip _clip;
    private bool isRecordingSpeech = false;
    private float silenceTimer = 0f;

    private int lastMicPosition = 0;
    private List<float> speechBuffer = new List<float>();
    [SerializeField] private TextMeshProUGUI responseText;
    //public float volume;

    protected override async void Start()
    {
        apiKey = await APIKeyManager.GetAPIKeyAsync();
        MicrophoneStart();
    }

    void Update()
    {
        if (!Microphone.IsRecording(null)) return;
        float volume = GetMicVolume();
        
        if (volume > startThreshold)
        {
            Debug.Log(volume);
            if (!isRecordingSpeech)
            {
                isRecordingSpeech = true;
                speechBuffer.Clear();
                silenceTimer = 0f;
            }
            silenceTimer = 0f; // Reset timer while talking
        }
        else if (isRecordingSpeech && volume < stopThreshold)
        {
            silenceTimer += Time.deltaTime;
            if (silenceTimer >= silenceTimeout)
            {
                isRecordingSpeech = false;
                silenceTimer = 0f;
                MicrophoneStop();
                ProcessAudio();
            }
        }

        if (isRecordingSpeech)
        {
            AppendCurrentMicSample();
        }
    }

    float GetMicVolume()
    {
        int micPosition = Microphone.GetPosition(null);
        int micSamples = _clip.samples;

        int startPos = micPosition - sampleWindow;
        if (startPos < 0) startPos += micSamples;

        float[] samples = new float[sampleWindow];

        if (startPos + sampleWindow <= micSamples)
        {
            _clip.GetData(samples, startPos);
        }
        else
        {
            int samplesToEnd = micSamples - startPos;
            float[] temp1 = new float[samplesToEnd];
            float[] temp2 = new float[sampleWindow - samplesToEnd];
            _clip.GetData(temp1, startPos);
            _clip.GetData(temp2, 0);

            samples = new float[temp1.Length + temp2.Length];
            temp1.CopyTo(samples, 0);
            temp2.CopyTo(samples, temp1.Length);
        }

        float totalLoudness = 0f;
        foreach (var s in samples)
            totalLoudness += Mathf.Abs(s);

        return totalLoudness / sampleWindow;
    }

    void AppendCurrentMicSample()
    {
        if (_clip == null) return;

        int micPos = Microphone.GetPosition(null);
        int micSamples = _clip.samples;

        int diff = micPos - lastMicPosition;
        if (diff < 0) diff += micSamples;
        if (diff == 0) return;

        float[] samples = new float[diff];

        if (lastMicPosition + diff <= micSamples)
        {
            _clip.GetData(samples, lastMicPosition);
        }
        else
        {
            int samplesToEnd = micSamples - lastMicPosition;
            float[] temp1 = new float[samplesToEnd];
            float[] temp2 = new float[diff - samplesToEnd];

            _clip.GetData(temp1, lastMicPosition);
            _clip.GetData(temp2, 0);

            samples = new float[temp1.Length + temp2.Length];
            temp1.CopyTo(samples, 0);
            temp2.CopyTo(samples, temp1.Length);
        }

        speechBuffer.AddRange(samples);
        lastMicPosition = micPos;
    }

    async void ProcessAudio()
    {
        int channels = _clip.channels;
        byte[] wavData = ConvertToWav(speechBuffer.ToArray(), AudioSettings.outputSampleRate, channels);
        await TranscribeAsync(wavData);

        lastMicPosition = Microphone.GetPosition(null);

        speechBuffer.Clear();
    }

    public void MicrophoneStart()
    {
        if (!GameFlowManager.Instance.State.Equals(GameFlowState.Level5))
        {
            MicrophoneStop();
        }
        else
        {
            _clip = Microphone.Start(null, true, 20, AudioSettings.outputSampleRate);
        }
    }

    public void MicrophoneStop()
    {
        Microphone.End(null);
    }

    async Task TranscribeAsync(byte[] wavData)
    {
        string text = await RequestAudioTranscription(wavData);
        if (!string.IsNullOrEmpty(text))
        {
            if (responseText)
                responseText.text = "transcribed text: " + text;
            onSpeak?.Invoke(text);
        }
    }

    byte[] ConvertToWav(float[] samples, int sampleRate, int channels)
    {
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];
        float rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
            intData[i] = (short)(samples[i] * rescaleFactor);

        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);

        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream);

        int headerSize = 44;
        int fileSize = bytesData.Length + headerSize - 8;

        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(fileSize);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * 2);
        writer.Write((short)(channels * 2));
        writer.Write((short)16);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(bytesData.Length);
        writer.Write(bytesData);

        writer.Flush();
        return stream.ToArray();
    }

    public async Task<string> RequestAudioTranscription(byte[] audioData)
    {

        var model = Model.FromAudioModel(AudioModel.Whisper);

        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(model.ModelName), "model");
        formData.Add(new StringContent(Temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)), "temperature");
        if (!string.IsNullOrEmpty(language))
            formData.Add(new StringContent(language), "language");

        var audioContent = new ByteArrayContent(audioData);
        audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
        formData.Add(audioContent, "file", "audio.wav");

        var result = await DoRequest<AudioTranscriptionResponse>(urlSpeechToText, HttpMethod.Post, formData);
        Debug.Log("Transcribed text:" + result.text);
        return result.text;
    }

    private async Task<T> DoRequest<T>(string url, HttpMethod method, HttpContent content, IProgress<double> progress = null, CancellationToken token = default) where T : class
    {
        using var request = new UnityWebRequest(url, method.ToString().ToUpperInvariant());
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Accept", "application/json");

        if (content != null)
        {
            byte[] contentBytes = await content.ReadAsByteArrayAsync();
            request.uploadHandler = new UploadHandlerRaw(contentBytes);
            request.SetRequestHeader("Content-Type", content.Headers.ContentType.ToString());
        }

        request.downloadHandler = new DownloadHandlerBuffer();

        var answer = await SendWebRequestAsync(request, progress, token);

        if (answer.result != UnityWebRequest.Result.Success && !token.IsCancellationRequested)
            Debug.LogError($"[STT ERROR] {answer.error}");

        var responseJson = answer.downloadHandler.text;
        return JsonUtility.FromJson<T>(responseJson);
    }

    public async Task<UnityWebRequest> SendWebRequestAsync(UnityWebRequest request, IProgress<double> progress, CancellationToken token = default)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();
        var operation = request.SendWebRequest();

        if (Application.isEditor)
        {
            while (!operation.isDone)
            {
                progress?.Report(operation.progress);
                if (token.IsCancellationRequested)
                    request.Abort();
                await Task.Delay(10);
            }
        }

        operation.completed += _ => tcs.SetResult(request);
        return await tcs.Task;
    }

    void OnDisable()
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);

        }
    }
}
