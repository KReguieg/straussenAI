using System.Collections;
using Oculus.Interaction;
using UnityEngine;


namespace XRStorz.Highlighting
{
    /// <summary>
    /// Aligns the object to face the user.
    /// Alignment happens in steps, so that the object remains static unless its
    /// rotation relative to the user exceeds a threshold.
    /// </summary>
    public class AlignToUser : MonoBehaviour
    {
        [Tooltip("Threshold in degrees above which the auto-alignment will happen.")]
        [SerializeField] private float _alignmentThreshold = 20.0f;
        [Tooltip("Duration over which the orientation is aligned to the user (when exceeding the threshold).")]
        [SerializeField] private float _alignmentDuration = 0.25f;
        [Tooltip("If true, the pitch will be kept zero, otherwise it will point up/down to face the user.")]
        [SerializeField] private bool _maintainZeroPitch = true;
        [Tooltip("If true, the roll will be kept zero.")]
        [SerializeField] private bool _maintainZeroRoll = true;
        [Tooltip("If true, the target rotation will be set every frame")]
        [SerializeField] private bool _alwaysMaintainRotation = true;
        [Optional, Tooltip("If set, alignment will be paused while the object is grabbed.")]
        [SerializeField] private Grabbable _grabbable;

        private Quaternion _lastGlobalRotation;
        private bool _isAligning = false;

        private void OnEnable()
        {
            StartCoroutine(AlignInitially());
        }

        private IEnumerator AlignInitially()
        {
            while (Camera.main == null)
            {
                yield return null;
            }

            _isAligning = false;

            _lastGlobalRotation = GetTargetRotation();
            transform.rotation = _lastGlobalRotation;

            UpdateAlignment();
        }

        private void Update()
        {
            if (Camera.main == null)
            {
                return;
            }

            if (_grabbable && _grabbable.GrabPoints != null && _grabbable.GrabPoints.Count > 0)
            {
                return;
            }

            UpdateAlignment();
        }

        private void UpdateAlignment()
        {
            // always restore previous states
            if (_alwaysMaintainRotation)
            {
                transform.rotation = _lastGlobalRotation;
            }

            // Check if angle exceeds threshold
            if (_isAligning == false)
            {
                Quaternion targetRotation = GetTargetRotation();
                float angle = Quaternion.Angle(targetRotation, transform.rotation);
                if (angle >= _alignmentThreshold)
                {
                    StartCoroutine(StartAlignment());
                }
            }
        }

        private IEnumerator StartAlignment()
        {
            _isAligning = true;

            Quaternion alignmentStartRotation = transform.rotation;
            float time = 0;

            while (time < _alignmentDuration)
            {
                yield return null;
                time += Time.deltaTime;
                float fraction = time / _alignmentDuration;
                fraction = Mathf.Clamp(fraction, 0, 1);
                _lastGlobalRotation = Quaternion.Slerp(alignmentStartRotation, GetTargetRotation(), fraction);
                transform.rotation = _lastGlobalRotation;
            }

            _isAligning = false;
        }

        private Quaternion GetTargetRotation()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return Quaternion.identity;
            }
            
            Vector3 viewDirection = transform.position - mainCamera.transform.position;
            if (_maintainZeroPitch)
            {
                viewDirection.y = 0;
            }

            viewDirection.Normalize();
            Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, viewDirection);
            if (_maintainZeroRoll)
            {
                Vector3 rotationEuler = rotation.eulerAngles;
                rotationEuler.z = 0;
                return Quaternion.Euler(rotationEuler);
            }
            else
            {
                return rotation;
            }
        }
    }
}