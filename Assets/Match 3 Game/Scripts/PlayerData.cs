﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string Name;
    public string PlayerID;
    //public int TotalXP;

    public int PlayerBombAbilityCount;
    public int PlayerColorBombAbilityCount;
    public int PlayerExtraMoveAbilityCount;

    public int CurrentLevelId; // 🔥 NEW universal current level tracker

    public List<LevelInfo> Levels = new List<LevelInfo>();
}

[Serializable]
public class LevelInfo
{
    public int LevelID;
    public int Stars; // 0 to 3
    public int XP;
    public int LevelLocked; // 0 = unlocked, 1 = locked
}
