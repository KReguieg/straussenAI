using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_WholeTesterNew : MonoBehaviour
{
    public AI_SpeechToText SpeechToText;
    public AI_TextToSpeech TextToSpeech;
    public AI_Conversator Conversator;
    public AI_CharacterDefiner CharacterDefinition;



    private void Start()
    {
        if (SpeechToText)
        {
            SpeechToText.AudioWasRecorded.AddListener(RecordedTextReceivedAsync);
        }

        PrepareCharacter();
    }

    private void PrepareCharacter()
    {
        string prompt = "You are acting as an AI or NPC inside a game, a player might talk to you and you will say something. The following describes the world you are living in:" + CharacterDefinition.WorldDefinition;

        prompt += "The following are the instructions for your character:\n";
        prompt += CharacterDefinition.CharacterName != null ? $"Your name is {CharacterDefinition.CharacterName}. " : "";
        prompt += CharacterDefinition.PublicCharacter != null ? $"Your public character is the following: {CharacterDefinition.PublicCharacter}." : "";
        prompt += CharacterDefinition.Abilities != null ? $"Your character and responses are defined by the following traits. Behave like this: {CharacterDefinition.Abilities}." : "";
        //prompt += ProvidePublicInformationOfAll(character);
        prompt += "Do not break character. Be creative";
        prompt += "Do not talk excessively. Instead encourage the player to ask questions";
        prompt += "Keep your answers bellow 30 words.";

        Conversator.AllMessages.Add(new ChatCompletionMessage
        {
            role = "system",
            content = prompt
        });
    }

    private async void RecordedTextReceivedAsync(string text)
    {
        string answer = await Conversator.RequestPromptAnswerAsync(text);

        TextToSpeech.ConvertTextToSpeechAsync(answer);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("Recording started");
            SpeechToText.ToggleRecording();
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            SpeechToText.ToggleRecording();
        }
    }


}
