using System;
using UnityEngine;
using UnityEngine.Events;


public class HandWallCollision : MonoBehaviour
{
    public UnityEvent OnWallTriggerEnter;
    public UnityEvent OnWallTriggerExit;
    
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Hand entered wall trigger: {gameObject.name}");
        OnWallTriggerEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"Hand exited wall trigger: {gameObject.name}");
        OnWallTriggerExit.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"Hand stay wall trigger: {gameObject.name}");
    }
}
