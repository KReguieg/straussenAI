using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;


public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    private GameFlowState _state;
    public GameFlowState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                OnStateChanged();
            }
        }
    }

    [SerializeField] private GameFlowState initialState = GameFlowState.Intro;


    public static event Action<GameFlowState> OnFlowStateChanged;

    public List<FlowStateHandlerEntry> _handlers = new();


    private GameFlowStateHandler currentHandler;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ExitAllStates();
        SetState(initialState);
    }

    public void SetState(GameFlowState newState)
    {
        State = newState;
    }

    private void OnStateChanged()
    {
        HandleStateChange(State);
        OnFlowStateChanged?.Invoke(State);
    }

    private void HandleStateChange(GameFlowState newState)
    {
        // When there was a previous handler, call its ExitState
        currentHandler?.ExitState();

        foreach (FlowStateHandlerEntry entry in _handlers)
        {
            if (entry.state == newState && entry.handler is GameFlowStateHandler newHandler)
            {
                currentHandler = newHandler;
                currentHandler.EnterState();
                return;
            }
        }

        Debug.LogWarning($"No handler for state '{newState}' found.");
    }

    [ContextMenu("JumpToNextState")]
    public void JumpToNextState()
    {
        // Erstelle eine Liste der States aus den Handlern, in der Reihenfolge wie im Inspector
        List<GameFlowState> handlerStates = _handlers.Select(h => h.state).ToList();

        int currentIndex = handlerStates.IndexOf(State);
        if (currentIndex < 0 || currentIndex + 1 >= handlerStates.Count)
        {
            Debug.LogWarning("Already at the last handler state or current state not found in handlers.");
            return;
        }

        GameFlowState nextState = handlerStates[currentIndex + 1];

        Debug.Log($"[FLOW] Jumped from {State} to {nextState}");
        SetState(nextState);
    }

    public GameFlowStateHandler GetCurrentHandler()
    {
        return currentHandler;
    }

    public void ExitAllStates()
    {
        foreach (FlowStateHandlerEntry entry in _handlers)
        {
            if (entry.handler is GameFlowStateHandler handler)
            {
                handler.ExitState();
            }
        }
    }

}

[Serializable]
public class FlowStateHandlerEntry
{
    public GameFlowState state;
    public MonoBehaviour handler;
}


[Serializable]
public enum GameFlowState
{
    None,
    Intro,
    Level1,
    Level2,
    Level3,
    Level4,
    Level5,
    BossFight,
    Outro,
}
