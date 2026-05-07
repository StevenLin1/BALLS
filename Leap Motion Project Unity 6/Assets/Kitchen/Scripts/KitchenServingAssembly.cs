using System;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using UnityEngine;

namespace KitchenGame
{
    public class KitchenServingAssembly : MonoBehaviour
    {
        [SerializeField]
        private ServingOrderSlot[] orderSlots;

        [SerializeField]
        private KitchenGameManager gameManager;

        [SerializeField]
        private float stackHeightPerLayer = 0.025f;

        [SerializeField]
        private float topBunHeightBonus = 0.01f;

        [SerializeField]
        private int completedOrderBaseScore = 200;

        [SerializeField]
        private int ingredientMismatchPenalty = 35;

        [SerializeField]
        private int burntMeatPenalty = 50;

        [SerializeField]
        private int minimumCompletedOrderScore = 50;

        [SerializeField]
        private Collider servingDetectionZone;

        private int currentOrderIndex;
        private readonly HashSet<int> consumedInstanceIds = new();

        public int CurrentOrderIndex => currentOrderIndex;
        public int OrderSlotCount => orderSlots != null ? orderSlots.Length : 0;

        public event Action<int, ServingOrderResult> OrderCompleted;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<KitchenGameManager>();
            }

            if (servingDetectionZone == null)
            {
                servingDetectionZone = GetComponent<Collider>();
            }
        }

        private void Update()
        {
            TryConsumeHeldIngredientsInZone();
        }

        private void OnTriggerStay(Collider other)
        {
            if (currentOrderIndex >= orderSlots.Length)
            {
                return;
            }

            var ingredient = other.GetComponentInParent<KitchenIngredient>();
            if (ingredient == null || !ingredient.IsHeld)
            {
                return;
            }

            if (!IsIngredientInsideServingZone(ingredient))
            {
                return;
            }

            var instanceId = ingredient.gameObject.GetInstanceID();
            if (consumedInstanceIds.Contains(instanceId))
            {
                return;
            }

            if (!TryConsumeIngredient(ingredient))
            {
                return;
            }

            consumedInstanceIds.Add(instanceId);
        }

        private void TryConsumeHeldIngredientsInZone()
        {
            if (currentOrderIndex >= orderSlots.Length || servingDetectionZone == null)
            {
                return;
            }

            var ingredients = FindObjectsByType<KitchenIngredient>(FindObjectsSortMode.None);
            foreach (var ingredient in ingredients)
            {
                if (ingredient == null || !ingredient.IsHeld)
                {
                    continue;
                }

                if (!IsIngredientInsideServingZone(ingredient))
                {
                    continue;
                }

                var instanceId = ingredient.gameObject.GetInstanceID();
                if (consumedInstanceIds.Contains(instanceId))
                {
                    continue;
                }

                if (!TryConsumeIngredient(ingredient))
                {
                    continue;
                }

                consumedInstanceIds.Add(instanceId);
            }
        }

        private bool IsIngredientInsideServingZone(KitchenIngredient ingredient)
        {
            if (ingredient == null || servingDetectionZone == null)
            {
                return false;
            }

            var zoneBounds = servingDetectionZone.bounds;
            var ingredientColliders = ingredient.GetComponentsInChildren<Collider>(true);
            foreach (var ingredientCollider in ingredientColliders)
            {
                if (ingredientCollider == null || ingredientCollider == servingDetectionZone || !ingredientCollider.enabled)
                {
                    continue;
                }

                if (zoneBounds.Intersects(ingredientCollider.bounds))
                {
                    return true;
                }
            }

            var closestPoint = zoneBounds.ClosestPoint(ingredient.transform.position);
            return Vector3.Distance(closestPoint, ingredient.transform.position) <= 0.03f;
        }

        public bool TryConsumeIngredientDirect(KitchenIngredient ingredient)
        {
            if (ingredient == null)
            {
                return false;
            }

            var instanceId = ingredient.gameObject.GetInstanceID();
            if (consumedInstanceIds.Contains(instanceId))
            {
                return false;
            }

            if (!TryConsumeIngredient(ingredient))
            {
                return false;
            }

            consumedInstanceIds.Add(instanceId);
            return true;
        }

        public ServingOrderSnapshot GetOrderSnapshot(int index)
        {
            if (orderSlots == null || index < 0 || index >= orderSlots.Length)
            {
                return default;
            }

            var slot = orderSlots[index];
            return new ServingOrderSnapshot
            {
                OrderIndex = index,
                IsConfigured = slot.IsConfigured,
                IsCurrent = index == currentOrderIndex,
                IsCompleted = slot.IsCompleted,
                RequiredMeatCount = slot.requiredMeatCount,
                RequiredCheeseCount = slot.requiredCheeseCount,
                RequiredVegetableCount = slot.requiredVegetableCount,
                MaxMeatCount = slot.maxMeatCount,
                MaxCheeseCount = slot.maxCheeseCount,
                MaxVegetableCount = slot.maxVegetableCount,
                AcceptedMeatCount = slot.AcceptedMeatCount,
                AcceptedCheeseCount = slot.AcceptedCheeseCount,
                AcceptedVegetableCount = slot.AcceptedVegetableCount,
                AcceptedBurntMeatCount = slot.AcceptedBurntMeatCount,
                HasTopBun = slot.HasTopBun
            };
        }

        private bool TryConsumeIngredient(KitchenIngredient ingredient)
        {
            if (ingredient == null || currentOrderIndex >= orderSlots.Length)
            {
                return false;
            }

            ref var slot = ref orderSlots[currentOrderIndex];
            if (!slot.IsConfigured || slot.IsCompleted)
            {
                return false;
            }

            if (!CanAcceptIngredient(slot, ingredient))
            {
                return false;
            }

            SpawnServedVisual(slot, ingredient);
            ReleaseIngredient(ingredient);
            ingredient.gameObject.SetActive(false);
            RegisterIngredient(ref slot, ingredient);
            Destroy(ingredient.gameObject);

            if (slot.IsCompleted)
            {
                CompleteOrder(slot);
                currentOrderIndex++;
            }

            return true;
        }

        private bool CanAcceptIngredient(ServingOrderSlot slot, KitchenIngredient ingredient)
        {
            if (ingredient == null)
            {
                return false;
            }

            return ingredient.IngredientKind switch
            {
                KitchenIngredientKind.Meat => slot.AcceptedMeatCount < slot.maxMeatCount &&
                                             (ingredient.CurrentState == KitchenIngredientState.Cooked ||
                                              ingredient.CurrentState == KitchenIngredientState.Burnt),
                KitchenIngredientKind.Cheese => slot.AcceptedCheeseCount < slot.maxCheeseCount,
                KitchenIngredientKind.Vegetable => slot.AcceptedVegetableCount < slot.maxVegetableCount,
                KitchenIngredientKind.Bread => !slot.HasTopBun,
                _ => false
            };
        }

        private void ReleaseIngredient(KitchenIngredient ingredient)
        {
            var interactionBehaviour = ingredient.InteractionBehaviour;
            if (interactionBehaviour != null && interactionBehaviour.isGrasped)
            {
                foreach (var controller in interactionBehaviour.graspingControllers)
                {
                    controller?.ReleaseGrasp();
                }
            }
        }

        private void SpawnServedVisual(ServingOrderSlot slot, KitchenIngredient sourceIngredient)
        {
            var targetParent = slot.StackRoot != null ? slot.StackRoot : slot.StackAnchor;
            var stackHeight = GetStackHeight(slot, sourceIngredient);
            var verticalExtent = GetIngredientVerticalExtent(sourceIngredient);
            var targetPosition = slot.StackAnchor.position + Vector3.up * (stackHeight + verticalExtent);
            var targetRotation = GetServingRotation(slot, sourceIngredient);
            var targetScale = sourceIngredient.transform.lossyScale;

            var servedObject = Instantiate(sourceIngredient.gameObject, targetPosition, targetRotation);
            servedObject.transform.SetParent(targetParent, true);
            ApplyWorldScale(servedObject.transform, targetScale);

            var servedIngredient = servedObject.GetComponent<KitchenIngredient>();
            if (servedIngredient != null)
            {
                servedIngredient.enabled = false;
            }

            var rigidbodies = servedObject.GetComponentsInChildren<Rigidbody>(true);
            foreach (var rigidbody in rigidbodies)
            {
                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.isKinematic = true;
                rigidbody.detectCollisions = false;
            }

            var colliders = servedObject.GetComponentsInChildren<Collider>(true);
            foreach (var servedCollider in colliders)
            {
                servedCollider.enabled = false;
            }

            var interactionBehaviours = servedObject.GetComponentsInChildren<InteractionBehaviour>(true);
            foreach (var interactionBehaviour in interactionBehaviours)
            {
                interactionBehaviour.enabled = false;
            }
        }

        private static void ApplyWorldScale(Transform target, Vector3 desiredWorldScale)
        {
            if (target == null)
            {
                return;
            }

            var parentScale = target.parent != null ? target.parent.lossyScale : Vector3.one;
            target.localScale = new Vector3(
                SafeDivide(desiredWorldScale.x, parentScale.x),
                SafeDivide(desiredWorldScale.y, parentScale.y),
                SafeDivide(desiredWorldScale.z, parentScale.z));
        }

        private static float SafeDivide(float value, float divisor)
        {
            return Mathf.Abs(divisor) > 0.0001f ? value / divisor : value;
        }

        private static float GetIngredientVerticalExtent(KitchenIngredient ingredient)
        {
            var collider = ingredient.GetComponent<Collider>();
            if (collider == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, collider.bounds.extents.y);
        }

        private static Quaternion GetServingRotation(ServingOrderSlot slot, KitchenIngredient ingredient)
        {
            var euler = slot.StackAnchor.rotation.eulerAngles;
            return ingredient.IngredientKind switch
            {
                KitchenIngredientKind.Cheese => Quaternion.Euler(0f, euler.y, 0f),
                KitchenIngredientKind.Vegetable => Quaternion.Euler(0f, euler.y, 0f),
                _ => slot.StackAnchor.rotation
            };
        }

        private float GetStackHeight(ServingOrderSlot slot, KitchenIngredient ingredient)
        {
            var height = slot.AcceptedLayerCount * stackHeightPerLayer;

            if (ingredient.IngredientKind == KitchenIngredientKind.Bread)
            {
                height += topBunHeightBonus;
            }

            return height;
        }

        private void RegisterIngredient(ref ServingOrderSlot slot, KitchenIngredient ingredient)
        {
            slot.AcceptedLayerCount++;

            switch (ingredient.IngredientKind)
            {
                case KitchenIngredientKind.Meat:
                    slot.AcceptedMeatCount++;
                    if (ingredient.CurrentState == KitchenIngredientState.Burnt)
                    {
                        slot.AcceptedBurntMeatCount++;
                    }
                    break;
                case KitchenIngredientKind.Cheese:
                    slot.AcceptedCheeseCount++;
                    break;
                case KitchenIngredientKind.Vegetable:
                    slot.AcceptedVegetableCount++;
                    break;
                case KitchenIngredientKind.Bread:
                    slot.HasTopBun = true;
                    break;
            }
        }

        private void CompleteOrder(ServingOrderSlot slot)
        {
            var result = EvaluateOrder(slot);
            OrderCompleted?.Invoke(result.AwardedScore, result);

            if (gameManager != null)
            {
                gameManager.AddScore(result.AwardedScore, result.FeedbackMessage);
            }
        }

        private ServingOrderResult EvaluateOrder(ServingOrderSlot slot)
        {
            var mismatchCount =
                Mathf.Abs(slot.AcceptedMeatCount - slot.requiredMeatCount) +
                Mathf.Abs(slot.AcceptedCheeseCount - slot.requiredCheeseCount) +
                Mathf.Abs(slot.AcceptedVegetableCount - slot.requiredVegetableCount);

            var awardedScore = completedOrderBaseScore;
            awardedScore -= mismatchCount * ingredientMismatchPenalty;
            awardedScore -= slot.AcceptedBurntMeatCount * burntMeatPenalty;
            awardedScore = Mathf.Max(minimumCompletedOrderScore, awardedScore);

            var feedback = mismatchCount == 0 && slot.AcceptedBurntMeatCount == 0
                ? $"Order {currentOrderIndex + 1} complete (+{awardedScore})"
                : $"Order {currentOrderIndex + 1} complete with penalties (+{awardedScore})";

            return new ServingOrderResult
            {
                OrderIndex = currentOrderIndex,
                AwardedScore = awardedScore,
                MismatchCount = mismatchCount,
                BurntMeatCount = slot.AcceptedBurntMeatCount,
                FeedbackMessage = feedback
            };
        }

        [Serializable]
        public struct ServingOrderSlot
        {
            [SerializeField]
            private Transform stackAnchor;

            [SerializeField]
            private Transform stackRoot;

            [Min(0)]
            public int requiredMeatCount;

            [Min(0)]
            public int requiredCheeseCount;

            [Min(0)]
            public int requiredVegetableCount;

            [Min(0)]
            public int maxMeatCount;

            [Min(0)]
            public int maxCheeseCount;

            [Min(0)]
            public int maxVegetableCount;

            [NonSerialized]
            public int AcceptedMeatCount;

            [NonSerialized]
            public int AcceptedCheeseCount;

            [NonSerialized]
            public int AcceptedVegetableCount;

            [NonSerialized]
            public int AcceptedBurntMeatCount;

            [NonSerialized]
            public int AcceptedLayerCount;

            [NonSerialized]
            public bool HasTopBun;

            public Transform StackAnchor => stackAnchor;
            public Transform StackRoot => stackRoot;
            public bool IsConfigured => stackAnchor != null;
            public bool IsCompleted => HasTopBun;
        }

        public struct ServingOrderResult
        {
            public int OrderIndex;
            public int AwardedScore;
            public int MismatchCount;
            public int BurntMeatCount;
            public string FeedbackMessage;
        }

        public struct ServingOrderSnapshot
        {
            public int OrderIndex;
            public bool IsConfigured;
            public bool IsCurrent;
            public bool IsCompleted;
            public int RequiredMeatCount;
            public int RequiredCheeseCount;
            public int RequiredVegetableCount;
            public int MaxMeatCount;
            public int MaxCheeseCount;
            public int MaxVegetableCount;
            public int AcceptedMeatCount;
            public int AcceptedCheeseCount;
            public int AcceptedVegetableCount;
            public int AcceptedBurntMeatCount;
            public bool HasTopBun;
        }
    }
}
