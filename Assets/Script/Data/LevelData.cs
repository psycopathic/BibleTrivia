using System;
using System.Collections.Generic;

[Serializable]
public class LevelsData
{
    public List<LevelData> levels;
}

[Serializable]
public class LevelData
{
    public int id;
    public List<string> letters;
    public List<string> answers;
}