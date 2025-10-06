using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using UnityEditor.UI;
using UnityEditor.Build;
using NUnit.Framework;
using FMODUnity;

/// <summary>
/// Base class for all units, enemy and ally
/// </summary>
public abstract class BaseUnit : Hittable
{
    public bool Dead => _dead;
    [SerializeField] protected float _movementSpeed = 10f;
    [SerializeField] protected float _cooldownBetweenAttacks = 0.2f;
    [SerializeField] float _retargetFrequecy = 1f;
    [SerializeField] protected float _spawnTweenDuration = 1f;
    [SerializeField] protected Animator _animator;
    [SerializeField] protected float _stepHeight = 0.5f;
    [SerializeField] protected float _stepDuration = 0.1f; // Time between steps when moving
    [SerializeField] protected EventReference _stepSound;
    protected float _slowMultiplier = 1f; // Multiplier for movement speed when slowed
    protected float _hastenMultiplier = 1f; // Multiplier for movement speed when hastened
    protected Hittable _target;
    protected bool _paused = true;
    protected bool _dead = false;
    protected float _currentAttackCooldown;
    protected int _currentAttackIndex = 0;
    protected List<BaseAttack> _attacks = new List<BaseAttack>();
    protected bool _moving = false;
    protected bool _stepping = false;

    /// <summary>
    /// Initializes the unit's current health and its squared size, as well as its attacks. Can be overriden by derived classes.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        _attacks = new List<BaseAttack>(GetComponents<BaseAttack>());
        foreach (var attack in _attacks)
        {
            attack.isAlly = this is AllyUnit;
        }
        _currentAttackCooldown = _cooldownBetweenAttacks;

