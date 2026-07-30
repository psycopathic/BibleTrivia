using System;
using System.Collections.Generic;

[Serializable]
public class PuzzleData
{
    public List<string> letters;
    public List<string> answers;
}

[Serializable]
public class BonusQuestion
{
    public string question;
    public List<string> options;
    public string correctAnswer;
}

[Serializable]
public class StageData
{
       public List<BonusQuestion> bonusQuestions;
    public List<PuzzleData> puzzles;

}

[Serializable]
public class WordData
{
    public List<StageData> stages;
}