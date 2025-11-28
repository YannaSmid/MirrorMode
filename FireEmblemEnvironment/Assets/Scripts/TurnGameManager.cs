using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TurnGameManager : MonoBehaviour
{
    private GridManager gridManager;
    private EnvironmentManager envManager;
    private EnemyBehavior enemyBehavior;
    private MirrorBehavior mirrorBehavior;
    private AugmentationManager augmentationManager;
    private TextManager textManager;
    private PlayerMetrics metricsLogger;
    private Transform DemoRecorderManager;
    public Dictionary<Transform, bool> unitTurns;
    public Dictionary<Transform, bool> enemyTurns;

    public bool isPlayerTurn = false;
    public bool isEnemyTurn = false;
    private bool startTurn = false;
    public int playerUnits;
    public int enemyUnits;
    public bool gameEnded;
    public int wins = 0; // keep track of the wins for UNIT PLAYERS
    public bool gameWon = false;
    public int totalRounds = 1;
    private int turn = 0;

  
    void Awake()
    {
        gridManager = this.transform.parent.GetComponent<GridManager>();
        envManager = this.transform.GetComponent<EnvironmentManager>();
        enemyBehavior = this.transform.GetComponent<EnemyBehavior>();
        augmentationManager = GameObject.Find("AugmentationManager").GetComponent<AugmentationManager>();
        textManager = GameObject.Find("TextManager").GetComponent<TextManager>();
        DemoRecorderManager = GameObject.Find("DemoRecorderManager").transform;
        metricsLogger = GameObject.Find("MetricsLogger").transform.GetComponent<PlayerMetrics>();

        unitTurns = new Dictionary<Transform, bool>();
        enemyTurns = new Dictionary<Transform, bool>();
        playerUnits = envManager.units.Length;
        enemyUnits = envManager.enemies.Length;
        gameEnded = false;
    }

    public void BeginNewRound()
    {

        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining && !envManager.augmentated)
        {
        
            textManager.ShowNewTurnCount(totalRounds);
        }
        playerUnits = envManager.units.Length;
        enemyUnits = envManager.enemies.Length;
        gameEnded = false;
        gameWon = false;
        turn = 0;
       
    }


    public void StartPlayerTurn()
    {
        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining)
        {
            textManager.ShowPlayerTurn();
            turn += 1;

        }

        foreach (var unit in unitTurns.Keys.ToList())
            {
                UnitInformation unitScript = unit.GetComponent<UnitInformation>();
                if (!unitScript.isDead)
                {
                    unitTurns[unit] = true;
                    unitScript.ResetSpriteColor();
                    unitScript.startUnitTurn();
                }

            }

        isPlayerTurn = true;
        startTurn = true;

    }

    public void StartEnemyTurn()
    {
        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining) textManager.ShowEnemyTurn();
        enemyBehavior.controlledUnits.Clear();
        
        foreach (var enemy in enemyTurns.Keys.ToList())
        {
            
            EnemyInformation enemyScript = enemy.GetComponent<EnemyInformation>();
            if (!enemyScript.isDead)
            {
                enemyTurns[enemy] = true;
                enemyScript.ResetSpriteColor();
                enemyBehavior.controlledUnits.Add(enemyScript.transform);

            }

        }


        isEnemyTurn = true;

    }


    public void disableTurn(Transform unit, bool isEnemy)
    {
        if (unit == null) return;

        if (isEnemy){
            enemyTurns[unit] = false;
        }
        else 
        {
            unitTurns[unit] = false;
        }

    }

    public void enableTurn(Transform unit, bool isEnemy)
    {
        if (unit == null) return;


        if (isEnemy){
            enemyTurns[unit] = true;
        }
        else 
        {
            unitTurns[unit] = true;
        }
    }
    public bool checkTurn(Transform unit, bool isEnemy)
    {
        if (unit == null) return false;

        if (isEnemy)
        {
            if (enemyTurns.TryGetValue(unit, out var turn)){
                return turn;
            }

            else {
                return false;
            }
        }
        else
        {
            if (unitTurns.TryGetValue(unit, out var turn)){
                return turn;
            }

            else {
                return false;
            }
        }
    }
    public void FallUnit(bool isEnemy)
    {
      
        if (isEnemy)
        {
            if (enemyUnits > 1)
            {
                enemyUnits -= 1;
            }
            else
            {
                wins += 1;
                gameWon = true;

                gameEnded = true;
                isPlayerTurn = false;
                isEnemyTurn = false;
                startTurn = false;
                enemyUnits -= 1;

                LogMetrics(4 - enemyUnits, 4 - playerUnits, 1);
           
            }
        }
        else
        {
            if (playerUnits > 1) playerUnits -= 1;
            else
            {

                gameEnded = true;
                isPlayerTurn = false;
                isEnemyTurn = false;
                startTurn = false;
                playerUnits -= 1;
                LogMetrics(4 - enemyUnits, 4 - playerUnits, 0);

            }
        }
    }


    void Update()
    {
        if (gameEnded) return;

        if (startTurn && isPlayerTurn) 
        {
            foreach (var unit in unitTurns.Keys.ToList()) 
            {
                if (unitTurns[unit] == true) return;

                
            }
            isPlayerTurn = false;

            // only switch turn for og environment if augmentation is enabled
            if (augmentationManager.enableAugmentation)
            {
                if (!envManager.augmentated) StartCoroutine(WaitBetweenTurns(0));
            }
            else
            {
                StartCoroutine(WaitBetweenTurns(2));
            }
          

        }

        else if (startTurn && isEnemyTurn)
        {
            foreach (var enemy in enemyTurns.Keys.ToList()){
    
                if (enemyTurns[enemy] == true) return;

            }
       
            isEnemyTurn = false;
            startTurn = false;
            
            // only switch turn for og environment if augmentation is enabled
            if (augmentationManager.enableAugmentation)
            {
                if (!envManager.augmentated) StartCoroutine(WaitBetweenTurns(1));
            }
            else
            {
                StartCoroutine(WaitBetweenTurns(3));
            }
        }
  
        
    }

     public IEnumerator WaitBetweenTurns(int phase)
    {
       
        yield return new WaitForSeconds(2f);
     
        
        switch (phase)
        {
            case 0:
            GameManager.Instance.SwitchState(GameState.enemyTurn);

            break;

            case 1:
            GameManager.Instance.SwitchState(GameState.playerTurn);
  
            break;

            case 2:
            StartEnemyTurn();
            break;

            case 3:
            StartPlayerTurn();
            break;


        }
        
    }

    void LogMetrics(int kills, int deaths, int win)
    {
        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining && !envManager.augmentated)
        {
            metricsLogger.LogEndRoundMetrics(totalRounds, kills, deaths, win);
        }
    }

    public void EndGame()
    {
        totalRounds += 1;

        if (augmentationManager.enableAugmentation)
        {
            envManager.SetEndGameReward(gameWon);
            GameManager.Instance.SwitchState(GameState.endGame);
        }
        else
        {
            
            envManager.SetEndGameReward(gameWon);
            envManager.RespawnUnits();
        }

    }
}
