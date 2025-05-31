using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/TripleRanged")]
public class TripleRangedAbility : Ability
{
    [SerializeField]
    private GameObject projectile;

    [SerializeField]
    private float speed = 15;

    [SerializeField]
    private float angleOffset = 15f; // Угол отклонения боковых снарядов

    public override void Execute(Human user)
    {
        float damage = user.baseDamage;
        float damageFI = user.additionalDamageFromItems;

        
        FireProjectile(user, 0, damage + damageFI); // Центральный
        
        FireProjectile(user, -angleOffset, damage + damageFI); // Левый
        FireProjectile(user, angleOffset, damage + damageFI); // Правый
    }

    private void FireProjectile(Human user, float angle, float damage)
    {
        // Определяем направление снаряда
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up) * user.transform.rotation;
        Vector3 direction = rotation * Vector3.forward; // Теперь direction точно корректный

        // Создаём снаряд
        GameObject inst = Instantiate(projectile, user.transform.position + direction, rotation);

        // Устанавливаем урон
        var damageable = inst.GetComponent<DamageTrigger>();
        if (damageable != null)
        {
            damageable.SetDamage(damage);
        }

        // Добавляем Rigidbody перед изменением скорости
        Rigidbody rb = inst.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = inst.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.velocity = direction * speed; // Устанавливаем скорость в правильном направлении

        // Удаляем снаряд через 3 секунды
        Destroy(inst, 3);
    }
}