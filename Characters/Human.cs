using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class Human : MonoBehaviour
{
    private Vector3 destination;
    public int moveSpeed = 3;
    public float health = 100;
    public float mana = 100;

    private bool isInvulnerable = false;

    [Header("Stats")]
    [SerializeField]
    public float manaRegenSpeed = 25;
    [SerializeField]
    private float healthRegenSpeed = 25;

    private float baseHealth;
    private float baseMana;
    private float baseManaRegenSpeed;
    private float baseHealthRegenSpeed;
	private float maxMana;
	private float maxHealth;
    public float baseDamage = 10;
    public float additionalDamageFromItems = 0;

     private float damageMultiplier = 1f;

    [SerializeField] private Image SecondAbilityIcon;
	
    private float SecondSpellcooldown = 2f; // Кулдаун в секундах
    private float SecondSpelllastUseTime; // Время последнего использования
    public bool IsDead { get; private set; } = false;

    public Ability[] Firstabilities;
    public Ability[] Secondbilities;

    private Ability[] baseFirstAbilities;
    private Ability[] baseSecondAbilities;

    [SerializeField]
    private Animator animator;

    public event Action<Human> OnHealthChanged = delegate { };
    public event Action<Human> OnManaChanged = delegate { };
    public event Action<Human> OnDied = delegate { };

    private void Awake()
    {
        baseFirstAbilities = Firstabilities.ToArray();
        baseSecondAbilities = Secondbilities.ToArray();
        Move(transform.position);

        baseHealth = health;
        baseMana = mana;
		maxMana = mana;
		maxHealth=health;
        baseManaRegenSpeed = manaRegenSpeed;
        baseHealthRegenSpeed = healthRegenSpeed;

        
    }

    public void Move(Vector3 destination)
    {
        this.destination = destination;
    }

    public void DealDamage(float v)
    {
        if (isInvulnerable)
        {
            // Игнорируем урон
            return;
        }

        if (IsDead) return; // Если персонаж уже мёртв, игнорируем урон

        health -= v;
        OnHealthChanged(this);

        if (health <= 0)
        {
            Die(); // Вызов метода смерти
        }
    }

    public void Die()
{
    OnDied(this); // Событие смерти

    if (CompareTag("Player")) // Проверяем, является ли объект игроком
    {
        // Скрыть объект игрока (отключить визуализацию и взаимодействие)
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
            renderer.enabled = false; // Прячем рендерер

        Collider collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false; // Отключаем коллайдер, чтобы нельзя было взаимодействовать
        

        // Получаем точку возрождения и запускаем таймер на возрождение
        Vector3 respawnPosition = RespawnManager.Instance.GetRespawnPoint();
        UIManager.Instance.StartRespawnCountdown(1f, () => Respawn(respawnPosition));
    }
    else
    {
        // Логика для врагов (например, уничтожение объекта)
        Destroy(gameObject);
    }
}
    

    public void Respawn(Vector3 respawnPosition)
    {
        health = maxHealth;
        mana = maxMana;
        OnHealthChanged(this);
        OnManaChanged(this);

        // Восстанавливаем видимость и взаимодействие
        GetComponentInChildren<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;

        // Возвращаем игрока на стартовую позицию (или сохранённую)
        transform.position = respawnPosition;

        
    }

    public void Attack(Human player)
    {
        animator.SetTrigger("attack");
        player.DealDamage(20);
        Rotate(player.transform.position - transform.position);
    }

    public float GetCurrentDamage()
    {
        return baseDamage + additionalDamageFromItems;
    }


    public void Rotate(Vector3 forwad, float lerp = 1)
    {
        Vector3 rot = forwad;
        rot.y = 0;
        if(lerp == 1)
        {
            transform.rotation = Quaternion.LookRotation(rot);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(rot), lerp);
        }
    }

    public void UseFirstAbility(int abilitySlot)
    {
        Ability abilityData = Firstabilities[abilitySlot];
        

        if (Firstabilities!=null && mana >= abilityData.Mana && abilitySlot == 0)
        {
            
            AddMana(-abilityData.Mana);
            animator?.SetTrigger("skill");
            abilityData.Execute(this);
        }
        
    }

    public void UseSecondAbility(int abilitySlot)
    {
        
        Ability abilityData = Secondbilities[abilitySlot];

        if (Secondbilities!=null && mana >= abilityData.Mana && abilitySlot == 0)
        {
            
            if (Time.time - SecondSpelllastUseTime >= SecondSpellcooldown)
            {
                animator?.SetTrigger("skill");
                abilityData.Execute(this);
                AddMana(-abilityData.Mana);
                SecondSpelllastUseTime = Time.time;

                // Затухание иконки для второго умения
                if (SecondAbilityIcon != null) // Убедимся, что иконка подключена
                {
                    StartCoroutine(IconCooldownEffect(SecondAbilityIcon, SecondSpellcooldown));
                }
            }
        }
    }

    private IEnumerator IconCooldownEffect(Image icon, float cooldown)
    {
        // Устанавливаем полупрозрачность на время кулдауна
        Color originalColor = icon.color;
        icon.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);

        // Ждём окончания кулдауна
        yield return new WaitForSeconds(cooldown);

        // Восстанавливаем цвет
        icon.color = originalColor;
    }

    public void UseDash(int abilitySlot)
    {
        Ability abilityData = Firstabilities[abilitySlot];
        if (mana >= abilityData.Mana)
        {
            AddMana(-15);
            
            
        }
    }

    private void Update()
    {
        Vector3 flatDestination = destination;
        flatDestination.y = transform.position.y;

        if (Vector3.Distance(transform.position, flatDestination) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, flatDestination, moveSpeed * Time.deltaTime);

            if (animator != null)
            {
                animator?.SetInteger("run", 1);
            }
        }
        else
        {
            if (animator != null)
            {
                animator?.SetInteger("run", 0);
            }
        }


        if(mana < 1000)
        {
            AddMana(Time.deltaTime * manaRegenSpeed);
        }

        if (health < 1000)
        {
            AddHealth(Time.deltaTime * healthRegenSpeed);
        }
    }
    private void AddHealth(float value)
    {
        health = Mathf.Clamp(health + value, 0, maxHealth );
        OnHealthChanged(this);
    }

    private void AddMana(float value)
    {
        mana = Mathf.Clamp(mana + value, 0, maxMana);
        OnManaChanged(this);
    }

	private void ChangeMaxMana(float newValue)
	{
		maxMana = newValue;
	}
	private void ChangeMaxHealth(float newValue)
	{
		maxHealth = newValue;
	}
	
    public void ApplyItemStats(ItemSO item)
{
	ChangeMaxHealth(maxHealth + item.healthBonus);
    health += item.healthBonus;
	ChangeMaxMana(maxMana + item.manaBonus);
    mana += item.manaBonus;
    healthRegenSpeed += item.healthRegenBonus;
    manaRegenSpeed += item.manaRegenBonus;
    additionalDamageFromItems += item.damageModifier;
}

