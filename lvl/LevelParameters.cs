using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelParameters
{
    public static int GridSize { get; set; }
    public static int CurrentLevel { get; set; } = 1;

    public static void UpdateLevelSize(int levelNumber)
    {
        GridSize = 2 + (levelNumber - 1) / 2;
    }
}
