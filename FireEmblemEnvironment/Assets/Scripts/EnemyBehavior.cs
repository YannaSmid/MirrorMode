using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyBehavior : MonoBehaviour
{
    private EnvironmentManager envManager;
    private TurnGameManager turnGameManager;
    private GridManager gridManager;
    private CombatManager combatManager;
    private AugmentationManager augmentationManager;
    private TextManager textManager;
    private Transform DemoRecorderManager;
    public List<Transform> controlledUnits;
    public StrategyEnemyAgent enemyAgent;

    int[] possibleActions;

    bool isTurn = false;


    void Start()
    {
        gridManager = this.transform.parent.GetComponent<GridManager>();
        envManager = this.transform.GetComponent<EnvironmentManager>();
        turnGameManager = envManager.transform.GetComponent<TurnGameManager>();
        combatManager = this.transform.parent.Find("EnvironmentManager").GetComponent<CombatManager>();
        augmentationManager = GameObject.Find("AugmentationManager").GetComponent<AugmentationManager>();
        textManager = GameObject.Find("TextManager").GetComponent<TextManager>();
        DemoRecorderManager = GameObject.Find("DemoRecorderManager").transform;
        controlledUnits = new List<Transform>();
    }

   
    void Update()
    {
        isTurn = turnGameManager.isEnemyTurn;

        if (isTurn && controlledUnits.Count > 0 && !envManager.unitSelected)
        {
            // select enemy to perform its action
            Transform selectedUnit = SelectUnit();

            controlledUnits.Remove(selectedUnit);

            envManager.unitSelected = true;
            envManager.setSelectedUnit(selectedUnit);


            if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining)
            {
                if (!envManager.augmentated)
                {
                    // Wait for UI to disappear, then perform action
                    StartCoroutine(WaitForStatsUI(1.5f, selectedUnit));
                }
            }
            else
            {
                // StandardAlgorithm(selectedUnit);
                if (!GameManager.Instance.SeeGameMode())
                {
                    StandardAlgorithm(selectedUnit);
                }
                else
                {
                    MirrorModel(selectedUnit);
                }
            }


        }

    }

    private Transform SelectUnit()
    {
        List<Transform> meleeUnits = new List<Transform>();
        List<int> movementCosts = new List<int>();

        int movementCost = 0;
        Transform bestUnit = null;

        foreach (Transform unit in controlledUnits)
        {
            EnemyInformation unitScript = unit.GetComponent<EnemyInformation>();
            // prioritize melee attacks over range
            if (unitScript.attackRange == 1)
            {
                
                meleeUnits.Add(unit);

                // if more melee units, pick unit with highest movement range
                if (unitScript.stepSize > movementCost)
                {
                    bestUnit = unit;
                    movementCost = unitScript.stepSize;
                }
                movementCosts.Add(unitScript.stepSize);
            }
            else
            {
                movementCosts.Add(unitScript.stepSize);

            }
        }

        if (bestUnit == null)
        {


            int maxValue = movementCosts.Max();
            int lastMaxIndex = movementCosts.IndexOf(maxValue);

            bestUnit = controlledUnits[lastMaxIndex];
        }

        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining) textManager.ShowEnemyStats(true, bestUnit.GetComponent<EnemyInformation>().unitIndex);
        return bestUnit;
    }

    private void StandardAlgorithm(Transform selectedUnit)
    {
        EnemyInformation currentUnit = selectedUnit.GetComponent<EnemyInformation>();
        List<Tile> moves = currentUnit.GetTilesInRange(0);
        List<Tile> attackableUnits = envManager.GetAttackableUnitsTiles(selectedUnit);

        if (attackableUnits.Count > 0)
        {

            // calculate
            int targetHealth = 0;
            int thisHealth = 0;
            int highestDamage = -1;
            Transform bestAttack = null;
            Tile targetTile = null;

            foreach (Tile unitTile in attackableUnits)
            {

                Vector2 tilePos = gridManager.GetPositionFromTile(unitTile);
                Transform unit = unitTile.checkUnit(tilePos);
                UnitInformation playerScript = unit.GetComponent<UnitInformation>();

                int damage = combatManager.CalculateDamage(playerScript, currentUnit, currentUnit.isEnemy);

                targetHealth = playerScript.health - damage;
              
                if (targetHealth <= 0)
                {
                    // attack this unit!
                    highestDamage = damage;
                    bestAttack = unit;
                    targetTile = unitTile;
                }
                else if (damage >= highestDamage)
                {
                    highestDamage = damage;
                    bestAttack = unit;
                    targetTile = unitTile;
                }

            }

            UnitInformation target = bestAttack.GetComponent<UnitInformation>();
           
            if (target)
            {
                currentUnit.WantToAttack(target);
              
                List<Tile> launchableMoves = gridManager.GetAttackTilesInRange(targetTile, currentUnit.attackRange, moves);

                // check if there is a unit already at this tile
                for (int i = launchableMoves.Count - 1; i >= 0; i--)
                {
                    Vector2 tilePos = gridManager.GetPositionFromTile(launchableMoves[i]);
                    if (launchableMoves[i].checkUnit(tilePos))
                    {
                        if (launchableMoves[i].checkUnit(tilePos) != currentUnit.transform)
                        {
                            launchableMoves.RemoveAt(i);
                        }

                    }
                }

                // if place to attack then attack
                if (launchableMoves.Count > 0)
                {
                    // if attack is not blocked you can choose a tile from the launchable tiles
                    int randomAction = DrawRandomAction(launchableMoves);

                    if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining) textManager.ShowCombatUI(true);
                    performAction(launchableMoves[randomAction], 2);
                   
                }
                // else just move
                else
                {
                    combatManager.ResetCombat();

                    int randomAction = DrawRandomAction(moves);
                    performAction(moves[randomAction], 1);

                }

            }

            else
            {
                Debug.LogWarning("Fix bug; no target found");
                performAction(null, 0);
              
                return;
            }

        }

        else
        {
            // if no unit in range, move closer to targets

            Transform target = FindWeakestTarget(currentUnit);
            Tile targetTile = target.GetComponent<UnitInformation>().currentTile;

            Tile selectedTile = SelectTileCloserToTarget(moves, targetTile);
            performAction(selectedTile, 1);
          
        }

    }

    private void MirrorModel(Transform selectedUnit)
    {

        enemyAgent = selectedUnit.GetComponent<StrategyEnemyAgent>();
        enemyAgent.RequestDecision();

    }
    private void performAction(Tile tile, int actionType)
    {
       
        Transform selectedUnit = envManager.selectedUnit;

        EnemyInformation enemyScript = selectedUnit.GetComponent<EnemyInformation>();

        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining)
        {
            
            StartCoroutine(WaitForCombatUI(3f, tile, actionType));
        }
        else
        {


            enemyScript.performAction(tile, actionType);


            enemyScript.endUnitTurn(); 
        }



    }

    private Tile SelectTileCloserToTarget(List<Tile> moves, Tile target)
    {
        float minimalDist = 100f;

        Tile bestTile = null;

        foreach (Tile tile in moves)
        {
            float dist = gridManager.GetDistance(tile, target);

            if (dist < minimalDist)
            {
                bestTile = tile;
                minimalDist = dist;
            }
        }

        return bestTile;
    }

    private Transform FindWeakestTarget(EnemyInformation currentUnit)
    {
        Transform weakestTarget = null;
        int highestDamage = -1;
        foreach (Transform unit in envManager.units)
        {
            UnitInformation playerScript = unit.GetComponent<UnitInformation>();
            if (playerScript.isDead) continue;

            int damage = combatManager.CalculateDamage(playerScript, currentUnit, currentUnit.isEnemy);

            if (damage > highestDamage)
            {
                highestDamage = damage;
                weakestTarget = unit;
            }

        }

        return weakestTarget;
    }

    private void RandomAlgorithm(Transform selectedUnit)
    {

        EnemyInformation currentUnit = selectedUnit.GetComponent<EnemyInformation>();
        List<Tile> moves = currentUnit.GetTilesInRange(0);
        List<Tile> attackableUnits = currentUnit.GetTilesInRange(2);

        // filter moveable tiles
        for (int i = moves.Count - 1; i >= 0; i--)
        {
            Vector2 tilePos = gridManager.GetPositionFromTile(moves[i]);
            if (moves[i].checkUnit(tilePos))
            {
                moves.RemoveAt(i);
            }
        }

        // if attack is possible, attack by a chance 
        if (attackableUnits.Count > 0 && Random.Range(0, 3) == 2)
        {

            // Means attack
            int targetInt = DrawRandomAction(attackableUnits);
            Tile targetTile = attackableUnits[targetInt];
            Vector2 targetPos = gridManager.GetPositionFromTile(targetTile);
            Transform target = targetTile.checkUnit(targetPos);

            if (target == null)
            {
                Debug.Log("no target here");

            }
            else
            {
                currentUnit.WantToAttack(target.GetComponent<UnitInformation>());
            }

           
            List<Tile> launchableMoves = currentUnit.GetTilesInRange(3);

            // check if there is a unit already at this tile
            for (int i = launchableMoves.Count - 1; i >= 0; i--)
            {
                Vector2 tilePos = gridManager.GetPositionFromTile(launchableMoves[i]);
                if (launchableMoves[i].checkUnit(tilePos))
                {
                    launchableMoves.RemoveAt(i);
                }
            }

            if (launchableMoves.Count > 0)
            {

                int randomAction = DrawRandomAction(launchableMoves);

                currentUnit.performAction(launchableMoves[randomAction], 2);
            }
            else
            {
                combatManager.ResetCombat();

                int randomAction = DrawRandomAction(moves);
                currentUnit.performAction(moves[randomAction], 1);
            }

        }
        // otherwise just move
        else
        {
            // just move
            int randomAction = DrawRandomAction(moves);

            foreach (Tile tile in moves)
            {
                tile.moveable = true;

            }

            currentUnit.performAction(moves[randomAction], 1);

        }
    }

    public int DrawRandomAction(List<Tile> moves)
    {
        return Random.Range(0, moves.Count);
    }

    // wait for stats ui 
    IEnumerator WaitForStatsUI(float time, Transform selectedUnit)
    {
        yield return new WaitForSeconds(time);

        // choose enemy behavior type
        if (!GameManager.Instance.SeeGameMode())
        {
            StandardAlgorithm(selectedUnit);
        }
        else
        {
            MirrorModel(selectedUnit);
        }
        

    }
    
    
    // Wait a bit to show combat UI during enemy combat
    IEnumerator WaitForCombatUI(float time, Tile tile, int actionType)
    {
        yield return new WaitForSeconds(time);
        Transform selectedUnit = envManager.selectedUnit;

        EnemyInformation enemyScript = selectedUnit.GetComponent<EnemyInformation>();

        enemyScript.PushAction(tile, actionType);

    }

}
