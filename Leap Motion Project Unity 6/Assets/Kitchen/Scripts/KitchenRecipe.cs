using System;
using System.Collections.Generic;
using UnityEngine;

namespace KitchenGame
{
    [CreateAssetMenu(menuName = "Kitchen/Recipe", fileName = "KitchenRecipe")]
    public class KitchenRecipe : ScriptableObject
    {
        [SerializeField]
        private string recipeName = "Kitchen Recipe";

        [SerializeField]
        private float roundDurationSeconds = 180f;

        [SerializeField]
        private int completionBonus = 500;

        [SerializeField]
        private int mistakePenalty = 25;

        [SerializeField]
        private List<KitchenRecipeStep> steps = new();

        public string RecipeName => recipeName;
        public float RoundDurationSeconds => roundDurationSeconds;
        public int CompletionBonus => completionBonus;
        public int MistakePenalty => mistakePenalty;
        public IReadOnlyList<KitchenRecipeStep> Steps => steps;
    }

    [Serializable]
    public class KitchenRecipeStep
    {
        [TextArea]
        public string instruction = "Do the next kitchen action";

        public KitchenStationType requiredStation = KitchenStationType.None;
        public KitchenGestureType requiredGesture = KitchenGestureType.None;
        public KitchenToolType requiredTool = KitchenToolType.None;
        public KitchenIngredientKind targetIngredient = KitchenIngredientKind.None;
        public KitchenIngredientState requiredStateBefore = KitchenIngredientState.None;
        public KitchenIngredientState requiredStateAfter = KitchenIngredientState.None;
        public int baseScore = 100;
        public int techniqueBonus = 50;
        public float idealIntensityMin = 0.75f;
        public float idealIntensityMax = 1.75f;
    }
}
