using System;
using UnityEngine;
using UnityEngine.Events;

public class VisibilityTrigger : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private Camera viewCamera;

    private bool wasVisible = false;

    public UnityEvent OnTargetBecameVisible;

    private void OnEnable()
    {
        viewCamera = Camera.main;
    }

    private void Update()
    {
        if (targetObject == null || viewCamera == null) return;

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(viewCamera);
        Bounds targetBounds = targetObject.GetComponent<Renderer>().bounds;

        bool isVisible = GeometryUtility.TestPlanesAABB(frustumPlanes, targetBounds);

        if (isVisible && !wasVisible)
        {
            wasVisible = true;
            Debug.Log("Target is now visible!");
            OnTargetBecameVisible?.Invoke();
            enabled = false; // Disable this script after the first visibility event
        }
        else if (!isVisible)
        {
            wasVisible = false;
        }
    }
}
