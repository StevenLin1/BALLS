using System;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Interaction;

namespace KitchenGame
{
    public class KitchenIngredient : KitchenItem
    {
        [SerializeField]
        private KitchenIngredientKind ingredientKind = KitchenIngredientKind.None;

        [SerializeField]
        private KitchenIngredientState currentState = KitchenIngredientState.Raw;

        [SerializeField]
        private int chopsRequired = 3;

        [SerializeField]
        private float cookStepsRequired = 4f;

        [SerializeField]
        private float burnStepsThreshold = 6f;

        [Header("Slicing")]
        [SerializeField]
        private bool sliceable;

        [SerializeField]
        private GameObject slicedPrefab;

        [SerializeField]
        private int slicedPieceCount = 2;

        [SerializeField]
        private float sliceDurationSeconds = 2.5f;

        [SerializeField]
        private Transform sliceSpawnOrigin;

        [SerializeField]
        private float sliceScatterRadius = 0.08f;

        [Header("Stove Cooking")]
        [SerializeField]
        private bool dualSidedCooking;

        [SerializeField]
        private float sideCookTargetSeconds = 4f;

        [SerializeField]
        private float sideBurnSeconds = 7f;

        private int completedChops;
        private float cookProgress;
        private float sliceProgress;
        private Chirality activeSlicerHand;
        private int activeCookSide;
        private readonly float[] sideCookTimes = new float[2];

        public event Action<KitchenIngredient, KitchenIngredientState, KitchenIngredientState> StateChanged;

        public KitchenIngredientKind IngredientKind => ingredientKind;
        public KitchenIngredientState CurrentState => currentState;
        public int CompletedChops => completedChops;
        public float CookProgress => cookProgress;
        public bool Sliceable => sliceable;
        public bool SupportsDualSidedCooking => dualSidedCooking;
        public float SliceProgress01 => sliceDurationSeconds > 0.01f ? Mathf.Clamp01(sliceProgress / sliceDurationSeconds) : 1f;
        public float SideACookTime => sideCookTimes[0];
        public float SideBCookTime => sideCookTimes[1];
        public int ActiveCookSide => activeCookSide;
        public bool IsReadyForServing => currentState == KitchenIngredientState.Cooked;

        public bool ApplyChop(float intensity)
        {
            if (currentState == KitchenIngredientState.Burnt || currentState == KitchenIngredientState.Plated)
            {
                return false;
            }

            if (currentState != KitchenIngredientState.Raw)
            {
                return false;
            }

            completedChops += Mathf.Max(1, Mathf.RoundToInt(intensity));
            if (completedChops >= chopsRequired)
            {
                SetState(KitchenIngredientState.Chopped);
            }

            return true;
        }

        public bool ApplySliceStep(Chirality toolHand, float deltaTime, out bool completed)
        {
            completed = false;

            if (!sliceable || slicedPrefab == null || currentState != KitchenIngredientState.Raw)
            {
                ResetSliceProgress();
                return false;
            }

            var ingredientHand = GetHeldHandOpposite(toolHand);
            if (ingredientHand == null)
            {
                ResetSliceProgress();
                return false;
            }

            if (activeSlicerHand != toolHand)
            {
                activeSlicerHand = toolHand;
                sliceProgress = 0f;
            }

            sliceProgress += Mathf.Max(0.01f, deltaTime);
            if (sliceProgress < sliceDurationSeconds)
            {
                return true;
            }

            completed = CompleteSlice();
            return completed;
        }

        public void ResetSliceProgress()
        {
            sliceProgress = 0f;
        }

        public bool ApplyCookingPulse(float intensity)
        {
            if (currentState == KitchenIngredientState.Burnt || currentState == KitchenIngredientState.Plated)
            {
                return false;
            }

            if (currentState != KitchenIngredientState.Raw &&
                currentState != KitchenIngredientState.Chopped &&
                currentState != KitchenIngredientState.Mixed &&
                currentState != KitchenIngredientState.Cooked)
            {
                return false;
            }

            cookProgress += Mathf.Max(0.25f, intensity);

            if (cookProgress >= burnStepsThreshold)
            {
                SetState(KitchenIngredientState.Burnt);
                return true;
            }

            if (cookProgress >= cookStepsRequired)
            {
                SetState(KitchenIngredientState.Cooked);
            }

            return true;
        }

