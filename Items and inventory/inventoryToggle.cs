using UnityEngine;

public class InventoryToggle : MonoBehaviour
{
    public GameObject Inventory; // Ссылка на панель инвентаря
    private bool isInventoryOpen = false; // Состояние инвентаря

    void Update()
    {
        // Проверяем, нажата ли клавиша "Tab"
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen; // Меняем состояние инвентаря
        Inventory.SetActive(isInventoryOpen); // Показываем или скрываем панель
    }
}
