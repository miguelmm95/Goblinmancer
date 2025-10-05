using UnityEngine;

/// <summary>
/// This exists because unity doesnt allow using enums directly in button OnClick events.
/// </summary>
[CreateAssetMenu(fileName = "UnitEnumHolder", menuName = "ScriptableObjects/UnitEnumHolder", order = 0)]
public class UnitEnumHolder : ScriptableObject
{
    public AllyUnitsEnum unitType;
}