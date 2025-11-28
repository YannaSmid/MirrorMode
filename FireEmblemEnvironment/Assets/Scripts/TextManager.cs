using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextManager : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager; // Combat of the original environment!!!
    [SerializeField] private TurnGameManager turnGameManager;
    [SerializeField] private GameObject statsPlayerCanvas;
    [SerializeField] private GameObject statsEnemyCanvas;
    [SerializeField] private GameObject combatCanvas;

    // Important: assign characters, mathcing portraits, and types all in same order so their index match!
    // example: if type order: infantry, cavalry, flying, heavy armor -- then characters: lucina, veronica, claude, edelgard
    // same goes for weapons and image weapons!
    [SerializeField] private List<Sprite> imageTypes;
    [SerializeField] private List<Sprite> imageWeapons;
    [SerializeField] private List<Sprite> characters;
    [SerializeField] private List<Sprite> portraitImages;
    [SerializeField] private List<Sprite> weapons;


    [Header("Text boxes stats Player")]
    [SerializeField] private TMP_Text InitialHP_P;
    [SerializeField] private TMP_Text currentHP_P;
    [SerializeField] private TMP_Text Attack_P;
    [SerializeField] private TMP_Text Defense_P;
    [SerializeField] private TMP_Text Speed_P;
    [SerializeField] private TMP_Text Resistance_P;
    [SerializeField] private Image Type_P;
    [SerializeField] private Image Weapon_P;
    [SerializeField] private Image Portrait_P;

    [Header("Text boxes stats Enemy")]
    [SerializeField] private TMP_Text InitialHP_E;
    [SerializeField] private TMP_Text currentHP_E;
    [SerializeField] private TMP_Text Attack_E;
    [SerializeField] private TMP_Text Defense_E;
    [SerializeField] private TMP_Text Speed_E;
    [SerializeField] private TMP_Text Resistance_E;
    [SerializeField] private Image Type_E;
    [SerializeField] private Image Weapon_E;
    [SerializeField] private Image Portrait_E;

    [Header("Text boxes combat")]
    [SerializeField] private TMP_Text HealthBefore_P;
    [SerializeField] private TMP_Text HealthBefore_E;
    [SerializeField] private TMP_Text HealthAfter_P;
    [SerializeField] private TMP_Text HealthAfter_E;
    [SerializeField] private TMP_Text DamageBy_P;
    [SerializeField] private TMP_Text DamageBy_E;
    [SerializeField] private Image CombatPortrait_P;
    [SerializeField] private Image CombatPortrait_E;

    [Header("UI that is always on display")]
    [SerializeField] private GameObject turn_P;
    [SerializeField] private GameObject turn_E;
    [SerializeField] private TMP_Text turn;

    List<int[]> statsPlayer = new List<int[]>
    {
        new int[8],
        new int[8],
        new int[8],
        new int[8]
    };

    List<int[]> statsEnemy = new List<int[]>
    {
        new int[8],
        new int[8],
        new int[8],
        new int[8]
    };

    public void SetPlayerStats(int unit, int currenthp, int hp, int atk, int def, int spd, int res, int type, int weapon)
    {
        statsPlayer[unit][0] = currenthp;
        statsPlayer[unit][1] = hp;
        statsPlayer[unit][2] = atk;
        statsPlayer[unit][3] = def;
        statsPlayer[unit][4] = spd;
        statsPlayer[unit][5] = res;
        statsPlayer[unit][6] = type;
        statsPlayer[unit][7] = weapon;



    }

    public void SetEnemyStats(int unit, int currenthp, int hp, int atk, int def, int spd, int res, int type, int weapon)
    {
        statsEnemy[unit][0] = currenthp;
        statsEnemy[unit][1] = hp;
        statsEnemy[unit][2] = atk;
        statsEnemy[unit][3] = def;
        statsEnemy[unit][4] = spd;
        statsEnemy[unit][5] = res;
        statsEnemy[unit][6] = type;
        statsEnemy[unit][7] = weapon;
    }

    public void AdjustStatValuePlayer(int unit, int stat, int newvalue)
    {
        statsPlayer[unit][stat] = newvalue;
    }

    public void AdjustStatValueEnemy(int unit, int stat, int newvalue)
    {
        statsEnemy[unit][stat] = newvalue;
    }
    public Sprite GetCharacter(int ind)
    {
        return characters[ind];
    }

    public Sprite GetWeaponSprite(int ind)
    {
        return weapons[ind];
    }

    public void ShowPlayerTurn()
    {
        turn_E.SetActive(false);
        turn_P.SetActive(true);
    }
    public void ShowEnemyTurn()
    {
        turn_P.SetActive(false);
        turn_E.SetActive(true);
    }
    public void ShowNewTurnCount(int t)
    {
        
        turn.text = string.Format("Round {0}", t);
    }
    public void ShowPlayerStats(bool activate, int unitInd)
    {
        currentHP_P.text = statsPlayer[unitInd][0].ToString();
        InitialHP_P.text = statsPlayer[unitInd][1].ToString();
        Attack_P.text = statsPlayer[unitInd][2].ToString();
        Defense_P.text = statsPlayer[unitInd][3].ToString();
        Speed_P.text = statsPlayer[unitInd][4].ToString();
        Resistance_P.text = statsPlayer[unitInd][5].ToString();

        Type_P.sprite = imageTypes[statsPlayer[unitInd][6]];
        Weapon_P.sprite = imageWeapons[statsPlayer[unitInd][7]];
        Portrait_P.sprite = portraitImages[statsPlayer[unitInd][6]];
        statsPlayerCanvas.SetActive(activate);


    }

    public void ShowEnemyStats(bool activate, int unitInd)
    {
        currentHP_E.text = statsEnemy[unitInd][0].ToString();
        InitialHP_E.text = statsEnemy[unitInd][1].ToString();
        Attack_E.text = statsEnemy[unitInd][2].ToString();
        Defense_E.text = statsEnemy[unitInd][3].ToString();
        Speed_E.text = statsEnemy[unitInd][4].ToString();
        Resistance_E.text = statsEnemy[unitInd][5].ToString();

        Type_E.sprite = imageTypes[statsEnemy[unitInd][6]];
        Weapon_E.sprite = imageWeapons[statsEnemy[unitInd][7]];
        Portrait_E.sprite = portraitImages[statsEnemy[unitInd][6]];

        statsEnemyCanvas.SetActive(activate);
    }

    public void ShowCombatUI(bool activate)
    {
        if (activate)
        {
            statsPlayerCanvas.SetActive(false);
            statsEnemyCanvas.SetActive(false);
            int playerInd = 0;
            int enemyInd = 0;
            int damageByPlayer = 0; // calculate damage that player deals
            int damageByEnemy = 0;
            bool counter = false;
            bool followup = false;

            if (turnGameManager.isPlayerTurn)
            {
                damageByPlayer = combatManager.GetDamageFrom(true); // calculate damage that player deals
                damageByEnemy = combatManager.GetDamageFrom(false);
                counter = combatManager.GetCounterInf();
                followup = combatManager.GetFollowUpInf();

                UnitInformation unitScript = combatManager.GetAttacker().GetComponent<UnitInformation>();
                EnemyInformation enemyScript = combatManager.GetTarget().GetComponent<EnemyInformation>();
                enemyInd = enemyScript.unitIndex;
                playerInd = unitScript.unitIndex;

                if (followup)
                {
                    DamageBy_P.text = string.Format("{0} X 2", damageByPlayer / 2);
                }
                else
                {
                    
                    DamageBy_P.text = string.Format("{0}", damageByPlayer);
                }

                if (counter)
                {
                   
                    DamageBy_E.text = string.Format("{0}", damageByEnemy);
                }
                else
                {
                  
                    DamageBy_E.text = string.Format("-");
                    damageByEnemy = 0;

                }

            }
            else
            {
                damageByPlayer = combatManager.GetDamageFrom(false);
                damageByEnemy = combatManager.GetDamageFrom(true); //damage from attacker
                counter = combatManager.GetCounterInf();
                followup = combatManager.GetFollowUpInf();

                UnitInformation unitScript = combatManager.GetTarget().GetComponent<UnitInformation>();
                EnemyInformation enemyScript = combatManager.GetAttacker().GetComponent<EnemyInformation>();
                enemyInd = enemyScript.unitIndex;
                playerInd = unitScript.unitIndex;

                if (followup)
                {
                    DamageBy_E.text = string.Format("{0} X 2", damageByEnemy / 2);
                }
                else
                {
                   
                    DamageBy_E.text = string.Format("{0}", damageByEnemy);
                }

                if (counter)
                {
                    
                    DamageBy_P.text = string.Format("{0}", damageByPlayer);
                }
                else
                {
                   
                    DamageBy_P.text = string.Format("-");
                    damageByPlayer = 0;

                }


            }

            int afterHP_P = Math.Max(statsPlayer[playerInd][0] - damageByEnemy, 0);
            int afterHP_E = Math.Max(statsEnemy[enemyInd][0] - damageByPlayer, 0);
            HealthAfter_P.text = string.Format("{0}", afterHP_P);
            HealthBefore_P.text = string.Format("{0}", statsPlayer[playerInd][0]);

            HealthAfter_E.text = string.Format("{0}", afterHP_E);
            HealthBefore_E.text = string.Format("{0}", statsEnemy[enemyInd][0]);
            CombatPortrait_P.sprite = portraitImages[statsPlayer[playerInd][6]];
            CombatPortrait_E.sprite = portraitImages[statsEnemy[enemyInd][6]];


            combatCanvas.SetActive(activate);
        }
        else
        {
            combatCanvas.SetActive(activate);
        }
    }


}
