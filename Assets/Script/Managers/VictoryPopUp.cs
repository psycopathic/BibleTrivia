using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryPopup : MonoBehaviour
{
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void GoHome()
    {
        SceneManager.LoadScene("MainScene");
    }
}