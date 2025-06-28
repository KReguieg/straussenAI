using UnityEngine;
using TMPro;

public class RotationMatcher : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("Object References")]
    [SerializeField] private Transform myObject;
    [SerializeField] private Axis myAxis = Axis.X;

    [SerializeField] private Transform targetObject;
    [SerializeField] private Axis targetAxis = Axis.X;

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private TMP_Text angleText;

    [Header("Settings")]
    [SerializeField] private float allowedOffset = 5f;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failColor = Color.red;

    private void Update()
    {
        float myAngle = GetAxisAngle(myObject, myAxis);
        float targetAngle = GetAxisAngle(targetObject, targetAxis);

        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(myAngle, targetAngle));
        angleText.text = $"Angle Difference: {angleDifference:F1}°";

        bool isMatch = angleDifference <= allowedOffset;
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
