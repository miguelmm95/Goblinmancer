using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public UnitMenu unitMenu => _unitMenu;
    public List<AllyUnit> AlliedUnits => _alliedUnits;
    public static GameManager Instance { get; private set; } // Singleton instance, new game managers will override old ones
    public Hittable Castle => _castle;
    public PhaseEnum CurrentPhase => _currentPhase;
    [SerializeField] CameraController _cameraController;
    [SerializeField] BaseTower[] _treePrefabs;
    [SerializeField] GameObject _portalPrefab;
    [SerializeField] float _portalAnimationDuration = 1f;
    [SerializeField] float _timeBetweenSpawns = 0.05f;
    [SerializeField] private Cemetery _castle;
    [SerializeField] private Price _initialResources;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private float _tileSize = 10f;
    [SerializeField] private int _mapWidth = 10;
    [SerializeField] private int _mapHeight = 10;
    [SerializeField] Round[] _rounds;
    [SerializeField] Round _freePlay;
    [SerializeField] float _freePlayEnemyIncreaseRate = 1.1f; // Percentage increase of enemies per round in free play mode
    [SerializeField] int _maxBlood = 20;
    [SerializeField] AllyUnit _zombiePrefab;
    [SerializeField] Transform _enemySpawnPoint;
    [SerializeField] Vector2 _enemySpawnSize = new Vector2(40f, 20f);
    [SerializeField] List<AllyUnitPrice> _unitPrices;
    [SerializeField] List<SpellPrice> _spellUnlockPrices;
    [SerializeField] List<SpellPrice> _spellCastingPrices;
    [SerializeField] List<TowerPrice> _towerPrices;
    [SerializeField] UnitMenu _unitMenu;
    [SerializeField] ConstructionMenu _constructionMenu;
    [SerializeField] CancelMenu _cancelMenu;
    [SerializeField] SpellCastingMenu _spellCastingMenu;
    [SerializeField] SpellUnlockMenu _spellUnlockMenu;
    [SerializeField] NextRoundMenu _nextRoundMenu;
    [SerializeField] CurrencyIndicator _currencyIndicator;
    [SerializeField] float _updateGoblinSoundsInterval = 1f;
    [SerializeField] int _maxExpectedGoblin = 500; // Used for audio manager to know the max number of goblins to expect
    List<AllyUnit> _alliedUnits = new List<AllyUnit>();
    List<EnemyUnit> _enemyUnits = new List<EnemyUnit>();
    List<BaseTower> _towers = new List<BaseTower>();
    List<Cemetery> _cemeteries = new List<Cemetery>();
    int _currentRound = 0;
    int _blood = 0;
    PhaseEnum _currentPhase = PhaseEnum.Build;
    Tile[] _tiles;
    float _updateGoblinSoundsTimer = 0f;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this && Instance.gameObject != null)
            {
                Destroy(Instance.gameObject);
            }

            Instance = this;
        }

        _unitMenu.Init(_unitPrices);

        _constructionMenu.Init(_towerPrices);

        _spellUnlockMenu.Init(_spellUnlockPrices);
        
        _currencyIndicator.UpdateCurrencyDisplay(GetBodies(), GetBlood());
    }

    void Update()
    {
        if (_currentPhase == PhaseEnum.Combat)
        {
            if (_enemyUnits.Count == 0)
            {
                EndRound();
            }
        }
        _updateGoblinSoundsTimer += Time.deltaTime;
        if (_updateGoblinSoundsTimer >= _updateGoblinSoundsInterval)
        {
            _updateGoblinSoundsTimer = 0f;
            int goblinCount = GetAllAliveGoblins();
            AudioManager.instance.UpdateGoblinNumber(goblinCount, _maxExpectedGoblin);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        _tiles = new Tile[_mapWidth * _mapHeight];
        for (int x = 0; x < _mapWidth; x++)
        {
            for (int y = 0; y < _mapHeight; y++)
            {
                Tile newTile = Instantiate(_tilePrefab, new Vector3(x * _tileSize, 0, y * _tileSize), Quaternion.identity);
                if (x <= 5 || y <= 4 || x >= 9 || y >= _mapHeight - 7)
                {
                    if (y <= 4 || y >= _mapHeight - 7 || x <= 2)
                    {
                        Instantiate(_treePrefabs[Random.Range(0, _treePrefabs.Length)], new Vector3(x * _tileSize, 0, y * _tileSize), Quaternion.identity, newTile.transform);
                        newTile.InitializeTile(Tile.TileState.Unused);
                    }
                    else if (x <= 5 && (y <= 8 || y >= _mapHeight - 11))
                    {
                        Instantiate(_treePrefabs[Random.Range(0, _treePrefabs.Length)], new Vector3(x * _tileSize, 0, y * _tileSize), Quaternion.identity, newTile.transform);
                        newTile.InitializeTile(Tile.TileState.Unused);
                    }
                    else newTile.InitializeTile(Tile.TileState.Battlefield);
                    
                }
                else if (y <= 8 || y >= _mapHeight - 11)
                {
                    BaseTower tree = Instantiate(_treePrefabs[Random.Range(0, _treePrefabs.Length)], new Vector3(x * _tileSize, 0, y * _tileSize), Quaternion.identity, newTile.transform);
                    newTile.InitializeTile(Tile.TileState.Occupied, tree);
                }
                else newTile.InitializeTile(Tile.TileState.Buildable);
                _tiles[x + y * _mapWidth] = newTile;
            }
        }
        StartCoroutine(MapSpawnAnimation());

        _nextRoundMenu.UpdateEnemyIcons(GetNextRound(), _currentRound + 1);

        _blood = _initialResources.bloodPrice;
    }

    /// <summary>
    /// Prepares the next round by clearing existing enemies and spawning new ones based on defined probabilities.
    /// </summary>
    void PrepareRound()
    {
        AudioManager.instance.ChangePhase();
        Round round;
        if (_currentRound < _rounds.Length)
        {
            round = _rounds[_currentRound];
        }
        else
        {
            round = _freePlay;
            round.TotalRandomEnemies = Mathf.RoundToInt(round.TotalRandomEnemies * Mathf.Pow(_freePlayEnemyIncreaseRate, _currentRound - _rounds.Length + 1)); // Increase enemies in free play mode
        }
        for (int i = 0; i < round.TotalRandomEnemies; i++)
        {
            float rand = Random.Range(0f, 1f);
            float cumulativeProbability = 0f;
            foreach (RandomRoundEnemy roundEnemy in round.RandomEnemiesInRound)
            {
                cumulativeProbability += roundEnemy.Probability;
                if (rand <= cumulativeProbability)
                {
                    _enemyUnits.Add(Instantiate(
                        roundEnemy.enemy.enemyUnit,
                        _enemySpawnPoint.position + new Vector3(Random.Range(-_enemySpawnSize.x * 0.5f, _enemySpawnSize.x * 0.5f), 0, Random.Range(-_enemySpawnSize.y * 0.5f, _enemySpawnSize.y * 0.5f)),
                        Quaternion.identity
                    ));
                    _enemyUnits[_enemyUnits.Count - 1].transform.LookAt(_castle.transform);
                    _enemyUnits[_enemyUnits.Count - 1].transform.localScale = Vector3.zero;
                    _enemyUnits[_enemyUnits.Count - 1].transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutCubic);
                    break;
                }
            }
        }
        foreach (FixedRoundEnemy fixedEnemy in round.FixedEnemiesInRound)
        {
            for (int j = 0; j < fixedEnemy.amount; j++)
            {
                _enemyUnits.Add(Instantiate(
                    fixedEnemy.enemy.enemyUnit,
                    _enemySpawnPoint.position + new Vector3(Random.Range(-_enemySpawnSize.x, _enemySpawnSize.x), 0, Random.Range(-_enemySpawnSize.y, _enemySpawnSize.y)),
                    Quaternion.identity
                ));
                _enemyUnits[_enemyUnits.Count - 1].transform.LookAt(_castle.transform);
                _enemyUnits[_enemyUnits.Count - 1].transform.localScale = Vector3.zero;
                _enemyUnits[_enemyUnits.Count - 1].transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutCubic);
            }
        }
        foreach (BaseTower tower in _towers)
        {
            tower.OnPrepare();
            tower.Pause();
        }
        HideHUD(false);
        _currentPhase = PhaseEnum.Combat;
    }

    /// <summary>
    /// Starts the round by unpausing all units.
    /// </summary>
    void StartRound()
    {
        foreach (BaseUnit unit in _enemyUnits)
        {
            unit.Unpause();
        }
        foreach (BaseUnit unit in _alliedUnits)
        {
            unit.Unpause();
        }
        foreach (BaseTower tower in _towers)
        {
            if (tower is AttackingTower)
            {
                tower.Unpause();
            }
        }
        _spellCastingMenu.OpenMenu();
    }

    /// <summary>
    /// Ends the round by rewarding the player and cleaning up units.
    /// </summary>
    void EndRound()
    {
        AudioManager.instance.ChangePhase();
        foreach (EnemyUnit unit in _enemyUnits)
        {
            if (!unit.Dead)
            {
                Debug.Log("Some enemies are still alive, cannot end round.");
                return;
            }
        }

        foreach (AllyUnit unit in _alliedUnits)
        {
            unit.Pause();
            unit.Reset();
            if (unit.Dead)
            {
                Destroy(unit.gameObject);
            }
        }
        _alliedUnits.RemoveAll(unit => unit.Dead);

        StartCoroutine(DoPortalAnimation());
    }

    /// <summary>
    /// Returns the closest allied unit to the given position. If no allied units exist, returns the closest tower. If no towers exist, returns the castle.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Hittable GetClosestAllyUnit(Vector3 position, Hittable exclude = null)
    {
        if (_alliedUnits.Count == 0) return GetClosestTower(position); // If no allied units, return closest tower

        Hittable closestUnit = null;
        float closestDistance = Mathf.Infinity;

        foreach (BaseUnit unit in _alliedUnits)
        {
            if (unit.Dead || unit == exclude) continue;

            float distanceSqr = Vector3.SqrMagnitude(position - unit.transform.position);
            if (distanceSqr < closestDistance && unit)
            {
                closestDistance = distanceSqr;
                closestUnit = unit;
            }
        }

        if (closestUnit == null) return GetClosestTower(position); // If all allied units are dead, return closest tower

        return closestUnit;
    }

    /// <summary>
    /// Returns the closest enemy unit to the given position. If no enemies exist, ends the round and returns null.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Hittable GetClosestEnemy(Vector3 position, Hittable exclude = null)
    {
        if (_enemyUnits.Count == 0)
        {
            EndRound();
            return null;
        }

        Hittable closestUnit = null;
        float closestDistance = Mathf.Infinity;

        foreach (BaseUnit unit in _enemyUnits)
        {
            if (unit.Dead || unit == exclude) continue;

            float distanceSqr = Vector3.SqrMagnitude(position - unit.transform.position);
            if (distanceSqr < closestDistance)
            {
                closestDistance = distanceSqr;
                closestUnit = unit;
            }
        }

        if (closestUnit == null)
        {
            EndRound();
            return null;
        }

        return closestUnit;
    }

    /// <summary>
    /// Returns the closest tower to the given position. If no towers exist, returns the castle.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Hittable GetClosestTower(Vector3 position, Hittable exclude = null)
    {
        if (_towers.Count == 0) return _castle; // If no towers, return castle

        Hittable closestTower = null;
        float closestDistance = Mathf.Infinity;

        foreach (BaseTower tower in _towers)
        {
            if (tower == exclude) continue;

            float distanceSqr = Vector3.SqrMagnitude(position - tower.transform.position);
            if (distanceSqr < closestDistance)
            {
                closestDistance = distanceSqr;
                closestTower = tower;
            }
        }

        if (closestTower == null) return _castle; // If all towers are destroyed, return castle

        return closestTower;
    }

    /// <summary>
    /// Returns the allied unit with the highest max health. If no allied units exist, returns the closest tower.
    /// </summary>
    public Hittable GetHighestHealthAllyUnit(Vector3 position, Hittable exclude = null)
    {
        if (_alliedUnits.Count == 0) return GetClosestTower(position, exclude);

        Hittable highestHealthUnit = null;
        float highestHealth = -Mathf.Infinity;
        foreach (BaseUnit unit in _alliedUnits)
        {
            if (unit.Dead || unit == exclude) continue;

            if (unit.MaxHealth > highestHealth)
            {
                highestHealth = unit.MaxHealth;
                highestHealthUnit = unit;
            }
        }

        if (highestHealthUnit == null) return GetClosestTower(position);

        return highestHealthUnit;
    }

    /// <summary>
    /// Returns the enemy unit with the highest max health. If no enemies exist, ends the round and returns null.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Hittable GetHighestHealthEnemy(Hittable exclude = null)
    {
        if (_enemyUnits.Count == 0)
        {
            EndRound();
            return null;
        }

        Hittable highestHealthUnit = null;
        float highestHealth = -Mathf.Infinity;
        foreach (BaseUnit unit in _enemyUnits)
        {
            if (unit.Dead || unit == exclude) continue;

            if (unit.MaxHealth > highestHealth)
            {
                highestHealth = unit.MaxHealth;
                highestHealthUnit = unit;
            }
        }

        if (highestHealthUnit == null)
        {
            EndRound();
            return null;
        }

        return highestHealthUnit;
    }

    /// <summary>
    /// Returns all enemies in range of the given position. If no enemies exist, ends the round and returns null.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public List<Hittable> GetAllEnemiesInRange(Vector3 position, float range)
    {
        List<Hittable> enemiesInRange = new List<Hittable>();

        if (_enemyUnits.Count == 0)
        {
            EndRound();
            return enemiesInRange;
        }

        float rangeSquared = range * range;
        foreach (BaseUnit unit in _enemyUnits)
        {
            if (unit.Dead) continue;

            float distanceSqr = Vector3.SqrMagnitude(position - unit.transform.position);
            if (distanceSqr <= rangeSquared)
            {
                enemiesInRange.Add(unit);
            }
        }

        if (enemiesInRange.Count == 0)
        {
            EndRound();
        }

        return enemiesInRange;
    }

    /// <summary>
    /// Returns all allied hittables (units, towers and castle) in range of the given position.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public List<Hittable> GetAllAlliesInRange(Vector3 position, float range)
    {
        float rangeSquared = range * range;
        List<Hittable> alliesInRange = new List<Hittable>();
        foreach (BaseUnit unit in _alliedUnits)
        {
            if (unit.Dead) continue;

            float distanceSqr = Vector3.SqrMagnitude(position - unit.transform.position);
            if (distanceSqr <= rangeSquared)
            {
                alliesInRange.Add(unit);
            }
        }
        foreach (BaseTower tower in _towers)
        {
            float distanceSqr = Vector3.SqrMagnitude(position - tower.transform.position);
            if (distanceSqr <= rangeSquared)
            {
                alliesInRange.Add(tower);
            }
        }
        float castleDistanceSqr = Vector3.SqrMagnitude(position - _castle.transform.position);
        if (castleDistanceSqr <= rangeSquared)
        {
            alliesInRange.Add(_castle);
        }
        return alliesInRange;
    }

    /// <summary>
    /// Adds a zombie to a random cemetery if there is space available.
    /// </summary>
    public void AddBody()
    {
        List<Cemetery> cemeteries = new List<Cemetery>();
        foreach (BaseTower tower in _towers)
        {
            if (tower is Cemetery)
            {
                cemeteries.Add(tower as Cemetery);
            }
        }
        bool added = false;
        while (cemeteries.Count > 0 && !added)
        {
            int index = Random.Range(0, cemeteries.Count);
            Cemetery cemetery = cemeteries[index];
            if (cemetery != null && !cemetery.IsFull())
            {
                AllyUnit newZombie = Instantiate(_zombiePrefab, cemetery.transform.position, Quaternion.identity);
                newZombie.spawnSize = cemetery.SpawnSize;
                added = true;
                newZombie.ChangeCemetery(cemetery);
                _alliedUnits.Add(newZombie);
            }
            cemeteries.RemoveAt(index);
        }
        _currencyIndicator.UpdateCurrencyDisplay(GetBodies(), GetBlood());
    }

    /// <summary>
    /// Removes the specified amount of bodies (zombies) from the player's units.
    /// </summary>
    public void RemoveBodies(int amount)
    {
        int bodiesToRemove = amount;
        for (int i = 0; i < _alliedUnits.Count && bodiesToRemove > 0; i++)
        {
            if (_alliedUnits[i].unitType == AllyUnitsEnum.Zombie)
            {
                if (_alliedUnits[i].cemetery != null)
                {
                    _alliedUnits[i].cemetery.RemoveUnit(_alliedUnits[i]);
                }
                Destroy(_alliedUnits[i].gameObject);
                _alliedUnits.RemoveAt(i);
                i--;
                bodiesToRemove--;
            }
        }
        if (bodiesToRemove > 0)
        {
            Debug.LogError("Not enough bodies to remove!");
        }
        _currencyIndicator.UpdateCurrencyDisplay(GetBodies(), GetBlood());
    }

    public void EndGame()
    {
        Debug.Log($"Castle is destroyed! You survived {_currentRound} rounds.");
    }

    /// <summary>
    /// Adds blood to the player's resources.
    /// </summary>
    /// <param name="amount"></param>
    public void AddBlood(int amount)
    {
        _blood += amount;
        if (_blood > _maxBlood) _blood = _maxBlood;
        _currencyIndicator.UpdateCurrencyDisplay(GetBodies(), GetBlood());
    }

    /// <summary>
    /// Returns the current amount of bodies (zombies) the player has.
    /// </summary>
    public int GetBodies()
    {
        int zombieCount = 0;
        foreach (AllyUnit unit in _alliedUnits)
        {
            if (unit.unitType == AllyUnitsEnum.Zombie)
            {
                zombieCount++;
            }
        }
        return zombieCount;
    }

    int GetAllAliveGoblins()
    {
        int goblinCount = 0;
        foreach (AllyUnit unit in _alliedUnits)
        {
            if (!unit.Dead)
            {
                goblinCount++;
            }
        }
        foreach (EnemyUnit enemy in _enemyUnits)
        {
            if (!enemy.Dead)
            {
                goblinCount++;
            }
        }
        return goblinCount;
    }

    /// <summary>
    /// Returns the maximum number of bodies (zombies) the player can have based on cemetery capacities.
    /// </summary>
    public int GetMaxBodies()
    {
        int maxBodies = 0;
        foreach (Cemetery cemetery in _cemeteries)
        {
            maxBodies += cemetery.Capacity;
        }
        foreach (AllyUnit unit in _alliedUnits)
        {
            if (unit.unitType != AllyUnitsEnum.Zombie)
            {
                maxBodies--; // Count non-zombie units against max bodies
            }
        }
        return maxBodies;
    }

    /// <summary>
    /// Returns the current amount of blood the player has.
    /// </summary>
    public int GetBlood()
    {
        return _blood;
    }

    /// <summary>
    /// Starts the round.
    /// </summary>
    /// <returns></returns>
    IEnumerator StartRoundCoroutine()
    {
        PrepareRound();
        yield return new WaitForSeconds(3f); // Wait for 3 seconds before starting the round
        StartRound();
    }

    /// <summary>
    /// Increases the maximum blood capacity of the player.
    /// </summary>
    /// <param name="numberBlood"></param>
    public void AddMaxBlood(int numberBlood)
    {
        _maxBlood += numberBlood;
        if (_blood > _maxBlood) _blood = _maxBlood;
        _currencyIndicator.UpdateCurrencyDisplay(GetBodies(), GetBlood());
    }

    /// <summary>
    /// Returns the maximum blood capacity of the player.
    /// </summary>
    public int GetMaxBlood()
    {
        return _maxBlood;
    }

    /// <summary>
    /// Registers an enemy unit to the game.
    /// </summary>
    /// <param name="enemy"></param>
    public void AddEnemyUnit(EnemyUnit enemy)
    {
        _enemyUnits.Add(enemy);
    }

    /// <summary>
    /// Registers an allied unit to the game.
    /// </summary>
    /// <param name="ally"></param>
    public void AddAllyUnit(AllyUnit ally)
    {
        _alliedUnits.Add(ally);
    }

    /// <summary>
    /// Returns the price of the specified allied unit.
    /// </summary>
    /// <param name="unitType"></param>
    public AllyUnitPrice GetUnitPrice(AllyUnitsEnum unitType)
    {
        foreach (AllyUnitPrice unitPrice in _unitPrices)
        {
            if (unitPrice.unitType == unitType)
            {
                return unitPrice;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the price of the specified spell.
    /// </summary>
    /// <param name="spellType"></param>
    public SpellPrice GetSpellCastPrice(SpellEnum spellType)
    {
        foreach (SpellPrice spellPrice in _spellCastingPrices)
        {
            if (spellPrice.spellType == spellType)
            {
                return spellPrice;
            }
        }
        return null;
    }

    public SpellPrice GetSpellUnlockPrice(SpellEnum spellType)
    {
        foreach (SpellPrice spellPrice in _spellUnlockPrices)
        {
            if (spellPrice.spellType == spellType)
            {
                return spellPrice;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the list of cemeteries excluding the specified one.
    /// </summary>
    public List<Cemetery> GetCemeteries(Cemetery exclude = null)
    {
        List<Cemetery> cemeteries = new List<Cemetery>();
        foreach (BaseTower tower in _towers)
        {
            if (tower is Cemetery && tower != exclude)
            {
                cemeteries.Add(tower as Cemetery);
            }
        }
        return cemeteries;
    }

    /// <summary>
    /// Cleans up the specified cemetery by removing killing units inside it and removing dead units from the allied units list.
    /// </summary>
    /// <param name="cemetery"></param>
    public void CleanUpCemetery(Cemetery cemetery)
    {
        foreach (AllyUnit unit in cemetery.GetComponentsInChildren<AllyUnit>())
        {
            unit.ChangeCemetery(null);
            unit.Reset();
            if (unit.Dead)
            {
                Destroy(unit.gameObject);
            }
        }
        _alliedUnits.RemoveAll(unit => unit.Dead);
    }

    /// <summary>
    /// Enters destruction mode to allow the player to demolish buildings and trees
    /// </summary>
    public void EnterDestructionMode()
    {
        _cameraController.EnterDestructionMode();
        HideHUD();
    }

    /// <summary>
    /// Registers a tower to the game.
    /// </summary>
    public void AddTower(BaseTower tower)
    {
        _towers.Add(tower);
    }

    /// <summary>
    /// Registers a cemetery to the game.
    /// </summary>
    public void AddCemetery(Cemetery cemetery)
    {
        _cemeteries.Add(cemetery);
        _currencyIndicator.UpdateCurrencyDisplay(GetBodies(), GetBlood());
    }

    /// <summary>
    /// Removes a tower from the game.
    /// </summary>
    public void RemoveTower(BaseTower tower)
    {
        _towers.Remove(tower);
        Destroy(tower.gameObject);
    }

    /// <summary>
    /// Enters building mode to allow the player to construct a building.
    /// </summary>
    public void Construct(TowerPrice tower)
    {
        _cameraController.ConstructBuilding(tower);
        HideHUD();
    }

    /// <summary>
    /// Unlocks a new spell for the player.
    /// </summary>
    public void UnlockSpell(BaseSpell spellPrefab)
    {
        Instantiate(spellPrefab, _spellCastingMenu.transform);
    }

    /// <summary>
    /// Hides the HUD and shows the cancel menu if specified.
    /// </summary>
    public void HideHUD(bool showCancel = true, bool hideCurrency = false)
    {
        if (hideCurrency)
        {
            _currencyIndicator.CloseMenu();
        }
        if (_currentPhase == PhaseEnum.Build)
        {
            _nextRoundMenu.CloseMenu();
            _constructionMenu.CloseMenu();
            _currencyIndicator.CloseMenu();
        }
        else if (_currentPhase == PhaseEnum.Combat)
        {
            _spellCastingMenu.CloseMenu();
        }
        if (showCancel)
        {
            _cancelMenu.OpenMenu();
        }
    }

    /// <summary>
    /// Shows the HUD and hides other menus.
    /// </summary>
    public void ShowHUD()
    {
        if (_currentPhase == PhaseEnum.Build)
        {
            _constructionMenu.OpenMenu();
            _nextRoundMenu.OpenMenu();
            _cancelMenu.CloseMenu();
            _unitMenu.CloseMenu();
            _spellUnlockMenu.CloseMenu();
            _currencyIndicator.OpenMenu();
        }
        else if (_currentPhase == PhaseEnum.Combat)
        {
            _spellCastingMenu.OpenMenu();
            _cancelMenu.CloseMenu();
            _constructionMenu.CloseMenu();
            _unitMenu.CloseMenu();
            _spellUnlockMenu.CloseMenu();
            _nextRoundMenu.CloseMenu();
            _currencyIndicator.OpenMenu();
        }
    }

    /// <summary>
    /// Returns the price of the specified tower.
    /// </summary>
    /// <param name="towerType"></param>
    public Price GetTowerPrice(TowersEnum towerType)
    {
        foreach (TowerPrice price in _towerPrices)
        {
            if (price.towerType == towerType)
            {
                return price.price;
            }
        }
        return null;
    }

    public void OpenSpellUnlockMenu()
    {
        _spellUnlockMenu.OpenMenu();
        HideHUD(hideCurrency: true);
    }

    /// <summary>
    /// Attempts to purchase and unlock a spell if the player has enough resources.
    /// </summary>
    public void BuySpell(SpellEnum spell)
    {
        SpellPrice spellPrice = GetSpellCastPrice(spell);
        if (spellPrice == null)
        {
            Debug.LogError($"SpellPrice for {spell} not found.");
            return;
        }
        if (GetBodies() >= spellPrice.price.bodyPrice && GetBlood() >= spellPrice.price.bloodPrice)
        {
            RemoveBodies(spellPrice.price.bodyPrice);
            AddBlood(-spellPrice.price.bloodPrice);
            UnlockSpell(spellPrice.spellPrefab);
            Debug.Log($"{spell} spell purchased and unlocked.");
        }
        else
        {
            Debug.Log("Not enough resources to buy this spell.");
        }
    }

    public void EnterCastingMode(BaseSpell spell)
    {
        _cameraController.EnterCastingMode(spell);
        HideHUD();
    }

    public void ExitCastingMode()
    {
        ShowHUD();
    }

    public PhaseEnum GetCurrentPhase()
    {
        return _currentPhase;
    }

    public Round GetNextRound()
    {
        if (_currentRound < _rounds.Length)
        {
            return _rounds[_currentRound];
        }
        else
        {
            return _freePlay;
        }
    }

    public void StartNextRound()
    {
        if (_currentPhase == PhaseEnum.Build)
        {
            StartCoroutine(StartRoundCoroutine());
        }
    }

    private IEnumerator DoPortalAnimation()
    {
        Debug.Log("Starting portal animation for defeated enemies...");
        foreach (EnemyUnit unit in _enemyUnits)
        {
            for (int i = 0; i < unit.BodyReward; i++)
            {
                AddBody();
            }
            _blood += unit.BloodReward;
            if (_blood > _maxBlood) _blood = _maxBlood;
            GameObject portal = Instantiate(_portalPrefab, unit.transform.position, Quaternion.identity);
            portal.transform.DOScale(Vector3.one, _portalAnimationDuration * 0.5f).From(Vector3.zero).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                unit.transform.DOMove(unit.transform.position + Vector3.down * 10f, _portalAnimationDuration * 0.2f).SetEase(Ease.InOutCubic).onComplete = () =>
                {
                    Destroy(unit.gameObject);
                };

                portal.transform.DOScale(Vector3.zero, _portalAnimationDuration * 0.5f).SetEase(Ease.InOutCubic).onComplete = () =>
                {
                    Destroy(portal);
                };
            });
            yield return new WaitForSeconds(_timeBetweenSpawns);
        }
        _enemyUnits.Clear();

        foreach (BaseTower tower in _towers)
        {
            tower.Unpause();
            if (tower is AttackingTower)
            {
                tower.Pause();
            }
        }

        _currentRound++;
        if (_currentRound == _rounds.Length)
        {
            Debug.Log("All rounds completed!");
        }
        _currentPhase = PhaseEnum.Build;

        _nextRoundMenu.UpdateEnemyIcons(GetNextRound(), _currentRound + 1);

        ShowHUD();
        _spellCastingMenu.CloseMenu();
    }

    private IEnumerator MapSpawnAnimation()
    {
        for (int i = 0; i < _mapWidth; i++)
        {
            StartCoroutine(TileRowAnimation(i));
            yield return new WaitForSeconds(0.15f);
        }
        StartCoroutine(InitialZombieSpawns());

        ShowHUD();
    }
    private IEnumerator TileRowAnimation(int row)
    {
        for (int i = 0; i < _mapHeight; i++)
        {
            _tiles[i + row * _mapWidth].SpawnTileAnimation();
            yield return new WaitForSeconds(0.05f);
        }
    }
    private IEnumerator InitialZombieSpawns()
    {
        for (int i = 0; i < _initialResources.bodyPrice; i++)
        {
            AddBody();
            yield return new WaitForSeconds(_timeBetweenSpawns);
        }
    }
}