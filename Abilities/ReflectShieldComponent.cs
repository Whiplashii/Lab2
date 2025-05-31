using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectShieldComponent : MonoBehaviour
{
    private float expireTime;

    public void Initialize(float duration)
    {
        expireTime = Time.time + duration;
    }

    public bool IsActive => Time.time < expireTime;

    public void TryReflect(float damage, GameObject attacker)
{
    if (!IsActive || attacker == null)
        return;

    if (attacker.TryGetComponent<Enemy>(out var enemy))
    {
        Debug.Log($"Reflecting {damage} back to {attacker.name}");
        enemy.TakeDamage(damage);
    }
    else
    {
        Debug.LogWarning($"Attacker {attacker.name} is not an Enemy");
    }
}
}
