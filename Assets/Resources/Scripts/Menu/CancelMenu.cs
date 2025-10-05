using DG.Tweening;
using UnityEngine;

public class CancelMenu : BaseMenu
{
    [SerializeField] CameraController _cameraController;

    /// <summary>
    /// Closes the current menu and returns to the normal game state. 
    /// </summary>
    public void Cancel()
    {
        GameManager.Instance.ShowHUD();
        _cameraController.ExitState();
    }
}
