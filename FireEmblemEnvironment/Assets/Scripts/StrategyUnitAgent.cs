using System;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Policies;

public class StrategyUnitAgent : Agent
{
    [Header("References")]
    public UnitInformation unitController; 
    public EnemyInformation enemyController; 
    private UnitInformation targetUnit;
    private EnemyInformation targetEnemy;
    public GridManager gridManager;
    public CombatManager combatManager;
    public TurnGameManager turnGameManager;
    public EnvironmentManager envManager;
    private PlayerMetrics metricsLogger;
    public Transform DemoRecorderManager;
    private DemonstrationRecorder demoRecorder;

    //recording stats for configs
    private StatsRecorder statsRecorder;
    public int validActions;
    public int validAttacks;
    public int validMovements;
    public int totalActions;
    public int kills;
    public int wins;
    
    // variables for chosen action
    public bool pushAction = false;
    public bool tookAction = false;
    private int chosenAction;
    private int chosenTileIndex; 
    private int chosenTarget;

    [Header("Agent Settings")]
    public bool isEnemy = false;
    public bool maskActions = true;
    public int maxActions = 30;
    public int maxEpisodeSteps = 12;
   

    private List<Tile> currentLegalTiles = new List<Tile>();
    public List<Tile> movementTiles = new List<Tile>();
    public List<Tile> attackTiles = new List<Tile>();
    public List<Tile> attackableUnits = new List<Tile>();
    public List<Tile> launchableTiles = new List<Tile>();
    private Transform unitTransform;
    
    public enum WeaponType {Sword, Lance, Axe, Bow, Magic, LastWeaponType }
    public enum UnitType {Infantry, Cavalry, Flying, HeavyArmor, LastUnitType }

    const int NUM_WEAPON_TYPES = (int)WeaponType.LastWeaponType + 1;
    const int NUM_UNIT_TYPES = (int)UnitType.LastUnitType + 1;

    void Awake()
    {
        if (isEnemy)
        {
            enemyController = this.unitTransform.GetComponent<EnemyInformation>();
        }
        else
        {
            unitController = this.transform.GetComponent<UnitInformation>();
        }
    }
    
    public void Start()
    {
        statsRecorder = Academy.Instance.StatsRecorder;
        validActions = 0;
        validAttacks = 0;
        validMovements = 0;
        totalActions = 0;
        kills = 0;
        wins = 0;

        gridManager = this.transform.parent.GetComponent<GridManager>();
        envManager = this.transform.parent.Find("EnvironmentManager").GetComponent<EnvironmentManager>();
        turnGameManager = envManager.transform.GetComponent<TurnGameManager>();
        combatManager = envManager.transform.GetComponent<CombatManager>();
        metricsLogger = GameObject.Find("MetricsLogger").transform.GetComponent<PlayerMetrics>();
        demoRecorder = GetComponent<DemonstrationRecorder>();
        DemoRecorderManager = GameObject.Find("DemoRecorderManager").transform;

        this.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BehaviorType = DemoRecorderManager.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BehaviorType;
        if (demoRecorder != null && DemoRecorderManager != null)
        {
            demoRecorder.Record = DemoRecorderManager.GetComponent<DemonstrationRecorder>().Record;
            demoRecorder.DemonstrationDirectory = DemoRecorderManager.GetComponent<DemonstrationRecorder>().DemonstrationDirectory;

        }

    }

    public void EndEpisodeExternally()
    {
        LogEpisodeStats(); // call this before EndEpisode()
        EndEpisode();
    }

    public void LogEpisodeStats()
    {
        statsRecorder.Add("AgentMetrics/ValidActions", validActions / (float)totalActions);
        statsRecorder.Add("AgentMetrics/ValidAttacks", validAttacks / (float)totalActions);
        statsRecorder.Add("AgentMetrics/ValidMovements", validMovements / (float)totalActions);
        statsRecorder.Add("AgentMetrics/Kills", kills);
        statsRecorder.Add("AgentMetrics/Wins", turnGameManager.wins / (float)turnGameManager.totalRounds);

        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining && !envManager.augmentated && !GameManager.Instance.SeeGameMode())
        {

            metricsLogger.AddActions(unitController.unitIndex, validAttacks, validMovements);
            
        }
 
