using TMPro;
using UnityEngine;


public class MeasuringTool : MonoBehaviour
{
    [SerializeField] private Transform _startPoint;
    [SerializeField] private Transform _endPoint;
    [SerializeField] private LineRenderer _measuringTapeLineRenderer;
    [SerializeField] private TMP_Text _measuringTapeText;
    [SerializeField] private Vector3 _textOffsetY;
    [SerializeField] private Vector2 _textSizeRemapInputInMillimeters = new(0f, 150f);
    [SerializeField] private Vector2 _textSizeRemapOutputInFontSize = new(0.09f, 0.25f);
    [SerializeField] private CapsuleCollider _collider;

    private readonly Vector3[] _lineRendererPositions = new Vector3[2];
    private static readonly int _DistanceInCentimeters = Shader.PropertyToID("_DistanceInCentimeters");
    private Material _lineRendererMaterial;
    private Vector3 _center;
    private float _lastUpdateDistance;

    private bool _isQuitting;
    
    
    private void Start()
    {
        _center = transform.position;
        _lineRendererMaterial = _measuringTapeLineRenderer.material;
    }
    
    private void LateUpdate()
    {
        MeasureDistance();
    }
    
    private void MeasureDistance()
    {
        var startPos = _startPoint.position;
        var endPos = _endPoint.position;
        _lineRendererPositions[0] = startPos;
        _lineRendererPositions[1] = endPos;
        _measuringTapeLineRenderer.SetPositions(_lineRendererPositions);

        // Divide by world scale of object so that if the object is scaled the correct centimeters are still calculated
        var distance = Vector3.Distance(startPos, endPos) / transform.lossyScale.x;
        var distanceInCentimeters = distance * 100f;

        _lineRendererMaterial.SetFloat(_DistanceInCentimeters, distanceInCentimeters);

        _measuringTapeText.text = $"{distanceInCentimeters:0.00} cm";
        // Set text object position in middle of distance and a little bit up
        _center = (startPos + endPos) * 0.5f;
        _measuringTapeText.transform.position = _center + _textOffsetY;

        _collider.transform.position = _center;
        _collider.transform.rotation = Quaternion.LookRotation(endPos - startPos);
        _collider.height = distance;

        // Scale text size depending on distance
        _measuringTapeText.fontSize = MathHelper.Remap(
            _textSizeRemapInputInMillimeters.x, _textSizeRemapInputInMillimeters.y,
            _textSizeRemapOutputInFontSize.x, _textSizeRemapOutputInFontSize.y,
            distanceInCentimeters);
    }
    
    protected virtual void OnValidate()
    {
        _center = transform.position;
        _lineRendererMaterial = _measuringTapeLineRenderer.sharedMaterial;
        MeasureDistance();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_center, 0.02f);
    }
}
