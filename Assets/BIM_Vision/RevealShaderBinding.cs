using UnityEngine;

public class RevealShaderBinding : MonoBehaviour
{
    [Tooltip("The transform whose position will be sent to the material")]
    public Transform trackedTransform1;
    public Transform trackedTransform2;
    public Transform trackedTransform3;
    [Tooltip("The renderer whose material will receive the position")]
    public Renderer targetRenderer;
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
        targetRenderer = GetComponent<Renderer>();


        //warnn if  positionProperty is not existing in the material
        if (!targetRenderer.material.HasProperty(positionProperty1))
            Debug.LogError($"Material on {targetRenderer.name} does not have a property named '{positionProperty1}'.");
        if (!targetRenderer.material.HasProperty(positionProperty2))
            Debug.LogError($"Material on {targetRenderer.name} does not have a property named '{positionProperty2}'.");
        if (!targetRenderer.material.HasProperty(positionProperty3))
            Debug.LogError($"Material on {targetRenderer.name} does not have a property named '{positionProperty3}'.");
    }

    // Update is called once per frame
    void Update()
    {
        if (trackedTransform1 == null || trackedTransform2 == null || trackedTransform3 == null)
            Debug.LogError("Tracked Transform is not set. Please assign a Transform to track.");

        if (targetRenderer != null && trackedTransform1 != null)
        {
            Vector3 pos1 = trackedTransform1.position;
            Vector3 pos2 = trackedTransform2.position;
            Vector3 pos3 = trackedTransform3.position;
            targetRenderer.material.SetVector(positionProperty1, new Vector4(pos1.x, pos1.y, pos1.z, 1));
            targetRenderer.material.SetVector(positionProperty2, new Vector4(pos2.x, pos2.y, pos2.z, 1));
            targetRenderer.material.SetVector(positionProperty3, new Vector4(pos3.x, pos3.y, pos3.z, 1));
        }
    }
}
