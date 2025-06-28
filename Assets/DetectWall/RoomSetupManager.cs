using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using UnityEngine;


public class RoomSetupManager : MonoBehaviour
{
    [SerializeField] private MRUK _mruk;
    [SerializeField] private EffectMesh _metaEffectMesh;
    [Range(1.0f, 5.0f)]
    [SerializeField] private float _selectionTime;

    [SerializeField] private WallScanController _wallEffect;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _wallSelectedClip;
    [SerializeField] private GameObject _shelfPrefab;
    [SerializeField] private GameObject _wallInners;
    [SerializeField] private OneGrabTranslateTransformer _shelfGrabTransformer;
    [SerializeField] private Grabbable _grabbable;
    
    [SerializeField] private GameObject _measuretape1;
    [SerializeField] private GameObject _measuretape2;

    [SerializeField] private Transform _headTransform;
    
    private MRUKRoom _room;
    [SerializeField] private MRUKAnchor _tempSelectedWall;
    private bool wallEffectSpawned = false;
    private bool _selectWallAsWorkingSpace = false;
    private float timer = 0f;

    public MRUKAnchor SelectedWall { get; set; }


    private OneGrabTranslateTransformer.OneGrabTranslateConstraints _stuckToWallConstraints;
    private bool done = false;


    private void OnEnable()
    {
        _mruk.RoomCreatedEvent.AddListener(OnRoomCreated);
    }

    private void OnDisable()
    {
        _mruk.RoomCreatedEvent.RemoveListener(OnRoomCreated);

    }

    private void OnRoomCreated(MRUKRoom room)
    {
        _room = room;
        StartCoroutine(StartWaitForSeconds(0.5f));
    }
    
