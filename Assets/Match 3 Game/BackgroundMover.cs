using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class BackgroundManager : MonoBehaviour
{
    [Header("Bubble Settings")]
    public GameObject bubblePrefab;
    public List<Transform> bubbleSpawnPoints;
    public float bubbleSpawnInterval = 0.5f;
    public float bubbleMoveDistance = 10f;
    public float bubbleMinScale = 1f;
    public float bubbleMaxScale = 5f;
    public float bubbleMinSpeed = 3f;
    public float bubbleMaxSpeed = 7f;

    [Header("Cloud Settings")]
    public GameObject[] cloudPrefabs;
    public List<Transform> leftSpawnPoints;
    public List<Transform> rightSpawnPoints;
    public float cloudSpawnInterval = 3f;
    public float cloudMinSpeed = 2f;
    public float cloudMaxSpeed = 5f;
    public float cloudTravelDistance = 30f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnBubble), 0f, bubbleSpawnInterval);
        InvokeRepeating(nameof(SpawnCloud), 1f, cloudSpawnInterval);
    }

    void SpawnBubble()
    {
        if (bubbleSpawnPoints.Count == 0 || bubblePrefab == null) return;

        Transform spawnPoint = bubbleSpawnPoints[Random.Range(0, bubbleSpawnPoints.Count)];
        GameObject bubble = Instantiate(bubblePrefab, spawnPoint.position, Quaternion.identity, transform);

        float scale = Random.Range(bubbleMinScale, bubbleMaxScale);
        bubble.transform.localScale = Vector3.one * scale;

        float moveDuration = Random.Range(bubbleMinSpeed, bubbleMaxSpeed);

        bubble.transform.DOMoveY(spawnPoint.position.y + bubbleMoveDistance, moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => Destroy(bubble));
    }

    void SpawnCloud()
    {
        bool spawnFromLeft = Random.value > 0.5f;

        List<Transform> spawnList = spawnFromLeft ? leftSpawnPoints : rightSpawnPoints;
        if (spawnList.Count == 0 || cloudPrefabs.Length == 0) return;

        Transform spawnPoint = spawnList[Random.Range(0, spawnList.Count)];
        GameObject cloudPrefab = cloudPrefabs[Random.Range(0, cloudPrefabs.Length)];
        GameObject cloud = Instantiate(cloudPrefab, spawnPoint.position, Quaternion.identity, transform);

        float moveDuration = Random.Range(cloudMinSpeed, cloudMaxSpeed);
        Vector3 targetPos = spawnPoint.position + (spawnFromLeft ? Vector3.right : Vector3.left) * cloudTravelDistance;

        cloud.transform.DOMoveX(targetPos.x, moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => Destroy(cloud));
    }
}