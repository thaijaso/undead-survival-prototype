using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryPreset", menuName = "ScriptableObjects/InventoryPreset")]
public class InventoryPreset : ScriptableObject
{
    public List<ItemStack> startingItems = new();
}
