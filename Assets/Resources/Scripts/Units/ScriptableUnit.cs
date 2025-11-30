using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableUnit", menuName = "ScriptableObjects/ScriptableUnit")]
public class ScriptableUnit : ScriptableObject
{
    public float maxHealth;
    public float size;
    public float movementSpeed;
    public float cooldownBetweenAttacks;
    public float retargetFrequecy;
    public float spawnTweenDuration;
    public float stepHeight;
    public float stepDuration;
    public AllyUnitsEnum unitType;
}
