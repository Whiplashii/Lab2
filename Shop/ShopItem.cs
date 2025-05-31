using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItem : MonoBehaviour
{
    private ItemSO item;
    public Image itemImage; // Иконка предмета
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPrice;
    private ShopManager shopManager;

    public void Setup(ItemSO newItem, ShopManager manager)
    {
        item = newItem;
        shopManager = manager;
       
        itemImage.sprite = item.icon; // Присваиваем иконку
        itemNameText.text = item.name;
        itemPrice.text = item.price.ToString();
        // Назначаем обработчик клика
        GetComponent<Button>().onClick.AddListener(OnItemClick);
    }

    private void OnItemClick()
    {
        if (item != null && shopManager != null)
        {
            shopManager.TryBuyItem(item);
        }
    }
}
