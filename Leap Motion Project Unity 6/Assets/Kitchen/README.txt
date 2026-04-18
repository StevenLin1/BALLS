Kitchen prototype wiring

1. Use Assets/Scenes/Simple Desktop Interaction.unity as the base scene.
2. Create station trigger volumes and add KitchenStation:
   - CuttingBoard for the board area
   - Stove for the pan heat zone
   - Toaster for the toaster waving zone
   - Serving for the burger box area
3. Add KitchenTool to pan / knife / spatula prefabs and keep InteractionBehaviour on each tool.
4. Add KitchenIngredient to every movable ingredient that can be picked up by hand.
5. For carrot and cheese, enable slicing on KitchenIngredient and assign the sliced output prefab.
6. For meat, enable dual-sided cooking on KitchenIngredient and place it in the Stove trigger while the pan is also in the trigger.
7. Add KitchenToasterStation to the toaster zone and assign bun spawn points.
8. Add KitchenStoveCooker to the stove zone.
9. Add KitchenGameManager and KitchenGestureTracker to a bootstrap object, assign the LeapProvider.

Gesture defaults

- Hold knife in one hand and the ingredient in the other, then keep chopping over the cutting board: Slice progress
- Wave an empty hand over the toaster station: Start bun batch
- Hold spatula and flick upward over the stove: Flip meat
- Pinch over serving station: Plate
