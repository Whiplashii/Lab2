using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;  //

public class LevelGenerator : MonoBehaviour
{
    public int gridSize = 5; // Размер уровня N x N
    public GameObject[] tilePrefabs; // Набор тайлов для генерации
    private GameObject[,] grid; // Сетка с тайлами
    private List<Vector2Int> path;
    public GameObject portalPrefab;

    public GameObject[] enemyPrefabs; // Префабы врагов
    public int enemyCount = 5; // Сколько врагов спавнить
    public NavMeshSurface navMeshSurface;

    public EnemyActivatorRadius enemyActivator;

    public GameObject[] wallPrefabs;


    void Start()
{
    if (LevelParameters.GridSize > 0)
    {
        gridSize = LevelParameters.GridSize;
    }

    GenerateLevel();

    // Перестроим NavMesh, и запустим врагов только после его завершения
    StartCoroutine(BakeNavMeshAndSpawnEnemies());
}

IEnumerator BakeNavMeshAndSpawnEnemies()
{
    navMeshSurface.BuildNavMesh();

    // Ждём один кадр или больше (лучше всего подождать завершения)
    yield return null;

    // Если хочешь более надёжно - жди 1-2 кадра или проверку
    yield return new WaitForSeconds(0.1f); 

    // Теперь спавним врагов, когда NavMesh точно готов
    SpawnEnemies();
}

    void SpawnPortal()
    {
        Vector2Int portalPos = new Vector2Int(gridSize - 1, gridSize - 1);
        if (grid[portalPos.x, portalPos.y] != null)
        {
            Vector3 spawnPosition = new Vector3(portalPos.x * 40 - 20, 1+6, portalPos.y * 40+10);
            Instantiate(portalPrefab, spawnPosition, Quaternion.identity, transform);
        }
    }
    
    void GenerateLevel()
    {
        grid = new GameObject[gridSize, gridSize];
        path = GeneratePath();

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (path.Contains(position) /*|| IsNextToPath(position)*/)
                {
                    Vector3 worldPosition = new Vector3(x * 40, 0, y * 40);
                    GameObject tilePrefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];
                    GameObject newTile = Instantiate(tilePrefab, worldPosition, Quaternion.identity, transform);
                    grid[x, y] = newTile;

                    
                }
            }
        }
        SpawnPortal();

        SpawnWalls();
        
        Invoke("BakeNavMesh", 0.1f);
    }
    
    void BakeNavMesh()
{
    navMeshSurface.BuildNavMesh();

    // Дождёмся одного кадра, чтобы NavMesh полностью построился
    StartCoroutine(DelayedSpawnEnemies());
}

IEnumerator DelayedSpawnEnemies()
{
    yield return null; // Ждём 1 кадр
    SpawnEnemies();
}

    
    
    List<Vector2Int> GeneratePath()
{
    List<Vector2Int> generatedPath = new List<Vector2Int>();
    HashSet<Vector2Int> visited = new HashSet<Vector2Int>(); // Чтобы не ходить по одной и той же клетке
    Vector2Int current = new Vector2Int(0, 0);
    generatedPath.Add(current);
    visited.Add(current);

    while (current != new Vector2Int(gridSize - 1, gridSize - 1))
    {
        List<Vector2Int> possibleMoves = new List<Vector2Int>();

        // Добавляем все направления, но ограничиваем выбор
        if (current.x < gridSize - 1) possibleMoves.Add(new Vector2Int(current.x + 1, current.y)); // вправо
        if (current.y < gridSize - 1) possibleMoves.Add(new Vector2Int(current.x, current.y + 1)); // вверх
        if (current.x > 0 && Random.value < 0.3f) possibleMoves.Add(new Vector2Int(current.x - 1, current.y)); // влево (редко)
        if (current.y > 0 && Random.value < 0.3f) possibleMoves.Add(new Vector2Int(current.x, current.y - 1)); // вниз (редко)

        // Фильтруем уже посещенные
        possibleMoves.RemoveAll(move => visited.Contains(move));

        if (possibleMoves.Count > 0)
        {
            current = possibleMoves[Random.Range(0, possibleMoves.Count)];
            generatedPath.Add(current);
            visited.Add(current);
        }
        else
        {
            // Если нет доступных ходов, откатываемся назад (чтобы не застрять)
            current = generatedPath[generatedPath.Count - 2];
            generatedPath.RemoveAt(generatedPath.Count - 1);
        }
    }

    return generatedPath;
}
    
    bool IsNextToPath(Vector2Int position)
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in directions)
        {
            if (path.Contains(position + dir))
            {
                return true;
            }
        }
        return false;
    }

void SpawnWalls()
{
    if (wallPrefabs.Length < 4)
    {
        Debug.LogError("Не все префабы стен заданы!");
        return;
    }

    float offset = 20f; // Смещение стен относительно центра тайла
    Vector3 wallOffset = new Vector3(-21f, 7f, 2f); // Настройка позиции стены относительно тайла

    for (int x = 0; x < gridSize; x++)
    {
        for (int y = 0; y < gridSize; y++)
        {
            if (grid[x, y] == null) continue; // Пропускаем пустые клетки

            Vector3 tilePosition = grid[x, y].transform.position;

            // **ЛЕВАЯ СТЕНА**
            if (x == 0 || grid[x - 1, y] == null)
            {
                Instantiate(wallPrefabs[2], tilePosition + new Vector3(-offset, 0, 0) + wallOffset, wallPrefabs[2].transform.rotation, transform);
            }

            // **НИЖНЯЯ СТЕНА**
            if (y == 0 || grid[x, y - 1] == null)
            {
                Instantiate(wallPrefabs[1], tilePosition + new Vector3(0, 0, -offset) + wallOffset, wallPrefabs[1].transform.rotation, transform);
            }

            // **ПРАВАЯ СТЕНА (только если это крайний правый тайл или справа пусто)**
            if (x == gridSize - 1 || grid[x + 1, y] == null)
            {
                Instantiate(wallPrefabs[3], tilePosition + new Vector3(offset, 0, 0) + wallOffset, wallPrefabs[3].transform.rotation, transform);
            }

            // **ВЕРХНЯЯ СТЕНА (только если это крайний верхний тайл или сверху пусто)**
            if (y == gridSize - 1 || grid[x, y + 1] == null)
            {
                Instantiate(wallPrefabs[0], tilePosition + new Vector3(0, 0, offset) + wallOffset, wallPrefabs[0].transform.rotation, transform);
            }
        }
    }
}

void SpawnEnemies()
{
    List<Vector2Int> validTiles = new List<Vector2Int>();

    for (int x = 0; x < gridSize; x++)
    {
        for (int y = 0; y < gridSize; y++)
        {
            if (grid[x, y] != null)
                validTiles.Add(new Vector2Int(x, y));
        }
    }

    // ❗ Удаляем старт и портал
    Vector2Int start = new Vector2Int(0, 0);
    Vector2Int portal = new Vector2Int(gridSize - 1, gridSize - 1);
    validTiles.Remove(start);
    validTiles.Remove(portal);

    if (validTiles.Count == 0) return;

    int dynamicEnemyCount = validTiles.Count * 2;

    for (int i = 0; i < dynamicEnemyCount; i++)
    {
        Vector2Int spawnTile = validTiles[Random.Range(0, validTiles.Count)];

        Vector3 offset = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
        Vector3 spawnPos = new Vector3(spawnTile.x * 40 - 20, 1, spawnTile.y * 40) + offset;

        GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        newEnemy.SetActive(false);

        if (enemyActivator != null)
            enemyActivator.enemies.Add(newEnemy);
    }
}

    

}