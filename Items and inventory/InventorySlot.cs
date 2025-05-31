using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour
{
    public GameObject heldItem;  // Объект, находящийся в слоте

   
    public void SetHeldItem(GameObject item)
    {
        heldItem = item;
        heldItem.transform.position = transform.position;
    }
}
