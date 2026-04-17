using TMPro;
using UnityEngine;

namespace KitchenGame
{
    public class KitchenHudPresenter : MonoBehaviour
    {
        [SerializeField]
        private KitchenGameManager gameManager;

        [SerializeField]
        private TMP_Text recipeLabel;

        [SerializeField]
        private TMP_Text timerLabel;

        [SerializeField]
        private TMP_Text scoreLabel;

        [SerializeField]
        private TMP_Text stepLabel;

        [SerializeField]
        private TMP_Text feedbackLabel;

        private void Reset()
        {
            gameManager = FindFirstObjectByType<KitchenGameManager>();
        }

        private void OnEnable()
        {
            if (gameManager == null)
            {
                return;
            }

            gameManager.ScoreChanged += UpdateScore;
            gameManager.TimeChanged += UpdateTime;
            gameManager.StepChanged += UpdateStep;
            gameManager.FeedbackIssued += UpdateFeedback;
            gameManager.RoundEnded += HandleRoundEnded;

            recipeLabel?.SetText(gameManager.ActiveRecipe != null ? gameManager.ActiveRecipe.RecipeName : "Kitchen Freeplay");
            UpdateScore(gameManager.Score);
            UpdateTime(gameManager.TimeRemaining);
            UpdateStep(gameManager.CurrentStep, gameManager.CurrentStepIndex, gameManager.ActiveRecipe != null ? gameManager.ActiveRecipe.Steps.Count : 0);
        }

        private void OnDisable()
        {
            if (gameManager == null)
            {
                return;
            }

            gameManager.ScoreChanged -= UpdateScore;
            gameManager.TimeChanged -= UpdateTime;
            gameManager.StepChanged -= UpdateStep;
            gameManager.FeedbackIssued -= UpdateFeedback;
            gameManager.RoundEnded -= HandleRoundEnded;
        }

        private void UpdateScore(int value)
        {
            scoreLabel?.SetText($"Score {value}");
        }

        private void UpdateTime(float value)
        {
            var seconds = Mathf.CeilToInt(value);
            timerLabel?.SetText($"Time {seconds}");
        }

        private void UpdateStep(KitchenRecipeStep step, int stepIndex, int totalSteps)
        {
            if (step == null)
            {
                stepLabel?.SetText("All steps complete");
                return;
            }

            stepLabel?.SetText($"Step {stepIndex + 1}/{totalSteps}: {step.instruction}");
        }

        private void UpdateFeedback(string message)
        {
            feedbackLabel?.SetText(message);
        }

        private void HandleRoundEnded(bool success, int finalScore)
        {
            feedbackLabel?.SetText(success ? $"Service complete. Final {finalScore}" : $"Time up. Final {finalScore}");
        }
    }
}
