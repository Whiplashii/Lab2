using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Area Ability", menuName = "Abilities/Healing Area Ability")]
public class HealingAreaAbility : Ability
{
    [SerializeField]
    private GameObject healingAreaPrefab; // Префаб зоны лечения

    [SerializeField]
    private float areaDuration = 5f; // Сколько держится область

    [SerializeField]
    private int healAmountPerTick = 5; // Сколько лечит за один тик

    [SerializeField]
    private float tickRate = 1f; // Каждые сколько секунд лечит

    public override void Execute(Human user)
    {
        if (user != null)
        {
            Vector3 spawnPosition = user.transform.position; // Пока что ставим область в позицию игрока
            GameObject healingArea = Instantiate(healingAreaPrefab, spawnPosition, Quaternion.identity);

            HealingArea areaScript = healingArea.AddComponent<HealingArea>();
            areaScript.Initialize(areaDuration, healAmountPerTick, tickRate);
        }
    }
}