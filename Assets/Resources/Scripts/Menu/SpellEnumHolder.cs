using UnityEngine;

/// <summary>
/// This exists because unity doesnt allow using enums directly in button OnClick events.
/// </summary>
[CreateAssetMenu(fileName = "SpellEnumHolder", menuName = "ScriptableObjects/SpellEnumHolder", order = 2)]
public class SpellEnumHolder : ScriptableObject
{
    public SpellEnum spellType;
}