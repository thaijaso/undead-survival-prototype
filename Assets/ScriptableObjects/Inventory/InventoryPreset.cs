using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivalGame.Gameplay
{
    [CreateAssetMenu(fileName = "InventoryPreset", menuName = "ScriptableObjects/InventoryPreset")]
    public class InventoryPreset : ScriptableObject
    {
        public List<ItemStack> startingItems = new();
    }
}
