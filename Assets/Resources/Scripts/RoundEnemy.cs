using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Round Enemy", menuName = "ScriptableObjects/Round Enemy", order = 3)]
public class RoundEnemy : ScriptableObject
{
    public EnemyUnit enemyUnit;
    public Sprite icon;
}