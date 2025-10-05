using UnityEngine;

/// <summary>
/// Represents a specific enemy type and the quantity to spawn in a round.
/// </summary>
[System.Serializable]
public class FixedRoundEnemy
{
    public RoundEnemy enemy;
    [SerializeField] public int amount; // Amount of this enemy to spawn in the round
}