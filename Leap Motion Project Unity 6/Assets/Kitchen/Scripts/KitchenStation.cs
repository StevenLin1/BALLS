using System.Collections.Generic;
using UnityEngine;

namespace KitchenGame
{
    public class KitchenStation : MonoBehaviour
    {
        [SerializeField]
        private KitchenStationType stationType = KitchenStationType.Counter;

        [SerializeField]
        private Transform actionPoint;

        [SerializeField]
        private float actionRadius = 0.35f;

        private readonly List<KitchenItem> occupants = new();

        public KitchenStationType StationType => stationType;
        public Vector3 ActionPosition => actionPoint != null ? actionPoint.position : transform.position;

        public bool IsInRange(Vector3 worldPosition)
        {
            return Vector3.Distance(ActionPosition, worldPosition) <= actionRadius;
        }

        public KitchenIngredient GetPrimaryIngredient()
        {
            CleanupOccupants();

            foreach (var occupant in occupants)
            {
                if (occupant is KitchenIngredient ingredient)
                {
                    return ingredient;
                }
            }

            return GetComponentInChildren<KitchenIngredient>();
        }

        public bool TryApplyGesture(KitchenGestureType gesture, KitchenItem heldItem, float intensity, out KitchenIngredient ingredient,
            out KitchenIngredientState stateBefore, out KitchenIngredientState stateAfter)
        {
            ingredient = GetPrimaryIngredient();
            stateBefore = ingredient != null ? ingredient.CurrentState : KitchenIngredientState.None;
            stateAfter = stateBefore;

            if (ingredient == null)
            {
                return false;
            }

            bool applied = stationType switch
            {
                KitchenStationType.CuttingBoard => TryCut(gesture, heldItem, ingredient, intensity),
                KitchenStationType.Stove => TryCook(gesture, heldItem, ingredient, intensity),
                KitchenStationType.PrepBowl => TryMix(gesture, ingredient),
                KitchenStationType.Serving => TryPlate(gesture, ingredient),
                _ => false
            };

            stateAfter = ingredient.CurrentState;
            return applied;
        }

        private bool TryCut(KitchenGestureType gesture, KitchenItem heldItem, KitchenIngredient ingredient, float intensity)
        {
            if (gesture != KitchenGestureType.Chop)
            {
                return false;
            }

            if (heldItem is not KitchenTool tool || tool.ToolType != KitchenToolType.Knife)
            {
                return false;
            }

            return ingredient.ApplyChop(intensity);
        }

        private bool TryCook(KitchenGestureType gesture, KitchenItem heldItem, KitchenIngredient ingredient, float intensity)
        {
            if (gesture != KitchenGestureType.Stir && gesture != KitchenGestureType.Flip)
            {
                return false;
            }

            if (heldItem is KitchenTool tool &&
                tool.ToolType != KitchenToolType.Pan &&
                tool.ToolType != KitchenToolType.Spatula)
            {
                return false;
            }

            var bonus = gesture == KitchenGestureType.Flip ? 1.5f : 1f;
            return ingredient.ApplyCookingPulse(intensity * bonus);
        }

        private bool TryMix(KitchenGestureType gesture, KitchenIngredient ingredient)
        {
            if (gesture != KitchenGestureType.Stir)
            {
                return false;
            }

            return ingredient.Mix();
        }

        private bool TryPlate(KitchenGestureType gesture, KitchenIngredient ingredient)
        {
            if (gesture != KitchenGestureType.Place && gesture != KitchenGestureType.Pinch)
            {
                return false;
            }

            return ingredient.Plate();
        }

        private void OnTriggerEnter(Collider other)
        {
            var item = other.GetComponentInParent<KitchenItem>();
            if (item != null && !occupants.Contains(item))
            {
                occupants.Add(item);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var item = other.GetComponentInParent<KitchenItem>();
            if (item != null)
            {
                occupants.Remove(item);
            }
        }

        private void CleanupOccupants()
        {
            occupants.RemoveAll(item => item == null);
        }
    }
}
