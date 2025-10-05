using DG.Tweening;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Manages popup panels in the game.
/// </summary>
public class Popup : MonoBehaviour
{
    [SerializeField] GameObject _popupPanel;
    [SerializeField] float _animationDuration = 0.5f;
    [SerializeField] EventReference _menuInteractionSound;

    void Start()
    {
        _popupPanel.transform.localScale = Vector3.zero;
    }
    
    /// <summary>
    /// Opens the popup panel.
    /// </summary>
    public void OpenPopup()
    {
        _popupPanel.transform.DOScale(Vector3.one, _animationDuration).SetEase(Ease.OutBack);
        RuntimeManager.PlayOneShot(_menuInteractionSound);
    }

    /// <summary>
    /// Closes the popup panel.
    /// </summary>
    public void ClosePopup()
    {
        _popupPanel.transform.DOScale(Vector3.zero, _animationDuration).SetEase(Ease.InBack);
    }
}