using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Boost Ability", menuName = "Abilities/Damage Boost Ability")]
public class DamageBoostAbility : Ability
{
    [SerializeField]
    private float boostMultiplier = 2f; // Во сколько раз увеличить урон

    [SerializeField]
    private float boostDuration = 5f; // На сколько секунд действует бафф

    [SerializeField]
    private GameObject boostEffectPrefab; // Префаб визуального эффекта баффа

    public override void Execute(Human user)
    {
        if (user != null)
        {
            user.StartCoroutine(ApplyDamageBoost(user));
        }
    }

    private IEnumerator ApplyDamageBoost(Human user)
    {
        user.SetDamageMultiplier(boostMultiplier);

        GameObject boostEffect = null;

        if (boostEffectPrefab != null)
        {
            boostEffect = Instantiate(boostEffectPrefab, user.transform);
            boostEffect.transform.localPosition = Vector3.zero; // Центрируем эффект на персонаже
        }

        yield return new WaitForSeconds(boostDuration);

        user.SetDamageMultiplier(1f);

        if (boostEffect != null)
        {
            Destroy(boostEffect);
        }
    }
}