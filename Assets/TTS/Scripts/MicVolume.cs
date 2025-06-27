using UnityEngine;

public class MicVolume : MonoBehaviour
{
    private AudioClip micClip;
    private const int sampleWindow = 128;

    void Start()
    {
        // Start recording from default microphone
        micClip = Microphone.Start(null, true, 10, 44100);
    }

    void Update()
    {
        float volume = GetMicVolume();
        Debug.Log("Mic Volume: " + volume);
    }

    float GetMicVolume()
    {
        int micPosition = Microphone.GetPosition(null) - sampleWindow + 1;
        if (micPosition < 0) return 0;

        float[] samples = new float[sampleWindow];
        micClip.GetData(samples, micPosition);

        float sum = 0f;
        for (int i = 0; i < sampleWindow; i++)
        {
            sum += samples[i] * samples[i]; // square for RMS
        }

        return Mathf.Sqrt(sum / sampleWindow); // Root Mean Square
    }
}