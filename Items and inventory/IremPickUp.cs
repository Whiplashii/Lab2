using UnityEngine;
using UnityEngine.UI;

public class ItemPickupArea : MonoBehaviour
{
    [SerializeField] private Image pickupArea; // Ваш прозрачный круг
    private PlayerController playerController;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Например, при нажатии 'E'
        {
            TryPickupItems();
        }
    }

    private void TryPickupItems()
    {
        // Получаем все DropLabel на сцене
        DropLabel[] dropLabels = FindObjectsOfType<DropLabel>();
        RectTransform pickupAreaRect = pickupArea.GetComponent<RectTransform>();

        foreach (var dropLabel in dropLabels)
        {
            // Проверяем, находится ли DropLabel внутри области проверки
            if (IsWithinPickupArea(dropLabel, pickupAreaRect))
            {
                playerController.PickUpDrop(dropLabel); // Подбираем предмет
                Destroy(dropLabel.gameObject); // Удаляем DropLabel после подбора
                break; // Выход из цикла, если предмет подобран
            }
        }
    }

    private bool IsWithinPickupArea(DropLabel dropLabel, RectTransform pickupAreaRect)
    {
        Vector3[] corners = new Vector3[4];
        pickupAreaRect.GetWorldCorners(corners);

        Vector3 labelPosition = dropLabel.transform.position;

        // Проверяем, находится ли позиция DropLabel в пределах области проверки
        return labelPosition.x >= corners[0].x && labelPosition.x <= corners[2].x &&
               labelPosition.y >= corners[0].y && labelPosition.y <= corners[1].y;
    }
}
