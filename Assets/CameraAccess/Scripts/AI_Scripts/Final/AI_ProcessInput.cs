using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class AI_ProcessInput : MonoBehaviour
{
    public UnityEvent FirstListCreated;

    [SerializeField] private AI_Conversator conversator;

    [SerializeField] private Transform tasksObjectParent;
    [SerializeField] private GameObject taskPrefab;

    public List<string> CurrentList = new();

    private bool firstList = true;

    private void Start()
    {
        conversator.AnswerWasGiven.AddListener(ProcessAnswer);
    }

    private void ProcessAnswer(string answer)
    {


        CurrentList = ExtractNumberedEntries(answer);
        if (CurrentList == null) return;

        FindObjectOfType<AI_TextToSpeech>().AudioFinished.AddListener(() =>
        {
            if (firstList)
            {
                FirstListCreated.Invoke();
                firstList = false;
            }
        });

        // Remove previous list
        foreach (Transform child in tasksObjectParent)
        {
            Destroy(child.gameObject);
        }
        // Add entries
        for (int i = 0; i < CurrentList.Count; i++)
        {
            Debug.Log(CurrentList[i]);
            GameObject task = Instantiate(taskPrefab);
            task.GetComponentInChildren<TMP_Text>().text = CurrentList[i];

            task.transform.parent = tasksObjectParent;
            task.transform.localPosition = Vector3.zero;
            task.transform.localRotation = Quaternion.identity;

            task.transform.localPosition += new Vector3(-0.15f, 0.1f - 0.05f * i, 0);
        }

    }

    private List<string> ExtractNumberedEntries(string text)
    {
        Debug.LogError("regex " + text);
        if (text == null) return null;
        List<string> entries = new List<string>();
        Regex regex = new Regex(@"\d+\.\s+[^.]+\.");

        foreach (Match match in regex.Matches(text))
        {
            entries.Add(match.Value.Trim());
        }

        return entries;
    }
}
