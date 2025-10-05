using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the spell unlock menu.
/// </summary>
public class SpellUnlockMenu : BaseMenu
{
    [Serializable]
    class Article
    {
        public SpellEnum spellType;
        public TextMeshProUGUI bodyPriceText;
        public TextMeshProUGUI bloodPriceText;
    }
    [SerializeField] List<Article> _articles = new List<Article>();
    [SerializeField] SpellUnlockButton[] _buttons;
    [SerializeField] TextMeshProUGUI BloodAmountText;
    [SerializeField] TextMeshProUGUI ZombieAmountText;
    List<SpellPrice> _prices = new List<SpellPrice>();

    public void Init(List<SpellPrice> prices)
    {
        _prices = prices;

        foreach (SpellPrice price in _prices)
        {
            foreach (Article article in _articles)
            {
                if (article.spellType == price.spellType)
                {
                    article.bodyPriceText.text = price.price.bodyPrice.ToString() + " Z";
                    article.bloodPriceText.text = price.price.bloodPrice.ToString() + " B";
                    break;
                }
            }
        }
    }

    public void UnlockSpell(SpellEnumHolder spellEnumHolder)
    {
        SpellPrice price = GameManager.Instance.GetSpellUnlockPrice(spellEnumHolder.spellType);
        if (GameManager.Instance.GetBlood() < price.price.bloodPrice || GameManager.Instance.GetBodies() < price.price.bodyPrice) return;

        GameManager.Instance.AddBlood(-price.price.bloodPrice);
        GameManager.Instance.RemoveBodies(price.price.bodyPrice);

        GameManager.Instance.UnlockSpell(price.spellPrefab);
        GameObject button = GetUnlockButton(spellEnumHolder);
        if(button.TryGetComponent<Popup>(out var popup))
        {
            popup.ClosePopup();
        }
        Destroy(button);

        BloodAmountText.text = GameManager.Instance.GetBlood().ToString() + "/" + GameManager.Instance.GetMaxBlood().ToString();
        ZombieAmountText.text = GameManager.Instance.GetBodies().ToString() + "/" + GameManager.Instance.GetMaxBodies().ToString();
    }

    GameObject GetUnlockButton(SpellEnumHolder spellEnumHolder)
    {
        foreach (SpellUnlockButton button in _buttons)
            if (button.spellType == spellEnumHolder.spellType)
                return button.gameObject;
        return null;
    }

    public override void CloseMenu()
    {
        if (_state == MenuState.Closed) return;

        base.CloseMenu();
        GameManager.Instance.ShowHUD();
    }

    public override void OpenMenu()
    {
        if (_state == MenuState.Open) return;

        base.OpenMenu();
        GameManager.Instance.HideHUD();

        BloodAmountText.text = GameManager.Instance.GetBlood().ToString() + "/" + GameManager.Instance.GetMaxBlood().ToString();
        ZombieAmountText.text = GameManager.Instance.GetBodies().ToString() + "/" + GameManager.Instance.GetMaxBodies().ToString();
    }
}