using UnityEngine;

namespace KitchenGame
{
    public class KitchenTool : KitchenItem
    {
        [SerializeField]
        private KitchenToolType toolType = KitchenToolType.None;

        [SerializeField]
        private float instantSliceCooldownSeconds = 0.15f;

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
    }
}
