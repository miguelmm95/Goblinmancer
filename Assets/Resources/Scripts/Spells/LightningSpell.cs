using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// A spell that summons a lightning strike on the enemy with the highest health, dealing direct damage.
/// </summary>
public class LightningSpell : BaseSpell
{
    [SerializeField] GameObject _lightningPrefab;
    [SerializeField] float _damage = 100f;

    /// <summary>
    /// The specific effect of the lightning spell.
    /// </summary>
    protected override void Effect(Vector3 targetPosition)
    {
        Hittable enemy = GameManager.Instance.GetHighestHealthEnemy();
        if (enemy == null) return;

        GameObject lightning = Instantiate(_lightningPrefab, enemy.transform.position, Quaternion.identity);
        enemy.TakeDamage(_damage);
    }
}