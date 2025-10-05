using System.Collections.Generic;
using FMODUnity;
using UnityEngine;

/// <summary>
/// A tower that can attack enemies using its equipped attacks.
/// </summary>
public class AttackingTower : BaseTower
{
    [SerializeField] protected float _cooldownBetweenAttacks; // Time between attacks
    float _currentAttackCooldown; // Time left until next attack
    readonly List<BaseAttack> _attacks = new List<BaseAttack>();

    /// <summary>
    /// Initializes the attacking tower by gathering its attacks and setting the initial cooldown.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        _attacks.AddRange(GetComponents<BaseAttack>());
        foreach (BaseAttack attack in _attacks)
        {
            attack.isAlly = true;
        }
        _currentAttackCooldown = _cooldownBetweenAttacks;
        Pause();
    }

    /// <summary>
    /// Handles the attack logic, checking if the tower can attack and executing attacks as needed.
    /// </summary>
    void Update()
    {
        if (_paused) return;

        if (_currentAttackCooldown > 0f)
        {
            _currentAttackCooldown -= Time.deltaTime;
        }
        else
        {
            foreach (BaseAttack attack in _attacks)
            {
                CheckAttack(attack);
                _currentAttackCooldown = _cooldownBetweenAttacks;
                break;
            }
        }
    }

    /// <summary>
    /// Checks if the given attack can be executed and performs the attack if possible.
    /// </summary>
    /// <param name="attack"></param>
    protected void CheckAttack(BaseAttack attack)
    {
        Debug.Log($"Tower {name} is checking attack {attack.GetType().Name}");
        if (attack == null) return;
        if (attack.CanAttack)
        {
            Debug.Log($"Tower {name} is attacking with {attack.GetType().Name}");
            attack.Attack(attack.GetTarget(transform.position, null));
        }
    }

    public override void Pause()
    {
        base.Pause();
        foreach (BaseAttack attack in _attacks)
        {
            attack.paused = true;
        }
    }
    public override void Unpause()
    {
        base.Unpause();
        foreach (BaseAttack attack in _attacks)
        {
            attack.paused = false;
        }
    }
}