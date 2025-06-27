using UnityEngine;
using UnityEngine.VFX;

public class WallScanController : MonoBehaviour
{


    Vector3 WallCenter = new Vector3(0, 0, 0);
    Vector2 wallScanPosition;


    VisualEffect vfx;

    void Start()
    {
        vfx = GetComponent<VisualEffect>();
    }
    
    public void InitWall(Vector2 size, Vector3 center)
    {
        gameObject.transform.position = center;
        vfx.SetVector2("dimensionsWH", size);
    }

    public void SetWallEffectAmount(float amount)
    {
        vfx.SetFloat("revealAmount", amount);
    }

}
