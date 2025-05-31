using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public GameObject Inventory;
    public GameObject ShopUi;
    public GameObject shopObject;
    private bool isShopOpen = false;
    private GameObject playerInRange = null;
    public bool isInventoryOpen => isShopOpen; // Игрок в зоне магазина

    private void Update()
    {
        if (playerInRange != null && Input.GetKeyDown(KeyCode.F))
        {
            ToggleShop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Убедись, что у игрока стоит тег "Player"
        {
            playerInRange = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.gameObject == playerInRange)
            {
                playerInRange = null;
                if (isShopOpen)
                {
                    ToggleShop(); // Автоматически закрываем магазин если игрок уходит
                }
            }
        }
    }

    private void ToggleShop()
    {
        isShopOpen = !isShopOpen;
        Inventory.SetActive(isShopOpen);
        ShopUi.SetActive(isShopOpen);
        Time.timeScale = isShopOpen ? 0 : 1;
    }
}