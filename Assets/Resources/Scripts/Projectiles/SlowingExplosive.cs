using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A slowing explosive projectile that damages and slows enemies in an area upon impact.
/// </summary>
public class SlowingExplosive : ExplosiveProjectile
{
    [SerializeField] float _slowAmount = 30f; // Percentage to slow
    [SerializeField] float _slowDuration = 1f; // Duration of the slow effect in seconds

    protected override void Hit()
    {
        if (isAlly)
        {
            List<Hittable> hittables = GameManager.Instance.GetAllEnemiesInRange(transform.position, _explosionRadius);
            foreach (Hittable hittable in hittables)
            {
                hittable.TakeDamage(_damage);
                if (hittable is BaseUnit unit)
                {
                    unit.ApplySlow(_slowAmount, _slowDuration);
                }
            }
        }
        else
        {
            Debug.Log($"{name}: Transform: {transform.position} Explosion Radius: {_explosionRadius} GameManager: {GameManager.Instance}");
            List<Hittable> hittables = GameManager.Instance.GetAllAlliesInRange(transform.position, _explosionRadius);
            foreach (Hittable hittable in hittables)
            {
                hittable.TakeDamage(_damage);
                if (hittable is BaseUnit unit)
                {
                    unit.ApplySlow(_slowAmount, _slowDuration);
                }
            }
        }
        Destroy(gameObject);
    }
}