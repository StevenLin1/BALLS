using UnityEngine;

namespace KitchenGame
{
    public class KitchenTool : KitchenItem
    {
        [SerializeField]
        private KitchenToolType toolType = KitchenToolType.None;

        public KitchenToolType ToolType => toolType;
    }
}
