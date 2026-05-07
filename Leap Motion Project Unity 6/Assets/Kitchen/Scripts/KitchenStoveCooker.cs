using UnityEngine;
using Leap.Unity;
using System.Collections.Generic;

namespace KitchenGame
{
    [RequireComponent(typeof(KitchenStation))]
    public class KitchenStoveCooker : MonoBehaviour
    {
        [SerializeField]
        private KitchenStation station;

        [SerializeField]
        private KitchenToolType requiredCookware = KitchenToolType.Pan;

        [SerializeField]
        private KitchenServingAssembly servingAssembly;

        [SerializeField]
        private float idealCookSeconds = 7f;

        [SerializeField]
        private float burnSeconds = 10f;

        [SerializeField]
        private float serveTransferCooldownSeconds = 0.25f;

        [SerializeField]
        private float cookingDetectionRadius = 0.28f;

        private readonly Dictionary<KitchenIngredient, float> cookTimes = new();
        private float nextAllowedServeTransferTime;

        private void Reset()
        {
            station = GetComponent<KitchenStation>();
        }

        private void Awake()
        {
            if (station == null)
            {
                station = GetComponent<KitchenStation>();
            }

            if (servingAssembly == null)
            {
                servingAssembly = FindFirstObjectByType<KitchenServingAssembly>();
            }
        }

        private void Update()
        {
            if (station == null || !HasPanInCookingZone())
            {
                cookTimes.Clear();
                return;
            }

            foreach (var ingredient in FindIngredientsInCookingZone())
            {
                if (ingredient == null || ingredient.IngredientKind != KitchenIngredientKind.Meat)
                {
                    continue;
                }

                AdvanceMeatCooking(ingredient, Time.deltaTime);
            }

            CleanupMissingIngredients();
        }

        public bool TryHandleCookGesture(KitchenGestureType gesture, KitchenItem heldItem, Chirality chirality, out KitchenIngredient ingredient,
            out KitchenIngredientState stateBefore, out KitchenIngredientState stateAfter, out int scoreAward)
        {
            ingredient = station.GetPrimaryIngredient();
            stateBefore = ingredient != null ? ingredient.CurrentState : KitchenIngredientState.None;
            stateAfter = stateBefore;
            scoreAward = 0;
            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            TryServeCookedMeat(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryServeCookedMeat(other);
        }

        private void AdvanceMeatCooking(KitchenIngredient ingredient, float deltaTime)
        {
            if (ingredient.CurrentState == KitchenIngredientState.Burnt || ingredient.CurrentState == KitchenIngredientState.Plated)
            {
                return;
            }

            if (!cookTimes.TryGetValue(ingredient, out var elapsed))
            {
                elapsed = 0f;
            }

            elapsed += Mathf.Max(0f, deltaTime);
            cookTimes[ingredient] = elapsed;

            if (elapsed >= burnSeconds)
            {
                ingredient.ForceState(KitchenIngredientState.Burnt);
            }
            else if (elapsed >= idealCookSeconds)
            {
                ingredient.ForceState(KitchenIngredientState.Cooked);
            }
        }

        private void CleanupMissingIngredients()
        {
            var keysToRemove = new List<KitchenIngredient>();

            foreach (var pair in cookTimes)
            {
                if (pair.Key == null)
                {
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (var ingredient in keysToRemove)
            {
                cookTimes.Remove(ingredient);
            }
        }

        private void TryServeCookedMeat(Collider other)
        {
            if (Time.time < nextAllowedServeTransferTime || servingAssembly == null || other == null)
            {
                return;
            }

            var tool = other.GetComponentInParent<KitchenTool>();
            if (tool == null || tool.ToolType != KitchenToolType.Spatula)
            {
                return;
            }

            var ingredient = FindNearestCookedIngredientInZone();
            if (ingredient == null || ingredient.IngredientKind != KitchenIngredientKind.Meat)
            {
                return;
            }

            if (ingredient.CurrentState != KitchenIngredientState.Cooked &&
                ingredient.CurrentState != KitchenIngredientState.Burnt)
            {
                return;
            }

            if (!servingAssembly.TryConsumeIngredientDirect(ingredient))
            {
                return;
            }

            cookTimes.Remove(ingredient);
            nextAllowedServeTransferTime = Time.time + serveTransferCooldownSeconds;
        }

        private bool HasPanInCookingZone()
        {
            if (station.HasTool(requiredCookware))
            {
                return true;
            }

            var tools = FindObjectsByType<KitchenTool>(FindObjectsSortMode.None);
            foreach (var tool in tools)
            {
                if (tool == null || tool.ToolType != requiredCookware)
                {
                    continue;
                }

                if (Vector3.Distance(tool.transform.position, station.ActionPosition) <= cookingDetectionRadius)
                {
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<KitchenIngredient> FindIngredientsInCookingZone()
        {
            var ingredients = FindObjectsByType<KitchenIngredient>(FindObjectsSortMode.None);
            foreach (var ingredient in ingredients)
            {
                if (ingredient == null || ingredient.IngredientKind != KitchenIngredientKind.Meat)
                {
                    continue;
                }

                if (Vector3.Distance(ingredient.transform.position, station.ActionPosition) <= cookingDetectionRadius)
                {
                    yield return ingredient;
                }
            }
        }

        private KitchenIngredient FindNearestCookedIngredientInZone()
        {
            KitchenIngredient best = null;
            var bestDistance = float.PositiveInfinity;

            foreach (var ingredient in FindIngredientsInCookingZone())
            {
                if (ingredient.CurrentState != KitchenIngredientState.Cooked &&
                    ingredient.CurrentState != KitchenIngredientState.Burnt)
                {
                    continue;
                }

                var distance = Vector3.Distance(ingredient.transform.position, station.ActionPosition);
                if (distance < bestDistance)
                {
                    best = ingredient;
                    bestDistance = distance;
                }
            }

            return best;
        }
    }
}
