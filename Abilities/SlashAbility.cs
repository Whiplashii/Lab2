using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SlashAbility")]
public class SlashAbility : Ability
{
    [SerializeField]
    private GameObject slashEffect; // Префаб с анимацией удара

    public override void Execute(Human user)
    {
        float damage = user.baseDamage;
        float damageFI = user.additionalDamageFromItems;

        // Создаем эффект прямо перед персонажем
        Vector3 spawnPosition = user.transform.position + user.transform.forward * 1.5f;
        GameObject slash = Instantiate(slashEffect, spawnPosition, user.transform.rotation);

        // Настраиваем урон через DamageTrigger
        var damageable = slash.GetComponent<DamageTrigger>();
        if (damageable != null)
        {
            damageable.SetDamage(4 * damage + damageFI);
        }

        Destroy(slash, 0.5f); // Удаляем эффект через полсекунды

        Debug.Log($"Slash executed with damage: {4 * damage + damageFI}");
    }
}