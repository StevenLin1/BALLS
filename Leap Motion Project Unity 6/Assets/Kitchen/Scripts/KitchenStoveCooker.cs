using UnityEngine;
using Leap.Unity;

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
        private float scoreForCookedMeat = 150f;

        [SerializeField]
        private float scoreForBurntMeat = 25f;

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
        }

        private void Update()
        {
            if (station == null || !station.HasTool(requiredCookware))
            {
                return;
            }

            foreach (var ingredient in station.GetOccupants<KitchenIngredient>())
            {
                if (ingredient == null || !ingredient.SupportsDualSidedCooking)
                {
                    continue;
                }

                ingredient.AdvanceCooking(Time.deltaTime);
            }
        }

        public bool TryHandleCookGesture(KitchenGestureType gesture, KitchenItem heldItem, Chirality chirality, out KitchenIngredient ingredient,
            out KitchenIngredientState stateBefore, out KitchenIngredientState stateAfter, out int scoreAward)
        {
            ingredient = station.GetPrimaryIngredient();
            stateBefore = ingredient != null ? ingredient.CurrentState : KitchenIngredientState.None;
            stateAfter = stateBefore;
            scoreAward = 0;

            if (gesture != KitchenGestureType.Flip)
            {
                return false;
            }

            if (heldItem is not KitchenTool tool || tool.ToolType != KitchenToolType.Spatula)
            {
                return false;
            }

            ingredient = station.GetNearestIngredient(station.ActionPosition, heldOnly: false, requireDualSidedCooking: true);
            if (ingredient == null)
            {
                return false;
            }

            if (!ingredient.FlipCookingSide())
            {
                return false;
            }

            stateAfter = ingredient.CurrentState;

            if (stateBefore != stateAfter)
            {
                scoreAward = stateAfter == KitchenIngredientState.Cooked
                    ? Mathf.RoundToInt(scoreForCookedMeat * ingredient.GetCookQuality01())
                    : Mathf.RoundToInt(scoreForBurntMeat);
            }

            return true;
        }
    }
}
