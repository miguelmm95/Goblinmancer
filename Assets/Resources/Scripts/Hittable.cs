using System.Collections;
using FMODUnity;
using UnityEngine;

// Base class for all objects that can be hit and take damage
public class Hittable : MonoBehaviour
{
    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    [SerializeField] protected float _maxHealth = 10f;
    [SerializeField] protected float _size = .5f; //Radius of the object for range calculations
    [SerializeField] protected EventReference _deathSound;
    protected float _currentHealth;
    protected float _overHealth = 0f;
    protected float _sizeSquared;
    

    /// <summary>
    /// Initializes the object's current health to its maximum health, and its squared size. Can be overriden by derived classes.
    /// </summary>
    protected virtual void Start()
    {
        _currentHealth = _maxHealth;
        _sizeSquared = _size * _size;
    }

    /// <summary>
    /// Subtracts damage from the object's current health. If health drops to 0 or below, the object dies.
    /// Can also heal if damage is negative. Healing cannot exceed max health.
    /// If the object has a barrier, damage is subtracted from the barrier health first.
    /// </summary>
    /// <param name="damage">Amount to subtract from current health.</param>
    public virtual void TakeDamage(float damage)
    {
        if (_overHealth > 0 && damage > 0)
        {
            float overHealthUsed = Mathf.Min(_overHealth, damage);
            _overHealth -= overHealthUsed;
            damage -= overHealthUsed;
        }
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Handles the death of the object. By default, it destroys the game object. Can be overridden by derived classes for custom death behavior.
    /// </summary>
    protected virtual void Die()
    {
        AudioManager.instance.PlayOneShot(_deathSound, transform.position);
        Destroy(gameObject);
    }

    public float GetSize()
    {
        return _size;
    }

    /// <summary>
    /// Returns the size of the unit.
    /// </summary>
    public float GetSizeSquared()
    {
        return _sizeSquared;
    }

    public void AddOverHealth(float amount, float duration = 0f, GameObject effect = null)
    {
        Debug.Log($"Adding {amount} overhealth to {name} for {duration} seconds with effect {effect?.name}.");
        _overHealth += amount;
        if (effect != null)
        {
            Instantiate(effect, transform);
        }
        if (duration > 0f)
        {
            StartCoroutine(RemoveOverHealthAfterDuration(amount, duration, effect));
        }
    }

    IEnumerator RemoveOverHealthAfterDuration(float amount, float duration, GameObject effect = null)
    {
        yield return new WaitForSeconds(duration);
        _overHealth -= amount;
        if (_overHealth < 0) _overHealth = 0;
        if (effect != null)
        {
            Destroy(effect);
        }
    }
}