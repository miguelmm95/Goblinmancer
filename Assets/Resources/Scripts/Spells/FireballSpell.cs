using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// A spell that summons a fireball at a target location, dealing area damage upon impact.
/// </summary>
public class FireballSpell : BaseSpell
{
    [SerializeField] GameObject _fireballPrefab;
    [SerializeField] GameObject _explosionPrefab;
    [SerializeField] float _explosionRadius = 5f;
    [SerializeField] float _damage = 20f;

    /// <summary>
    /// The specific effect of the fireball spell.
    /// </summary>
    /// <param name="targetPosition"></param>
    protected override void Effect(Vector3 targetPosition)
    {
        GameObject fireball = Instantiate(_fireballPrefab, targetPosition + Vector3.up * 30, Quaternion.identity);
        fireball.transform.DOScale(1, 1.5f).From(0).SetEase(Ease.OutBack);
        fireball.transform.forward = -Vector3.up;
        fireball.transform.DOMove(targetPosition, 1.5f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            List<Hittable> hittables = GameManager.Instance.GetAllEnemiesInRange(targetPosition, _explosionRadius);
            foreach (Hittable hittable in hittables)
            {
                hittable.TakeDamage(_damage);
            }
            Destroy(fireball);
            var explosion = Instantiate(_explosionPrefab, targetPosition, Quaternion.identity); 
            Destroy(explosion, 4f);
        });
    }
}