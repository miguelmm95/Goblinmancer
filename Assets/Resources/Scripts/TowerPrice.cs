using UnityEngine;

/// <summary>
/// Represents the price and prefab of a tower.
/// </summary>
[System.Serializable]
public class TowerPrice
{
    public TowersEnum towerType;
    public BaseTower towerPrefab;
    public Price price;
}