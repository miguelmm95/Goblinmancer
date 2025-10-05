using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine;

public class ConstructionMenu : BaseMenu
{
    [System.Serializable]
    class TowerArticle
    {
        public TowersEnum towerType;
        public TextMeshProUGUI bodyPriceText;
        public TextMeshProUGUI bloodPriceText;
    }
    [SerializeField] List<TowerArticle> _articles = new List<TowerArticle>();
    [SerializeField] TextMeshProUGUI ZombieAmountText;
    [SerializeField] TextMeshProUGUI BloodAmountText;
    [SerializeField] float _superOpenOffset = -170f;
    List<TowerPrice> _prices = new List<TowerPrice>();
    
    void Update()
    {
        if (_state != MenuState.SuperOpen) return;
        // Update resources display
        ZombieAmountText.text = GameManager.Instance.GetBodies().ToString() + "/" + GameManager.Instance.GetMaxBodies().ToString();
        BloodAmountText.text = GameManager.Instance.GetBlood().ToString() + "/" + GameManager.Instance.GetMaxBlood().ToString();
    }

    /// <summary>
    /// Initializes the construction menu with tower prices.
    /// </summary>
    /// <param name="towerPrices">A list of TowerPrice objects.</param>
    public void Init(List<TowerPrice> towerPrices)
    {
        _prices = towerPrices;

        foreach (TowerPrice price in _prices)
        {
            foreach (TowerArticle article in _articles)
            {
                if (article.towerType == price.towerType)
                {
                    article.bodyPriceText.text = price.price.bodyPrice.ToString() + " Z";
                    article.bloodPriceText.text = price.price.bloodPrice.ToString() + " B";
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Handles the purchase of a tower when a button is clicked.
    /// </summary>
    /// <param name="towerEnumHolder"></param>
    public void BuyTower(TowerEnumHolder towerEnumHolder)
    {
        int priceIndex = GetPriceIndex(towerEnumHolder.towerType);
        if (priceIndex != -1)
        {
            TowerPrice price = _prices[priceIndex];
            if (GameManager.Instance.GetBodies() >= price.price.bodyPrice && GameManager.Instance.GetBlood() >= price.price.bloodPrice)
            {
                GameManager.Instance.Construct(price);
                RuntimeManager.PlayOneShot(_menuInteractionSound);
            }
        }
    }

    /// <summary>
    /// Enters destruction mode to allow the player to demolish buildings.
    /// </summary>
    public void DestructionMode()
    {
        AudioManager.instance.PlayOneShot(_menuInteractionSound, transform.position);
        GameManager.Instance.EnterDestructionMode();
    }

    public void SuperOpen()
    {
        if (_state == MenuState.SuperOpen)
        {
            OpenMenu();
            return;
        }

        _state = MenuState.SuperOpen;
        _menuTransform.DOLocalMove(new Vector3(0, _superOpenOffset, 0), _animationDuration).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Gets the index of the tower price in the list based on the tower type.
    /// </summary>
    /// <param name="towerType"></param>
    /// <returns></returns>
    int GetPriceIndex(TowersEnum towerType)
    {
        for (int i = 0; i < _prices.Count; i++)
        {
            if (_prices[i].towerType == towerType)
                return i;
        }
        return -1;
    }
}