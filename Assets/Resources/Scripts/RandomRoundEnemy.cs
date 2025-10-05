using UnityEngine;

[System.Serializable]
public class RandomRoundEnemy
{
    public RoundEnemy enemy;
    [SerializeField, Range(0, 1)] public float Probability; // Probability of this enemy appearing in the round (0 to 1)
}