using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;
using System.Text;


public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Settings")]
    [SerializeField] private GameObject tooltipPanel; // Панель для отображения подсказки
    [SerializeField] private TMP_Text tooltipText;    // Текст для характеристик предмета
    [SerializeField] private Vector2 tooltipOffset = new Vector2(20, -20); // Смещение относительно курсора

    private GameObject currentHoveredItem;

    [HideInInspector] public bool isStorageOpened;

    [SerializeField] private Human player;
    [SerializeField] private GameObject[] Slots = new GameObject[12];
    [SerializeField] private Transform handParent;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] public GameObject helmetSlot;
    [SerializeField] public GameObject armorSlot;
    [SerializeField] public GameObject weaponSlot;
    [SerializeField] private TMP_Text StatsText;

    [SerializeField] private GameObject shopUI; // Ссылка на UI магазина
    [SerializeField] private PlayerWallet playerWallet;

    private GameObject draggedObject;
    
    private GameObject lastItemSlot;
    private bool isInventoryOpened;
    private int selectedHotbarSlot = 0;

    public bool IsInventoryOpened => isInventoryOpened;

    private void Start()
    {
        LoadInventory();
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Human>();
            ShowInfo();
        }
    }

    private void Update()
    {
        /*if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            tooltipPanel.GetComponent<RectTransform>().position = 
                (Vector2)Input.mousePosition + tooltipOffset;
        }*/
        

        if (draggedObject != null)
            draggedObject.transform.position = Input.mousePosition;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isInventoryOpened)
            {
                Cursor.lockState = CursorLockMode.None;
                isInventoryOpened = false;
                isStorageOpened = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                isInventoryOpened = true;
            }
        }

        ShowInfo();
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Получаем объект, на который навели курсор
        // 1. Получаем слот, на который навели (не сам предмет!)
        GameObject hoveredSlot = eventData.pointerEnter; 
        if (hoveredSlot == null ) return;
        

        // 2. Ищем компонент InventorySlot (если он есть на слоте)
        InventorySlot slot = hoveredSlot.GetComponent<InventorySlot>();
        if (slot == null || slot.heldItem == null) return;
       

        // 3. Получаем ItemSO из визуального отображения предмета
        InventoryItem item = slot.heldItem.GetComponent<InventoryItem>();
        if (item?.itemScriptableObject == null) return;
       

        // 4. Показываем подсказку
        ShowTooltip(item.itemScriptableObject, eventData.position);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
       
        HideTooltip();
        currentHoveredItem = null;
        
    }

    private void ShowTooltip(ItemSO item, Vector2 position)
    {
        if (tooltipPanel == null || tooltipText == null) 
        {
            Debug.LogError("Tooltip references not set!");
            return;
        }

        // Формируем текст с характеристиками
        StringBuilder stats = new StringBuilder();
        stats.AppendLine($"<b>{item.name}</b>");
        stats.AppendLine($"Тип: {item.itemType}");
        stats.AppendLine($"Цена: {item.price}");

        // Добавляем только ненулевые характеристики
        if (item.healthBonus != 0) stats.AppendLine($"Здоровье: +{item.healthBonus}");
        if (item.manaBonus != 0) stats.AppendLine($"Мана: +{item.manaBonus}");
        if (item.healthRegenBonus != 0) stats.AppendLine($"Реген здоровья: +{item.healthRegenBonus}");
        if (item.manaRegenBonus != 0) stats.AppendLine($"Реген маны: +{item.manaRegenBonus}");
        if (item.damageModifier != 0) stats.AppendLine($"Урон: +{item.damageModifier}");
        if (item.spellschanged != false) stats.AppendLine($"МЕНЯЕТ СПОСОБНОСТЬ!!");

        tooltipText.text = stats.ToString();
        tooltipPanel.SetActive(true);

        // Позиционируем подсказку
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        if (tooltipRect != null)
        {
            tooltipRect.position = position + tooltipOffset;
        }
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlot slot = clickedObject.GetComponent<InventorySlot>();

            if (slot != null)
            {
                if (slot.CompareTag("EquipSlot") && slot.heldItem != null)
                {
                    RemoveEquippedItem(slot);
                    player.UpdateAbilitiesFromEquipment(this);
                }
                else if (!slot.CompareTag("EquipSlot") && slot.heldItem != null)
                {
                    draggedObject = slot.heldItem;
                    slot.heldItem = null;
                    lastItemSlot = clickedObject;
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
{
    if (draggedObject != null && eventData.button == PointerEventData.InputButton.Left)
    {
        if (eventData.pointerCurrentRaycast.gameObject == null)
        {
            Debug.Log("[OnPointerUp] Отпустили предмет в пустое место.");

            if (shopUI.activeSelf)
            {
                Debug.Log($"[OnPointerUp] Продаем предмет {draggedObject.name}.");
                SellItem(draggedObject);
            }
            else
            {
                Debug.Log($"[OnPointerUp] Возвращаем предмет {draggedObject.name} на старое место.");
                ReturnItemToPreviousSlot();
            }
            draggedObject = null;
            return;
        }

        GameObject targetObject = eventData.pointerCurrentRaycast.gameObject;
        InventorySlot targetSlot = targetObject.GetComponent<InventorySlot>();

        if (targetSlot != null)
        {
            Debug.Log($"[OnPointerUp] Перемещаем {draggedObject.name} в слот {targetSlot.name}.");

            if (targetSlot.CompareTag("EquipSlot"))
            {
                HandleEquipSlot(targetSlot, draggedObject);  // Вызов метода обработки для слота экипировки
            }
            else
            {
                HandleInventorySlot(targetSlot);  // Если не слот экипировки, обрабатываем как обычный инвентарь
            }
        }
        else
        {
            Debug.Log($"[OnPointerUp] Невозможно положить {draggedObject.name} в {targetObject.name}, возвращаем обратно.");
            ReturnItemToPreviousSlot();
        }

        draggedObject = null;
    }
}

private void SellItem(GameObject item)
{
    InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
    if (inventoryItem == null) return;

    ItemSO itemData = inventoryItem.itemScriptableObject;
    int sellPrice = itemData.price / 2; // Половина цены

    playerWallet.AddMoney(sellPrice); // Добавляем деньги
    Destroy(item); // Удаляем предмет из инвентаря

    Debug.Log($"Продан {itemData.name} за {sellPrice} монет!");
}

    private void HandleEquipSlot(InventorySlot targetSlot, GameObject draggedItem)
{
    InventoryItem draggedInventoryItem = draggedItem.GetComponent<InventoryItem>();
    ItemSO itemSO = draggedInventoryItem.itemScriptableObject;

    // Проверяем совместимость предмета с этим слотом
    if (itemSO.itemType == ItemSO.ItemType.Default || !IsItemCompatibleWithSlot(itemSO, targetSlot))
    {
        Debug.Log($"[HandleEquipSlot] Предмет {itemSO.name} несовместим со слотом {targetSlot.name}, отправляем в инвентарь.");
        MoveItemToInventory(draggedItem);
        return;
    }

    // Если слот пустой, просто экипируем предмет
    if (targetSlot.heldItem == null)
    {
        Debug.Log($"[HandleEquipSlot] Слот {targetSlot.name} пуст, экипируем предмет {itemSO.name}.");
        EquipItemToSlot(targetSlot, draggedItem);
    }
    else
    {
        // Если предметы одинаковые, оставляем их на своих местах
        if (targetSlot.heldItem == draggedItem)
        {
            Debug.Log($"[HandleEquipSlot] Предметы одинаковые, оставляем их на своих местах.");
            return;
        }

        Debug.Log($"[HandleEquipSlot] В слоте {targetSlot.name} уже есть {targetSlot.heldItem.name}, меняем местами с {itemSO.name}.");
        SwapItemsBetweenSlots(targetSlot, draggedItem);
    }
}

    private void MoveItemToInventory(GameObject draggedItem)
    {
        foreach (var slot in Slots)
        {
            InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();
            if (inventorySlot.heldItem == null)
            {
                inventorySlot.SetHeldItem(draggedItem);
                draggedItem.transform.SetParent(inventorySlot.transform);
                draggedItem.transform.localPosition = Vector3.zero;
                return;
            }
        }
    }

    private bool IsItemCompatibleWithSlot(ItemSO itemSO, InventorySlot targetSlot)
    {
        string slotName = targetSlot.gameObject.name.ToLower();

        return itemSO.itemType switch
        {
            ItemSO.ItemType.Helmet => slotName.Contains("helmet"),
            ItemSO.ItemType.Armor => slotName.Contains("armor"),
            ItemSO.ItemType.Weapon => slotName.Contains("weapon"),
            _ => false
        };
    }

    private void HandleInventorySlot(InventorySlot targetSlot)
    {
        if (lastItemSlot.CompareTag("EquipSlot"))
        {
            InventoryItem draggedItem = draggedObject.GetComponent<InventoryItem>();
            ItemSO itemSO = draggedItem.itemScriptableObject;

            player.RemoveItemStats(itemSO);

            if (targetSlot.heldItem == null)
            {
                targetSlot.SetHeldItem(draggedObject);
                draggedObject.transform.SetParent(targetSlot.transform);
                draggedObject.transform.localPosition = Vector3.zero;
            }
            else
            {
                SwapItemsBetweenSlots(targetSlot, draggedObject);
            }
        }
        else
        {
            if (targetSlot.heldItem == null)
            {
                targetSlot.SetHeldItem(draggedObject);
                draggedObject.transform.SetParent(targetSlot.transform);
                draggedObject.transform.localPosition = Vector3.zero;
            }
            else
            {
                SwapItemsBetweenSlots(targetSlot, draggedObject);
            }
        }
    }

    private void RemoveEquippedItem(InventorySlot equipSlot)
    {
        GameObject equippedItem = equipSlot.heldItem;
        if (equippedItem == null) return;

        InventoryItem inventoryItem = equippedItem.GetComponent<InventoryItem>();
        player.RemoveItemStats(inventoryItem.itemScriptableObject);

        equipSlot.heldItem = null;
        MoveItemToInventory(equippedItem);

        player.UpdateAbilitiesFromEquipment(this);
    }

    private void SwapItemsBetweenSlots(InventorySlot targetSlot, GameObject newItem)
{
    GameObject existingItem = targetSlot.heldItem;

    // Проверяем, что новые предметы одинаковые, и если это так, не меняем их местами
    if (existingItem != null && existingItem == newItem)
    {
        Debug.Log("[SwapItemsBetweenSlots] Предметы одинаковые, оставляем их на местах.");
        return;
    }

    // Если предмет был в слоте экипировки, удаляем его статистику
    if (existingItem != null && targetSlot.CompareTag("EquipSlot"))
    {
        InventoryItem existingItemComponent = existingItem.GetComponent<InventoryItem>();
        player.RemoveItemStats(existingItemComponent.itemScriptableObject);
    }

    // Помещаем новый предмет в слот
    targetSlot.SetHeldItem(newItem);
    newItem.transform.SetParent(targetSlot.transform);
    newItem.transform.localPosition = Vector3.zero;

    // Если был старый предмет, возвращаем его в последний слот
    if (existingItem != null)
    {
        InventorySlot lastSlot = lastItemSlot.GetComponent<InventorySlot>();
        lastSlot.SetHeldItem(existingItem);
        existingItem.transform.SetParent(lastSlot.transform);
        existingItem.transform.localPosition = Vector3.zero;
    }

    // Если предмет был в слоте экипировки, применяем новые бонусы
    if (newItem != null && targetSlot.CompareTag("EquipSlot"))
    {
        InventoryItem newItemComponent = newItem.GetComponent<InventoryItem>();
        player.ApplyItemStats(newItemComponent.itemScriptableObject);
    }
}


    private void EquipItemToSlot(InventorySlot slot, GameObject item)
    {
        GameObject existingItem = slot.heldItem;

        if (existingItem != null) // Снимаем бонусы старого предмета
        {
            InventoryItem existingItemComponent = existingItem.GetComponent<InventoryItem>();
            player.RemoveItemStats(existingItemComponent.itemScriptableObject);
        }

        // Одеваем новый предмет
        InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
        ItemSO itemSO = inventoryItem.itemScriptableObject;

        player.ApplyItemStats(itemSO);
        slot.SetHeldItem(item);
        item.transform.SetParent(slot.transform);
        item.transform.localPosition = Vector3.zero;
        player.UpdateAbilitiesFromEquipment(this);
    }

    private void ReturnItemToPreviousSlot()
    {
        if (lastItemSlot != null)
        {
            lastItemSlot.GetComponent<InventorySlot>().SetHeldItem(draggedObject);
            draggedObject.transform.SetParent(lastItemSlot.transform);
            draggedObject.transform.localPosition = Vector3.zero;
        }
    }

    public void ItemPicked(DropLabel dropLabel)
    {
        ItemSO itemSO = dropLabel.DropInfo.item;
        foreach (var slot in Slots)
        {
            InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();
            if (inventorySlot.heldItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab);
                newItem.GetComponent<InventoryItem>().itemScriptableObject = itemSO;
                newItem.GetComponent<InventoryItem>().stackCurrent = 1;
                newItem.transform.SetParent(inventorySlot.transform, false);
                inventorySlot.SetHeldItem(newItem);
                Destroy(dropLabel.gameObject);
                return;
            }
        }
    }

    public void ShowInfo()
    {
        if (player != null)
        {
            StatsText.text = $"скорость передвижения: {player.moveSpeed}\n" +
                             $"регенерация энергии: {player.manaRegenSpeed}\n" +
                             $"урон: {player.baseDamage + player.additionalDamageFromItems}\n" +
                             $"Здоровье: <color=red>{(int)player.health}</color>\n" +
                             $"энергия: <color=purple>{(int)player.mana}</color>";
        }
    }

public void SaveInventory()
{
    InventoryData data = new InventoryData();

    // Обычные слоты инвентаря
    for (int i = 0; i < Slots.Length; i++)
    {
        InventorySlot slot = Slots[i].GetComponent<InventorySlot>();

        if (slot.heldItem != null)
        {
            InventoryItem item = slot.heldItem.GetComponent<InventoryItem>();
            InventoryItemData itemData = new InventoryItemData(
                item.itemScriptableObject.name,
                item.stackCurrent,
                i,
                "inventory" // Обозначаем, что это обычный инвентарь
            );

            data.items.Add(itemData);
        }
    }

    // Сохранение экипировки
    SaveEquipSlot(helmetSlot, "helmet", data);
    SaveEquipSlot(armorSlot, "armor", data);
    SaveEquipSlot(weaponSlot, "weapon", data);

    string json = JsonUtility.ToJson(data, true);
    string path = Path.Combine(Application.persistentDataPath, "inventory.json");

    File.WriteAllText(path, json);
    Debug.Log("Инвентарь сохранён в: " + path);
}

// Вспомогательный метод для сохранения экипировки
private void SaveEquipSlot(GameObject slotObject, string slotType, InventoryData data)
{
    InventorySlot slot = slotObject.GetComponent<InventorySlot>();
    
    if (slot.heldItem != null)
    {
        InventoryItem item = slot.heldItem.GetComponent<InventoryItem>();
        InventoryItemData itemData = new InventoryItemData(
            item.itemScriptableObject.name,
            item.stackCurrent,
            -1, // В экипировке индекс не нужен
            slotType // Указываем, что это слот экипировки
        );

        data.items.Add(itemData);
    }
}

public void LoadInventory()
{
    string path = Path.Combine(Application.persistentDataPath, "inventory.json");

    if (!File.Exists(path))
    {
        Debug.LogWarning("Файл инвентаря не найден. Загружаем пустой инвентарь.");
        return;
    }

    string json = File.ReadAllText(path);
    if (string.IsNullOrWhiteSpace(json))
    {
        Debug.LogWarning("Файл инвентаря пуст. Загружаем пустой инвентарь.");
        return;
    }

    InventoryData data = JsonUtility.FromJson<InventoryData>(json);
    if (data == null || data.items == null)
    {
        Debug.LogWarning("Ошибка загрузки инвентаря: данные некорректны. Загружаем пустой инвентарь.");
        return;
    }

    foreach (var itemData in data.items)
    {
        GameObject newItem = Instantiate(itemPrefab);
        InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();

        ItemSO itemSO = FindItemSOByName(itemData.itemName);
        if (itemSO == null) continue;

        inventoryItem.itemScriptableObject = itemSO;
        inventoryItem.stackCurrent = itemData.stackCount;

        if (itemData.slotType == "inventory")
        {
            InventorySlot slot = Slots[itemData.slotIndex].GetComponent<InventorySlot>();
            slot.SetHeldItem(newItem);
            SetItemTransform(newItem, slot.transform);
        }
        else
        {
            InventorySlot equipSlot = GetEquipSlotByType(itemData.slotType);
            if (equipSlot != null)
            {
                EquipItemToSlot(equipSlot, newItem);
                SetItemTransform(newItem, equipSlot.transform); // Подгоняем размер предмета под слот
            }
        }
    }

    Debug.Log("Инвентарь успешно загружен.");
}

// Вспомогательный метод для корректного отображения предметов
private void SetItemTransform(GameObject item, Transform parent)
{
    item.transform.SetParent(parent, false);
    item.transform.localPosition = Vector3.zero;  // Устанавливаем позицию
    item.transform.localScale = Vector3.one;       // Убеждаемся, что масштаб нормальный
    RectTransform rect = item.GetComponent<RectTransform>();
    if (rect != null)
    {
        rect.anchoredPosition = Vector2.zero; // Центрируем в слоте
    }
}

// Получаем слот экипировки по типу
private InventorySlot GetEquipSlotByType(string slotType)
{
    return slotType switch
    {
        "helmet" => helmetSlot.GetComponent<InventorySlot>(),
        "armor" => armorSlot.GetComponent<InventorySlot>(),
        "weapon" => weaponSlot.GetComponent<InventorySlot>(),
        _ => null
    };
}

private ItemSO FindItemSOByName(string itemName)
{
    foreach (ItemSO itemSO in Resources.LoadAll<ItemSO>("Items")) // Убедитесь, что ScriptableObjects хранятся в Resources/Items
    {
        if (itemSO.name == itemName)
        {
            return itemSO;
        }
    }
    return null;
}
public void SetInventoryState(bool state)
{
    isInventoryOpened = state;
}

public void AddItem(ItemSO item)
{
    // Поиск пустого слота в инвентаре
    foreach (var slot in Slots)
    {
        InventorySlot inventorySlot = slot.GetComponent<InventorySlot>();
        if (inventorySlot.heldItem == null)
        {
            // Создаём новый объект предмета
            GameObject newItem = Instantiate(itemPrefab, inventorySlot.transform);
            InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
            
            // Устанавливаем данные предмета
            inventoryItem.itemScriptableObject = item;
            inventoryItem.stackCurrent = 1; // Начинаем с 1 предмета
            
            // Привязываем предмет к слоту
            inventorySlot.SetHeldItem(newItem);
            return;
        }
    }

    Debug.Log("Нет свободных слотов в инвентаре");
}

}
