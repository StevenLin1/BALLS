using UnityEngine;

namespace KitchenGame
{
    public class KitchenTool : KitchenItem
    {
        [SerializeField]
        private KitchenToolType toolType = KitchenToolType.None;

        [SerializeField]
        private float instantSliceCooldownSeconds = 0.15f;

        [SerializeField]
        private float panSnapHeightOffset = 0.025f;

        private float nextAllowedInstantSliceTime;

        public KitchenToolType ToolType => toolType;

        private void OnTriggerEnter(Collider other)
        {
            TryAutoSlice(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
            {
                return;
            }

            TryAutoSlice(collision.collider);
            TryPanInteractions(collision.collider);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision == null)
            {
                return;
            }

            TryPanInteractions(collision.collider);
        }

        private void TryAutoSlice(Collider other)
        {
            if (toolType != KitchenToolType.Knife || other == null || Time.time < nextAllowedInstantSliceTime)
            {
                return;
            }

            var ingredient = other.GetComponentInParent<KitchenIngredient>();
            if (ingredient == null)
            {
                return;
            }

            if (!ingredient.TryInstantSlice())
            {
                return;
            }

            nextAllowedInstantSliceTime = Time.time + instantSliceCooldownSeconds;
        }

        private void OnTriggerStay(Collider other)
        {
            TryPanInteractions(other);
        }

        private void TryPanInteractions(Collider other)
        {
            if (other == null)
            {
                return;
            }

            if (toolType == KitchenToolType.Pan)
            {
                TrySnapMeatIntoPan(other);
            }
            else if (toolType == KitchenToolType.Spatula)
            {
                TryServeFromPan(other);
            }
        }

        private void TrySnapMeatIntoPan(Collider other)
        {
            var ingredient = other.GetComponentInParent<KitchenIngredient>();
            if (ingredient == null || ingredient.IngredientKind != KitchenIngredientKind.Meat)
            {
                return;
            }

            var rigidbody = ingredient.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }

            var ownCollider = GetComponent<Collider>();
            var targetPosition = ownCollider != null
                ? ownCollider.bounds.center + Vector3.up * panSnapHeightOffset
                : transform.position + Vector3.up * panSnapHeightOffset;

            ingredient.transform.SetParent(transform, true);
            ingredient.transform.position = targetPosition;
        }

        private void TryServeFromPan(Collider other)
        {
            if (!IsStoveServeSurface(other))
            {
                return;
            }

            var stoveCookers = FindObjectsByType<KitchenStoveCooker>(FindObjectsSortMode.None);
            foreach (var stoveCooker in stoveCookers)
            {
                if (stoveCooker != null && stoveCooker.TryServeNearestCookedMeat())
                {
                    return;
                }
            }
        }

        private static bool IsStoveServeSurface(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            var panTool = other.GetComponentInParent<KitchenTool>();
            if (panTool != null && panTool.ToolType == KitchenToolType.Pan)
            {
                return true;
            }

            if (other.GetComponentInParent<KitchenStoveCooker>() != null)
            {
                return true;
            }

            var station = other.GetComponentInParent<KitchenStation>();
            return station != null && station.StationType == KitchenStationType.Stove;
        }
    }
}
