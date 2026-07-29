using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ProgressManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider progressSlider;

    [Header("Settings")]
    [SerializeField] private int requiredCorrectAnswers = 10;
    [SerializeField] private float fillAnimationDuration = 0.3f;

    private int correctAnswers = 0;

    private void Start()
    {
        progressSlider.minValue = 0;
        progressSlider.maxValue = requiredCorrectAnswers;
        progressSlider.value = 0;
    }

    public void AddCorrectAnswer()
    {
        if (correctAnswers >= requiredCorrectAnswers)
            return;

        correctAnswers++;

        StopAllCoroutines();
        StartCoroutine(AnimateProgress(progressSlider.value, correctAnswers));

        Debug.Log($"Correct Answers: {correctAnswers}/{requiredCorrectAnswers}");

        if (correctAnswers >= requiredCorrectAnswers)
        {
            ChestOpened();
        }
    }

    IEnumerator AnimateProgress(float startValue, float endValue)
    {
        float elapsed = 0f;

        while (elapsed < fillAnimationDuration)
        {
            elapsed += Time.deltaTime;

            progressSlider.value = Mathf.Lerp(startValue, endValue, elapsed / fillAnimationDuration);

            yield return null;
        }

        progressSlider.value = endValue;
    }

    void ChestOpened()
    {
        Debug.Log("🎉 Chest Opened!");

        // TODO:
        // Play chest animation
        // Give reward
        // Show reward popup

        StartCoroutine(ResetProgress());
    }

    IEnumerator ResetProgress()
    {
        // Wait so player can see the full bar
        yield return new WaitForSeconds(2f);

        correctAnswers = 0;

        StopAllCoroutines();
        StartCoroutine(AnimateProgress(progressSlider.value, 0));
    }

    public int GetCorrectAnswers()
    {
        return correctAnswers;
    }
}