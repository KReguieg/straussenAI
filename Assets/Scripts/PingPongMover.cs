using UnityEngine;

public class PingPongMover : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);
    [SerializeField] private float duration = 2f; 

    private Vector3 startPos;
    private Vector3 endPos;
    private float timer;

    void Start()
    {
        startPos = transform.localPosition;
        endPos = startPos + offset;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.PingPong(timer / duration, 1f);
        transform.localPosition = Vector3.Lerp(startPos, startPos + offset, t);
    }
}
