using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class InventoryData
{
    public List<InventoryItemData> items = new List<InventoryItemData>();
}

[System.Serializable]
public class InventoryItemData
{
    public string itemName;
    public int stackCount;
    public int slotIndex; // Только для обычных слотов
    public string slotType; // "inventory", "helmet", "armor", "weapon"

    public InventoryItemData(string itemName, int stackCount, int slotIndex, string slotType)
    {
        this.itemName = itemName;
        this.stackCount = stackCount;
        this.slotIndex = slotIndex;
        this.slotType = slotType;
    }
}
