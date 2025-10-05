using UnityEngine;

/// <summary>
/// A tower that allows players to purchase and unlock spells.
/// </summary>
public class GoblinomancyStudy : BaseTower
{
    public override void OnInteract()
    {
        if (_paused) return;

        base.OnInteract();
        GameManager.Instance.OpenSpellUnlockMenu();
    }
}
