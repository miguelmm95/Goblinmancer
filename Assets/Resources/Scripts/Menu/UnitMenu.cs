using System;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the unit menu for purchasing and displaying ally units.
/// </summary>
public class UnitMenu : BaseMenu
{
    /// <summary>
    /// Represents a unit article in the menu.
    /// </summary>
    [Serializable]
    class UnitArticle
    {
        public AllyUnitsEnum unitType;
        public TextMeshProUGUI bodyPriceText;
        public TextMeshProUGUI bloodPriceText;
    }

    /// <summary>
     /// Represents the amount of a specific unit type.
     /// </summary>
    [Serializable]
    class UnitAmount
    {
        public AllyUnitsEnum unitType;
        public TextMeshProUGUI amountText;
        [HideInInspector] public int amount;
    }
    [HideInInspector] public Cemetery cemetery;
    [SerializeField] TextMeshProUGUI TotalZombieAmountText;
    [SerializeField] TextMeshProUGUI BloodAmountText;
    [SerializeField] List<UnitArticle> articles = new List<UnitArticle>();
    [SerializeField] List<UnitAmount> amounts = new List<UnitAmount>();
    List<AllyUnitPrice> _prices = new List<AllyUnitPrice>();

    /// <summary>
    /// Initializes the unit menu with unit prices.
    /// </summary>
    /// <param name="prices">A list of AllyUnitPrice objects.</param>
    public void Init(List<AllyUnitPrice> prices)
    {
        _prices = prices;
        foreach (AllyUnitPrice price in _prices)
        {
            foreach (UnitArticle article in articles)
            {
                if (article.unitType == price.unitType)
                {
                    article.bodyPriceText.text = price.price.bodyPrice.ToString() + " Z";
                    article.bloodPriceText.text = price.price.bloodPrice.ToString() + " B";
                    break;
                }
            }
        }

    }

    /// <summary>
    /// Opens the unit menu. Updates the display with current resources and unit counts.
    /// </summary>
    /// <param name="cemetery"></param>
    public void OpenMenu(Cemetery cemetery)
    {
        OpenMenu();
        this.cemetery = cemetery;

        UpdateResources();

        foreach (UnitAmount amount in amounts)
        {
            amount.amount = 0;
            foreach (AllyUnit unit in cemetery.Units)
            {
                if (unit.unitType == amount.unitType)
                    amount.amount++;
            }
            amount.amountText.text = "x" + amount.amount.ToString();
        }
        gameObject.SetActive(true);
        GameManager.Instance.HideHUD();
    }

    /// <summary>
    /// Handles the purchase of a unit when a button is clicked. Updates resources and unit counts.
    /// </summary>
    /// <param name="unitEnumHolder"></param>
    public void BuyUnit(UnitEnumHolder unitEnumHolder)
    {
        if (!cemetery.BuyUnit(unitEnumHolder.unitType)) return;

        AudioManager.instance.PlayOneShot(_menuInteractionSound, transform.position);

        int zombieAmount = 0;
        int unitAmount = 0;
        foreach (AllyUnit unit in cemetery.Units)
        {
            Debug.Log("Cemetery unit: " + unit.unitType);
            if (unit.unitType == AllyUnitsEnum.Zombie)
                zombieAmount++;
            if (unit.unitType == unitEnumHolder.unitType)
                unitAmount++;
        }

        UpdateResources();

        GetUnitAmount(AllyUnitsEnum.Zombie).amountText.text = "x" + zombieAmount.ToString();
        GetUnitAmount(unitEnumHolder.unitType).amountText.text = "x" + unitAmount.ToString();
    }

    /// <summary>
    /// Gets the UnitAmount object for a specific unit type.
    /// </summary>
    /// <param name="unitType"></param>
    /// <returns></returns>
    UnitAmount GetUnitAmount(AllyUnitsEnum unitType)
    {
        foreach (UnitAmount amount in amounts)
        {
            if (amount.unitType == unitType)
                return amount;
        }
        return null;
    }

    /// <summary>
    /// Updates the resource display in the menu.
    /// </summary>
    void UpdateResources()
    {
        TotalZombieAmountText.text = GameManager.Instance.GetBodies().ToString() + "/" + GameManager.Instance.GetMaxBodies().ToString();
        BloodAmountText.text = GameManager.Instance.GetBlood().ToString() + "/" + GameManager.Instance.GetMaxBlood().ToString();
    }

    /// <summary>
    /// Closes the unit menu.
    /// </summary>
    public override void CloseMenu()
    {
        if (_state == MenuState.Closed) return;

        _state = MenuState.Closed;
        _menuTransform.DOLocalMove(_closedPosition, _animationDuration).SetEase(Ease.InBack).OnComplete(() => gameObject.SetActive(false));
        GameManager.Instance.ShowHUD();
    }

    /// <summary>
    /// Opens the unit menu.
    /// </summary>
    public override void OpenMenu()
    {
        if (_state == MenuState.Open) return;

        _state = MenuState.Open;
        _menuTransform.DOLocalMove(Vector3.zero, _animationDuration).SetEase(Ease.OutBack);
        GameManager.Instance.HideHUD();
    }
}
