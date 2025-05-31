using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Human controlledCharacter; // Управляемый персонаж
    [SerializeField] private InventoryManager InventoryManager;
    [SerializeField] private ShopTrigger Shop;

    [SerializeField] private Transform respawnPoint; // Точка возрождения
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashManaCost = 15f;

    private float lastDashTime;
    private Vector3 movementDirection;
    private Enemy target;
    public float rotationSpeed = 10f;

    

    public Enemy Target
    {
        get => target;
        private set => target = value;
    }

    public event Action<Enemy> OnTargetChanged = delegate { };

    private void Start()
{
    
    Time.timeScale = 1; // Включаем время
    if (controlledCharacter == null)
    {
        controlledCharacter = FindObjectOfType<Human>(); // Ищем персонажа
        if (controlledCharacter == null)
        {
            Debug.LogError("Игрок не найден!");
        }
    }
}
    private void Update()
    {
        if (InventoryManager.IsInventoryOpened)
        {
            return; // Прекращаем выполнение метода, если инвентарь открыт
        }
        if (Shop.isInventoryOpen==true)
        {
            return; // Прекращаем выполнение метода, если инвентарь открыт
        }

        HandleMovement();
        HandleMouseLook();
        HandleMouseInputs();

        if (Input.GetKeyDown(KeyCode.Space) && movementDirection != Vector3.zero)
        {
            PerformDash();
        }

        CheckDeath(); // Проверяем, мертв ли персонаж
    }

    private void CheckDeath()
    {
        if (controlledCharacter.health <= 0)
        {
            Respawn(); // Если здоровье <= 0, возрождаем игрока
        }
    }

    private void Respawn()
{
    RespawnManager RespPoint = FindObjectOfType<RespawnManager>();
    Debug.Log("Игрок умер. Возрождение...");
    Vector3 respawnPosition = RespPoint.GetRespawnPoint(); // Используем точку возрождения
    controlledCharacter.Respawn(respawnPosition); // Вызываем метод Respawn у Human
}

    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        respawnPoint = newRespawnPoint; // Обновляем точку возрождения
        Debug.Log($"Точка возрождения обновлена: {newRespawnPoint.position}");
    }

    private void HandleMovement()
    {
        
            
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        movementDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (movementDirection.magnitude >= 0.1f)
        {
            Vector3 targetPosition = controlledCharacter.transform.position + movementDirection;
            controlledCharacter.Move(targetPosition);
        }
        else
        {
            controlledCharacter.Move(controlledCharacter.transform.position); // Остановка персонажа
        }
    }

    private void HandleMouseLook()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 lookDirection = (hit.point - controlledCharacter.transform.position).normalized;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                controlledCharacter.transform.rotation = Quaternion.Lerp(
                    controlledCharacter.transform.rotation, 
                    targetRotation, 
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    private void HandleMouseInputs()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    controlledCharacter.UseFirstAbility(0);
                }
                if (Input.GetMouseButtonDown(1))
                {
                    controlledCharacter.UseSecondAbility(0);
                }

                SeekTarget(hit);
            }
        }
    }

    private void SeekTarget(RaycastHit hit)
    {
        Enemy newTarget = hit.collider.GetComponent<Enemy>();
        if (Target != newTarget)
        {
            if (newTarget != null && newTarget.tag == "Player")
            {
                Target = null;
            }
            else
            {
                Target = newTarget;
            }

            OnTargetChanged(Target);
        }
    }

    private void PerformDash()
{
    if (Time.time - lastDashTime < dashCooldown) return; // Проверка перезарядки рывка

    if (controlledCharacter.mana >= dashManaCost) // Проверяем, достаточно ли маны
    {
        Vector3 dashTarget = controlledCharacter.transform.position + movementDirection * dashDistance;
        RaycastHit hit;

        // Проверяем на наличие препятствия по направлению рывка
        if (Physics.Raycast(controlledCharacter.transform.position, movementDirection, out hit, dashDistance))
        {
            // Если препятствие найдено, устанавливаем цель на минимальное расстояние до него
            dashTarget = hit.point - movementDirection * 0.4f; // Останавливаемся перед препятствием
            Debug.Log("Рывок остановлен перед препятствием");
        }

        // Проверка маны и запуск рывка
        controlledCharacter.UseDash(0); // Снимаем ману за рывок
        StartCoroutine(DashCoroutine(dashTarget)); // Запуск корутины для быстрого рывка
        lastDashTime = Time.time; // Обновление времени последнего рывка
    }
    else
    {
        Debug.Log("Недостаточно маны для выполнения рывка"); // Сообщение об ошибке
    }
}

private IEnumerator DashCoroutine(Vector3 targetPosition)
{
    while (Vector3.Distance(controlledCharacter.transform.position, targetPosition) > 0.1f)
    {
        controlledCharacter.transform.position = Vector3.MoveTowards(
            controlledCharacter.transform.position,
            targetPosition,
            dashSpeed * Time.deltaTime
        );
        yield return null; // Переход к следующему кадру
    }
}

    // Новый метод для подбора предметов







public void PickUpDrop(DropLabel dropLabel)
{
    if (dropLabel.DropInfo.item != null)
    {
        InventoryManager.ItemPicked(dropLabel); // Добавление предмета в инвентарь
        DropManager.Instance.RemoveDrop(dropLabel.DropInfo.id); // Удаление дропа
    }
}

}