public void RemoveItemStats(ItemSO item)
{
    ChangeMaxHealth(maxHealth - item.healthBonus);
    health -= item.healthBonus;
    ChangeMaxMana(maxMana - item.manaBonus);
    mana -= item.manaBonus;
    healthRegenSpeed -= item.healthRegenBonus;
    manaRegenSpeed -= item.manaRegenBonus;
    additionalDamageFromItems -= item.damageModifier;
    Debug.Log($"Item stats applied: {item.damageModifier} damage. New BaseDamage: {baseDamage}");
}





public void UpdateAbilitiesFromEquipment(InventoryManager inventory)
{
    Dictionary<string, string> equipmentAbilities = new Dictionary<string, string>
    {
        { "Magic Helmet", "Triple Ranged Ability" },
        { "slashing cap", "Slash Ability" },
        { "Shielding", "Purple Shield" },
        { "Teleporting", "New Teleport Ability" },
        { "Booster", "New Damage Boost Ability" },
        { "Healer", "New Healing Area Ability" },
        { "Lightning", "mini aoe" },
        { "Том Пламенных Сфер", "Shotgun" }
    };

    // Начинаем с дефолтных способностей
    List<Ability> updatedFirstAbilities = baseFirstAbilities.ToList();
    List<Ability> updatedSecondAbilities = baseSecondAbilities.ToList();

    Ability helmetAbility = null;

    foreach (var entry in equipmentAbilities)
    {
        string itemName = entry.Key;
        string abilityName = entry.Value;

        Ability ability = FindAbilityByName(abilityName);
        if (ability == null) continue;

        bool isEquippedHelmet = inventory.helmetSlot != null
            && inventory.helmetSlot.GetComponent<InventorySlot>().heldItem != null
            && inventory.helmetSlot.GetComponent<InventorySlot>().heldItem.GetComponent<InventoryItem>()?.itemScriptableObject.name == itemName;

        bool isEquippedArmor = inventory.armorSlot != null
            && inventory.armorSlot.GetComponent<InventorySlot>().heldItem != null
            && inventory.armorSlot.GetComponent<InventorySlot>().heldItem.GetComponent<InventoryItem>()?.itemScriptableObject.name == itemName;

        bool isEquippedWeapon = inventory.weaponSlot != null
            && inventory.weaponSlot.GetComponent<InventorySlot>().heldItem != null
            && inventory.weaponSlot.GetComponent<InventorySlot>().heldItem.GetComponent<InventoryItem>()?.itemScriptableObject.name == itemName;

        if (isEquippedHelmet)
        {
            // Если это шлем — вставляем в FirstAbilities в начало
            if (!updatedFirstAbilities.Contains(ability))
            {
                updatedFirstAbilities.Insert(0, ability);
            }
            helmetAbility = ability;
        }
        else if (isEquippedArmor || isEquippedWeapon)
        {
            // Если это броня/оружие — вставляем во SecondAbilities
            if (!updatedSecondAbilities.Contains(ability))
            {
                updatedSecondAbilities.Insert(0, ability);
            }
        }
    }

    // Применяем списки
    Firstabilities = updatedFirstAbilities.ToArray();
    Secondbilities = updatedSecondAbilities.ToArray();
}


    private Ability FindAbilityByName(string abilityName)
    {
        return Resources.Load<Ability>($"Abilities/{abilityName}");
    }

    public void SetInvulnerable(bool value)
    {
        isInvulnerable = value;
        // Возможно, тут ещё активировать/деактивировать визуальные эффекты
    }

    public bool IsInvulnerable()
    {
        return isInvulnerable;
    }

    public void Heal(int amount)
    {
        // Здесь можно добавить проверку на максимальное здоровье
        health = Mathf.Min(maxHealth, health + amount);
    }

    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    public int CalculateDamage(int baseDamage)
    {
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    public void DealDamage(Enemy enemy, int baseDamage)
    {
        if (enemy != null)
        {
            int totalDamage = CalculateDamage(baseDamage);
            enemy.TakeDamage(totalDamage);
        }
    }

}
