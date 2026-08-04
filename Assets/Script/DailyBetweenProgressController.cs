using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DailyBetweenProgressController : MonoBehaviour
{
    [Header("Content")]
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private TMP_Text nextLevelText;
    [SerializeField] private Button nextLevelButton;

    [Header("Animation Targets")]
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private RectTransform ribbon;
    [SerializeField] private RectTransform coinGroup;
    [SerializeField] private RectTransform loaderGroup;
    [SerializeField] private RectTransform buttonGroup;
    [SerializeField] private GameObject scroll;

    [Header("Progress")]
    [SerializeField] private Image[] loaderFills = new Image[DailyChallengeProgress.TotalQuestions];
    [SerializeField] private Graphic[] silverStars = new Graphic[DailyChallengeProgress.TotalQuestions];
    [SerializeField] private Graphic[] goldStars = new Graphic[DailyChallengeProgress.TotalQuestions];
    [SerializeField] private RectTransform[] goldStarTransforms = new RectTransform[DailyChallengeProgress.TotalQuestions];

    [Header("Timing")]
    [SerializeField] private float popupDuration = 0.25f;
    [SerializeField] private float rewardPopDuration = 0.18f;
    [SerializeField] private float loaderFillDuration = 0.45f;
    [SerializeField] private float starPopDuration = 0.22f;
    [SerializeField] private float buttonPopDuration = 0.18f;

    private CanvasGroup popupCanvasGroup;
    private Coroutine showCoroutine;

    private void Awake()
    {
        if (popupRoot == null)
        {
            popupRoot = transform as RectTransform;
        }

        popupCanvasGroup = EnsureCanvasGroup(popupRoot);

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveListener(OnNextLevel);
            nextLevelButton.onClick.AddListener(OnNextLevel);
        }
    }

    private void Start()
    {
        int completedCount = DailyChallengeSession.LastCompletedQuestionCount > 0
            ? DailyChallengeSession.LastCompletedQuestionCount
            : DailyChallengeProgress.CompletedCount;

        completedCount = Mathf.Clamp(completedCount, 0, DailyChallengeProgress.TotalQuestions);

        int reward = DailyChallengeSession.PendingReward > 0
            ? DailyChallengeSession.PendingReward
            : completedCount * 10;

        if (rewardText != null)
        {
            rewardText.text = $"x{reward}";
        }

        if (nextLevelText != null)
        {
            nextLevelText.text = "Next Level";
        }

        if (scroll != null)
        {
            scroll.SetActive(false);
        }

        AddRewardCoinsIfNeeded(reward);
        PrepareProgressState(completedCount);

        showCoroutine = StartCoroutine(ShowRoutine(completedCount));
    }

    public void OnNextLevel()
    {
        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = false;
        }

        DailyChallengeSession.ClearReward();

        if (DailyChallengeProgress.IsComplete)
        {
            ShowDailyVictoryPopup();
            return;
        }

        SceneManager.LoadScene("DailyPuzzleScene");
    }

    private void AddRewardCoinsIfNeeded(int reward)
    {
        if (reward <= 0 || DailyChallengeSession.RewardCoinsAdded)
        {
            return;
        }

        CoinManager.EnsureInstance().AddCoins(reward);
        DailyChallengeSession.MarkRewardCoinsAdded();
    }

    private void PrepareProgressState(int completedCount)
    {
        int newIndex = completedCount - 1;

        for (int i = 0; i < loaderFills.Length; i++)
        {
            if (loaderFills[i] != null)
            {
                loaderFills[i].fillAmount = i < newIndex ? 1f : 0f;
            }
        }

        for (int i = 0; i < goldStars.Length; i++)
        {
            bool alreadyComplete = i < newIndex;

            SetGraphicVisible(goldStars[i], alreadyComplete);
            SetGraphicVisible(silverStars[i], !alreadyComplete);

            if (goldStarTransforms[i] != null)
            {
                goldStarTransforms[i].localScale = alreadyComplete ? Vector3.one : Vector3.zero;
            }
        }

        SetScale(ribbon, Vector3.zero);
        SetScale(coinGroup, Vector3.zero);
        SetScale(loaderGroup, Vector3.one);
        SetScale(buttonGroup, Vector3.zero);

        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.alpha = 0f;
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = false;
        }
    }

    private IEnumerator ShowRoutine(int completedCount)
    {
        yield return FadeCanvasGroup(popupCanvasGroup, 0f, 1f, popupDuration);
        yield return PopScale(ribbon, popupDuration);
        yield return PopScale(coinGroup, rewardPopDuration);

        int newIndex = completedCount - 1;
        if (newIndex >= 0 && newIndex < loaderFills.Length)
        {
            yield return AnimateFill(loaderFills[newIndex], loaderFillDuration);
            yield return AnimateStar(newIndex);
        }

        if (completedCount >= DailyChallengeProgress.TotalQuestions)
        {
            if (buttonGroup != null)
            {
                buttonGroup.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(1f);
            ShowDailyVictoryPopup();
            yield break;
        }

        yield return PopScale(buttonGroup, buttonPopDuration);

        if (nextLevelButton != null)
        {
            nextLevelButton.interactable = true;
        }

        showCoroutine = null;
    }

    private void ShowDailyVictoryPopup()
    {
        DailyChallengeSession.ClearReward();

        if (DailyPuzzleManager.Instance == null)
        {
            Debug.LogError("[DailyBetweenProgressController] DailyPuzzleManager is missing. Returning to MainScene.", this);
            SceneManager.LoadScene("MainScene");
            return;
        }

        DailyPuzzleManager.Instance.ShowDailyVictoryAndReturnHome();
    }

    private IEnumerator AnimateFill(Image image, float duration)
    {
        if (image == null)
        {
            yield break;
        }

        float elapsed = 0f;
        image.fillAmount = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOutBack(Mathf.Clamp01(elapsed / duration), 0.7f);
            image.fillAmount = Mathf.Clamp01(t);
            yield return null;
        }

        image.fillAmount = 1f;
    }

    private IEnumerator AnimateStar(int index)
    {
        if (index < 0 || index >= goldStars.Length)
        {
            yield break;
        }

        SetGraphicVisible(silverStars[index], false);
        SetGraphicVisible(goldStars[index], true);
        yield return PopScale(goldStarTransforms[index], starPopDuration);
    }

    private IEnumerator PopScale(RectTransform target, float duration)
    {
        if (target == null)
        {
            yield break;
        }

        float elapsed = 0f;
        Vector3 overshoot = Vector3.one * 1.12f;
        target.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localScale = Vector3.LerpUnclamped(Vector3.zero, overshoot, EaseOutBack(t, 1.1f));
            yield return null;
        }

        elapsed = 0f;
        float settleDuration = Mathf.Max(0.05f, duration * 0.35f);

        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / settleDuration);
            target.localScale = Vector3.Lerp(overshoot, Vector3.one, t);
            yield return null;
        }

        target.localScale = Vector3.one;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
        {
            yield break;
        }

        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = to;
    }

    private CanvasGroup EnsureCanvasGroup(RectTransform target)
    {
        if (target == null)
        {
            return null;
        }

        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = target.gameObject.AddComponent<CanvasGroup>();
        }

        return group;
    }

    private static void SetGraphicVisible(Graphic graphic, bool visible)
    {
        if (graphic == null)
        {
            return;
        }

        Color color = graphic.color;
        color.a = visible ? 1f : 0f;
        graphic.color = color;
        graphic.gameObject.SetActive(visible);
    }

    private static void SetScale(RectTransform target, Vector3 scale)
    {
        if (target != null)
        {
            target.localScale = scale;
        }
    }

    private static float EaseOutBack(float t, float overshoot)
    {
        float shifted = t - 1f;
        return 1f + shifted * shifted * ((overshoot + 1f) * shifted + overshoot);
    }
}
