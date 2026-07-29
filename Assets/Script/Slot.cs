using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    [SerializeField] private Image filledBackground;
    [SerializeField] private TMP_Text letterText;

    private void Awake()
    {
        Clear();
    }

    public void SetLetter(string letter)
    {
        filledBackground.gameObject.SetActive(true);

        letterText.gameObject.SetActive(true);
        letterText.text = letter;
    }

    public void Clear()
    {
        Debug.Log($"Filled={filledBackground}, Text={letterText}");
        filledBackground.gameObject.SetActive(false);

        letterText.text = "";
        letterText.gameObject.SetActive(false);
    }
}