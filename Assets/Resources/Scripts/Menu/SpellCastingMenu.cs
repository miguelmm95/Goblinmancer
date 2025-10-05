using DG.Tweening;
using UnityEngine;

/// <summary>
/// Manages the spell unlock menu.
/// </summary>
public class SpellCastingMenu : BaseMenu
{
    public override void OpenMenu()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out BaseSpell spell))
            {
                spell.OpenMenu();
            }
        }
        _state = MenuState.Open;
    }

    public override void CloseMenu()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out BaseSpell spell))
            {
                Debug.Log("Closing spell menu");
                spell.CloseMenu();
            }
        }
        _state = MenuState.Closed;
    }
}