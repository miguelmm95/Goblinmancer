using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

/// <summary>
/// A tower that generates barriers for allied units.
/// </summary>
public class BarrierGenerator : BaseTower
{
    [SerializeField] int _barrierAmount = 5;
    [SerializeField] int numberBarriers = 10;
    [SerializeField] GameObject _barrierEffectPrefab;

    /// <summary>
    /// Prepares the barrier generator by adding barriers to random allied units.
    /// </summary>
    public override void OnPrepare()
    {
        GameManager manager = GameManager.Instance;

        List<BaseUnit> allies = new List<BaseUnit>(manager.AlliedUnits);

        for (int i = 0; i < numberBarriers; i++)
        {
            allies[Random.Range(0, allies.Count)].AddOverHealth(_barrierAmount, effect: _barrierEffectPrefab);
        }
    }
}
