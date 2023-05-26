using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameItemData : ScriptableObject
{
    public ulong id;
    public Sprite icon;
    public GameItemType itemType;
    public GameItemUsageType usageType;
    public string displayName;
    public string description;
    public int minPossibleDropLevel;

    // For equipable

    public Vector2Int inventorySize;
    public List<int> possiblePrefixes;
    public List<int> possibleSuffixes;
}
