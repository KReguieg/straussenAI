using System;
using UnityEngine;
using UnityEngine.Events;


public class HandWallCollision : MonoBehaviour
{
    public UnityEvent<GameObject> OnWallTriggerEnter = new();
    public UnityEvent OnWallTriggerExit = new();

    private static bool jumped;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{other.gameObject.name} entered wall trigger: {gameObject.name}");
        OnWallTriggerEnter.Invoke(gameObject);
        if (!jumped)
        {
            GameFlowManager.Instance.JumpToNextState();
            jumped = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"{other.gameObject.name} exited wall trigger: {gameObject.name}");
        OnWallTriggerExit.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"{other.gameObject.name} stayed in wall trigger: {gameObject.name}");
    }
}
