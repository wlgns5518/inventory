using System;

[Serializable]
public enum GameItemType
{
    None,
    Armor,
    Shield,
    Helm,
    Staff,
    Sword,
    Axe,
    Bow,
    Club,
    Jewel,
    Count,
}

[Serializable]
public enum GameItemUsageType
{
    None,
    Consumable,
    Equippable,
    Count,
}