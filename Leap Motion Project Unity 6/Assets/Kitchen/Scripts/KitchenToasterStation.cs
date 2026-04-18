using UnityEngine;

namespace KitchenGame
{
    public class KitchenToasterStation : MonoBehaviour
    {
        [SerializeField]
        private GameObject outputPrefab;

        [SerializeField]
        private Transform[] spawnPoints;

        [SerializeField]
        private float bakeDurationSeconds = 2.5f;

        [SerializeField]
        private float cooldownSeconds = 1f;

        private float activeTimer = -1f;
        private float cooldownTimer;

        public bool IsBusy => activeTimer > 0f;

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            if (activeTimer <= 0f)
            {
                return;
            }

            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0f)
            {
                SpawnBatch();
                cooldownTimer = cooldownSeconds;
            }
        }

        public bool TryStartToasting()
        {
            if (outputPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
            {
                return false;
            }

            if (activeTimer > 0f || cooldownTimer > 0f)
            {
                return false;
            }

            activeTimer = bakeDurationSeconds;
            return true;
        }

        private void SpawnBatch()
        {
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint == null)
                {
                    continue;
                }

                Instantiate(outputPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}
