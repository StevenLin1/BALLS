using UnityEngine;

namespace KitchenGame
{
    public class KitchenZoneMover : MonoBehaviour
    {
        [SerializeField]
        private Transform[] zoneAnchors;

        [SerializeField]
        private int startingZoneIndex = 1;

        [SerializeField]
        private float moveSpeed = 3.5f;

        [SerializeField]
        private bool snapToStartingZoneOnEnable = true;

        private int currentZoneIndex;
        private Vector3 targetPosition;
        private bool initialized;

        public int CurrentZoneIndex => currentZoneIndex;

        private void OnEnable()
        {
            Initialize();
        }

        private void Update()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (zoneAnchors == null || zoneAnchors.Length == 0)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                MoveToZone(currentZoneIndex - 1);
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                MoveToZone(currentZoneIndex + 1);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }

        public void MoveToZone(int zoneIndex)
        {
            if (zoneAnchors == null || zoneAnchors.Length == 0)
            {
                return;
            }

            currentZoneIndex = Mathf.Clamp(zoneIndex, 0, zoneAnchors.Length - 1);
            targetPosition = zoneAnchors[currentZoneIndex].position;
        }

        private void Initialize()
        {
            initialized = true;

            if (zoneAnchors == null || zoneAnchors.Length == 0)
            {
                return;
            }

            currentZoneIndex = Mathf.Clamp(startingZoneIndex, 0, zoneAnchors.Length - 1);
            targetPosition = zoneAnchors[currentZoneIndex].position;

            if (snapToStartingZoneOnEnable)
            {
                transform.position = targetPosition;
            }
        }
    }
}
