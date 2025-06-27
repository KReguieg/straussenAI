using System;
using System.IO;
using UnityEngine;

public class WavUtility
{
    public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples = 0, string name = "wav")
    {
        using (MemoryStream stream = new MemoryStream(fileBytes))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            string riff = new string(reader.ReadChars(4));
            if (riff != "RIFF") throw new Exception("Invalid WAV file: RIFF header not found");

            int fileSize = reader.ReadInt32();
            if (fileSize == -1) fileSize = fileBytes.Length - 8; // Korrigiere die Dateigröße
            string wave = new string(reader.ReadChars(4));
            if (wave != "WAVE") throw new Exception("Invalid WAV file: WAVE header not found");

            string fmt = new string(reader.ReadChars(4));
            if (fmt != "fmt ") throw new Exception("Invalid WAV file: fmt header not found");

            int subchunk1Size = reader.ReadInt32(); // Should be 16 for PCM
            UInt16 audioFormat = reader.ReadUInt16();
            if (audioFormat != 1) throw new Exception("Invalid WAV file: Only PCM format is supported");

            UInt16 numChannels = reader.ReadUInt16();
            int sampleRate = reader.ReadInt32();
            reader.ReadInt32(); // Byte rate
            reader.ReadInt16(); // Block align
            UInt16 bitsPerSample = reader.ReadUInt16();

            string dataChunkId = new string(reader.ReadChars(4));
            while (dataChunkId != "data")
            {
                int chunkSize = reader.ReadInt32();
                reader.ReadBytes(chunkSize);
                dataChunkId = new string(reader.ReadChars(4));
            }

            int dataSize = reader.ReadInt32();
            if (dataSize == -1) dataSize = fileBytes.Length - (int)reader.BaseStream.Position; // Korrigiere die Datenlänge
            byte[] data = reader.ReadBytes(dataSize);

            float[] floatData = ConvertByteToFloat(data, bitsPerSample);
            AudioClip audioClip = AudioClip.Create(name, floatData.Length / numChannels, numChannels, sampleRate, false);
            audioClip.SetData(floatData, 0);

            return audioClip;
        }
    }

    private static float[] ConvertByteToFloat(byte[] data, UInt16 bitsPerSample)
    {
        int bytesPerSample = bitsPerSample / 8;
        int sampleCount = data.Length / bytesPerSample;
        float[] floatData = new float[sampleCount];

        switch (bitsPerSample)
        {
            case 8:
                for (int i = 0; i < sampleCount; i++)
                {
                    floatData[i] = (data[i] - 128) / 128f;
                }
                break;
            case 16:
                for (int i = 0; i < sampleCount; i++)
                {
                    short value = BitConverter.ToInt16(data, i * 2);
                    floatData[i] = value / 32768f;
                }
                break;
            case 24:
                for (int i = 0; i < sampleCount; i++)
                {
                    int value = BitConverter.ToInt32(new byte[] { data[i * 3], data[i * 3 + 1], data[i * 3 + 2], 0 }, 0);
                    floatData[i] = value / 8388608f;
                }
                break;
            case 32:
                for (int i = 0; i < sampleCount; i++)
                {
                    int value = BitConverter.ToInt32(data, i * 4);
                    floatData[i] = value / 2147483648f;
                }
                break;
            default:
                throw new Exception(bitsPerSample + " bit depth is not supported.");
        }

        return floatData;
    }
}
