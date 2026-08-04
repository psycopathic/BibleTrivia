using System;
using UnityEngine;

public static class DailyChallengeProgress
{
    private const string DateKey = "DAILY_CHALLENGE_DATE";
    private const string CompletedCountKey = "DAILY_CHALLENGE_COMPLETED_COUNT";

    public const int TotalQuestions = 3;

    public static string Today => DateTime.Now.ToString("yyyy-MM-dd");

    public static int CompletedCount
    {
        get
        {
            EnsureToday();
            return Mathf.Clamp(PlayerPrefs.GetInt(CompletedCountKey, 0), 0, TotalQuestions);
        }
    }

    public static int CurrentQuestionIndex
    {
        get
        {
            EnsureToday();
            return Mathf.Clamp(CompletedCount, 0, TotalQuestions - 1);
        }
    }

    public static bool IsComplete => CompletedCount >= TotalQuestions;

    public static void PrepareForDailyStart()
    {
        EnsureToday();

        if (IsComplete)
        {
            ResetToday();
        }
    }

    public static void MarkQuestionCompleted(int completedCount)
    {
        EnsureToday();

        int clampedCount = Mathf.Clamp(completedCount, 0, TotalQuestions);
        PlayerPrefs.SetString(DateKey, Today);
        PlayerPrefs.SetInt(CompletedCountKey, clampedCount);
        PlayerPrefs.Save();
    }

    public static void ResetToday()
    {
        PlayerPrefs.SetString(DateKey, Today);
        PlayerPrefs.SetInt(CompletedCountKey, 0);
        PlayerPrefs.Save();
        DailyChallengeSession.ClearReward();
    }

    private static void EnsureToday()
    {
        if (PlayerPrefs.GetString(DateKey, string.Empty) == Today)
        {
            return;
        }

        PlayerPrefs.SetString(DateKey, Today);
        PlayerPrefs.SetInt(CompletedCountKey, 0);
        PlayerPrefs.Save();
        DailyChallengeSession.ClearReward();
    }
}
