using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class EnvironmentManager : MonoBehaviour
{
    [SerializeField] private Transform gameEnvironment;
    [SerializeField] public Transform[] units;
    [SerializeField] public Transform[] enemies;
    private Dictionary<Transform, List<Tile>> movementTiles = new Dictionary<Transform, List<Tile>>();
    private Dictionary<Transform, List<Tile>> attackTiles = new Dictionary<Transform, List<Tile>>();
    private Dictionary<Transform, List<Tile>> attackableUnits = new Dictionary<Transform, List<Tile>>();
    private GridManager gridManager;
    private CombatManager combatManager;
    private AugmentationManager augmentationManager;
    private TurnGameManager turnGameManager;
    public bool unitSelected = false; // controls if player has selected a unit
    public Transform selectedUnit = null;
    public bool augmentated = false;
    private int playingSide = 0;


    void Awake()
    {
        gridManager = gameEnvironment.GetComponent<GridManager>();
        combatManager = this.GetComponent<CombatManager>();
        augmentationManager = GameObject.Find("AugmentationManager").GetComponent<AugmentationManager>();
        turnGameManager = this.transform.GetComponent<TurnGameManager>();

    }
    void Start()
    {
  

    }

    public void StartNewEpisode()
    {
        //playingSide = Random.Range(0, 2);
        bool enemySide = UnityEngine.Random.value > 0.5f; // 0 = down, 1 = up
        List<Tile> gridHalfUnit = gridManager.GetGridHalf(enemySide);


        foreach (Transform unit in units)
        {
            UnitInformation unitScript = unit.GetComponent<UnitInformation>();
            gridHalfUnit = unitScript.RandomInitialPosition(gridHalfUnit);


        }

        enemySide = !enemySide;
        List<Tile> gridHalfEnemy = gridManager.GetGridHalf(enemySide);
      

        foreach (Transform enemy in enemies)
        {
            EnemyInformation enemyScript = enemy.GetComponent<EnemyInformation>();
            gridHalfEnemy = enemyScript.RandomInitialPosition(gridHalfEnemy);

        }

        if (augmentationManager.enableAugmentation)
        {
            augmentationManager.BeginNewTurns();
            GameManager.Instance.SwitchState(GameState.prepareAugmentation);
        }
        else
        {
            // set units for each environment individually 
            TurnGameManager turnManager = this.GetComponent<TurnGameManager>();
            turnManager.BeginNewRound();
            setUnits();
        }
    }

    public void RespawnUnits()
    {
        foreach (Transform unit in units)
        {
            UnitInformation unitScript = unit.GetComponent<UnitInformation>();
            unitScript.ResetInformation();
            unitScript.Respawn();

        }


        foreach (Transform enemy in enemies)
        {
            EnemyInformation enemyScript = enemy.GetComponent<EnemyInformation>();
            enemyScript.ResetInformation();
            enemyScript.Respawn();

        }

        // control new episode individually if training is happening
        if (!augmentationManager.enableAugmentation) StartNewEpisode();

    }

    public void setUnits()
    {
        for (int i = 0; i < units.GetLength(0); i++)
        {
            int x = (int)units[i].GetComponent<UnitInformation>().newPosition.x;
            int y = (int)units[i].GetComponent<UnitInformation>().newPosition.y;

            units[i].GetComponent<UnitInformation>().StartRound();

            gridManager.SetUnit(x, y, units[i]);
        }

        for (int i = 0; i < enemies.GetLength(0); i++)
        {
            int x = (int)enemies[i].GetComponent<EnemyInformation>().newPosition.x;
            int y = (int)enemies[i].GetComponent<EnemyInformation>().newPosition.y;

            enemies[i].GetComponent<EnemyInformation>().StartRound();
            gridManager.SetUnit(x, y, enemies[i]);
        }


        if (!augmentationManager.enableAugmentation)
        {
            InitializeInformation(2);
        }
        else
        {
            InitializeInformation(0);
            InitializeInformation(1);
        }
    }


    public void InitializeInformation(int phase)
    {
        switch (phase)
        {
            // update player characters
            case 0:
                for (int i = 0; i < units.GetLength(0); i++) {

                    units[i].GetComponent<UnitInformation>().updateInformation();
                }
                break;

            // update enemy characters
            case 1:
                for (int i = 0; i < enemies.GetLength(0); i++) {

                    enemies[i].GetComponent<EnemyInformation>().updateInformation();

                }
                break;

            // both
            case 2:
                for (int i = 0; i < units.GetLength(0); i++)
                {

                    units[i].GetComponent<UnitInformation>().updateInformation();
                    units[i].GetComponent<StrategyUnitAgent>().EndEpisodeExternally();

                }
                for (int i = 0; i < enemies.GetLength(0); i++)
                {

                    enemies[i].GetComponent<EnemyInformation>().updateInformation();

                    if (GameManager.Instance.SeeGameMode())
                    {
                        enemies[i].GetComponent<StrategyEnemyAgent>().EndEpisodeExternally();
                    }


                }
              
                if (!augmentationManager.enableAugmentation) turnGameManager.StartPlayerTurn();

                break;

            default:
                for (int i = 0; i < units.GetLength(0); i++) {

                    units[i].GetComponent<UnitInformation>().updateInformation();
                }
                for (int i = 0; i < enemies.GetLength(0); i++) {

                    enemies[i].GetComponent<EnemyInformation>().updateInformation();

                }
                break;

        }

    }

    public void TransformMap(int env)
    {

        bool[] mirrored = MirroredOrientation(env);


        for (int i = 0; i < units.Length; i++)
        {
           
            units[i].localPosition = gridManager.MirrorPosition(units[i].GetComponent<UnitInformation>().newPosition, mirrored[0], mirrored[1]);
            units[i].GetComponent<UnitInformation>().newPosition = units[i].localPosition;
        }

        for (int i = 0; i < enemies.Length; i++)
        {
           
            enemies[i].localPosition = gridManager.MirrorPosition(enemies[i].GetComponent<EnemyInformation>().newPosition, mirrored[0], mirrored[1]);
            enemies[i].GetComponent<EnemyInformation>().newPosition = enemies[i].localPosition;
        }

    }

    // returns MirrorX, MirrorY bool values for environment env
    public bool[] MirroredOrientation(int env)
    {
        bool[] mirrored = { false, false };
        switch (env)
        {
            case 0:
                //mirrored = {false, false};

                return mirrored;

            case 1:
                //bool [] mirrored = {true, false};
                mirrored[0] = true;

                return mirrored;

            case 2:

                //bool[] mirrored = {false, true};
                mirrored[1] = true;

                return mirrored;

            case 3:

                mirrored[0] = true;
                mirrored[1] = true;

                return mirrored;

            default:

                return mirrored;
        }


    }

    public void setSelectedUnit(Transform unit) {


        if (!selectedUnit) {
            selectedUnit = unit;
        }
        else {
            selectedUnit = null;
        }

    }

    public void ResetTileInformation(Transform unit)
    {
        if (movementTiles.ContainsKey(unit)) movementTiles[unit].Clear();
        if (attackTiles.ContainsKey(unit)) attackTiles[unit].Clear();
        if (attackableUnits.ContainsKey(unit)) attackableUnits[unit].Clear();

    }

    public void setTilesInRange(Transform unit, Vector2 start, int stepSize, int attackRange, bool isEnemy)
    {
        string unitType = "infantry";
        int startX = (int)start.x;
        int startY = (int)start.y;

        // Clear previous data
        if (movementTiles.ContainsKey(unit)) movementTiles[unit].Clear();
        if (attackTiles.ContainsKey(unit)) attackTiles[unit].Clear();
        if (attackableUnits.ContainsKey(unit)) attackableUnits[unit].Clear();

        List<Tile> moveTilesList = new List<Tile>();
        List<Tile> attackTilesList = new List<Tile>();
        List<Tile> attackableUnitsList = new List<Tile>();

        Queue<Tile> queue = new Queue<Tile>();
        Dictionary<Tile, int> movementCost = new Dictionary<Tile, int>();

        Tile startTile = gridManager.GetTileAtPosition(startX, startY);
        if (startTile == null) return;

        queue.Enqueue(startTile);
        movementCost[startTile] = 0;

        if (isEnemy) unitType = unit.GetComponent<EnemyInformation>().unitType;
        else unitType = unit.GetComponent<UnitInformation>().unitType;

        // Movement Range BFS 
        while (queue.Count > 0)
        {
            Tile currentTile = queue.Dequeue();
            int tileType = currentTile.GetTileType();
            Vector2 currentPos = new Vector2((int)currentTile.transform.localPosition.x, (int)currentTile.transform.localPosition.y);
            int currentStep = movementCost[currentTile];

            if (currentStep > stepSize) continue;

            if (!CanBeReached(unitType, tileType)) continue;

            if (currentTile.IsWithinMovementRange(start, currentPos, stepSize))
            {
                moveTilesList.Add(currentTile);
                currentTile.inRangeUnit = unit;
            }


            foreach (Tile neighbor in gridManager.GetAdjacentTiles(currentTile))
            {
                if (neighbor == null || movementCost.ContainsKey(neighbor)) continue;

                // check if neighbor contains enemy
                Transform containUnit = gridManager.GetUnit((int)neighbor.transform.localPosition.x, (int)neighbor.transform.localPosition.y);

                bool isOpponent = containUnit && ((containUnit.gameObject.tag == "Enemy" && unit.gameObject.tag == "Player") ||
                                                    (containUnit.gameObject.tag == "Player" && unit.gameObject.tag == "Enemy"));

                bool isBlocked = !neighbor.IsWalkable || isOpponent;

                // still add to attackable list if within reach but blocked by enemy
                if (isOpponent && currentStep + 1 <= stepSize + attackRange)
                {
                    if (!attackTilesList.Contains(neighbor))
                        attackTilesList.Add(neighbor);
                    if (!attackableUnitsList.Contains(neighbor))
                        attackableUnitsList.Add(neighbor);
                }

                if (isBlocked) continue;

                queue.Enqueue(neighbor);
                movementCost[neighbor] = currentStep + 1;
            }
        }

        // Attack Range BFS
        HashSet<Tile> visitedAttackTiles = new HashSet<Tile>();

        foreach (Tile moveTile in moveTilesList)
        {
            Queue<Tile> attackQueue = new Queue<Tile>();
            Dictionary<Tile, int> attackCost = new Dictionary<Tile, int>();

            attackQueue.Enqueue(moveTile);
            attackCost[moveTile] = 0;

            while (attackQueue.Count > 0)
            {
                Tile current = attackQueue.Dequeue();
                int dist = attackCost[current];

                if (dist > attackRange) continue;

                if (dist > 0 && !visitedAttackTiles.Contains(current))
                {
                    visitedAttackTiles.Add(current);

                    // Don't include if it’s already in movement list
                    if (!moveTilesList.Contains(current) && !attackTilesList.Contains(current))
                    {
                        attackTilesList.Add(current);
                    }

                    // If there's an enemy on this tile, add to attackable
                    Transform unitOnTile = gridManager.GetUnit(
                        (int)current.transform.localPosition.x,
                        (int)current.transform.localPosition.y);

                    if (unitOnTile && ((unitOnTile.gameObject.tag == "Enemy" && unit.gameObject.tag == "Player") || (unitOnTile.gameObject.tag == "Player" && unit.gameObject.tag == "Enemy")))
                    {
                        if (!attackableUnitsList.Contains(current))
                            attackableUnitsList.Add(current);
                    }
                }

                foreach (Tile neighbor in gridManager.GetAdjacentTiles(current))
                {
                    if (neighbor == null || attackCost.ContainsKey(neighbor)) continue;

                    attackQueue.Enqueue(neighbor);
                    attackCost[neighbor] = dist + 1;
                }
            }
        }

        movementTiles[unit] = moveTilesList;
        attackTiles[unit] = attackTilesList.Except(attackableUnitsList).ToList();
        attackableUnits[unit] = attackableUnitsList;
    }

    public List<Tile> FilterTiles(List<Tile> tileList, Transform unit)
    {
        List<Tile> filteredList = new List<Tile>(tileList);

        foreach (Tile tile in tileList)
        {
            Transform containUnit = gridManager.GetUnit((int)tile.transform.localPosition.x, (int)tile.transform.localPosition.y);

            if (containUnit && containUnit != unit)
            {
                filteredList.Remove(tile);
            }
        }

        return filteredList;
    }

    bool CanBeReached(string unitType, int tileType)
    {
        // can it be reaching if mountain or water?
        if (tileType == 2 || tileType == 3)
        {

            if (unitType == "flying")
            {
                return true;
            }
            else
            {

                return false;
            }
        }
        // if it's forest
        else if (tileType == 1)
        {
            if (unitType == "cavalry") {

                return false;
            }
            else return true;
        }
        else
        {
            return true;
        }
    }

    public int GetUnitIndex(Transform unit, bool isEnemy)
    {
        if (isEnemy)
        {
            return Array.IndexOf(enemies, unit);
        }
        else
        {
            return Array.IndexOf(units, unit);
        }
    }

    public List<Tile> GetMovementTiles(Transform unit) {
        if (movementTiles.TryGetValue(unit, out var tiles)) {

            return FilterTiles(tiles, unit); ;
        }

        else {
            return null;
        }
    }

    public List<Tile> GetAttackTiles(Transform unit) {
        if (attackTiles.TryGetValue(unit, out var tiles)) {
            return tiles;
        }

        else {
            return null;
        }
    }

    public List<Tile> GetAttackableUnitsTiles(Transform unit) {
        if (attackableUnits.TryGetValue(unit, out var tiles)) {
            return tiles;
        }

        else {
            return null;
        }
    }

    public void SetEndGameReward(bool won)
    {
        foreach (Transform unit in units)
        {
            StrategyUnitAgent agent = unit.GetComponent<StrategyUnitAgent>();

            if (won)
            {
                agent.AddReward(1f);
            }
            else
            {
                agent.AddReward(-1f);
            }
        }
    }

}