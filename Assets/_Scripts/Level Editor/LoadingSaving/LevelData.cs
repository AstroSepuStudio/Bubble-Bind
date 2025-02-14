using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData
{
    public enum LevelDifficulty 
    {   Easy, Medium, Hard, Extreme, Ludicrous  }

    // Saved in the level page (before entering the level editor)
    public string LevelName = "";
    public string LevelDescription = "";
    public LevelDifficulty Level_Difficulty = LevelDifficulty.Easy;
    
    // ---
    public string Hash = "";
    public bool IsLevelVerified = false;

    // Camera settings
    public Vector3 CameraPosition = new Vector3(0, 0, -10);
    public float CameraSize = 5;

    // Saved from the LevelEditor scene (when the player is editing the level)
    public List<SavedElement> SavedElements = new();
}
