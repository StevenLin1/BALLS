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
        private float requiredWaveDurationSeconds = 1f;

        [SerializeField]
        private float waveContinuationWindowSeconds = 0.35f;

        [SerializeField]
        private float bakeDurationSeconds = 2.5f;

        [SerializeField]
        private float cooldownSeconds = 1f;

        private float activeTimer = -1f;
        private float cooldownTimer;
        private float waveProgressSeconds;
        private float lastWaveGestureTime = -1f;

        public bool IsBusy => activeTimer > 0f;
        public float WaveProgress01 => requiredWaveDurationSeconds > 0.01f
            ? Mathf.Clamp01(waveProgressSeconds / requiredWaveDurationSeconds)
            : 1f;

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            if (activeTimer <= 0f &&
                waveProgressSeconds > 0f &&
                (lastWaveGestureTime < 0f || Time.time - lastWaveGestureTime > waveContinuationWindowSeconds))
            {
                waveProgressSeconds = 0f;
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

            var now = Time.time;

            if (lastWaveGestureTime < 0f || now - lastWaveGestureTime > waveContinuationWindowSeconds)
            {
                waveProgressSeconds = 0f;
            }
            else
            {
                waveProgressSeconds += now - lastWaveGestureTime;
            }

            lastWaveGestureTime = now;

            if (waveProgressSeconds < requiredWaveDurationSeconds)
            {
                return false;
            }

            waveProgressSeconds = 0f;
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
