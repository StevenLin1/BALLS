using UnityEngine;
using Leap.Unity;

namespace KitchenGame
{
    public class KitchenToasterStation : MonoBehaviour
    {
        [SerializeField]
        private GameObject outputPrefab;

        [SerializeField]
        private Transform[] spawnPoints;

        [SerializeField]
        private LeapProvider leapProvider;

        [SerializeField]
        private float bakeDurationSeconds = 2.5f;

        [SerializeField]
        private float cooldownSeconds = 1f;

        private KitchenStation station;
        private float activeTimer = -1f;
        private float cooldownTimer;
        private bool leftHandWasInsideZone;
        private bool rightHandWasInsideZone;

        public bool IsBusy => activeTimer > 0f;

        private void Awake()
        {
            station = GetComponent<KitchenStation>();

            if (leapProvider == null)
            {
                leapProvider = FindFirstObjectByType<LeapProvider>();
            }
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }

            CheckForHandsEnteringZone();

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

        private void CheckForHandsEnteringZone()
        {
            if (leapProvider == null || station == null || activeTimer > 0f || cooldownTimer > 0f)
            {
                return;
            }

            var frame = leapProvider.CurrentFrame;
            if (frame == null || frame.Hands == null)
            {
                leftHandWasInsideZone = false;
                rightHandWasInsideZone = false;
                return;
            }

            var leftInsideNow = false;
            var rightInsideNow = false;

            foreach (var hand in frame.Hands)
            {
                var isInside = station.IsInRange(hand.PalmPosition);
                if (hand.IsLeft)
                {
                    leftInsideNow |= isInside;
                }
                else
                {
                    rightInsideNow |= isInside;
                }
            }

            if (leftInsideNow && !leftHandWasInsideZone)
            {
                TryStartToasting();
            }
            else if (rightInsideNow && !rightHandWasInsideZone)
            {
                TryStartToasting();
            }

            leftHandWasInsideZone = leftInsideNow;
            rightHandWasInsideZone = rightInsideNow;
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
