using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using static AI_Model;
using UnityEngine.Events;
using Oculus;
//using Oculus.Voice.Dictation;
using UnityEngine.UI;

public class AI_Conversator : AI_Base
{
    public List<ChatCompletionMessage> AllMessages = new();

    public StringEvent AnswerWasGiven;

    //public AppDictationExperience VoiceToText;

    public AI_CharacterDefiner CharacterDefinition;

    [SerializeField]
    private Image ButtonImage;

    private void Awake()
    {
        //VoiceToText.DictationEvents.OnFullTranscription.AddListener(RequestByOculus);

        PrepareCharacter();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Listen();
        }
    }

    public void Listen()
    {
        //VoiceToText.Activate();
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
        prompt += $"Keep your answers between {CharacterDefinition.MinimumWordsToUse} and {CharacterDefinition.MaximumAllowedWords} words.";

        AllMessages.Add(new ChatCompletionMessage
        {
            role = "system",
            content = prompt
        });
    }

    [ContextMenu("TestRequest")]
    public void TestRequest()
    {
        AllMessages.Add(new ChatCompletionMessage
        {
            role = "user",
            content = "What is your favourite color?"
        });

        _ = CreateRequest(AllMessages.ToArray());
    }

    private void RequestByOculus(string voice)
    {
        Debug.Log(voice);
        _ = RequestPromptAnswerAsync(voice);
    }

    public async Task<string> RequestPromptAnswerAsync(string prompt)
    {
        AllMessages.Add(new ChatCompletionMessage
        {
            role = "user",
            content = prompt
        });

        string result = await CreateRequest(AllMessages.ToArray());

        if (AnswerWasGiven != null && result != "")
        {
            AnswerWasGiven.Invoke(result);
            Debug.Log(result);
        }
            

        
        AllMessages.Add(new ChatCompletionMessage
        {
            role = "assistant",
            content = result
        });

        return result;
    }

    public async Task<string> CreateRequest(ChatCompletionMessage[] messages)
    {
        string model = Model.FromChatModel(ChatModel.ChatGPT4o).ModelName;

        ChatCompletionRequest requestData = new ChatCompletionRequest
        {
            model = model,
            messages = messages,
            temperature = Temperature,
            stream = false // Check this todo
        };

        UnityWebRequest webRequest = CreateRequest(requestData, urlChatCompletion);
        DownloadHandler dh = await SendWebRequestAsync(webRequest);


        if (dh != null)
        {
            string responseJson = dh.text;
  
            try
            {
                ChatCompletionResponse response = JsonConvert.DeserializeObject<ChatCompletionResponse>(responseJson);
                if (response != null && response.choices != null && response.choices.Length > 0)
                {
                    string responseContent = response.Text;

                    return responseContent;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error parsing response: " + ex.Message);
            }
        }

        return null;
    }

    public void ToggleButtonColor()
    {
        if (ButtonImage)
            ButtonImage.material.color = ButtonImage.material.color == Color.red ? Color.white : Color.red;
    }

    private void OnApplicationPause(bool pause)
    {
        ButtonImage.material.color = Color.white;
    }
}
