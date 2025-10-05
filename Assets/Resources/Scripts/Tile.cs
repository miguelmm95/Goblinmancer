using DG.Tweening;
using FMODUnity;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{
    public enum TileState
    {
        Buildable,
        Occupied,
        Battlefield,
    }
    [SerializeField] private GameObject _highlightEffectGameObject;
    [SerializeField] private Price _demolishCost;
    [SerializeField] private EventReference _constructSound;
    [SerializeField] private EventReference _demolishSound;
    [SerializeField] private EventReference _sellSound;
    private TileState _currentState;
    private BaseTower _currentBuilding;

    /// <summary>
    /// Initializes the tile with a given state and an optional building.
    /// </summary>
    /// <param name="initialState"></param> Initial state of the tile
    /// <param name="initialBuilding"></param> Optional initial building on the tile (trees)
    public void InitializeTile(TileState initialState, BaseTower initialBuilding = null)
    {
        _currentState = initialState;
        _currentBuilding = initialBuilding;
    }

    /// <summary>
    /// Attempts to construct a building on this tile. Returns true if successful, false otherwise.
    /// </summary>
    /// <param name="buildingPrefab"></param>
    /// <returns></returns>
    public bool ConstructBuilding(TowerPrice tower)
    {
        if (_currentState != TileState.Buildable) return false;

        if (GameManager.Instance.GetBodies() < tower.price.bodyPrice || GameManager.Instance.GetBlood() < tower.price.bloodPrice)
        {
            // Not enough resources to build
            return false;
        }

        GameManager.Instance.RemoveBodies(tower.price.bodyPrice);
        GameManager.Instance.AddBlood(-tower.price.bloodPrice);

        _currentBuilding = Instantiate(tower.towerPrefab, transform.position, Quaternion.identity, transform);
        _currentState = TileState.Occupied;
        _currentBuilding.tile = this;
        AudioManager.instance.PlayOneShot(_constructSound, transform.position);
        return true;
    }

    /// <summary>
    /// Demolishes the building on this tile if there is one. Trees just dissapear for a cost and towers return part of their cost.
    /// </summary>
    public void DemolishBuilding()
    {
        if (_currentState != TileState.Occupied) return;
        if (_currentBuilding.TowerType != TowersEnum.Tree)
        {
            AudioManager.instance.PlayOneShot(_sellSound, transform.position);
            _currentBuilding.OnSell();
        }
        else
        {
            if (GameManager.Instance.GetBlood() < _demolishCost.bloodPrice || GameManager.Instance.GetBodies() < _demolishCost.bodyPrice)
            {
                // Not enough resources to demolish
                return;
            }
            GameManager.Instance.RemoveBodies(_demolishCost.bodyPrice);
            GameManager.Instance.AddBlood(-_demolishCost.bloodPrice);
            Destroy(_currentBuilding); // TODO: Add cost logic for trees
            AudioManager.instance.PlayOneShot(_demolishSound, transform.position);
        }
        _currentBuilding = null;
        _currentState = TileState.Buildable;
    }

    public void BuildingDestroyed()
    {
        _currentBuilding = null;
        _currentState = TileState.Buildable;
    }

    /// <summary>
    /// Plays a spawn animation for the tile.
    /// </summary>
    public void SpawnTileAnimation()
    {
        transform.DOScale(Vector3.one, 0.5f).From(Vector3.zero).SetEase(Ease.OutFlash);
    }

    /// <summary>
    /// Highlights the tile to indicate it is selectable.
    /// </summary>
    public void HighlightTile(CameraController.InteractionMode mode)
    {
        if (mode == CameraController.InteractionMode.Building && _currentState == TileState.Buildable)
        {
            _highlightEffectGameObject.GetComponent<Renderer>().material.color = Color.green;
            _highlightEffectGameObject.SetActive(true);
        }
        else if (mode == CameraController.InteractionMode.Demolishing && _currentState == TileState.Occupied)
        {
            _highlightEffectGameObject.GetComponent<Renderer>().material.color = Color.red;
            _highlightEffectGameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Removes the highlight from the tile.
    /// </summary>
    public void UnhighlightTile()
    {
        _highlightEffectGameObject.SetActive(false);
    }

    /// <summary>
    /// If there is a tower on this tile, interact with it.
    /// </summary>
    public void InteractWithTower()
    {
        Debug.Log("Interacted with " + gameObject.name);
        if (_currentBuilding == null) return;
        Debug.Log("Interacted with " + gameObject.name + " which has a building: " + _currentBuilding.name);
        _currentBuilding.OnInteract();
    }
}
