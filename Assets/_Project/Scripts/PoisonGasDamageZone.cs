using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class PoisonGasDamageZone : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField, Min(0f)] private float damagePerSecond = 10f;

    private bool isActive = true;
    public bool IsActive => isActive;

    private void OnTriggerStay(Collider other)
    {

        if (!isActive)
        {
            return;
        }

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();


        if (health == null || health.IsDead)
        {
            return;
        }



        float damage = damagePerSecond * Time.fixedDeltaTime;

        health.TakeDamage(damage);

    }

    public void SetDamageActive(bool active)
    {
        isActive = active;
    }
}
