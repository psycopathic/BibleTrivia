using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LetterButton : MonoBehaviour,
    IPointerDownHandler,
    IPointerEnterHandler,
    IPointerUpHandler
{
    [Header("UI")]
    public TMP_Text letterText;

    private string letter;

    public string Letter => letter;

    public void SetLetter(string newLetter)
    {
        letter = newLetter;
        letterText.text = newLetter;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.StartSelection(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance.IsDragging)
        {
            GameManager.Instance.AddLetter(this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        GameManager.Instance.EndSelection();
    }
}