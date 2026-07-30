using System;
using System.Collections.Generic;

[Serializable]
public class BonusQuestionsData
{
    public List<BonusRound> bonusRounds;
}

[Serializable]
public class BonusRound
{
    public int afterLevel;
    public List<BonusRoundQuestion> questions;
}

[Serializable]
public class BonusRoundQuestion
{
    public string question;
    public List<string> options;
    public string correctAnswer;
}