    IEnumerator StartWaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        InitalizeWalls();
    }

    IEnumerator WaitForSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
    


    // Update is called once per frame
    void Update()
    {
        if (_selectWallAsWorkingSpace && !done)
        {
            if (timer <= _selectionTime)
            {
                timer += Time.deltaTime;
                if (!wallEffectSpawned)
                {
                    SelectWallAsWorkingSpace();
                    _wallEffect.transform.SetPositionAndRotation(SelectedWall.transform.position, SelectedWall.transform.rotation);
                    var size = new Vector2(Mathf.Abs(SelectedWall.PlaneBoundary2D[0].x) * 2,
                        Mathf.Abs(SelectedWall.PlaneBoundary2D[0].y) * 2);
                    _wallEffect.transform.SetParent(SelectedWall.transform);
                    _wallEffect.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                    
                    _wallEffect.InitWall(size);
                    wallEffectSpawned = true;
                }

                _wallEffect.SetWallEffectAmount(MathHelper.Remap(0, _selectionTime, 0, 1, timer));
            }
            else
            {
                _selectWallAsWorkingSpace = false;
                SpawnLogic();
                done = true;
            }
        }
    }

    private void SpawnLogic()
    {
        StartCoroutine(WaitForSeconds(0.5f));
        _audioSource.clip = _wallSelectedClip;
        _audioSource.Play();
            
        var constraint = CreateConstraint(SelectedWall.PlaneBoundary2D);
        _shelfGrabTransformer.InjectOptionalConstraints(constraint);
        _shelfGrabTransformer.Initialize(_grabbable);
        
        var posOnWall = -Mathf.Abs(SelectedWall.PlaneBoundary2D[0].y) + _headTransform.position.y;
        
        _shelfPrefab.transform.SetParent(SelectedWall.transform);
        _shelfPrefab.transform.SetLocalPositionAndRotation(new Vector3(0, posOnWall, SelectedWall.transform.forward.z * 0.1f), Quaternion.identity);
        _wallInners.transform.SetParent(SelectedWall.transform);
        var innerWallPos = new Vector3(0, -Mathf.Abs(SelectedWall.PlaneBoundary2D[0].y), 0);
        _wallInners.transform.SetLocalPositionAndRotation(innerWallPos, Quaternion.identity);
            
        _measuretape1.transform.SetParent(SelectedWall.transform);
        _measuretape1.transform.SetLocalPositionAndRotation(new Vector3(0, posOnWall - 0.2f, SelectedWall.transform.forward.z * 0.1f), Quaternion.identity);
        _measuretape1.transform.SetParent(null, true);
            
        _measuretape2.transform.SetParent(SelectedWall.transform);
        _measuretape2.transform.SetLocalPositionAndRotation(new Vector3(0, posOnWall - 0.3f, SelectedWall.transform.forward.z * 0.1f), Quaternion.identity);
        _measuretape2.transform.SetParent(null, true);
    }

    private OneGrabTranslateTransformer.OneGrabTranslateConstraints CreateConstraint(List<Vector2> wallSize)
    {
        return new()
        {
            ConstraintsAreRelative = false,
            MinX = new FloatConstraint()
            {
                Constrain = true,
                Value = -wallSize[0].x
            },
            MaxX = new FloatConstraint()
            {
                Constrain = true,
                Value = wallSize[0].x
            },
            MinY = new FloatConstraint()
            {
                Constrain = true,
                Value = -wallSize[0].y
            },
            MaxY = new FloatConstraint()
            {
                Constrain = true,
                Value = wallSize[0].y
            },
            MinZ = new FloatConstraint()
            {
                Constrain = true,
                Value = 0
            },
            MaxZ = new FloatConstraint()
            {
                Constrain = true,
                Value = 0
            },
        };
    }

    [ContextMenu("SELECT WALL")]
    private void SelectWallAsWorkingSpace()
    {
        SelectedWall = _tempSelectedWall;
        // _metaEffectMesh.HideMesh = true;
    }

    [ContextMenu("Log Walls")]
    public void InitalizeWalls()
    {
        foreach (var wall in _room.WallAnchors)
        {
            if (wall.GetComponentInChildren<Collider>() == null)
            {
                continue;
            }

            var wallCollider = wall.GetComponentInChildren<MeshCollider>();
            wallCollider.convex = true;
            wallCollider.isTrigger = true;
            Debug.Log(
                $"Name: {wall.gameObject.name} Pos: {wall.transform.position} Bounds: {wallCollider.bounds.size}");
            wallCollider.gameObject.layer = LayerMask.NameToLayer("Wall");
            var wallCollision = wallCollider.gameObject.AddComponent<HandWallCollision>();
            wallCollision.OnWallTriggerEnter.AddListener(StartSelectWallAsWorkingSpace);
            wallCollision.OnWallTriggerExit.AddListener(StopSelectingWallAsWorkingSpace);
            var rb = wallCollider.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    [ContextMenu("START selecting wall")]
    public void StartSelectWallAsWorkingSpace(GameObject arg0)
    {
        _tempSelectedWall = arg0.GetComponentInParent<MRUKAnchor>();
        _selectWallAsWorkingSpace = true;
    }

    [ContextMenu("STOP selecting wall")]
    public void StopSelectingWallAsWorkingSpace()
    {
        _selectWallAsWorkingSpace = false;
    }
    
    Vector3 ProjectHeightOnWall(Vector3 wallCenter, Vector3 wallNormal, Vector3 wallUp, float x, float y)
    {
        Vector3 right = Vector3.Cross(wallNormal, wallUp).normalized;
        Vector3 up = wallUp.normalized;
        Vector3 localPos = wallCenter + right * x + up * y;
        return localPos;
    }

    public Vector3 ProjectHeight(float height)
    {
        // Create wall-aligned coordinate system
        Vector3 wallRight = Vector3.Cross(SelectedWall.transform.forward, SelectedWall.transform.forward).normalized;
        Vector3 wallUp = Vector3.Cross(wallRight, SelectedWall.transform.forward).normalized;
        
        // Calculate projected point
        return SelectedWall.GetAnchorCenter() + (wallUp * height);
    }
}
