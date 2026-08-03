using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryPopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private Image overlayImage;
    [SerializeField] private RectTransform titleTransform;
    [SerializeField] private RectTransform copyTransform;
    [SerializeField] private RectTransform iconTransform;

    [Header("Show Animation")]
    [SerializeField] private float overlayFadeDuration = 0.2f;
    [SerializeField] private float popupScaleDuration = 0.22f;
    [SerializeField] private float popupSettleDuration = 0.12f;
    [SerializeField] private float contentFadeDuration = 0.18f;
    [SerializeField] private float contentStagger = 0.1f;
    [SerializeField] private float startScale = 0.85f;
    [SerializeField] private float overshootScale = 1.04f;
    [SerializeField] private float contentStartScale = 0.94f;
    [SerializeField] private float overlayTargetAlpha = 1f;

    [Header("Hide Animation")]
    [SerializeField] private float hideDuration = 0.16f;

    private CanvasGroup titleCanvasGroup;
    private CanvasGroup copyCanvasGroup;
    private CanvasGroup iconCanvasGroup;
    private Coroutine transitionCoroutine;
    private bool isVisible;
    private bool isTransitioning;
    private bool isInitialized;

    public float ShowDuration =>
        Mathf.Max(overlayFadeDuration, popupScaleDuration + popupSettleDuration)
        + (contentStagger * 2f)
        + contentFadeDuration;

    private void Awake()
    {
        InitializeIfNeeded();
        PrepareHiddenState();
    }

    private void Reset()
    {
        AutoAssignReferences();
    }

    public void Show()
    {
        InitializeIfNeeded();

        if (isVisible && !isTransitioning)
        {
            return;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        RestartTransition(ShowRoutine());
    }

    public void Hide()
    {
        InitializeIfNeeded();

        if (!gameObject.activeSelf || (!isVisible && !isTransitioning))
        {
            return;
        }

        RestartTransition(HideRoutine(false));
    }

    public void GoHome()
    {
        InitializeIfNeeded();

        if (!gameObject.activeSelf)
        {
            SceneManager.LoadScene("MainScene");
            return;
        }

        RestartTransition(HideRoutine(true));
    }

    private void RestartTransition(IEnumerator routine)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(routine);
    }

    private IEnumerator ShowRoutine()
    {
        isTransitioning = true;
        isVisible = true;

        gameObject.SetActive(true);
        InitializeIfNeeded();
        PrepareHiddenState();

        yield return RunInParallel(
            AnimateOverlay(0f, overlayTargetAlpha, overlayFadeDuration),
            AnimatePopupScale(startScale, overshootScale, 1f)
        );

        yield return AnimateContent(titleTransform, titleCanvasGroup);
        yield return new WaitForSeconds(contentStagger);
        yield return AnimateContent(copyTransform, copyCanvasGroup);
        yield return new WaitForSeconds(contentStagger);
        yield return AnimateContent(iconTransform, iconCanvasGroup);

        isTransitioning = false;
        transitionCoroutine = null;
    }

    private IEnumerator HideRoutine(bool loadHomeAfter)
    {
        isTransitioning = true;
        isVisible = false;

        yield return RunInParallel(
            AnimateOverlay(CurrentOverlayAlpha(), 0f, hideDuration),
            AnimateScale(popupRoot, popupRoot != null ? popupRoot.localScale : Vector3.one, Vector3.one * startScale, hideDuration),
            FadeCanvasGroup(titleCanvasGroup, titleCanvasGroup != null ? titleCanvasGroup.alpha : 0f, 0f, hideDuration),
            FadeCanvasGroup(copyCanvasGroup, copyCanvasGroup != null ? copyCanvasGroup.alpha : 0f, 0f, hideDuration),
            FadeCanvasGroup(iconCanvasGroup, iconCanvasGroup != null ? iconCanvasGroup.alpha : 0f, 0f, hideDuration)
        );

        PrepareHiddenState();
        gameObject.SetActive(false);

        isTransitioning = false;
        transitionCoroutine = null;

        if (loadHomeAfter)
        {
            SceneManager.LoadScene("MainScene");
        }
    }

    private IEnumerator AnimatePopupScale(float from, float overshoot, float settle)
    {
        if (popupRoot == null)
        {
            yield break;
        }

        yield return AnimateScale(
            popupRoot,
            Vector3.one * from,
            Vector3.one * overshoot,
            popupScaleDuration
        );

        yield return AnimateScale(
            popupRoot,
            Vector3.one * overshoot,
            Vector3.one * settle,
            popupSettleDuration
        );
    }

    private IEnumerator AnimateContent(RectTransform target, CanvasGroup group)
    {
        if (target == null || group == null)
        {
            yield break;
        }

        yield return RunInParallel(
            FadeCanvasGroup(group, 0f, 1f, contentFadeDuration),
            AnimateScale(
                target,
                Vector3.one * contentStartScale,
                Vector3.one,
                contentFadeDuration
            )
        );
    }

    private IEnumerator AnimateOverlay(float from, float to, float duration)
    {
        if (overlayImage == null)
        {
            yield break;
        }

        Color color = overlayImage.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(from, to, duration <= 0f ? 1f : elapsed / duration);
            overlayImage.color = color;
            yield return null;
        }

        color.a = to;
        overlayImage.color = color;
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
            group.alpha = Mathf.Lerp(from, to, duration <= 0f ? 1f : elapsed / duration);
            yield return null;
        }

        group.alpha = to;
    }

    private IEnumerator AnimateScale(RectTransform target, Vector3 from, Vector3 to, float duration)
    {
        if (target == null)
        {
            yield break;
        }

        float elapsed = 0f;
        target.localScale = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(from, to, duration <= 0f ? 1f : elapsed / duration);
            yield return null;
        }

        target.localScale = to;
    }

    private IEnumerator RunInParallel(params IEnumerator[] routines)
    {
        List<IEnumerator> activeRoutines = new List<IEnumerator>(routines.Length);

        foreach (IEnumerator routine in routines)
        {
            if (routine != null)
            {
                activeRoutines.Add(routine);
            }
        }

        while (activeRoutines.Count > 0)
        {
            for (int i = activeRoutines.Count - 1; i >= 0; i--)
            {
                if (!activeRoutines[i].MoveNext())
                {
                    activeRoutines.RemoveAt(i);
                }
            }

            yield return null;
        }
    }

    private void PrepareHiddenState()
    {
        SetOverlayAlpha(0f);
        SetContentState(titleTransform, titleCanvasGroup, 0f, contentStartScale);
        SetContentState(copyTransform, copyCanvasGroup, 0f, contentStartScale);
        SetContentState(iconTransform, iconCanvasGroup, 0f, contentStartScale);

        if (popupRoot != null)
        {
            popupRoot.localScale = Vector3.one * startScale;
        }
    }

    private void SetContentState(RectTransform target, CanvasGroup group, float alpha, float scale)
    {
        if (group != null)
        {
            group.alpha = alpha;
        }

        if (target != null)
        {
            target.localScale = Vector3.one * scale;
        }
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (overlayImage == null)
        {
            return;
        }

        Color color = overlayImage.color;
        color.a = alpha;
        overlayImage.color = color;
    }

    private float CurrentOverlayAlpha()
    {
        return overlayImage == null ? 0f : overlayImage.color.a;
    }

    private void AutoAssignReferences()
    {
        if (popupRoot == null)
        {
            popupRoot = GetComponent<RectTransform>();
        }

        if (overlayImage == null)
        {
            overlayImage = GetComponent<Image>();
        }

        if (titleTransform == null)
        {
            titleTransform = FindChildRectTransform("YouAreGreat");
        }

        if (copyTransform == null)
        {
            copyTransform = FindChildRectTransform("CongratulationsText");
        }

        if (iconTransform == null)
        {
            iconTransform = FindChildRectTransform("VictoryIcon");
        }
    }

    private void InitializeIfNeeded()
    {
        if (isInitialized)
        {
            return;
        }

        AutoAssignReferences();
        EnsureCanvasGroups();
        isInitialized = true;
    }

    private void EnsureCanvasGroups()
    {
        titleCanvasGroup = EnsureCanvasGroup(titleTransform);
        copyCanvasGroup = EnsureCanvasGroup(copyTransform);
        iconCanvasGroup = EnsureCanvasGroup(iconTransform);
    }

    private RectTransform FindChildRectTransform(string childName)
    {
        Transform child = transform.Find(childName);
        return child as RectTransform;
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
}
