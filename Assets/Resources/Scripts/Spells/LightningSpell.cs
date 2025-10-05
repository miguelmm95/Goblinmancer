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

        GameObject lightning = Instantiate(_lightningPrefab, enemy.transform.position + Vector3.up * 30, Quaternion.identity);
        lightning.transform.DOScale(1, 0.1f).From(0).SetEase(Ease.OutBack);
        lightning.transform.DOMove(enemy.transform.position, 0.1f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            enemy.TakeDamage(_damage);
            Destroy(lightning);
        });
    }
}