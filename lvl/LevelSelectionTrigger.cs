using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LevelSelectionTrigger : MonoBehaviour
{
    [Header("Меню выбора уровня")]
    public GameObject interactionUI;
    public GameObject levelSelectionMenu;
    public Transform buttonContainer;
    public Button levelButtonPrefab;

    private bool isPlayerNear = false;
    private bool isMenuOpen = false;
    private InventoryManager inventoryManager;

    private void Start()
    {
        
        interactionUI.SetActive(false);
        levelSelectionMenu.SetActive(false);
        inventoryManager = FindObjectOfType<InventoryManager>();
        UpdateLevelButtons();
    }

    private void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            ToggleMenu();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            interactionUI.SetActive(false);
            if (isMenuOpen) ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        levelSelectionMenu.SetActive(isMenuOpen);
        interactionUI.SetActive(!isMenuOpen);
        Time.timeScale = isMenuOpen ? 0 : 1;

        if (isMenuOpen)
        {
            UpdateLevelButtons();
        }
    }

    private void UpdateLevelButtons()
    {
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);
        Debug.Log("LevelReached: " + levelReached);
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 1; i <= levelReached + 1; i++)
        {
            Button newButton = Instantiate(levelButtonPrefab, buttonContainer);
            RectTransform rt = newButton.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            rt.sizeDelta = new Vector2(160, 40);
            newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "Level " + i;
            newButton.interactable = i <= levelReached;
            int levelNumber = i;
            newButton.onClick.AddListener(() => LoadLevel(levelNumber));
        }
    }

    public void LoadLevel(int levelNumber)
    {
        if (inventoryManager != null) inventoryManager.SaveInventory();
        LevelParameters.CurrentLevel = levelNumber;
        PlayerPrefs.SetInt("CurrentPlayingLevel", levelNumber);
        PlayerPrefs.Save();
        LevelParameters.UpdateLevelSize(levelNumber);
        Time.timeScale = 1;
        SceneManager.LoadScene("tiles");
    }

    public static void UnlockNextLevel()
    {
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);
        PlayerPrefs.SetInt("LevelReached", levelReached + 1);
        PlayerPrefs.Save();
    }

    public static int GetMaxLevel()
    {
        return PlayerPrefs.GetInt("LevelReached", 1);
    }
}
