using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield Ability", menuName = "Abilities/Shield Ability")]
public class ShieldAbility : Ability
{
    [SerializeField]
    private float duration = 2f; // Сколько держится щит

    [SerializeField]
    private GameObject shieldEffectPrefab; // Префаб визуального эффекта щита

    public override void Execute(Human user)
    {
        if (user != null)
        {
            user.StartCoroutine(ActivateShield(user));
        }
    }

    private IEnumerator ActivateShield(Human user)
    {
        user.SetInvulnerable(true);

        GameObject shieldEffect = null;

        // Спавним визуальный эффект
        if (shieldEffectPrefab != null)
        {
            shieldEffect = Instantiate(shieldEffectPrefab, user.transform);
            shieldEffect.transform.localPosition = Vector3.zero; // Центруем на модельке игрока
        }

        yield return new WaitForSeconds(duration);

        user.SetInvulnerable(false);

        // Убираем визуальный эффект
        if (shieldEffect != null)
        {
            Destroy(shieldEffect);
        }
    }
}