using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    public enum InteractionMode
    {
        Spellcasting,
        Building,
        Demolishing,
        Selecting
    }
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] BaseMenu _pauseMenu;
    private Camera _camera;
    private Vector3 _cameraForward;
    private InteractionMode _currentMode = InteractionMode.Selecting;
    private Tile _currentlyHighlightedTile;
    TowerPrice _currentTower;
    BaseSpell _currentSpell;

    private void Awake()
    {
        _camera = Camera.main;
        _cameraForward = Vector3.Cross(_camera.transform.right, Vector3.up);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.D)) transform.position += _moveSpeed * Time.deltaTime * _camera.transform.right;
        if (Input.GetKey(KeyCode.A)) transform.position += _moveSpeed * Time.deltaTime * -_camera.transform.right;
        if (Input.GetKey(KeyCode.W)) transform.position += _moveSpeed * Time.deltaTime * _cameraForward;
        if (Input.GetKey(KeyCode.S)) transform.position += _moveSpeed * Time.deltaTime * -_cameraForward;
        if (Input.GetKeyDown("1")) _currentMode = InteractionMode.Demolishing;
        else if (Input.GetKeyDown("2")) _currentMode = InteractionMode.Building;
        else if (Input.GetKeyDown("3")) _currentMode = InteractionMode.Spellcasting;
        if (Input.GetKeyDown(KeyCode.Escape)) _pauseMenu.OpenMenu();
        if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo))
            {
                if (hitInfo.collider.TryGetComponent<Tile>(out Tile tile))
                {
                    if (tile != _currentlyHighlightedTile)
                    {
                        _currentlyHighlightedTile?.UnhighlightTile();
                        _currentlyHighlightedTile = tile;
                        _currentlyHighlightedTile.HighlightTile(_currentMode);
                    }
                    if (Input.GetMouseButtonDown(1))
                    {
                        ExitState();
                        GameManager.Instance.ShowHUD();
                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        switch (_currentMode)
                        {
                            case InteractionMode.Spellcasting:
                                _currentSpell.CastSpell(hitInfo.point);
                                _currentSpell = null;
                                _currentMode = InteractionMode.Selecting;
                                GameManager.Instance.ExitCastingMode();
                                break;
                            case InteractionMode.Demolishing:
                                tile.DemolishBuilding();
                                _currentMode = InteractionMode.Selecting;
                                GameManager.Instance.ShowHUD();
                                break;
                            case InteractionMode.Building:
                                tile.ConstructBuilding(_currentTower);
                                _currentTower = null;
                                _currentMode = InteractionMode.Selecting;
                                GameManager.Instance.ShowHUD();
                                break;
                            case InteractionMode.Selecting:
                                tile.InteractWithTower();
                                break;
                        }
                    }
                }
            }
            else
            {
                _currentlyHighlightedTile?.UnhighlightTile();
                _currentlyHighlightedTile = null;
            }
    }

    /// <summary>
    /// Enters building mode to allow the player to construct a building.
    /// </summary>
    public void ConstructBuilding(TowerPrice tower)
    {
        _currentTower = tower;
        _currentMode = InteractionMode.Building;
    }

    /// <summary>
    /// Enters destruction mode to allow the player to demolish buildings and trees
    /// </summary>
    public void EnterDestructionMode()
    {
        _currentMode = InteractionMode.Demolishing;
    }

    public void EnterCastingMode(BaseSpell spell)
    {
        _currentSpell = spell;
        _currentMode = InteractionMode.Spellcasting;
    }

    /// <summary>
    /// Exits the current interaction mode and returns to selecting mode.
    /// </summary>
    public void ExitState()
    {
        if (_currentMode == InteractionMode.Selecting) return;

        _currentSpell = null;
        _currentMode = InteractionMode.Selecting;
        _currentTower = null;
        _currentlyHighlightedTile?.UnhighlightTile();
        _currentlyHighlightedTile = null;
    }
}