using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RevealShaderBinding : MonoBehaviour
{
    [Tooltip("The transform whose position will be sent to the material")]
    public Transform trackedTransform1;
    public Transform trackedTransform2;
    public Transform trackedTransform3;
    [Tooltip("The renderer whose material will receive the position")]
    public List<Renderer> targetRenderer = new();
    [Tooltip("The name of the shader property to set")]
    public string positionProperty1 = "_TrackedPosition1";
    public string positionProperty2 = "_TrackedPosition2";
    public string positionProperty3 = "_TrackedPosition3";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // If not set, try to get the Renderer on this GameObject
        if (targetRenderer == null)
            Debug.LogWarning("Target Renderer is not set. Attempting to get Renderer from this GameObject.");
        targetRenderer = GetComponentsInChildren<Renderer>().ToList();


        foreach (var renderer in targetRenderer)
        {
            //warnn if  positionProperty is not existing in the material
            if (!renderer.material.HasProperty(positionProperty1))
                Debug.LogError($"Material on {renderer.gameObject.name} {renderer.name} does not have a property named '{positionProperty1}'.");
            if (!renderer.material.HasProperty(positionProperty2))
                Debug.LogError($"Material on {renderer.name} does not have a property named '{positionProperty2}'.");
            if (!renderer.material.HasProperty(positionProperty3))
                Debug.LogError($"Material on {renderer.name} does not have a property named '{positionProperty3}'.");
        }

        if (trackedTransform1 == null || trackedTransform2 == null || trackedTransform3 == null)
            Debug.LogError("Tracked Transform is not set. Please assign a Transform to track.");
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var renderer in targetRenderer)
        {
            if (renderer != null && trackedTransform1 != null)
            {
                Vector3 pos1 = trackedTransform1.position;
                Vector3 pos2 = trackedTransform2.position;
                Vector3 pos3 = trackedTransform3.position;
                renderer.material.SetVector(positionProperty1, new Vector4(pos1.x, pos1.y, pos1.z, 1));
                renderer.material.SetVector(positionProperty2, new Vector4(pos2.x, pos2.y, pos2.z, 1));
                renderer.material.SetVector(positionProperty3, new Vector4(pos3.x, pos3.y, pos3.z, 1));
            }
        }
    }
}
