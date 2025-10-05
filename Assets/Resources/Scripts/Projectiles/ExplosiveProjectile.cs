using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A projectile that follows a parabolic trajectory and explodes upon impact, dealing area damage.
/// </summary>
public class ExplosiveProjectile : Projectile
{
    [SerializeField] protected float _explosionRadius = 5f;
    [SerializeField, Min(0.1f)] float _curvature = 0.5f;
    float a, b, c; // Variables for parabolic equation
    Vector3 startPosition;
    Vector3 targetPosition;

    /// <summary>
    /// Initializes the projectile's size squared and calculates the parabolic trajectory coefficients. Can be overridden by derived classes.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        startPosition = transform.position;
        targetPosition = target.position;

        Vector3 direction = (targetPosition - startPosition).normalized;
        targetPosition -= direction * targetSize; // Adjust target position to hit the edge of the target

        // Calculate coefficients for parabolic trajectory
        Vector2 end = new Vector2(Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z)), targetPosition.y);
        Vector2 middle = new Vector2(end.x / 2, Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z)) * _curvature);
        CalculateParabolaCoefficients(transform.position.y, middle, end);
    }

    /// <summary>
    /// Calculates the coefficients a, b, and c for the parabolic equation y = ax^2 + bx + c
    /// given three points: the start point, a middle point, and the end point.
    /// </summary>
    void CalculateParabolaCoefficients(float startY, Vector2 middle, Vector2 end)
    {
        float denom = middle.x * middle.x * end.x - end.x * end.x * middle.x;

        if (Mathf.Abs(denom) < Mathf.Epsilon)
        {
            throw new System.Exception("Points are collinear; cannot compute unique parabola.");
        }

        a = ((middle.y - startY) * end.x - (end.y - startY) * middle.x) / denom;
        b = ((end.y - startY) * middle.x * middle.x - (middle.y - startY) * end.x * end.x) / denom;
        c = startY;
    }

    /// <summary>
    /// Moves the projectile towards its target in a parabolic arc. If it reaches the target, it calls Hit(). Can be overridden by derived classes for custom movement behavior.
    /// </summary>
    protected override void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Move towards target horizontally
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPosition.x, transform.position.y, targetPosition.z), _speed * Time.deltaTime);

        // Calculate new height using parabolic equation
        float distanceToStart = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(startPosition.x, 0, startPosition.z));
        float newY = a * distanceToStart * distanceToStart + b * distanceToStart + c;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        transform.LookAt(target);
        if (Vector3.SqrMagnitude(transform.position - targetPosition) < _sizeSqr)
        {
            Hit();
        }
    }

    /// <summary>
    /// Called when the projectile hits its target. Deals area damage to all Hittable objects within the explosion radius, then destroys the projectile. Can be overridden by derived classes for custom hit behavior.
    /// </summary>
    protected override void Hit()
    {
        if (isAlly)
        {
            List<Hittable> hittables = GameManager.Instance.GetAllEnemiesInRange(transform.position, _explosionRadius);
            foreach (Hittable hittable in hittables)
            {
                hittable.TakeDamage(_damage);
            }
        }
        else
        {
            List<Hittable> hittables = GameManager.Instance.GetAllAlliesInRange(transform.position, _explosionRadius);
            foreach (Hittable hittable in hittables)
            {
                hittable.TakeDamage(_damage);
            }
        }
        Destroy(gameObject);
    }
}