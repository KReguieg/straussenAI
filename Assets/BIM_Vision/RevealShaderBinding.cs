using UnityEngine;

public class RevealShaderBinding : MonoBehaviour
{
    [Tooltip("The transform whose position will be sent to the material")] 
    public Transform trackedTransform;
    [Tooltip("The renderer whose material will receive the position")] 
    public Renderer targetRenderer;
    [Tooltip("The name of the shader property to set")] 
    public string positionProperty = "_TrackedPosition";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // If not set, try to get the Renderer on this GameObject
        if (targetRenderer == null)
            Debug.LogWarning("Target Renderer is not set. Attempting to get Renderer from this GameObject.");
            targetRenderer = GetComponent<Renderer>();
        if (trackedTransform == null)
            Debug.LogError("Tracked Transform is not set. Please assign a Transform to track.");

        //warnn if  positionProperty is not existing in the material
        if (!targetRenderer.material.HasProperty(positionProperty))
            Debug.LogError($"Material on {targetRenderer.name} does not have a property named '{positionProperty}'.");
    }

    // Update is called once per frame
    void Update()
    {
        if (targetRenderer != null && trackedTransform != null)
        {
            Vector3 pos = trackedTransform.position;
            targetRenderer.material.SetVector(positionProperty, new Vector4(pos.x, pos.y, pos.z, 1));

        }
    }
}
