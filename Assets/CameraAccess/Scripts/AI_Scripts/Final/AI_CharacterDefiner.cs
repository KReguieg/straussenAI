using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Character Data", order = 1)]
public class AI_CharacterDefiner : ScriptableObject
{
    public string CharacterName;
    public string Abilities;
    [TextArea(5, 10)]
    public string WorldDefinition;
    [TextArea]
    public string PublicCharacter;
    public int MinimumWordsToUse = 30;
    public int MaximumAllowedWords = 50;
}
