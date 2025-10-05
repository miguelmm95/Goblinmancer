using System;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Base class for projectiles that move towards a target and deal damage upon impact.
/// </summary>
public class Projectile : MonoBehaviour
{
    [HideInInspector] public bool isAlly;
    [HideInInspector] public Transform target;
    [HideInInspector] public float targetSize;
    [SerializeField] protected float _speed = 10f;
    [SerializeField] protected float _size = 0.5f;
    [SerializeField] EventReference _impactSound;
    protected float _sizeSqr;
    protected float _damage = 10f;
    protected float _targetSizeSqr;

    /// <summary>
    /// Initializes the projectile's size squared for distance calculations. Can be overridden by derived classes.
    /// </summary>
    protected virtual void Start()
    {
        _sizeSqr = _size * _size;
        _targetSizeSqr = targetSize * targetSize;
    }

    /// <summary>
    /// Moves the projectile towards its target. If it reaches the target, it calls Hit(). Can be overridden by derived classes for custom movement behavior.
    /// </summary>
    protected virtual void Update()
    {
        if (GameManager.Instance.CurrentPhase != PhaseEnum.Combat)
        {
            Destroy(gameObject);
            return;
        }
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, target.position, _speed * Time.deltaTime);
        transform.LookAt(target);
        if (Vector3.SqrMagnitude(transform.position - target.position) < _sizeSqr + _targetSizeSqr)
        {
            Hit();
        }
    }

    /// <summary>
    /// Called when the projectile hits its target. Deals damage to the target if it has a Hittable component, then destroys the projectile. Can be overridden by derived classes for custom hit behavior.
    /// </summary>
    protected virtual void Hit()
    {
        Hittable hittable = target.GetComponent<Hittable>();
        AudioManager.instance.PlayOneShot(_impactSound, transform.position);
        if (hittable != null)
        {
            hittable.TakeDamage(_damage);
        }
        Destroy(gameObject);
    }

    public void SetDamage(float damage)
    {
        _damage = damage;
    }
}