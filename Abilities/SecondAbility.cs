using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Second")]
public class SecondAbility : Ability
{
    [SerializeField]
    private GameObject projectile;

    [SerializeField]
    private int amount = 1;
    
    

    public override void Execute(Human user)
    {
        float damage = user.baseDamage;
        float damageFI = user.additionalDamageFromItems;

        // Создаем луч из камеры к позиции курсора на экране
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Спавним снаряд в точке, куда указывает курсор
            Vector3 spawnPosition = hit.point;

            GameObject inst = Instantiate(projectile, spawnPosition, Quaternion.identity);

            // Устанавливаем урон через компонент DamageTrigger
            var damageable = inst.GetComponent<DamageTrigger>();
            if (damageable != null)
            {
                damageable.SetDamage(4*damage + damageFI);
            }

            Destroy(inst, 1); // Уничтожение снаряда через 3 секунды

            Debug.Log($"Ability executed at cursor position with damage: {damage}");
        }
        else
        {
            Debug.Log("Cursor did not hit any targetable surface.");
        }
    }
}

