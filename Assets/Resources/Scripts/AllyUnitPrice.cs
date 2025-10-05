using UnityEngine;

/// <summary>
/// Represents the price and prefab of an allied unit.
/// </summary>
[System.Serializable]
public class AllyUnitPrice
{
    public AllyUnitsEnum unitType;
    public AllyUnit unitPrefab;
    public Price price;
}