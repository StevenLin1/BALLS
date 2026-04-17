using System;
using UnityEngine;

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

        private int completedChops;
        private float cookProgress;

        public event Action<KitchenIngredient, KitchenIngredientState, KitchenIngredientState> StateChanged;

        public KitchenIngredientKind IngredientKind => ingredientKind;
        public KitchenIngredientState CurrentState => currentState;
        public int CompletedChops => completedChops;
        public float CookProgress => cookProgress;

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
