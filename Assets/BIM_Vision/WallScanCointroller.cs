using UnityEngine;
using UnityEngine.VFX;

public class WallScanCointroller : MonoBehaviour
{


    Vector3 WallCenter = new Vector3(0, 0, 0);
    Vector2 wallScanPosition;


    VisualEffect vfx;

    void Start()
    {
        vfx = GetComponent<VisualEffect>();
    }
    public void initWall(Vector2 size, Vector3 center)
    {
        gameObject.transform.position = center;
        vfx.SetVector2("dimensionsWH", size);
    }

    public void setWallScanPosition(Vector2 position, float amount)
    {
        wallScanPosition = position;
        vfx.SetVector2("wallScanPosition", wallScanPosition);
        vfx.SetFloat("wallScanAmount", amount);
    }

}
