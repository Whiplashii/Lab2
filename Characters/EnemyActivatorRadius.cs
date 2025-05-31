using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;

public class EnemyActivatorRadius : MonoBehaviour
{
    public Transform player; // Перетащи игрока в инспекторе
    public float activationRadius = 10f; // Радиус активации
    public List<GameObject> enemies = new List<GameObject>(); // Список врагов

    void Update()
    {
        // Удаляем уничтоженных врагов из списка
        enemies.RemoveAll(enemy => enemy == null);

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeSelf) // Если враг выключен
            {
                float distance = Vector3.Distance(player.position, enemy.transform.position);
                if (distance <= activationRadius)
                {
                    enemy.SetActive(true); // Включаем врага
                    Debug.Log($"Враг {enemy.name} активирован!");
                }
            }
        }
    }
}
