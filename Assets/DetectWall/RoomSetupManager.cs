using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Meta.XR.MRUtilityKit;
using Oculus.Interaction;
using UnityEngine;


public class RoomSetupManager : MonoBehaviour
{
    [SerializeField] private MRUK _mruk;
    [SerializeField] private EffectMesh _metaEffectMesh;
    [Range(1.0f, 3.0f)]
    [SerializeField] private float _selectionTime;

    [SerializeField] private WallScanController _wallEffect;
    [SerializeField] private GameObject _shelfPrefab;
    [SerializeField] private OneGrabTranslateTransformer _shelfGrabTransformer;
    
    private MRUKRoom _room;
    [SerializeField] private MRUKAnchor _tempSelectedWall;
    private bool _selectWallAsWorkingSpace = false;
    private float timer = 0f;

    public MRUKAnchor SelectedWall { get; set; }


    private OneGrabTranslateTransformer.OneGrabTranslateConstraints _stuckToWallConstraints;
    

    
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
        StartCoroutine(WaitForSeconds());
    }

    IEnumerator WaitForSeconds()
    {
        yield return new WaitForSeconds(5.0f);
        LogAllWalls();
    }
    
    private bool wallEffectSpawned = false;
    
    // Update is called once per frame
    void Update()
    {
        if (_selectWallAsWorkingSpace)
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
                    _wallEffect.InitWall(size, SelectedWall.transform.position);
                    wallEffectSpawned = true;
                }

                _wallEffect.SetWallEffectAmount(Mathf.Lerp(0, 1,timer));
            }

            var constraint = CreateConstraint(SelectedWall.PlaneBoundary2D);
            _shelfGrabTransformer.InjectOptionalConstraints(constraint);
            _shelfGrabTransformer.Initialize(transform.GetComponent<IGrabbable>());
            _shelfPrefab.transform.position = SelectedWall.GetAnchorCenter();
            _shelfPrefab.transform.rotation = Quaternion.LookRotation(SelectedWall.transform.forward);
            _shelfPrefab.gameObject.SetActive(true);
        }
    }

    private OneGrabTranslateTransformer.OneGrabTranslateConstraints CreateConstraint(List<Vector2> wallSize)
    {
        return new()
        {
            ConstraintsAreRelative = false,
            MinX = new FloatConstraint()
            {
                Constrain = true,
                Value = wallSize[0].x
            },
            MaxX = new FloatConstraint()
            {
                Constrain = true,
                Value = wallSize[1].x
            },
            MinY = new FloatConstraint()
            {
                Constrain = true,
                Value = wallSize[0].y
            },
            MaxY = new FloatConstraint()
            {
                Constrain = true,
                Value = wallSize[1].y
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
    public void LogAllWalls()
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
}
