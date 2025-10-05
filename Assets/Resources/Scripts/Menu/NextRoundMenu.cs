using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NextRoundMenu : BaseMenu
{
    List<Image> fixedEnemyIcons = new List<Image>();
    List<Image> randomEnemyIcons = new List<Image>();

    [SerializeField] float iconSpacing = 120f;
    [SerializeField] GameObject enemyIconPrefab;
    [SerializeField] RectTransform fixedEnemyIconParent;
    [SerializeField] RectTransform randomEnemyIconParent;
    [SerializeField] TextMeshProUGUI randomEnemyText;
    [SerializeField] TextMeshProUGUI roundText;
    public void StartNextRound()
    {
        GameManager.Instance.StartNextRound();
    }
    public void UpdateEnemyIcons(Round round, int roundNumber)
    {
        roundText.text = "Round\n" + roundNumber;
        foreach (var icon in fixedEnemyIcons)
        {
            Destroy(icon.gameObject);
        }
        fixedEnemyIcons.Clear();
        foreach (var icon in randomEnemyIcons)
        {
            Destroy(icon.gameObject);
        }
        randomEnemyIcons.Clear();

        int randomEnemyAmount = 0;
        foreach (var fixedEnemy in round.FixedEnemiesInRound)
        {
            var iconObj = Instantiate(enemyIconPrefab, fixedEnemyIconParent);
            iconObj.transform.localPosition += Vector3.up * iconSpacing * fixedEnemyIcons.Count;
            var iconImage = iconObj.GetComponent<Image>();
            if (fixedEnemy.enemy.icon != null)
                iconImage.sprite = fixedEnemy.enemy.icon.sprite;
            var text = iconObj.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "x" + fixedEnemy.amount.ToString();
            fixedEnemyIcons.Add(iconImage);
        }
        foreach (var randomEnemy in round.RandomEnemiesInRound)
        {
            var iconObj = Instantiate(enemyIconPrefab, randomEnemyIconParent);
            iconObj.transform.localPosition += Vector3.up * iconSpacing * randomEnemyAmount;
            var iconImage = iconObj.GetComponent<Image>();
            if (randomEnemy.enemy.icon != null)
                iconImage.sprite = randomEnemy.enemy.icon.sprite;
            randomEnemyIcons.Add(iconImage);
            randomEnemyAmount++;
        }
        if (randomEnemyAmount > 0)
        {
            randomEnemyText.text = "x" + round.TotalRandomEnemies.ToString();
        }
        else
        {
            randomEnemyText.text = "";
        }
    }
}