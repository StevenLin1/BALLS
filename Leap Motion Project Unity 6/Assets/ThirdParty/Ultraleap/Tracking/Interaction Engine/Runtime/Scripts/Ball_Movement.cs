using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [Header("References")]
    public GameObject ballPrefab;
    public Transform shootPoint;
    public Transform goalTarget;

    [Header("Shoot Settings")]
    public float shootForce = 20f;
    public float randomOffsetX = 2f;
    public float randomOffsetY = 2f;
    public float respawnDelay = 3f;

    private GameObject currentBall;

    void Start()
    {
        SpawnAndShoot();
    }

    void SpawnAndShoot()
    {
        currentBall = Instantiate(ballPrefab, shootPoint.position, Quaternion.identity);

        Rigidbody rb = currentBall.GetComponent<Rigidbody>();

        Vector3 randomOffset = new Vector3(
            Random.Range(-randomOffsetX, randomOffsetX),
            Random.Range(-randomOffsetY, randomOffsetY),
            0f
        );

        Vector3 targetPosition = goalTarget.position + randomOffset;

        Vector3 direction = (targetPosition - shootPoint.position).normalized;

        rb.AddForce(direction * shootForce, ForceMode.Impulse);

        Invoke(nameof(RespawnBall), respawnDelay);
    }

    void RespawnBall()
    {
        if (currentBall != null)
        {
            Destroy(currentBall);
        }

        SpawnAndShoot();
    }
}