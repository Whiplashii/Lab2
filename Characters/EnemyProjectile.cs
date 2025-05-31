using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField]
    private float damage;

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }

    private void OnTriggerEnter(Collider other)
    {
        Human player = other.GetComponent<Human>();
        if (player != null && other.CompareTag("Player"))
        {
            Debug.Log($"Enemy projectile hit player for {damage} damage");
            player.DealDamage(damage);
        }
    }
}