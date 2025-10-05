using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using UnityEditor.UI;
using FMODUnity;

/// <summary>
/// Represents an allied unit that can be spawned and managed by cemeteries.
/// </summary>
public class AllyUnit : BaseUnit
{
    [HideInInspector] public Cemetery cemetery;
    [HideInInspector] public Vector2 spawnSize = new Vector2(15f, 15f); // Size of the spawn area
    [SerializeField] public AllyUnitsEnum unitType; // Type of the unit, used for identifying it
    [SerializeField] EventReference _spawnSound;

    /// <summary>
    /// Initializes the ally unit and plays spawn effects.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        AudioManager.instance.PlayOneShot(_spawnSound, transform.position);
        transform.DOMove(cemetery.UnitSpawnPoint.position + new Vector3(
            Random.Range(-spawnSize.x * 0.5f, spawnSize.x * 0.5f),
            0,
            Random.Range(-spawnSize.y * 0.5f, spawnSize.y * 0.5f)
        ), _spawnTweenDuration).SetEase(Ease.InOutCubic);
    }

    /// <summary>
    /// Handles the destruction of the ally unit, removing it from its cemetery.
    /// </summary>
    void OnDestroy()
    {
        if (cemetery == null)
        {
            return;
        }
        else
        {
            Debug.Log("Removing unit from cemetery: " + unitType);
            cemetery.RemoveUnit(this);
        }
    }

    /// <summary>
    /// Resets the unit's state for reuse.
    /// </summary>
    public void Reset()
    {
        if (_dead) return;

        if (cemetery == null)
        {
            _dead = true;
            return;
        }

        _currentHealth = _maxHealth;
        _target = null;
        _slowMultiplier = 1f;
        _hastenMultiplier = 1f;
        foreach (var attack in _attacks)
        {
            attack.paused = true;
            attack._slowMultiplier = 1f;
            attack._hastenMultiplier = 1f;
        }
        transform.DOMove(cemetery.UnitSpawnPoint.position + new Vector3(
            Random.Range(-spawnSize.x * 0.5f, spawnSize.x * 0.5f),
            0,
            Random.Range(-spawnSize.y * 0.5f, spawnSize.y * 0.5f)
        ), _spawnTweenDuration).SetEase(Ease.InOutCubic);
    }

    /// <summary>
    /// Changes the cemetery managing this unit.
    /// </summary>
    public void ChangeCemetery(Cemetery newCemetery)
    {
        if (cemetery != null)
        {
            cemetery.RemoveUnit(this);
        }
        newCemetery.AddUnit(this);
        Reset();
    }
}
