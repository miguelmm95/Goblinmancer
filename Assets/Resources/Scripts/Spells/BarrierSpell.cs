using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A spell that creates protective barriers around allied units within a certain radius.
/// </summary>
public class BarrierSpell : BaseSpell
{
    [SerializeField] float _duration = 5f;
    [SerializeField] float _barrierHealth = 100f;
    [SerializeField] float _effectRadius = 5f;
    [SerializeField] GameObject _barrierPrefab;

    /// <summary>
    /// The specific effect of the protective spell.
    /// </summary>
    protected override void Effect(Vector3 position)
    {
        List<Hittable> allies = GameManager.Instance.GetAllAlliesInRange(position, _effectRadius);
        foreach (Hittable ally in allies)
        {
            GameObject barrier = Instantiate(_barrierPrefab, ally.transform);
            ally.AddOverHealth(_barrierHealth, _duration, barrier);
        }
    }
}