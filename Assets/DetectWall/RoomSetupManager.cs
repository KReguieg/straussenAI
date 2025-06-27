using System;
using Meta.XR.MRUtilityKit;
using UnityEngine;


public class RoomSetupManager : MonoBehaviour
{
    [SerializeField] private MRUK _mruk;
    [SerializeField] private EffectMesh _metaEffectMesh;
    [Range(1.0f, 3.0f)]
    [SerializeField] private float _selectionTime;

    [SerializeField] private GameObject _wallEffect;
    
    private MRUKRoom _room;
    private MRUKAnchor _tempSelectedWall;
    private bool _selectWallAsWorkingSpace = false;
    private float timer = 0f;

    public MRUKAnchor SelectedWall { get; set; }

    
    private void OnEnable()
    {
        _mruk.RoomCreatedEvent.AddListener(OnRoomCreated);
    }

    private void OnDisable()
    {
        _mruk.RoomCreatedEvent.RemoveListener(OnRoomCreated);

    }

    private void Start()
    {

    }

    private void OnRoomCreated(MRUKRoom room)
    {
        Debug.Log("ROOM CREATED!");
        _room = room;
        
        foreach (var wall in _room.WallAnchors)
        {
            var wallCollider = wall.GetComponentInChildren<Collider>();
            Debug.Log(
                $"Name: {wall.gameObject.name} Pos: {wall.transform.position} Bounds: {wall.GetComponentInChildren<Collider>().bounds.size}");
            wall.gameObject.layer = LayerMask.NameToLayer("Wall");
            var wallCollision = wall.gameObject.AddComponent<HandWallCollision>();
            wallCollision.OnWallTriggerEnter.AddListener(StartSelectWallAsWorkingSpace);
            wallCollision.OnWallTriggerExit.AddListener(StopSelectingWallAsWorkingSpace);
            var rb = wall.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_selectWallAsWorkingSpace)
        {
            if (timer <= _selectionTime)
            {
                timer += Time.deltaTime;
            }
            else
            {
                SelectWallAsWorkingSpace();
            }
        }
    }

    [ContextMenu("SELECT WALL")]
    private void SelectWallAsWorkingSpace()
    {
        SelectedWall = _tempSelectedWall;
        _metaEffectMesh.HideMesh = true;
    }

    [ContextMenu("Log Walls")]
    public void LogAllWalls()
    {
        foreach (var wall in _room.WallAnchors)
        {
            var wallCollider = wall.GetComponentInChildren<Collider>();
            Debug.Log(
                $"Name: {wall.gameObject.name} Pos: {wall.transform.position} Bounds: {wall.GetComponentInChildren<Collider>().bounds.size}");
            wall.gameObject.layer = LayerMask.NameToLayer("Wall");
            var wallCollision = wall.gameObject.AddComponent<HandWallCollision>();
            wallCollision.OnWallTriggerEnter.AddListener(StartSelectWallAsWorkingSpace);
            wallCollision.OnWallTriggerExit.AddListener(StopSelectingWallAsWorkingSpace);
            var rb = wall.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    [ContextMenu("START selecting wall")]
    public void StartSelectWallAsWorkingSpace()
    {
        _selectWallAsWorkingSpace = true;
        
    }

    [ContextMenu("STOP selecting wall")]
    public void StopSelectingWallAsWorkingSpace()
    {
        _selectWallAsWorkingSpace = false;
    }
}
