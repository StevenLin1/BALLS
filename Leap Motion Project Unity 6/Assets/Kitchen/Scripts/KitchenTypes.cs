using System;
using Leap.Unity;

namespace KitchenGame
{
    public enum KitchenToolType
    {
        None,
        Pan,
        Knife,
        Spatula,
        Bowl,
        Plate
    }

    public enum KitchenIngredientKind
    {
        None,
        Vegetable,
        Meat,
        Fish,
        Sauce,
        Seasoning,
        Dough
    }

    public enum KitchenIngredientState
    {
        None,
        Raw,
        Chopped,
        Mixed,
        Cooked,
        Burnt,
        Plated
    }

    public enum KitchenStationType
    {
        None,
        Counter,
        CuttingBoard,
        Stove,
        Toaster,
        PrepBowl,
        Serving
    }

    public enum KitchenGestureType
    {
        None,
        Grab,
        Pinch,
        Chop,
        Stir,
        Flip,
        Place
    }

    [Serializable]
    public struct KitchenGestureContext
    {
        public Chirality Chirality;
        public KitchenGestureType Gesture;
        public KitchenStation Station;
        public KitchenItem HeldItem;
        public KitchenIngredient Ingredient;
        public KitchenIngredientState IngredientStateBefore;
        public KitchenIngredientState IngredientStateAfter;
        public float Intensity;
    }
}
