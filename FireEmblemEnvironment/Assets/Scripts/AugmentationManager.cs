using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentationManager : MonoBehaviour
{
    [SerializeField] private EnvironmentManager thisEnvManager;
    [SerializeField] private Transform[] environments; // all environments for training
    Transform[] thisUnits;
    Transform[] thisEnemies;
    [SerializeField] public EnvironmentManager[] envManagers; // the four augmented environments
    private CombatManager combatManager;
    private TurnGameManager turnGameManager;
    public bool enableAugmentation = false;
    

    public void PrepareGrids()
    {
        foreach (Transform env in environments)
        {
            GridManager gridManager = env.GetComponent<GridManager>();
            gridManager.GenerateGrid();

        }

        //GameManager.Instance.SwitchState(GameState.prepareAugmentation);
        if (enableAugmentation) GameManager.Instance.SwitchState(GameState.randomizePosition);
        else 
        {
            // start the episode for each environment indivudually
            foreach (Transform environment in environments)
            {
                EnvironmentManager env = environment.Find("EnvironmentManager").transform.GetComponent<EnvironmentManager>();
                env.StartNewEpisode();
            }
        }
    }

    public void PrepareNewEpisode()
    {
        
        for (int i = 0; i < envManagers.Length; i++)
        {
            EnvironmentManager env = envManagers[i];
            env.RespawnUnits();
        }

        GameManager.Instance.SwitchState(GameState.randomizePosition);
    }

    public void BeginNewTurns()
    {
        foreach (EnvironmentManager env in envManagers)
        {
            TurnGameManager turnManager = env.transform.GetComponent<TurnGameManager>();
            turnManager.BeginNewRound();
        }
    }

    public void EqualizeStats(int unitID, bool isEnemy)
    {
        if (isEnemy)
        {
            EnemyInformation ogEnemy = envManagers[0].enemies[unitID].GetComponent<EnemyInformation>();

            for (int i = 1; i < envManagers.Length; i++)
            {
                EnvironmentManager env = envManagers[i];

                EnemyInformation enemyScript = env.enemies[unitID].GetComponent<EnemyInformation>();
                enemyScript.SetStats(ogEnemy.unitType, ogEnemy.unitWeapon, ogEnemy.health, ogEnemy.atk, ogEnemy.def, ogEnemy.res, ogEnemy.spd);

            }
        }
        else
        {
            UnitInformation ogUnit = envManagers[0].units[unitID].GetComponent<UnitInformation>();
            for (int i = 1; i < envManagers.Length; i++)
            {
                EnvironmentManager env = envManagers[i];

                UnitInformation unitScript = env.units[unitID].GetComponent<UnitInformation>();
                unitScript.SetStats(ogUnit.unitType, ogUnit.unitWeapon, ogUnit.health, ogUnit.atk, ogUnit.def, ogUnit.res, ogUnit.spd);
            }
        }
        
    }

    public void ForwardNewPosition(Vector2 tile, int performerIndex, bool isEnemy)
    {
        // only forward position to other copied environments if augmentation is enabled
        if (!enableAugmentation) return;

        for (int i = 1; i < envManagers.Length; i++)
        {
            EnvironmentManager env = envManagers[i];
            GridManager gridManager = env.transform.parent.GetComponent<GridManager>();
            turnGameManager = env.transform.GetComponent<TurnGameManager>();
            Transform performer = null;


            if (isEnemy)
            {
                performer = env.enemies[performerIndex];
                EnemyInformation enemyScript = performer.GetComponent<EnemyInformation>();
                enemyScript.newPosition = tile;
            }
            else
            {
                performer = env.units[performerIndex];
                UnitInformation unitScript = performer.GetComponent<UnitInformation>();
                unitScript.newPosition = tile;
            }

        }
    }

    public void ApplyMirroring()
    {
   
        // if (!isMirrored) return;

        for (int i = 0; i < envManagers.Length; i++)
        {
            EnvironmentManager env = envManagers[i];
            env.TransformMap(i);
        }

        
        GameManager.Instance.SwitchState(GameState.setUnits);
    }

    public void PrepareUnits()
    {
  
        foreach (EnvironmentManager env in envManagers)
        {
            env.setUnits();
        }

        GameManager.Instance.SwitchState(GameState.initializeInformation);
    }

    public void PrepareInitialization()
    {
      

        foreach (EnvironmentManager env in envManagers)
        {
            env.InitializeInformation(2);
        }

        GameManager.Instance.SwitchState(GameState.playerTurn);
    }

    public void StartTurns()
    {
        foreach (EnvironmentManager env in envManagers)
        {
            TurnGameManager turnGameManager = env.transform.GetComponent<TurnGameManager>();
            turnGameManager.StartPlayerTurn();
            
        }
        
    }
    public void StartEnemyTurns()
    {
        foreach (EnvironmentManager env in envManagers)
        {
            TurnGameManager turnGameManager = env.transform.GetComponent<TurnGameManager>();
            turnGameManager.StartEnemyTurn();
        }
        
    }

    public void ForwardAction(Vector2 tile, int actionType, int performerIndex, int targetInd, bool isEnemy)
    {

        // only forward action to other environments if augmentation is enabled
        if (!enableAugmentation) return;

        for (int i = 1; i < envManagers.Length; i++)
        {
            EnvironmentManager env = envManagers[i];
            GridManager gridManager = env.transform.parent.GetComponent<GridManager>();
            turnGameManager = env.transform.GetComponent<TurnGameManager>();
            Transform performer = null;

            bool[] mirroredOrientation = env.MirroredOrientation(i);

            if (isEnemy)
            {
                performer = env.enemies[performerIndex];
            }
            else
            {
                performer = env.units[performerIndex];
            }

        

            Vector2 mirroredPos = gridManager.MirrorPosition(tile, mirroredOrientation[0], mirroredOrientation[1]);

            Tile mirroredTile = gridManager.GetTileAtPosition(mirroredPos);

            if (actionType == 2)
            {
                // if attacking set the combat variables   

                combatManager = env.transform.GetComponent<CombatManager>();
                combatManager.instantiateAttack = true;

                if (targetInd >= 0)
                {

                    if (isEnemy)
                    {
                        combatManager.SetAttacker(performer);
                        combatManager.SetTarget(env.units[targetInd], true);

                    }
                    else
                    {
                        combatManager.SetAttacker(performer);
                        combatManager.SetTarget(env.enemies[targetInd], false);

                    }

                }
                else continue;



                // finally perform the mirrored action
                if (isEnemy)
                {
                    
                    performer.GetComponent<EnemyInformation>().PushAction(mirroredTile, actionType);

                }
                else
                {
                    performer.GetComponent<UnitInformation>().PushAction(mirroredTile, actionType);
                  

                }


            }
            else if (actionType == 1)
            {
                // just move action 

                if (isEnemy)
                {
                    performer.GetComponent<EnemyInformation>().performAction(mirroredTile, actionType);
                }
                else
                {
                    performer.GetComponent<UnitInformation>().PushAction(mirroredTile, actionType);
                    
                }

            }


        }
    }

}
