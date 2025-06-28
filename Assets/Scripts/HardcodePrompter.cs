using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HardcodePrompter : MonoBehaviour
{
    public List<Promper> prompters = new List<Promper>();
    public AI_TextToSpeech aI_TextToSpeech;
}

[Serializable]
public class Promper
{
    public int IndexOfPrompt;
    [TextArea(2, 4)]
    public string PromptText;
}
