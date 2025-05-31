using System.Collections;
using UnityEngine;

public class RangedEnemy : Enemy
{
    public float rangedAttackRange = 8f;
    public float attackCooldown = 2f;
    public RangedAbility rangedAbility;

    private float lastAttackTime = -999f;

    protected override IEnumerator Start()
    {
        StartCoroutine(SeekTarget());

        while (true)
        {
            if (player != null)
            {
                float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

                if (distanceToPlayer <= rangedAttackRange)
                {
                    agent.isStopped = true;
                    animator?.SetInteger("run", 0);

                    UpdateRotationToPlayer();

                    if (Time.time - lastAttackTime >= attackCooldown)
                    {
                        animator?.SetTrigger("attack"); // если есть анимация стрельбы
                        rangedAbility.ExecuteAsEnemy(transform);
                        lastAttackTime = Time.time;
                    }
                }
                else
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.transform.position);
                    animator?.SetInteger("run", 1);
                }
            }
            else
            {
                // Патрулирование
                agent.isStopped = false;
                Vector3 randomPoint = RandomNavmeshLocation(patrolRadius);
                agent.SetDestination(randomPoint);
                animator?.SetInteger("run", 1);
                yield return new WaitUntil(() => agent.remainingDistance < 1 || player != null);
                animator?.SetInteger("run", 0);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }
}