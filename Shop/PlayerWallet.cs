using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerWallet : MonoBehaviour
{
    public int Money { get; private set; }
    [SerializeField] private TextMeshProUGUI moneyText;

    private void Start()
    {
        LoadMoney(); // Загружаем сохраненные деньги
        UpdateMoneyUI();
    }

    public void AddMoney(int amount)
    {
        Money += amount;
        Debug.Log($"Добавлено {amount} монет. Сейчас у тебя {Money} монет.");
        SaveMoney();
        UpdateMoneyUI();
    }

    public void SpendMoney(int amount)
    {
        if (Money >= amount)
        {
            Money -= amount;
            Debug.Log($"Потрачено {amount} монет. Осталось {Money} монет.");
            SaveMoney();
            UpdateMoneyUI();
        }
        else
        {
            Debug.Log("❌ Недостаточно денег!");
        }
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"Монеты: {Money}";
        }
        else
        {
            Debug.LogError("❌ Ошибка: не назначен moneyText в PlayerWallet!");
        }
    }

    private void SaveMoney()
    {
        PlayerPrefs.SetInt("PlayerMoney", Money);
        PlayerPrefs.Save();
    }

    private void LoadMoney()
    {
        Money = PlayerPrefs.GetInt("PlayerMoney", 0); // Если данных нет, устанавливаем 1000
    }
}