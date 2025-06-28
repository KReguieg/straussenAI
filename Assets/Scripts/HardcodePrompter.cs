using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HardcodePrompter : MonoBehaviour
{
    public List<Promper> prompters = new List<Promper>();
    public AI_TextToSpeech aI_TextToSpeech;

    public int debugIndex = 0;

    [ContextMenu("RequestPromptAnswerAsync")]
    public void RequestPromptAnswerAsync()
    {
        int index = debugIndex;

        if (index < 0 || index >= prompters.Count)
        {
            Debug.LogError("Index out of range for prompters list.");
            return;
        }
        Promper selectedPrompter = prompters[index];
        if (selectedPrompter == null)
        {
            Debug.LogError("Selected prompter is null.");
            return;
        }
        aI_TextToSpeech.ConvertTextToSpeechAsync(selectedPrompter.PromptText);
    }
}

[Serializable]
public class Promper
{
    public int IndexOfPrompt;
    [TextArea(2, 4)]
    public string PromptText;
}
