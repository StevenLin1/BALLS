using System;
using UnityEngine;

namespace KitchenGame
{
    public class KitchenGameManager : MonoBehaviour
    {
        [SerializeField]
        private KitchenRecipe activeRecipe;

        [SerializeField]
        private bool autoStartOnEnable = true;

        [SerializeField]
        private bool allowFreeplayWithoutRecipe = true;

        [SerializeField]
        private KitchenStation[] stations;

        private int currentStepIndex;
        private float timeRemaining;
        private int score;
        private int combo;
        private bool roundActive;

        public event Action<int> ScoreChanged;
        public event Action<float> TimeChanged;
        public event Action<KitchenRecipeStep, int, int> StepChanged;
        public event Action<string> FeedbackIssued;
        public event Action<bool, int> RoundEnded;

        public KitchenRecipe ActiveRecipe => activeRecipe;
        public float TimeRemaining => timeRemaining;
        public int Score => score;
        public int CurrentStepIndex => currentStepIndex;
        public bool RoundActive => roundActive;

        public KitchenRecipeStep CurrentStep
        {
            get
            {
                if (activeRecipe == null || currentStepIndex < 0 || currentStepIndex >= activeRecipe.Steps.Count)
                {
                    return null;
                }

                return activeRecipe.Steps[currentStepIndex];
            }
        }

        private void OnEnable()
        {
            if (autoStartOnEnable)
            {
                StartRound();
            }
        }

        private void Update()
        {
            if (!roundActive)
            {
                return;
            }

            timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
            TimeChanged?.Invoke(timeRemaining);

            if (timeRemaining <= 0f)
            {
                EndRound(false);
            }
        }

        public void StartRound()
        {
            currentStepIndex = 0;
            score = 0;
            combo = 0;
            timeRemaining = activeRecipe != null ? activeRecipe.RoundDurationSeconds : 180f;
            roundActive = true;

            ScoreChanged?.Invoke(score);
            TimeChanged?.Invoke(timeRemaining);
            StepChanged?.Invoke(CurrentStep, currentStepIndex, activeRecipe != null ? activeRecipe.Steps.Count : 0);
            FeedbackIssued?.Invoke(activeRecipe != null ? activeRecipe.RecipeName : "Kitchen Freeplay");
        }

        public void RegisterGesture(KitchenGestureType gesture, Vector3 worldPosition, KitchenItem heldItem, float intensity)
        {
            if (!roundActive)
            {
                return;
            }

            var station = FindNearestStation(worldPosition);
            if (station == null)
            {
                return;
            }

            if (!station.TryApplyGesture(gesture, heldItem, intensity, out var ingredient, out var stateBefore, out var stateAfter))
            {
                Penalize("Gesture did not hit a valid kitchen target");
                return;
            }

            var context = new KitchenGestureContext
            {
                Gesture = gesture,
                Station = station,
                HeldItem = heldItem,
                Ingredient = ingredient,
                IngredientStateBefore = stateBefore,
                IngredientStateAfter = stateAfter,
                Intensity = intensity
            };

            if (activeRecipe == null)
            {
                if (allowFreeplayWithoutRecipe)
                {
                    AwardFreeplayPoints(context);
                }

                return;
            }

            if (!DoesCurrentStepMatch(context))
            {
                Penalize($"Wrong step. Expected: {CurrentStep?.instruction}");
                return;
            }

            AwardStepPoints(context);
            AdvanceStep();
        }

        public KitchenStation FindNearestStation(Vector3 worldPosition)
        {
            if (stations == null || stations.Length == 0)
            {
                stations = FindObjectsByType<KitchenStation>(FindObjectsSortMode.None);
            }

            KitchenStation nearest = null;
            var nearestDistance = float.PositiveInfinity;

            foreach (var station in stations)
            {
                if (station == null || !station.IsInRange(worldPosition))
                {
                    continue;
                }

                var distance = Vector3.Distance(station.ActionPosition, worldPosition);
                if (distance < nearestDistance)
                {
                    nearest = station;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private bool DoesCurrentStepMatch(KitchenGestureContext context)
        {
            var step = CurrentStep;
            if (step == null)
            {
                return false;
            }

            if (step.requiredStation != KitchenStationType.None && context.Station.StationType != step.requiredStation)
            {
                return false;
            }

            if (step.requiredGesture != KitchenGestureType.None && context.Gesture != step.requiredGesture)
            {
                return false;
            }

            if (step.requiredTool != KitchenToolType.None)
            {
                if (context.HeldItem is not KitchenTool tool || tool.ToolType != step.requiredTool)
                {
                    return false;
                }
            }

            if (step.targetIngredient != KitchenIngredientKind.None)
            {
                if (context.Ingredient == null || context.Ingredient.IngredientKind != step.targetIngredient)
                {
                    return false;
                }
            }

            if (step.requiredStateBefore != KitchenIngredientState.None &&
                context.IngredientStateBefore != step.requiredStateBefore)
            {
                return false;
            }

            if (step.requiredStateAfter != KitchenIngredientState.None &&
                context.IngredientStateAfter != step.requiredStateAfter)
            {
                return false;
            }

            return true;
        }

        private void AwardFreeplayPoints(KitchenGestureContext context)
        {
            var awarded = 15 + Mathf.RoundToInt(context.Intensity * 10f);
            score += awarded;
            ScoreChanged?.Invoke(score);
            FeedbackIssued?.Invoke($"{context.Gesture} +{awarded}");
        }

        private void AwardStepPoints(KitchenGestureContext context)
        {
            var step = CurrentStep;
            var timeFactor = activeRecipe.RoundDurationSeconds > 0.01f ? timeRemaining / activeRecipe.RoundDurationSeconds : 0f;
            var awarded = step.baseScore + Mathf.RoundToInt(timeFactor * 100f);

            if (context.Intensity >= step.idealIntensityMin && context.Intensity <= step.idealIntensityMax)
            {
                awarded += step.techniqueBonus;
                combo++;
            }
            else
            {
                combo = 0;
            }

            awarded += combo * 10;
            score += awarded;
            ScoreChanged?.Invoke(score);
            FeedbackIssued?.Invoke($"Step complete: {step.instruction} (+{awarded})");
        }

        private void Penalize(string message)
        {
            combo = 0;

            if (activeRecipe != null)
            {
                score = Mathf.Max(0, score - activeRecipe.MistakePenalty);
                ScoreChanged?.Invoke(score);
            }

            FeedbackIssued?.Invoke(message);
        }

        private void AdvanceStep()
        {
            currentStepIndex++;

            if (activeRecipe == null || currentStepIndex >= activeRecipe.Steps.Count)
            {
                if (activeRecipe != null)
                {
                    score += activeRecipe.CompletionBonus + Mathf.RoundToInt(timeRemaining * 5f);
                    ScoreChanged?.Invoke(score);
                }

                EndRound(true);
                return;
            }

            StepChanged?.Invoke(CurrentStep, currentStepIndex, activeRecipe.Steps.Count);
        }

        private void EndRound(bool success)
        {
            roundActive = false;
            RoundEnded?.Invoke(success, score);
        }
    }
}
