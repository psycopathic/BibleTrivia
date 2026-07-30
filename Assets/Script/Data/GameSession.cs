public enum GameMode
{
    Normal,
    DailyChallenge
}

public static class GameSession
{
    public static GameMode CurrentMode = GameMode.Normal;
}