using System.Collections.Generic;
using UnityEngine;

public class GameFlowStateHandler : MonoBehaviour
{
    [Header("Actions on Enter")]
    [SerializeField] private List<StateActivationEntry> onEnter = new();

    [Header("Actions on Exit")]
    [SerializeField] private List<StateActivationEntry> onExit = new();

    public async void EnterState()
    {
        Debug.Log("[StateHandler] EnterState");

        foreach (StateActivationEntry entry in onEnter)
            await entry.Execute(this);
    }

    public async void ExitState()
    {
        Debug.Log("[StateHandler] ExitState");

        foreach (StateActivationEntry entry in onExit)
            await entry.Execute(this);
    }

    [ContextMenu("GenerateOnExitFromEnter")]
    private void GenerateOnExitFromEnter()
    {
        int added = 0;

        foreach (StateActivationEntry entry in onEnter)
        {
            if (entry.target == null)
                continue;

            StateActivationEntry.ActionType? inverted = entry.action switch
            {
                StateActivationEntry.ActionType.ActivateGameObject => StateActivationEntry.ActionType.DeactivateGameObject,
                StateActivationEntry.ActionType.DeactivateGameObject => StateActivationEntry.ActionType.ActivateGameObject,
                StateActivationEntry.ActionType.EnableComponent => StateActivationEntry.ActionType.DisableComponent,
                StateActivationEntry.ActionType.DisableComponent => StateActivationEntry.ActionType.EnableComponent,
                _ => null
            };

            if (inverted == null)
                continue;

            bool alreadyExists = onExit.Exists(e =>
                e.target == entry.target && e.action == inverted);

            if (!alreadyExists)
            {
                onExit.Add(new StateActivationEntry
                {
                    target = entry.target,
                    action = inverted.Value
                });

                added++;
            }
        }

        Debug.Log($"[StateHandler] Added {added} new entries to onExit (merge-safe + mirrored).");
    }
}

