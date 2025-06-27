using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomComplimentMaker : MonoBehaviour
{
    [SerializeField] private AI_TextToSpeech tts;

    private string welcomeMessage = "Hi, I’m your Focus Buddy. What do you want to accomplish? Click the microphone button to speak and click it again when you’re finished talking.";
    private string ptTutorialMessage = "Let’s create your focus areas. Click the plus button to add one.";
    private string goodJobMessage = "Well focused! +1 tomato to your basket. Time for a break!";
    private string rotateTutorialMessage = "Time to focus! Rotate me to set the timer for 25 minutes.";
    private string restartMessage = "Time to go back to work!";

    [TextArea]
    public List<string> Compliments = new();

    private string GetRandomElement()
    {
        int index = Random.Range(0, Compliments.Count);
        return Compliments[index];
    }

    [ContextMenu("SayRandomCompliment")]
    public void SayRandomCompliment()
    {
        string compliment = GetRandomElement();
        tts.ConvertTextToSpeechAsync(compliment);
    }

    public void SayStuff(int messageNumber)
    {
        string message = "Ok";
        switch (messageNumber)
        {
            case 0:
                message = welcomeMessage;
                break;
            case 1:
                message = ptTutorialMessage;
                break;
            case 2:
                message = rotateTutorialMessage;
                break;
            case 3:
                message = goodJobMessage;
                break;
            case 4:
                message = restartMessage;
                break;
            default:
                break;
        }
        tts.ConvertTextToSpeechAsync(message);
    }
}
