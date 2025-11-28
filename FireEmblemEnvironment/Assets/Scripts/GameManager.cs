using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState GameState;
    public bool MirrorMode = false;

    [SerializeField] private GridManager gridManager;
    [SerializeField] private EnvironmentManager ogEnvManager;
    [SerializeField] private TurnGameManager turnManager;
    [SerializeField] private AugmentationManager augmentationManager;

    void Awake(){
        Instance = this;
    }
  
    void Start()
    {
        
        gridManager = GameObject.Find("FEEnv").GetComponent<GridManager>();
        ogEnvManager = GameObject.Find("EnvironmentManager").GetComponent<EnvironmentManager>();
        SwitchState(GameState.generateGrid);
       
    }

    public bool SeeGameMode()
    {
        return MirrorMode;
    }

    public void SwitchState(GameState newState)
    {
        GameState = newState;

        switch (newState)
        {

            case GameState.generateGrid:
                Debug.Log("Generategrid!");
                augmentationManager.PrepareGrids();
                break;

            case GameState.randomizePosition:
                Debug.Log("Random initial position!");
                ogEnvManager.StartNewEpisode();

                break;

            case GameState.prepareAugmentation:
                Debug.Log("Prepare Augmentated Data!");
                augmentationManager.ApplyMirroring();
                break;

            case GameState.setUnits:
                Debug.Log("Set Units!");
                augmentationManager.PrepareUnits();
                break;

            // Call each new turn!
            case GameState.initializeInformation:
                Debug.Log("Initialize Information!");
                augmentationManager.PrepareInitialization();
                break;

            case GameState.playerTurn:
                Debug.Log("Player Turn!");
                augmentationManager.StartTurns();
                break;

            case GameState.enemyTurn:
                Debug.Log("Enemy turn");
                augmentationManager.StartEnemyTurns();
                break;

            case GameState.endGame:
                Debug.Log("End current game");
                augmentationManager.PrepareNewEpisode();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);

        }
    }



}

public enum GameState
    {
        generateGrid = 0,
        randomizePosition =1,
        prepareAugmentation = 2,
        setUnits = 3,
        initializeInformation = 4,
        playerTurn = 5,
        enemyTurn = 6,
        endGame = 7
    }