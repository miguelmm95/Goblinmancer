using DG.Tweening;
using FMODUnity;
using TMPro;
using UnityEngine;

/// <summary>
/// Base class for spells that can be cast by the player.
/// </summary>
public abstract class BaseSpell : BaseMenu
{
    [SerializeField] float _superOpenOffset;
    [SerializeField] float _cooldown = 20f; // Cooldown time in seconds
    [SerializeField] TextMeshProUGUI _cooldownText;
    [SerializeField] TextMeshProUGUI _bodyCostText;
    [SerializeField] TextMeshProUGUI _bloodCostText;
    [SerializeField] SpellEnum _spellType;
    [SerializeField] EventReference _spellSound;
    float _currentCooldown; // Time left until the spell can be cast again
    Price _castingCost;

    void Start()
    {
        _currentCooldown = 0f;
        _castingCost = GameManager.Instance.GetSpellCastPrice(_spellType).price;
        _bodyCostText.text = _castingCost.bodyPrice.ToString() + " Z";
        _bloodCostText.text = _castingCost.bloodPrice.ToString() + " B";
        _cooldownText.text = "";
        _state = MenuState.Open;
        CloseMenu();
    }

    /// <summary>
    /// Updates the spell's cooldown timer.
    /// </summary>
    void Update()
    {
        if (GameManager.Instance.GetCurrentPhase() != PhaseEnum.Combat) return;
        if (_currentCooldown > 0f)
        {
            _currentCooldown -= Time.deltaTime;
            _cooldownText.text = Mathf.Ceil(_currentCooldown).ToString();
        }
        else if (_state == MenuState.Open)
        {
            SuperOpen();
            _cooldownText.text = "";
        }
    }

    public void EnterCastingMode()
    {
        GameManager.Instance.EnterCastingMode(this);
    }

    /// <summary>
    /// Casts the spell at the specified target position if the spell is off cooldown and the player has enough resources.
    /// </summary>
    /// <param name="targetPosition"></param>
    public void CastSpell(Vector3 targetPosition)
    {
        if (_currentCooldown > 0f) return;
        if (GameManager.Instance.GetBlood() < _castingCost.bloodPrice || GameManager.Instance.GetBodies() < _castingCost.bodyPrice)
            return;

        GameManager.Instance.RemoveBodies(_castingCost.bodyPrice);
        GameManager.Instance.AddBlood(-_castingCost.bloodPrice);

        Effect(targetPosition);
        _currentCooldown = _cooldown;
        AudioManager.instance.PlayOneShot(_spellSound, targetPosition);
        OpenMenu();
    }

    /// <summary>
    /// The specific effect of the spell, to be implemented by derived classes.
    /// </summary>
    /// <param name="targetPosition"></param>
    protected abstract void Effect(Vector3 targetPosition);

    public void SuperOpen()
    {
        if (_state == MenuState.SuperOpen) return;

        _menuTransform.DOLocalMove(new Vector3(0, _superOpenOffset, 0), _animationDuration).SetEase(Ease.OutBack);
        _state = MenuState.SuperOpen;
    }
}