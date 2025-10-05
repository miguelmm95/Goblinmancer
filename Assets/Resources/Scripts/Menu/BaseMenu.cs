using DG.Tweening;
using FMODUnity;
using UnityEngine;

public abstract class BaseMenu : MonoBehaviour
{
    [SerializeField] protected EventReference _menuInteractionSound;
    [SerializeField] protected RectTransform _menuTransform;
    [SerializeField] protected float _animationDuration = 0.5f;
    [SerializeField] protected Vector3 _closedPosition;
    protected MenuState _state = MenuState.Closed;

    /// <summary>
    /// Opens the menu.
    /// </summary>
    public virtual void OpenMenu()
    {
        if (_state == MenuState.Open) return;

        _state = MenuState.Open;
        _menuTransform.DOLocalMove(Vector3.zero, _animationDuration).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Closes the menu.
    /// </summary>
    public virtual void CloseMenu()
    {
        if (_state == MenuState.Closed) return;

        _state = MenuState.Closed;
        _menuTransform.DOLocalMove(_closedPosition, _animationDuration).SetEase(Ease.InBack);
    }
}