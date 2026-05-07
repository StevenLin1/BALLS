using UnityEngine;

namespace KitchenGame
{
    public class KitchenZoneMover : MonoBehaviour
    {
        public enum MovementMode
        {
            ZoneStep,
            ContinuousWASD
        }

        [SerializeField]
        private MovementMode movementMode = MovementMode.ContinuousWASD;

        [SerializeField]
        private Transform[] zoneAnchors;

        [SerializeField]
        private int startingZoneIndex = 1;

        [SerializeField]
        private float moveSpeed = 3.5f;

        [SerializeField]
        private float verticalMoveSpeed = 2.5f;

        [SerializeField]
        private bool snapToStartingZoneOnEnable = true;

        [SerializeField]
        private Transform movementReference;

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

            if (movementMode == MovementMode.ZoneStep)
            {
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
            }
            else
            {
                UpdateContinuousMovement();
            }

            if (movementMode == MovementMode.ZoneStep)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
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

            if (movementReference == null && Camera.main != null)
            {
                movementReference = Camera.main.transform;
            }

            if (movementMode == MovementMode.ZoneStep)
            {
                if (zoneAnchors == null || zoneAnchors.Length == 0)
                {
                    targetPosition = transform.position;
                    return;
                }

                currentZoneIndex = Mathf.Clamp(startingZoneIndex, 0, zoneAnchors.Length - 1);
                targetPosition = zoneAnchors[currentZoneIndex].position;

                if (snapToStartingZoneOnEnable)
                {
                    transform.position = targetPosition;
                }
            }
            else
            {
                targetPosition = transform.position;
            }
        }

        private void UpdateContinuousMovement()
        {
            var input = Vector2.zero;

            if (Input.GetKey(KeyCode.A))
            {
                input.x -= 1f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                input.x += 1f;
            }

            if (Input.GetKey(KeyCode.S))
            {
                input.y -= 1f;
            }

            if (Input.GetKey(KeyCode.W))
            {
                input.y += 1f;
            }

            var verticalInput = 0f;
            if (Input.GetKey(KeyCode.Q))
            {
                verticalInput -= 1f;
            }

            if (Input.GetKey(KeyCode.E))
            {
                verticalInput += 1f;
            }

            if (input.sqrMagnitude <= 0.0001f)
            {
                if (Mathf.Abs(verticalInput) <= 0.0001f)
                {
                    return;
                }
            }

            input = input.normalized;

            var reference = movementReference != null ? movementReference : transform;

            var forward = reference.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;

            var right = reference.right;
            right.y = 0f;
            right = right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;

            var movement =
                (right * input.x + forward * input.y) * moveSpeed * Time.deltaTime +
                Vector3.up * (verticalInput * verticalMoveSpeed * Time.deltaTime);
            transform.position += movement;
        }
    }
}
