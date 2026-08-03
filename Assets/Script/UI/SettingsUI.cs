using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject popup;
    [SerializeField] private RectTransform popupRoot;
    [SerializeField] private Image overlayImage;

    [Header("Show Animation")]
    [SerializeField] private float overlayFadeDuration = 0.2f;
    [SerializeField] private float popupScaleDuration = 0.22f;
    [SerializeField] private float popupSettleDuration = 0.12f;
    [SerializeField] private float startScale = 0.85f;
    [SerializeField] private float overshootScale = 1.04f;
    [SerializeField] private float overlayTargetAlpha = 0.55f;

    [Header("Hide Animation")]
    [SerializeField] private float hideDuration = 0.16f;

    private Coroutine transitionCoroutine;
    private bool isVisible;
    private bool isTransitioning;

    private void Awake()
    {
        AutoAssignReferences();
        PrepareHiddenState();
        SetOverlayRaycastTarget(false);

        if (popup != null)
        {
            popup.SetActive(false);
        }
    }

    private void Reset()
    {
        AutoAssignReferences();
    }

    public void Open()
    {
        AutoAssignReferences();

        if (popup == null)
        {
            Debug.LogError("[SettingsUI] Popup is not assigned.", this);
            return;
        }

        if (isVisible && !isTransitioning)
        {
            return;
        }

        popup.SetActive(true);
        RestartTransition(ShowRoutine());
    }

    public void Close()
    {
        if (popup == null || (!popup.activeSelf && !isTransitioning))
        {
            return;
        }

        RestartTransition(HideRoutine(false, false));
    }

    public void GoHome()
    {
        if (popup == null || !popup.activeSelf)
        {
            SceneManager.LoadScene("MainScene");
            return;
        }

        RestartTransition(HideRoutine(true, false));
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

        if (popup != null && popup.activeSelf)
        {
            RestartTransition(HideRoutine(false, true));
            return;
        }

        QuitApplication();
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

        PrepareHiddenState();
        SetOverlayRaycastTarget(true);

        yield return RunInParallel(
            AnimateOverlay(0f, overlayTargetAlpha, overlayFadeDuration),
            AnimatePopupScale(startScale, overshootScale, 1f)
        );

        isTransitioning = false;
        transitionCoroutine = null;
    }

    private IEnumerator HideRoutine(bool loadHomeAfter, bool quitAfter)
    {
        isTransitioning = true;
        isVisible = false;

        yield return RunInParallel(
            AnimateOverlay(CurrentOverlayAlpha(), 0f, hideDuration),
            AnimateScale(popupRoot, popupRoot != null ? popupRoot.localScale : Vector3.one, Vector3.one * startScale, hideDuration)
        );

        PrepareHiddenState();
        SetOverlayRaycastTarget(false);

        if (popup != null)
        {
            popup.SetActive(false);
        }

        isTransitioning = false;
        transitionCoroutine = null;

        if (loadHomeAfter)
        {
            SceneManager.LoadScene("MainScene");
            yield break;
        }

        if (quitAfter)
        {
            QuitApplication();
        }
    }

    private IEnumerator AnimatePopupScale(float from, float overshoot, float settle)
    {
        if (popupRoot == null)
        {
            yield break;
        }

        yield return AnimateScale(popupRoot, Vector3.one * from, Vector3.one * overshoot, popupScaleDuration);
        yield return AnimateScale(popupRoot, Vector3.one * overshoot, Vector3.one * settle, popupSettleDuration);
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

        if (popupRoot != null)
        {
            popupRoot.localScale = Vector3.one * startScale;
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

    private void SetOverlayRaycastTarget(bool blocksRaycasts)
    {
        if (overlayImage != null)
        {
            overlayImage.raycastTarget = blocksRaycasts;
        }
    }

    private float CurrentOverlayAlpha()
    {
        return overlayImage == null ? 0f : overlayImage.color.a;
    }

    private void AutoAssignReferences()
    {
        if (popupRoot == null && popup != null)
        {
            popupRoot = popup.GetComponent<RectTransform>();
        }

        if (overlayImage == null && popup != null)
        {
            overlayImage = popup.GetComponent<Image>();
        }
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
