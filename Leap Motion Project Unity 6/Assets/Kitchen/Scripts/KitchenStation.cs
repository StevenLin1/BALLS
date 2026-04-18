using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

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
        private KitchenToasterStation toasterStation;
        private KitchenStoveCooker stoveCooker;

        public KitchenStationType StationType => stationType;
        public Vector3 ActionPosition => actionPoint != null ? actionPoint.position : transform.position;

        private void Awake()
        {
            toasterStation = GetComponent<KitchenToasterStation>();
            stoveCooker = GetComponent<KitchenStoveCooker>();
        }

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

        public bool TryApplyGesture(KitchenGestureType gesture, KitchenItem heldItem, Chirality chirality, float intensity, out KitchenIngredient ingredient,
            out KitchenIngredientState stateBefore, out KitchenIngredientState stateAfter, out int awardedScore)
        {
            awardedScore = 0;
            ingredient = GetPrimaryIngredient();
            stateBefore = ingredient != null ? ingredient.CurrentState : KitchenIngredientState.None;
            stateAfter = stateBefore;

            bool applied = stationType switch
            {
                KitchenStationType.CuttingBoard => TryCut(gesture, heldItem, chirality, out ingredient, out stateBefore, out stateAfter),
                KitchenStationType.Stove => TryCook(gesture, heldItem, chirality, out ingredient, out stateBefore, out stateAfter, out awardedScore),
                KitchenStationType.Toaster => TryToast(gesture, out ingredient, out stateBefore, out stateAfter),
                KitchenStationType.PrepBowl => TryMix(gesture, ingredient),
                KitchenStationType.Serving => TryPlate(gesture, ingredient),
                _ => false
            };

            return applied;
        }

        public IEnumerable<T> GetOccupants<T>() where T : KitchenItem
        {
            CleanupOccupants();

            foreach (var occupant in occupants)
            {
                if (occupant is T typed)
                {
                    yield return typed;
                }
            }
        }

        public bool HasTool(KitchenToolType toolType)
        {
            foreach (var tool in GetOccupants<KitchenTool>())
            {
                if (tool.ToolType == toolType)
                {
                    return true;
                }
            }

            return false;
        }

        public KitchenIngredient GetNearestIngredient(Vector3 worldPosition, bool heldOnly, bool requireDualSidedCooking)
        {
            KitchenIngredient best = null;
            var bestDistance = float.PositiveInfinity;

            foreach (var ingredient in GetOccupants<KitchenIngredient>())
            {
                if (ingredient == null)
                {
                    continue;
                }

                if (heldOnly && !ingredient.IsHeld)
                {
                    continue;
                }

                if (requireDualSidedCooking && !ingredient.SupportsDualSidedCooking)
                {
                    continue;
                }

                var distance = ingredient.DistanceTo(worldPosition);
                if (distance < bestDistance)
                {
                    best = ingredient;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private bool TryMix(KitchenGestureType gesture, KitchenIngredient ingredient)
        {
            if (gesture != KitchenGestureType.Stir || ingredient == null)
            {
                return false;
            }

            return ingredient.Mix();
        }

        private bool TryCut(KitchenGestureType gesture, KitchenItem heldItem, Chirality chirality, out KitchenIngredient ingredient,
            out KitchenIngredientState stateBefore, out KitchenIngredientState stateAfter)
        {
            ingredient = GetNearestIngredient(ActionPosition, heldOnly: true, requireDualSidedCooking: false);
            stateBefore = ingredient != null ? ingredient.CurrentState : KitchenIngredientState.None;
            stateAfter = stateBefore;

            if (gesture != KitchenGestureType.Chop)
            {
                if (ingredient != null)
                {
                    ingredient.ResetSliceProgress();
                }
                return false;
            }

            if (heldItem is not KitchenTool tool || tool.ToolType != KitchenToolType.Knife || ingredient == null)
            {
                if (ingredient != null)
                {
                    ingredient.ResetSliceProgress();
                }
                return false;
            }

            var applied = ingredient.ApplySliceStep(chirality, 0.35f, out _);
            stateAfter = ingredient.CurrentState;
            return applied;
        }

        private bool TryCook(KitchenGestureType gesture, KitchenItem heldItem, Chirality chirality, out KitchenIngredient ingredient,
            out KitchenIngredientState stateBefore, out KitchenIngredientState stateAfter, out int awardedScore)
        {
            if (stoveCooker == null)
            {
                ingredient = GetPrimaryIngredient();
                stateBefore = ingredient != null ? ingredient.CurrentState : KitchenIngredientState.None;
                stateAfter = stateBefore;
                awardedScore = 0;
                return false;
            }

            return stoveCooker.TryHandleCookGesture(gesture, heldItem, chirality, out ingredient, out stateBefore, out stateAfter, out awardedScore);
        }

        private bool TryToast(KitchenGestureType gesture, out KitchenIngredient ingredient, out KitchenIngredientState stateBefore, out KitchenIngredientState stateAfter)
        {
            ingredient = null;
            stateBefore = KitchenIngredientState.None;
            stateAfter = KitchenIngredientState.None;

            if (gesture != KitchenGestureType.Stir || toasterStation == null)
            {
                return false;
            }

            return toasterStation.TryStartToasting();
        }

        private bool TryPlate(KitchenGestureType gesture, KitchenIngredient ingredient)
        {
            if ((gesture != KitchenGestureType.Place && gesture != KitchenGestureType.Pinch) || ingredient == null)
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
