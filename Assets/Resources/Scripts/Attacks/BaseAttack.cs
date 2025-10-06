using System;
using UnityEngine;
using FMODUnity;

public class BaseAttack : MonoBehaviour
{
    [HideInInspector] public float _slowMultiplier = 1f; // Multiplier to slow down the attack speed
    [HideInInspector] public float _hastenMultiplier = 1f; // Multiplier to hasten the attack speed
    [HideInInspector] public bool paused = true; // Pauses the attack's cooldown when true
    public float RangeSqr => _range * _range;
    public bool CanAttack => _currentAttackCooldown <= 0f;
    public float CurrentAttackCooldown => _currentAttackCooldown;
    [HideInInspector] public bool isAlly; // True if the attack is from an ally unit, false if from an enemy unit
    [SerializeField] protected float _damage = 10f; // Damage dealt by the attack
    [SerializeField] protected float _attackCooldown = 1f; // Attack cooldown in seconds
    [SerializeField] protected float _range = 1f; // Maximum range of the attack
    [SerializeField] protected TargetingPriorities _targetingPriority = TargetingPriorities.Units; // Priority for selecting targets
    [SerializeField] protected EventReference _attackSound;
    protected float _currentAttackCooldown; // Time left until next attack

    /// <summary>
    /// Initializes the attack's cooldown based on its attack speed. Can be overridden by derived classes.
    /// </summary>
    protected void Start()
    {
        _currentAttackCooldown = _attackCooldown;
    }

    /// <summary>
    /// Updates the attack's cooldown timer. Can be overridden by derived classes.
    /// </summary>
    protected void Update()
    {
        if (paused) return;

        _currentAttackCooldown -= Time.deltaTime * _hastenMultiplier * _slowMultiplier;
    }

    /// <summary>
    /// Gets the target for the attack based on the attacks's targeting priority. Can be overridden by derived classes.
    /// </summary>
    public virtual Hittable GetTarget(Vector3 position, Hittable exclude)
    {
        if (isAlly)
        {
            switch (_targetingPriority)
            {
                case TargetingPriorities.Units:
                    return GameManager.Instance.GetClosestEnemy(position);
                case TargetingPriorities.Towers:
                    throw new System.Exception("Allies cannot target towers.");
                case TargetingPriorities.Castle:
                    throw new System.Exception("Allies cannot target the castle.");
                case TargetingPriorities.HighestHealth:
                    return GameManager.Instance.GetHighestHealthEnemy(transform.position);
                default:
                    throw new System.Exception("Invalid targeting priority.");
            }
        }
        else
        {
            switch (_targetingPriority)
            {
                case TargetingPriorities.Units:
                    return GameManager.Instance.GetClosestAllyUnit(position);
                case TargetingPriorities.Towers:
                    return GameManager.Instance.GetClosestTower(position);
                case TargetingPriorities.Castle:
                    return GameManager.Instance.Castle;
                case TargetingPriorities.HighestHealth:
                    return GameManager.Instance.GetHighestHealthAllyUnit(position);
                default:
                    throw new System.Exception("Invalid targeting priority.");
            }
        }
    }

    public void Attack(Hittable target)
    {
        if (target == null) return;

        AttackEffect(target);
        _currentAttackCooldown = _attackCooldown;
        AudioManager.instance.PlayOneShot(_attackSound, transform.position);
    }

    /// <summary>
    /// Executes the attack on the specified target. Can be overridden by derived classes for custom attack behavior.
    /// </summary>
    protected virtual void AttackEffect(Hittable target)
    {
        if (target != null)
        {
            target.TakeDamage(_damage);
        }
    }

    /// <summary>
    /// Applies a slow effect to the attack's speed for a specified duration. Multiplicative with other slows.
    /// </summary>
    /// <param name="slowAmount"></param>
    public void ApplySlow(float slowAmount)
    {
        _slowMultiplier *= (100f - slowAmount) / 100f;
    }

    /// <summary>
    /// Removes a previously applied slow effect from the attack's speed.
    /// </summary>
    /// <param name="slowAmount"></param>
    public void RemoveSlow(float slowAmount)
    {
        _slowMultiplier /= (100f - slowAmount) / 100f;
    }

    /// <summary>
    /// Applies a hasten effect to the attack's speed. Additive with other hasten effects.
    /// </summary>
    /// <param name="hastenAmount"></param>
    public void ApplyHasten(float hastenAmount)
    {
        _hastenMultiplier += hastenAmount / 100f;
    }

    /// <summary>
    /// Removes a previously applied hasten effect from the attack's speed.
    /// </summary>
    /// <param name="hastenAmount"></param>
    public void RemoveHasten(float hastenAmount)
    {
        _hastenMultiplier -= hastenAmount / 100f;
    }
}