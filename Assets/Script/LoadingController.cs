using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform rollingCross;

    [Header("Settings")]
    [SerializeField] private float loadingTime = 4f;
    [SerializeField] private string nextScene = "PuzzleScene";

    private float timer;

    private RectTransform fillRect;
    private float barWidth;

    private void Start()
    {
        fillRect = fillImage.rectTransform;

        fillImage.fillAmount = 0;

        // Width of the loading bar
        barWidth = fillRect.rect.width;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float progress = Mathf.Clamp01(timer / loadingTime);

        // Fill the bar
        fillImage.fillAmount = progress;

        // Move and rotate the cross
        MoveCross(progress);

        // Load next scene
        if (progress >= 1f)
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    private void MoveCross(float progress)
    {
        float x = (-barWidth / 2f) + (barWidth * progress);

        rollingCross.anchoredPosition = new Vector2(
            x,
            rollingCross.anchoredPosition.y
        );

        // Rotate while moving
        rollingCross.Rotate(0, 0, -360f * Time.deltaTime);
    }
}