        validActions = 0;
        validAttacks = 0;
        validMovements = 0;
        totalActions = 0;
        kills = 0;
    }

    private float NormalizeValues(int currentValue, int minValue, int maxValue)
    {
        float normalized = (currentValue - minValue) / (maxValue - minValue);

        return normalized;
    }

    private bool[] RangedTiles(List<Tile> tiles)
    {
        bool[] tileMap = new bool[gridManager.GetAllTileIndices().Count];

        foreach (Tile tile in tiles)
        {
            int index = gridManager.GetIndexFromPosition(tile);
            if (index >= 0 && index < tileMap.Length)
                tileMap[index] = true;
        }

        return tileMap;
    }

    public override void OnEpisodeBegin()
    {

        // Reset handled externally, e.g. by battle manager
        // This just clears the state for new turn/episode
       
        if (isEnemy)
        {
            enemyController = this.unitTransform.GetComponent<EnemyInformation>();
        }
        else
        {
            unitController = this.transform.GetComponent<UnitInformation>();

            movementTiles = unitController.GetTilesInRange(0);
            attackTiles = unitController.GetTilesInRange(1);
            attackableUnits = unitController.GetTilesInRange(2);
        }



        currentLegalTiles.Clear();
        SetAvailableActions(movementTiles, attackTiles);
    }
    public void ResetInfo()
    {
        tookAction = false;
        List<Tile> movementTiles = unitController.GetTilesInRange(0);
        List<Tile> attackTiles = unitController.GetTilesInRange(1);
        List<Tile> attackableUnits = unitController.GetTilesInRange(2);
        currentLegalTiles.Clear();
        SetAvailableActions(movementTiles, attackTiles);

    }

    public void SetUnit(Transform unit)
    {
        unitTransform = unit;
    }

    public void SetAvailableActions(List<Tile> moveTiles, List<Tile> attackTiles)
    {
        currentLegalTiles.Clear();
        currentLegalTiles.AddRange(moveTiles);
    }

    public void StartAgentTurn()
    {
        tookAction = false;
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
       
        Transform[] targets = new Transform[4];
        Tile thisCurrentTile = null;
        int enemyWeapon;
        int unitWeapon;

        int unitType;
        int enemyType;


        // add target info
        if (isEnemy)
        {
            targets = envManager.units;
            thisCurrentTile = enemyController.currentTile;
        }

        else
        {
            targets = envManager.enemies;
            thisCurrentTile = unitController.currentTile;
        }

        foreach (Transform target in targets)
        {

            if (isEnemy)
            {
 

                targetUnit = target.GetComponent<UnitInformation>();
                unitWeapon = targetUnit.GetWeaponType();
                unitType = targetUnit.GetUnitType();
                sensor.AddOneHotObservation((int)unitWeapon, NUM_WEAPON_TYPES);
                sensor.AddOneHotObservation((int)unitType, NUM_UNIT_TYPES);
                // add stats
                sensor.AddObservation(NormalizeValues(targetUnit.health, 1, 100));
                sensor.AddObservation(NormalizeValues(targetUnit.atk, 1, 100));
                sensor.AddObservation(NormalizeValues(targetUnit.def, 1, 100));
                sensor.AddObservation(NormalizeValues(targetUnit.res, 1, 100));
                sensor.AddObservation(NormalizeValues(targetUnit.spd, 1, 100));
            }
            else
            {

                targetEnemy = target.GetComponent<EnemyInformation>();

                enemyWeapon = targetEnemy.GetWeaponType();
                enemyType = targetEnemy.GetUnitType();
                sensor.AddOneHotObservation((int)enemyWeapon, NUM_WEAPON_TYPES);
                sensor.AddOneHotObservation((int)enemyType, NUM_UNIT_TYPES);

                sensor.AddObservation(NormalizeValues(targetEnemy.health, 1, 100));
                sensor.AddObservation(NormalizeValues(targetEnemy.atk, 1, 100));
                sensor.AddObservation(NormalizeValues(targetEnemy.def, 1, 100));
                sensor.AddObservation(NormalizeValues(targetEnemy.res, 1, 100));
                sensor.AddObservation(NormalizeValues(targetEnemy.spd, 1, 100));

            }

            // find distance between unit and target
            int distance = gridManager.GetDistance(this.transform, target);
            sensor.AddObservation(NormalizeValues(distance, 0, gridManager.GetMaxDistance()));

            // check type advantage

            if (attackableUnits.Contains(gridManager.GetTileAtPosition((int)target.localPosition.x, (int)target.localPosition.y)))
            {
                sensor.AddObservation(1); // if target is in range to attack
            }
            else
            {
                sensor.AddObservation(0);
            }
        }

        int currentTileIndex = gridManager.GetIndexFromPosition(thisCurrentTile);
        unitWeapon = unitController.GetWeaponType();
        unitType = unitController.GetUnitType();
        sensor.AddOneHotObservation((int)unitWeapon, NUM_WEAPON_TYPES);
        sensor.AddOneHotObservation((int)unitType, NUM_UNIT_TYPES);

        bool[] rangeMovementTiles = RangedTiles(unitController.GetTilesInRange(0));
        foreach (bool b in rangeMovementTiles)
        {
            sensor.AddObservation(b ? 1f : 0f);
        }

        // // own info
        sensor.AddObservation(NormalizeValues(unitController.health, 1, 100));
        sensor.AddObservation(NormalizeValues(unitController.atk, 1, 100));
        sensor.AddObservation(NormalizeValues(unitController.def, 1, 100));
        sensor.AddObservation(NormalizeValues(unitController.res, 1, 100));
        sensor.AddObservation(NormalizeValues(unitController.spd, 1, 100));

    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        
        if (!maskActions) return;

         // Prevents the agent from picking an action that is not allowed
        var positionX = (int)transform.localPosition.x;
        var positionY = (int)transform.localPosition.y;
        int maxX = gridManager.GetMaxGridWidth();
        int maxY = gridManager.GetMaxGridHeight();

        movementTiles = envManager.GetMovementTiles(this.transform);
        attackTiles = envManager.GetAttackTiles(this.transform);
        attackableUnits = envManager.GetAttackableUnitsTiles(this.transform);

        List<int> validTiles = new List<int>();

        // disable attack if no targets close
        if (attackableUnits.Count == 0)
        {
            actionMask.SetActionEnabled(0, 2, false);

        }
        else
        {
            var units = new List<int> { 0, 1, 2, 3 };
            foreach (Tile tile in attackableUnits)
            {
                Vector2 pos = gridManager.GetPositionFromTile(tile);
                int targetInd = gridManager.GetUnit((int)pos.x, (int)pos.y).GetComponent<EnemyInformation>().unitIndex;
                units.Remove(targetInd);

            }

            foreach (int i in units)
            {
                actionMask.SetActionEnabled(2, i, false);
                
            }
        }

        
        // mask invalid movement tiles
        foreach (Tile tile in movementTiles)
        {
            int index = gridManager.GetIndexFromPosition(tile);
            validTiles.Add(index);
        }


        if (validTiles.Count != 0)
        {

           
            List<int> notValidTiles = gridManager.GetAllTileIndices().Except(validTiles).ToList();
           


            foreach (int i in notValidTiles)
            {
                actionMask.SetActionEnabled(1, i, false);
            }
        }
        else
        {
            Debug.LogWarning("No valid movement tiles! Skipping action mask to avoid error.");
        }

        // mask dead targets
        if (isEnemy)
        {
            // also mask current tile as movement option, to equalize chances to wait on current tile
            if (movementTiles.Count > 1) actionMask.SetActionEnabled(1, gridManager.GetIndexFromPosition(enemyController.currentTile), false);

            //disable dead units as target
            foreach (Transform u in envManager.units)
            {
                int ind = envManager.GetUnitIndex(u, false);

                if (u.GetComponent<UnitInformation>().isDead)
                {
                    actionMask.SetActionEnabled(2, ind, false);
                }
            }
        }
        else
        {
            if (movementTiles.Count > 1) actionMask.SetActionEnabled(1, gridManager.GetIndexFromPosition(unitController.currentTile), false);

            //disable dead units as target
            foreach (Transform e in envManager.enemies)
            {
                int ind = envManager.GetUnitIndex(e, true);

                if (e.GetComponent<EnemyInformation>().isDead)
                {
                    actionMask.SetActionEnabled(2, ind, false);
                }
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        totalActions += 1;
        int selectedAction = actions.DiscreteActions[0];
        int tileIndex = actions.DiscreteActions[1];
        int targetIndex = actions.DiscreteActions[2];

        
        Vector2 selectedPos = gridManager.GetPositionFromIndex(tileIndex);
        Tile selectedTile = gridManager.GetTileAtPosition(selectedPos);
     
        movementTiles = envManager.GetMovementTiles(this.transform);
        attackTiles = envManager.GetAttackTiles(this.transform);
        attackableUnits = envManager.GetAttackableUnitsTiles(this.transform);
    
       
        if (selectedTile == unitController.currentTile && selectedAction == 1)
        {
            
            selectedAction = 0;
        }

        if (selectedAction == 2) // if they want to attack
            {
                if (targetIndex >= 0 && targetIndex < 4)
                {
                    Transform target = null;
                    Tile targetTile = null;

                    bool accepted = false;

                    // prepare for battle if target is valid
                    if (isEnemy)
                    {
                        target = envManager.units[targetIndex];
                        targetTile = target.GetComponent<UnitInformation>().currentTile;
                       
                        if (target.GetComponent<UnitInformation>().isDead)
                        {
                          
                            SetReward(-0.3f);
                            performAction(null, 0);

                        }
                        else
                        {
                            accepted = true;

                            enemyController.WantToAttack(target.GetComponent<UnitInformation>());
                            
                            launchableTiles = enemyController.GetTilesInRange(3);
                            
                        }



                    }
                    else
                    {
                        target = envManager.enemies[targetIndex];
                        targetTile = target.GetComponent<EnemyInformation>().currentTile;
                      
                        if (target.GetComponent<EnemyInformation>().isDead)
                        {
                            SetReward(-0.3f);
                            performAction(null, 0);

                        }
                        else
                        {
                            unitController.WantToAttack(target.GetComponent<EnemyInformation>());
                            accepted = true;
                           
                            launchableTiles = unitController.GetTilesInRange(3);
                           
                        }




                    }

                    if (!launchableTiles.Contains(selectedTile) && accepted)
                    {

                        
                        if (launchableTiles.Count > 0)
                        {
                            selectedTile = launchableTiles[UnityEngine.Random.Range(0, launchableTiles.Count - 1)];
                           
                            combatManager.SetAttacker(this.transform);
                            combatManager.instantiateAttack = true;


                            performAction(selectedTile, selectedAction);

                                if (target.GetComponent<EnemyInformation>().isDead)
                                {
                                 
                                    AddReward(1f);
                                    kills += 1;
                                }
                                else
                                {
                                    AddReward(0.3f);
                                }
                                
                            }
                    else
                    {
                       
                        // cancel attack
                        combatManager.ResetCombat();

                        if (movementTiles.Contains(selectedTile))
                        {
                            performAction(selectedTile, 1);
                            AddReward(-0.1f);
                        }
                        else
                        {
                            AddReward(-0.3f);
                        }
                    }
      

                    }

                    else if (launchableTiles.Contains(selectedTile) && accepted)
                    {
                        if (attackableUnits.Contains(targetTile))
                        {
                           
                            combatManager.SetAttacker(this.transform);
                            combatManager.instantiateAttack = true;

                            
                            performAction(selectedTile, selectedAction);
                            if (isEnemy)
                            {
                                if (target.GetComponent<UnitInformation>().isDead)
                                {
                                   
                                    AddReward(1f);
                                    kills += 1;
                                }
                                else
                                {
                                    AddReward(0.3f);
                                }
                            }
                            else 
                            {
                                if (target.GetComponent<EnemyInformation>().isDead)
                                {
                                    
                                    AddReward(1f);
                                    kills += 1;
                                }
                                else
                                {
                                    AddReward(0.3f);
                                }
                            }
                            validActions += 1;
                            validAttacks += 1;
                        }
                        else
                        {
                            performAction(null, 0);
                            AddReward(-0.1f);

                        }

                    }

                }
                else
                {
                    
                    
                    performAction(null, 0);

                }

            }

            // else only move
            else if (selectedAction == 1)

            {
                if (!movementTiles.Contains(selectedTile))
                {
                  
                    AddReward(-0.3f);
                    performAction(null, 0);

                }
                else
                {
                    if (isEnemy)
                    {
                        Debug.LogWarning("Needs enemy performing action!!!");
                        
                    }
                    else
                    {
                       
                        AddReward(0f);
                        performAction(selectedTile, selectedAction);
                        validActions += 1;
                        validMovements += 1;
                    }
                }
            }

            // else go idle
            else
            {
                performAction(null, 0);
                AddReward(-0.2f);

            }

        if (totalActions == maxEpisodeSteps)
        {
           
            turnGameManager.gameEnded = true;
            AddReward(-1f);
        }
  
    EndTurn(); // End turn logic from your game
    }

    
    private void performAction(Tile selectedTile, int actionType)
    {
    
        if (isEnemy)
        {
            enemyController.performAction(selectedTile, actionType);
        }
        else
        {
            unitController.performAction(selectedTile, actionType);
        }
        return;
               
    }
    public void PushActionThrough(int tileIndex, int action, int targetIndex)
    {
        
        chosenAction = action;
        chosenTileIndex = tileIndex;
        chosenTarget = targetIndex;
        pushAction = true;

        RequestDecision();

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        
        // Default to Idle
        int actionType = 0;
        int tileChosen = 0;
        int enemyIndex = 0;

        // Let's assume the player has clicked something via UI or mouse
        if (pushAction)
        {
            
            if (chosenTileIndex > 0) // some tile clicked
            {
                if (chosenAction == 1) // move
                {
                    actionType = 1; // Move
                    tileChosen = chosenTileIndex;
                   
                }
                else if (chosenAction == 2) // attack
                {
                 
                    actionType = 2; // Attack
                    tileChosen = chosenTileIndex;
                    enemyIndex = chosenTarget;
                    
                  
                }
                else
                {
                    actionType = 0;
                 

                }
            }
            pushAction = false;

            discreteActionsOut[0] = actionType;
            discreteActionsOut[1] = tileChosen;
            discreteActionsOut[2] = enemyIndex;
        }
        
    }

    void Update()
    {
        if (this.GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BehaviorType == BehaviorType.HeuristicOnly)
        {
            return;
        }
        if (unitController && unitController.isTurn && !tookAction){
            
            tookAction = true;
            RequestDecision();
        } 


    }


    private void EndTurn()
    {
       
        if (isEnemy)
        {
            enemyController.endUnitTurn();
        }
        else
        {
            unitController.endUnitTurn();
        }
    }
}
