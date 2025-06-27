using UnityEngine;

[CreateAssetMenu(fileName = "OpenAIPromptDefaults", menuName = "OpenAI/PromptDefaults")]
public class OpenAIPromptDefaults : ScriptableObject
{
    [TextArea(4, 10)]
    public string defaultPrefix;

    [TextArea(2, 6)]
    public string defaultSuffix;
}
