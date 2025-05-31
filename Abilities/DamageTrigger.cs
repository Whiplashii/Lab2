using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTrigger : MonoBehaviour
{
    [SerializeField]
    private float damage;

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    private void OnTriggerEnter(Collider other)
{
    Enemy enemy = other.GetComponent<Enemy>();

    if (enemy != null && !enemy.CompareTag("Player"))
    {
        Debug.Log($"DamageTrigger hit {enemy.name} with damage: {damage}");
        enemy.TakeDamage(damage);
    }
}
}
