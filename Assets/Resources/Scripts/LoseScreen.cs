using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _roundsText;
    void Start()
    {
        _roundsText.text = "Rounds Survived: " + GameManager.Instance.CurrentRound;
    }
    public void RestartGame()
    {
        Debug.Log("Restarting game...");Debug.Log("Restarting game...");Debug.Log("Restarting game...");Debug.Log("Restarting game...");Debug.Log("Restarting game...");Debug.Log("Restarting game...");Debug.Log("Restarting game...");Debug.Log("Restarting game...");Debug.Log("Restarting game...");Debug.Log("Restarting game...");
        SceneManager.LoadScene("MainMenu");
    }
}