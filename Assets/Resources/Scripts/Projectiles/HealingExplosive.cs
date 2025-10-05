using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A healing explosive projectile that heals allies in an area upon impact.
/// </summary>
public class HealingExplosive : ExplosiveProjectile
{
    protected override void Hit()
    {
        if (isAlly)
        {
            List<Hittable> hittables = GameManager.Instance.GetAllAlliesInRange(transform.position, _explosionRadius);
            foreach (Hittable hittable in hittables)
            {
                hittable.TakeDamage(-_damage);
            }
        }
        else
        {
            List<Hittable> hittables = GameManager.Instance.GetAllEnemiesInRange(transform.position, _explosionRadius);
            foreach (Hittable hittable in hittables)
            {
                hittable.TakeDamage(-_damage);
            }
        }
        Destroy(gameObject);
    }
}