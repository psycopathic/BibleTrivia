public static class DailyChallengeSession
{
    public static int LastCompletedQuestionCount { get; private set; }
    public static int PendingReward { get; private set; }
    public static bool RewardCoinsAdded { get; private set; }

    public static void SetReward(int completedQuestionCount)
    {
        LastCompletedQuestionCount = completedQuestionCount;
        PendingReward = completedQuestionCount * 10;
        RewardCoinsAdded = false;
    }

    public static void MarkRewardCoinsAdded()
    {
        RewardCoinsAdded = true;
    }

    public static void ClearReward()
    {
        LastCompletedQuestionCount = 0;
        PendingReward = 0;
        RewardCoinsAdded = false;
    }
}
