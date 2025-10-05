using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] BaseMenu _optionsMenu;
    [SerializeField] EventReference _menuInteractionSound;
    public void OpenMenu()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }
    public void CloseMenu()
    {
        Time.timeScale = 1f;
        _optionsMenu.CloseMenu();
        gameObject.SetActive(false);
    }
    public void OpenOptionsMenu()
    {
        AudioManager.instance.PlayOneShot(_menuInteractionSound, transform.position);
        _optionsMenu.OpenMenu();
    }

    public void ReturnToMenu()
    {
        AudioManager.instance.PlayOneShot(_menuInteractionSound, transform.position);
        SceneManager.LoadScene("MainMenu");
    }
}
