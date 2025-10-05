using FMODUnity;
using UnityEngine;

/// <summary>
/// Base class for all tower types.
/// </summary>
public class BaseTower : Hittable
{
    public Tile tile;
    public TowersEnum TowerType => _towerType;
    const float SELL_CASHBACK = 0.7f; // Percentage of bodies returned when sold.
    [SerializeField] TowersEnum _towerType;
    [SerializeField] EventReference _placeSound;
    [SerializeField] EventReference _interactSound;
    protected Price _price;
    protected bool _paused = false;

    /// <summary>
    /// Tower behaviour when bought.
    /// </summary>
    protected override void Start()
    {
        if (_towerType == TowersEnum.Tree) return; // Trees are not registered as towers.
        base.Start();
        AudioManager.instance.PlayOneShot(_placeSound, transform.position);
        GameManager.Instance.AddTower(this);
        _price = GameManager.Instance.GetTowerPrice(_towerType);
    }

    /// <summary>
    /// Called before the combat starts.
    /// </summary>
    public virtual void OnPrepare()
    {

    }

    /// <summary>
    /// Tower behaviour when sold.
    /// </summary>
    public virtual void OnSell()
    {
        for (int i = 0; i < Mathf.RoundToInt(_price.bodyPrice * SELL_CASHBACK * CurrentHealth / MaxHealth); i++)
        {
            GameManager.Instance.AddBody();
        }
        GameManager.Instance.AddBlood(Mathf.RoundToInt(_price.bloodPrice * SELL_CASHBACK * CurrentHealth / MaxHealth));

        GameManager.Instance.RemoveTower(this);
    }

    /// <summary>
    /// Tower behaviour when destroyed.
    /// </summary>
    protected override void Die()
    {
        AudioManager.instance.PlayOneShot(_deathSound, transform.position);
        GameManager.Instance.RemoveTower(this);
        tile.BuildingDestroyed();
    }

    /// <summary>
    /// Tower behaviour when clicked.
    /// </summary>
    public virtual void OnInteract()
    {
        Debug.Log("Interacted with " + gameObject.name);
        if (_paused) return;
        AudioManager.instance.PlayOneShot(_interactSound, transform.position);
    }
    /// <summary>
    /// Pauses the tower's behavior between rounds.
    /// </summary>
    public virtual void Pause()
    {
        _paused = true;
    }

    /// <summary>
    /// Unpauses the tower's behavior.
    /// </summary>
    public virtual void Unpause()
    {
        _paused = false;
    }
}