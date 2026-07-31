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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartSelection(this);
        }
        else if (DailyPuzzleManager.Instance != null)
        {
            DailyPuzzleManager.Instance.StartSelection(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsDragging)
        {
            GameManager.Instance.AddLetter(this);
        }
        else if (DailyPuzzleManager.Instance != null && DailyPuzzleManager.Instance.IsDragging)
        {
            DailyPuzzleManager.Instance.AddLetter(this);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndSelection();
        }
        else if (DailyPuzzleManager.Instance != null)
        {
            DailyPuzzleManager.Instance.EndSelection();
        }
    }
}