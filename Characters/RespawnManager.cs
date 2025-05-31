using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    [SerializeField] private List<Transform> respawnPoints = new List<Transform>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // Удаляем дубликат, если он есть
    }

    /// <summary>
    /// Добавляет новую точку респавна в список.
    /// </summary>
    /// <param name="newRespawnPoint">Точка респавна, которая будет добавлена.</param>
    public void AddRespawnPoint(Transform newRespawnPoint)
    {
        if (newRespawnPoint != null && !respawnPoints.Contains(newRespawnPoint))
        {
            respawnPoints.Add(newRespawnPoint);
            Debug.Log($"Добавлена новая точка респавна: {newRespawnPoint.position}");
        }
    }

    /// <summary>
    /// Возвращает последнюю добавленную точку респавна.
    /// </summary>
    /// <returns>Последняя точка респавна.</returns>
    public Vector3 GetRespawnPoint()
    {
        if (respawnPoints.Count == 0)
        {
            Debug.LogWarning("Список точек респавна пуст! Убедитесь, что они добавлены.");
            return Vector3.zero; // Возвращаем позицию (0,0,0) по умолчанию
        }

        return respawnPoints[respawnPoints.Count - 1].position; // Возвращаем последнюю добавленную точку
    }
}
