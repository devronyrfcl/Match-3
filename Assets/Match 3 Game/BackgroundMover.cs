using UnityEngine;

public class BackgroundMoverNoTween : MonoBehaviour
{
    [Header("Movement Settings")]
    public float startX = 10.5f;
    public float endX = -6.33f;
    public float moveSpeed = 2f;

    private float journeyLength;
    private Vector3 startPos;
    private Vector3 endPos;

    void Start()
    {
        startPos = new Vector3(startX, transform.position.y, transform.position.z);
        endPos = new Vector3(endX, transform.position.y, transform.position.z);
        journeyLength = Vector3.Distance(startPos, endPos);
    }

    void Update()
    {
        // PingPong returns a value between 0 and journeyLength
        float pingPong = Mathf.PingPong(Time.time * moveSpeed, journeyLength);
        transform.position = Vector3.Lerp(startPos, endPos, pingPong / journeyLength);
    }
}