using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingArea : MonoBehaviour
{
    private float duration;
    private int healAmountPerTick;
    private float tickRate;

    private List<Human> humansInArea = new List<Human>();

    public void Initialize(float duration, int healAmount, float tickRate)
    {
        this.duration = duration;
        this.healAmountPerTick = healAmount;
        this.tickRate = tickRate;

        StartCoroutine(HealRoutine());
        Destroy(gameObject, duration); // Уничтожаем объект после окончания действия
    }

    private IEnumerator HealRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(tickRate);

            foreach (Human human in humansInArea)
            {
                if (human != null)
                {
                    human.Heal(healAmountPerTick);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Human human = other.GetComponent<Human>();
        if (human != null && !humansInArea.Contains(human))
        {
            humansInArea.Add(human);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Human human = other.GetComponent<Human>();
        if (human != null && humansInArea.Contains(human))
        {
            humansInArea.Remove(human);
        }
    }
}