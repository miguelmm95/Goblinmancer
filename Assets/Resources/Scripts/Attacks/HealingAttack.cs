using UnityEngine;

public class HealingAttack : BaseAttack
{
    public override Hittable GetTarget(Vector3 position, Hittable exclude)
    {
        if (isAlly)
        {
            Debug.Log($"{gameObject.name} is trying to heal.");
            switch (_targetingPriority)
            {
                case TargetingPriorities.Units:
                    return GameManager.Instance.GetClosestAllyUnit(position, exclude);
                case TargetingPriorities.Towers:
                    return GameManager.Instance.GetClosestTower(position, exclude);
                case TargetingPriorities.Castle:
                    return GameManager.Instance.Castle;
                case TargetingPriorities.HighestHealth:
                    return GameManager.Instance.GetHighestHealthAllyUnit(position, exclude);
                default:
                    throw new System.Exception("Invalid targeting priority.");
            }
        }
        else
        {
            switch (_targetingPriority)
            {
                case TargetingPriorities.Units:
                    return GameManager.Instance.GetClosestEnemy(position, exclude);
                case TargetingPriorities.Towers:
                    throw new System.Exception("Invalid targeting priority.");
                case TargetingPriorities.Castle:
                    throw new System.Exception("Invalid targeting priority.");
                case TargetingPriorities.HighestHealth:
                    return GameManager.Instance.GetHighestHealthEnemy(transform.position,exclude);
                default:
                    throw new System.Exception("Invalid targeting priority.");
            }
        }
    }

    /// <summary>
    /// Executes the attack on the specified target, healing them. Can be overridden by derived classes for custom attack behavior.
    /// </summary>
    protected override void AttackEffect(Hittable target)
    {
        if (target != null)
        {
            target.TakeDamage(-_damage);
        }
    }
}