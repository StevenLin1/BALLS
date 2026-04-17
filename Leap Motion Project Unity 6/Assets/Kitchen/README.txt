Kitchen prototype wiring

1. Use Assets/Scenes/Simple Desktop Interaction.unity as the base scene.
2. Create station trigger volumes and add KitchenStation:
   - Cutting board
   - Stove
   - Prep bowl
   - Serving
3. Add KitchenTool to pan / knife / spatula prefabs and keep InteractionBehaviour on each tool.
4. Add KitchenIngredient to ingredients placed on the counter or station triggers.
5. Create a KitchenRecipe asset from Create > Kitchen > Recipe.
6. Add KitchenGameManager and KitchenGestureTracker to a bootstrap object, assign the recipe and LeapProvider.
7. Optionally add KitchenHudPresenter and wire TMP labels for timer / score / current step.

Gesture defaults

- Hold knife and move downward fast over the cutting board: Chop
- Move hand in short horizontal circles over bowl or stove: Stir
- Hold pan and flick upward over stove: Flip
- Pinch over serving station: Plate
