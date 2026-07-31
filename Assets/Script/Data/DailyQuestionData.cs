using System;
using System.Collections.Generic;

[Serializable]
public class DailyQuestionData
{
    public List<DailyDay> days;
}

[Serializable]
public class DailyDay
{
    public string date;
    public List<LevelData> levels;
}