        public void AdvanceCooking(float deltaTime)
        {
            if (!dualSidedCooking)
            {
                ApplyCookingPulse(deltaTime);
                return;
            }

            if (currentState == KitchenIngredientState.Burnt || currentState == KitchenIngredientState.Plated)
            {
                return;
            }

            sideCookTimes[activeCookSide] += Mathf.Max(0f, deltaTime);
            cookProgress = sideCookTimes[0] + sideCookTimes[1];

            if (sideCookTimes[activeCookSide] >= sideBurnSeconds)
            {
                SetState(KitchenIngredientState.Burnt);
                return;
            }

            if (sideCookTimes[0] >= sideCookTargetSeconds && sideCookTimes[1] >= sideCookTargetSeconds)
            {
                SetState(KitchenIngredientState.Cooked);
            }
        }

        public bool FlipCookingSide()
        {
            if (!dualSidedCooking || currentState == KitchenIngredientState.Burnt)
            {
                return false;
            }

            activeCookSide = 1 - activeCookSide;
            return true;
        }

        public float GetCookQuality01()
        {
            if (!dualSidedCooking)
            {
                return currentState == KitchenIngredientState.Cooked ? 1f : 0f;
            }

            if (currentState == KitchenIngredientState.Burnt)
            {
                return 0.15f;
            }

            var a = Mathf.Clamp01(sideCookTimes[0] / sideCookTargetSeconds);
            var b = Mathf.Clamp01(sideCookTimes[1] / sideCookTargetSeconds);
            return Mathf.Min(a, b);
        }

        public bool Mix()
        {
            if (currentState != KitchenIngredientState.Raw && currentState != KitchenIngredientState.Chopped)
            {
                return false;
            }

            SetState(KitchenIngredientState.Mixed);
            return true;
        }

        public bool Plate()
        {
            if (currentState != KitchenIngredientState.Cooked && currentState != KitchenIngredientState.Mixed)
            {
                return false;
            }

            SetState(KitchenIngredientState.Plated);
            return true;
        }

        public void ForceState(KitchenIngredientState newState)
        {
            SetState(newState);
        }

        private Chirality? GetHeldHandOpposite(Chirality toolHand)
        {
            var opposite = toolHand == Chirality.Left ? Chirality.Right : Chirality.Left;
            return IsHeldBy(opposite) ? opposite : null;
        }

        private bool CompleteSlice()
        {
            SetState(KitchenIngredientState.Chopped);

            if (slicedPrefab != null)
            {
                var origin = sliceSpawnOrigin != null ? sliceSpawnOrigin.position : transform.position;

                for (var i = 0; i < Mathf.Max(1, slicedPieceCount); i++)
                {
                    var offset = UnityEngine.Random.insideUnitSphere * sliceScatterRadius;
                    offset.y = Mathf.Abs(offset.y) * 0.25f;
                    var spawnedSlice = Instantiate(slicedPrefab, origin + offset, transform.rotation);
                    ConfigureSpawnedInteractions(spawnedSlice);
                }
            }

            gameObject.SetActive(false);
            return true;
        }

        private static void ConfigureSpawnedInteractions(GameObject spawnedObject)
        {
            if (spawnedObject == null)
            {
                return;
            }

            var interactionManager = FindFirstObjectByType<InteractionManager>();
            if (interactionManager == null)
            {
                return;
            }

            var interactionBehaviours = spawnedObject.GetComponentsInChildren<InteractionBehaviour>(true);
            foreach (var interactionBehaviour in interactionBehaviours)
            {
                if (interactionBehaviour != null)
                {
                    interactionBehaviour.manager = interactionManager;
                }
            }
        }

        private void SetState(KitchenIngredientState newState)
        {
            if (newState == currentState)
            {
                return;
            }

            var previous = currentState;
            currentState = newState;
            StateChanged?.Invoke(this, previous, currentState);
        }
    }
}
