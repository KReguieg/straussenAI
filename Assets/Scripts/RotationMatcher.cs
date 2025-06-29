using UnityEngine;
using TMPro;

public class RotationMatcher : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("Object References")]
    [SerializeField] private Transform myObject;
    [SerializeField] private Axis myAxis = Axis.X;

    [SerializeField] private Transform[] targetObjects;
    [SerializeField] private Transform selectedObject;
    [SerializeField] private Axis targetAxis = Axis.X;

    [SerializeField] private float enterDistance = 0.3f;

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private TMP_Text angleText;

    [Header("Settings")]
    [SerializeField] private float allowedOffset = 5f;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failColor = Color.red;

    [SerializeField] private string triggerTag = "Bohrloch";

    private void Update()
    {
        foreach (Transform obj in targetObjects)
        {
            float distance = Vector3.Distance(myObject.position, obj.position);
            if (distance <= enterDistance && !selectedObject)
            {
                Debug.LogError("trigger " + triggerTag, obj.gameObject);

                selectedObject = obj;
                angleText = obj.GetComponentInChildren<TMP_Text>();
                targetRenderer = obj.GetComponent<Renderer>();
            }
            else if(selectedObject == obj && distance > enterDistance)
            {
                Debug.LogError("distance " + distance, obj.gameObject);
                selectedObject = null;
                if (targetRenderer)
                    targetRenderer.material.color = failColor;
                if (angleText != null) angleText.text = "";
            }
        }

        if (!selectedObject) return;

        float myAngle = GetAxisAngle(myObject, myAxis);
        float targetAngle = GetAxisAngle(selectedObject, targetAxis);

        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(myAngle, targetAngle));
        if (angleText)
            angleText.text = $"Angle Difference: {angleDifference:F1}°";

        bool isMatch = angleDifference <= allowedOffset;

        if (targetRenderer)
            targetRenderer.material.color = isMatch ? successColor : failColor;


    }


    private float GetAxisAngle(Transform t, Axis axis)
    {
        Vector3 angles = t.eulerAngles;
        switch (axis)
        {
            case Axis.X: return angles.x;
            case Axis.Y: return angles.y;
            case Axis.Z: return angles.z;
            default: return 0f;
        }
    }
}