        transform.DOScale(Vector3.one, _spawnTweenDuration).From(Vector3.zero).SetEase(Ease.InOutCubic);
    }
    /// <summary>
    /// Default Unit behavior. Finds a target and tries to attack it if in range. Can be overriden by derived classes.
    /// </summary>
    void Update()
    {
        if (_paused) return;

        if (_dead) return;

        if (_target == null || (_target is BaseUnit && (_target as BaseUnit).Dead))
        {
            FindTarget();
            return;
        }
        if (_attacks.Count == 0)
        {
            return;
        }
        if (Vector3.SqrMagnitude(transform.position - _target.transform.position) - _sizeSquared - _target.GetSizeSquared() > _attacks[_currentAttackIndex].RangeSqr)
        {
            MoveToTarget();
        }
        else
        {
            if (_moving)
            {
                _moving = false;
            }
            if (_currentAttackCooldown <= 0f)
                CheckAttack();
        }
        _currentAttackCooldown -= Time.deltaTime * _hastenMultiplier * _slowMultiplier;
    }

    /// <summary>
    /// Moves the unit towards its target. Can be overridden by derived classes for custom movement behavior.
    /// </summary>
    void MoveToTarget()
    {
        if (_target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, _movementSpeed * Time.deltaTime * _slowMultiplier * _hastenMultiplier);
            transform.LookAt(_target.transform);
        }
        if (!_moving)
        {
            _moving = true;
            StartCoroutine(SteppingCoroutine());
        }
    }

    /// <summary>
    /// Checks if the unit can attack based on its attack cooldown, and attacks if possible.
    /// </summary>
    void CheckAttack()
    {
        if (_attacks[_currentAttackIndex].CanAttack)
        {
            _attacks[_currentAttackIndex].Attack(_target);
            _target = null; // Reset target after attack to find a new one next frame
            _currentAttackCooldown = _cooldownBetweenAttacks;
            _animator.Play("Rest");
            if (_attacks[_currentAttackIndex] is VampireAttack) _animator.Play("Bite");
            else if (_attacks[_currentAttackIndex] is AreaAttack) _animator.Play("Area attack");
            else _animator.Play("Attack");
        }
    }

    /// <summary>
    /// Picks the attack with the lowest cooldown and assigns it as the current attack, then finds a target for it.
    /// </summary>
    void FindTarget()
    {
        float minAttackCooldown = float.MaxValue;
        for (int i = 0; i < _attacks.Count; i++)
        {
            var attack = _attacks[i];
            if (attack.CurrentAttackCooldown < minAttackCooldown)
            {
                minAttackCooldown = attack.CurrentAttackCooldown;
                _target = attack.GetTarget(transform.position, this);
                _currentAttackIndex = i;
            }
        }
    }

    /// <summary>
    /// Handles the unit's death, marking it as dead. Can be overridden by derived classes for custom death behavior.
    /// </summary>
    protected override void Die()
    {
        if (_dead)
        {
            Debug.LogWarning($"{gameObject.name} is already dead. Die() called again.");
            return;
        }
        _animator.Play("Rest");
        _animator.Play("Dead");
        _dead = true;
    }

    /// <summary>
    /// Pauses the unit's behavior between rounds.
    /// </summary>
    public void Pause()
    {
        _paused = true;
        foreach (var attack in _attacks)
        {
            attack.paused = true;
        }
    }

    /// <summary>
    /// Unpauses the unit's behavior.
    /// </summary>
    public void Unpause()
    {
        _paused = false;
        foreach (var attack in _attacks)
        {
            attack.paused = false;
        }
        StartCoroutine(RetargetCoroutine());
    }

    /// <summary>
    /// Applies a slow effect to the unit and its attacks for a specified duration. Multiplicative with other slows.
    /// </summary>
    /// <param name="slowAmount"></param>
    /// <param name="duration"></param>
    public void ApplySlow(float slowAmount, float duration)
    {
        _slowMultiplier *= (100f - slowAmount) / 100f;
        foreach (var attack in _attacks)
        {
            attack.ApplySlow(slowAmount);
        }
        StartCoroutine(RemoveSlowAfterDuration(duration, slowAmount));
    }

    /// <summary>
    /// Applies a hasten effect to the unit and its attacks for a specified duration. Additive with other hasten effects.
    /// </summary>
    /// <param name="hastenAmount"></param>
    /// <param name="duration"></param>
    public void ApplyHasten(float hastenAmount, float duration)
    {
        _hastenMultiplier += hastenAmount / 100f;
        foreach (var attack in _attacks)
        {
            attack.ApplyHasten(hastenAmount);
        }
        StartCoroutine(RemoveHastenAfterDuration(duration, hastenAmount));
    }

    IEnumerator RemoveSlowAfterDuration(float duration, float slowAmount)
    {
        yield return new WaitForSeconds(duration);
        _slowMultiplier /= (100f - slowAmount) / 100f;
        foreach (var attack in _attacks)
        {
            attack.RemoveSlow(slowAmount);
        }
    }
    IEnumerator RemoveHastenAfterDuration(float duration, float hastenAmount)
    {
        yield return new WaitForSeconds(duration);
        _hastenMultiplier -= hastenAmount / 100f;
        foreach (var attack in _attacks)
        {
            attack.RemoveHasten(hastenAmount);
        }
    }
    IEnumerator SteppingCoroutine()
    {
        _stepping = true;
        while (_moving && !_dead && !_paused)
        {
            while (_stepping)
            {
                if (transform.position.y >= _stepHeight)
                {
                    _stepping = false;
                    transform.position = new Vector3(transform.position.x, _stepHeight, transform.position.z);
                    break;
                }
                else
                    transform.position += Vector3.up * _stepHeight * Time.deltaTime / (_stepDuration / 2);
                yield return new WaitForEndOfFrame();
            }
            while (!_stepping)
            {
                if (transform.position.y <= 0f)
                {
                    _stepping = true;
                    transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
                    AudioManager.instance.PlayOneShot(_stepSound, transform.position);
                    break;
                }
                else
                    transform.position -= Vector3.up * _stepHeight * Time.deltaTime / (_stepDuration / 2);
                yield return new WaitForEndOfFrame();
            }
        }
    }
    IEnumerator RetargetCoroutine()
    {
        while (!_dead && !_paused)
        {
            yield return new WaitForSeconds(_retargetFrequecy);
            FindTarget();
        }
    }
}
