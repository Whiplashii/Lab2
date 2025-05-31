using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTeleporter : MonoBehaviour
{
    public string sceneName;
    public string currentScene;
     // Название сцены (например "HubScene")
    private bool canTeleport = false;
    public int thisLevelNumber ;
    
    
    void Start()
    {
        thisLevelNumber = PlayerPrefs.GetInt("CurrentPlayingLevel", 1);
        Debug.Log($"[SceneTeleporter] Current playing level loaded: {thisLevelNumber}");
    }

    void Update()
    {
        if (canTeleport && Input.GetKeyDown(KeyCode.E))
        {
            // Сохраняем инвентарь
            InventoryManager inventory = FindObjectOfType<InventoryManager>();
            if (inventory != null)
            {
                inventory.SaveInventory();
            }

            // Награда за прохождение уровня
            PlayerWallet wallet = FindObjectOfType<PlayerWallet>();
            if (wallet != null)
            {
                wallet.AddMoney(100);
            }

            // Разблокировка следующего уровня при переходе в хаб
            if (currentScene != "tutorial")
            {
                if (thisLevelNumber >= PlayerPrefs.GetInt("LevelReached", 1))
                {
                    LevelSelectionTrigger.UnlockNextLevel();
                    Debug.Log("Следующий уровень разблокирован!");
                }
            }

            // Загружаем сцену
            Time.timeScale = 1;
            SceneManager.LoadScene(sceneName);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canTeleport = true;
            Debug.Log("Нажмите 'E' для перехода");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canTeleport = false;
        }
    }
}