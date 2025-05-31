using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject shopUI;
    [SerializeField] private GameObject[] Slots;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private PlayerWallet playerWallet;
    [SerializeField] private ItemSO[] shopItems;
    public GameObject Inventory;
    private bool isInventoryOpen = false;

    private bool isShopOpen = false;

    private void Start()
    {
        PopulateShop(); // Изначально магазин закрыт
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isShopOpen)
        {
            CloseShop();
        }
    }

    public void OpenShop()
    {
        Debug.Log(">>> Открытие магазина! <<<");
        if (!isShopOpen)
        {
            isShopOpen = true;
            shopUI.SetActive(true);
            isInventoryOpen = true; // Меняем состояние инвентаря
            Inventory.SetActive(isInventoryOpen);
            Cursor.lockState = CursorLockMode.None;
            PopulateShop();
        }
    }

    public void CloseShop()
    {
        isShopOpen = false;
        shopUI.SetActive(false);
        isInventoryOpen = false; // Меняем состояние инвентаря
        Inventory.SetActive(isInventoryOpen);
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void PopulateShop()
    {
        
        for (int i = 0; i < Slots.Length; i++)
        {
            InventorySlot slot = Slots[i].GetComponent<InventorySlot>();
            if (slot.heldItem == null && i < shopItems.Length)
            {
                
                GameObject itemGO = Instantiate(shopItemPrefab, Slots[i].transform);
                ShopItem shopItem = itemGO.GetComponent<ShopItem>();
                shopItem.Setup(shopItems[i], this); // Передаём данные о предмете
                slot.SetHeldItem(itemGO);
            }
        }
    }

    public void TryBuyItem(ItemSO item)
{
    if (playerWallet.Money >= item.price) // Проверяем, хватает ли денег
    {
        playerWallet.SpendMoney(item.price); // Списываем деньги
        inventory.AddItem(item); // Добавляем предмет в инвентарь
        Debug.Log($"Куплен {item.name} за {item.price} монет");
    }
    else
    {
        Debug.Log("Недостаточно денег для покупки!");
    }
}


    private void SetItemTransform(GameObject item, Transform parent)
{
    item.transform.SetParent(parent, false);
    item.transform.localPosition = Vector3.zero;  // Устанавливаем позицию
    item.transform.localScale = (Vector3.one);       // Убеждаемся, что масштаб нормальный
    RectTransform rect = item.GetComponent<RectTransform>();
    if (rect != null)
    {
        rect.anchoredPosition = Vector2.zero; // Центрируем в слоте
    }
}

    

    

}
