using UnityEngine;

/// <summary>
/// This exists because unity doesnt allow using enums directly in button OnClick events.
/// </summary>
[CreateAssetMenu(fileName = "TowerEnumHolder", menuName = "ScriptableObjects/TowerEnumHolder", order = 1)]
public class TowerEnumHolder : ScriptableObject
{
    public TowersEnum towerType;
}