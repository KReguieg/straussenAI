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

    [SerializeField] private UnityEvent<string> onSpeak;


    private static AudioClip _clip;

    protected override async Task InitializeAsync()
    {
        await base.InitializeAsync(); // optional
        Debug.LogError(apiKey);
    }



    public float volumeThreshold; // adjust as needed
    public float endThreshold;
    public float silenceTimeout = 1.0f;   // seconds of silence before stopping recording
    public int minRecordingLength = 500;  // ms, discard shorter noise
    private int sampleWindow = 64;

    private bool isRecordingSpeech = false;
    private float silenceTimer = 0f;

    private List<float> speechBuffer = new List<float>();
    public float loudnessSensibility = 10f;
    int lastMicPosition = 0;


    public async Task<string> RequestAudioTranscription(byte[] audioData)
    {
        Model model = Model.FromAudioModel(AudioModel.Whisper);

        using var formData = new MultipartFormDataContent();

        formData.Add(new StringContent(model.ModelName), "model");
        formData.Add(new StringContent(Temperature.ToString(System.Globalization.CultureInfo.InvariantCulture)), "temperature");

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

    byte[] ConvertToWav(float[] samples, int sampleRate, int channels)
    {
        short[] intData = new short[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        float rescaleFactor = 32767;

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
        }
        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);

        using MemoryStream stream = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(stream);

        int headerSize = 44;
        int fileSize = bytesData.Length + headerSize - 8;

        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(fileSize);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // PCM chunk size
        writer.Write((short)1); // PCM
        writer.Write((short)channels); // channels
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * 2); // byte rate
        writer.Write((short)(channels * 2)); // block align
        writer.Write((short)16); // bits per sample

        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(bytesData.Length);
        writer.Write(bytesData);

        writer.Flush();
        return stream.ToArray();
    }

    void Start()
    {
        // Start recording from default microphone
        _clip = Microphone.Start(null, true, 20, AudioSettings.outputSampleRate);
    }

    void Update()
    {
        float volume = GetMicVolume();
        //Debug.Log("Mic Volume: " + volume);

        if (volume > volumeThreshold)
        {
            Debug.LogError(volume);
            if (!isRecordingSpeech)
            {
                Debug.LogError("Started speech recording");
                isRecordingSpeech = true;
                speechBuffer.Clear();
                silenceTimer = 0f;
            }
            silenceTimer = 0f;
        }
        else if (isRecordingSpeech && volume < endThreshold)
        {
            silenceTimer += Time.deltaTime;
            if (silenceTimer >= silenceTimeout)
            {
                Debug.Log("Stopped recording speech");
                isRecordingSpeech = false;
                ProcessAudio();
                silenceTimer = 0f;
            }
        }


        if (isRecordingSpeech)
        {
            AppendCurrentMicSample();

        }
    }

    void AppendCurrentMicSample()
    {
        if (_clip == null) return;

        int micPos = Microphone.GetPosition(null);
        int micSamples = _clip.samples;
        Debug.Log("recorded samples: " + micSamples);


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
            _clip.GetData(temp1, lastMicPosition);

            float[] temp2 = new float[diff - samplesToEnd];
            _clip.GetData(temp2, 0);

            samples = new float[temp1.Length + temp2.Length];
            temp1.CopyTo(samples, 0);
            temp2.CopyTo(samples, temp1.Length);
        }

        speechBuffer.AddRange(samples);

        lastMicPosition = micPos;
        //Debug.Log("recorded samples: " + samples);
    }

    async void ProcessAudio()
    {
        Debug.Log($"Processing {speechBuffer.Count} samples");

        // Convert speechBuffer to proper audio file/format for your STT API here.
        int channels = _clip.channels;
        byte[] wavData = ConvertToWav(speechBuffer.ToArray(), AudioSettings.outputSampleRate, channels);


        string text = await RequestAudioTranscription(wavData);
       // string text = await ReturnTextFromAudio();
        if (!string.IsNullOrEmpty(text))
        {
            Debug.Log("text from audio: " + text);
            onSpeak?.Invoke(text);
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
            _clip.GetData(temp1, startPos);

            float[] temp2 = new float[sampleWindow - samplesToEnd];
            _clip.GetData(temp2, 0);

            samples = new float[temp1.Length + temp2.Length];
            temp1.CopyTo(samples, 0);
            temp2.CopyTo(samples, temp1.Length);
        }

        float totalLoudness = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            totalLoudness += Mathf.Abs(samples[i]);
        }

        return totalLoudness / sampleWindow;
    }



    void OnDisable()
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
            Debug.Log("Mic stopped.");
        }
    }
}
