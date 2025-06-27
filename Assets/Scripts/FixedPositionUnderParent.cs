using UnityEngine;

namespace PhygitalIntimacy.Gameplay
{
    public class FixedPositionUnderParent : MonoBehaviour
    {
        public Transform Parent;
        public Vector3 LocalOffset;
        public bool WithRotation = true;


        void LateUpdate()
        {
            if (Parent == null) return;

            transform.position = Parent.TransformPoint(LocalOffset);
            if (WithRotation)
                transform.rotation = Quaternion.identity;
        }
    }
}
