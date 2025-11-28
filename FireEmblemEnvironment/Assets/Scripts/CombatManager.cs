using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    //public static GameManager Instance;
    public GameState GameState;
    public Transform target;
    public Transform attacker;

    [SerializeField] private GridManager gridManager;
    [SerializeField] private EnvironmentManager envManager;
    [SerializeField] private TurnGameManager turnManager;
    private PlayerMetrics metricsLogger;
    private Transform DemoRecorderManager;
    private TextManager textManager;
    public bool instantiateAttack = false;

    private int damageByAttacker = 0;
    private int damageByTarget = 0;
    private bool canCounter = false;
    private bool canFollowUp = false;
    private bool effective = false;
    private bool adv = false;
    private bool disadv= false;

    
    void Start()
    {
        gridManager = this.transform.parent.GetComponent<GridManager>();
        envManager = this.transform.GetComponent<EnvironmentManager>();
        textManager = GameObject.Find("TextManager").GetComponent<TextManager>();
        DemoRecorderManager = GameObject.Find("DemoRecorderManager").transform;
        metricsLogger = GameObject.Find("MetricsLogger").transform.GetComponent<PlayerMetrics>();
    }

    // by Enemy is true if the target is set BY an enemy, and false if enemy IS the target and set by the player unit
    public void SetTarget(Transform unit, bool byEnemy)
    {
        if (target == null) target = unit;

        PrepareBattle(byEnemy);
    }

    void PrepareBattle(bool EnemyTurn)
    {


        if (!EnemyTurn)
        {
            UnitInformation unitScript = attacker.GetComponent<UnitInformation>();
            EnemyInformation enemyScript = target.GetComponent<EnemyInformation>();

            damageByAttacker = CalculateDamage(unitScript, enemyScript, false); // calculate damage that player deals
            damageByTarget = CalculateDamage(unitScript, enemyScript, true);
            canCounter = CheckCounterAttack(unitScript, enemyScript);
            canFollowUp = CheckFollowUpAttack(unitScript, enemyScript, false);

            if (canFollowUp)
            {
                damageByAttacker *= 2;
            }
        }

        else
        {
            UnitInformation unitScript = target.GetComponent<UnitInformation>();
            EnemyInformation enemyScript = attacker.GetComponent<EnemyInformation>();

            damageByAttacker = CalculateDamage(unitScript, enemyScript, true); // calculate damage that player deals
            damageByTarget = CalculateDamage(unitScript, enemyScript, false);
            canCounter = CheckCounterAttack(unitScript, enemyScript);
            canFollowUp = CheckFollowUpAttack(unitScript, enemyScript, true);

            if (canFollowUp)
            {
                damageByAttacker *= 2;
            }
        }


    }

    public Transform GetTarget()
    {
        return target;
    }
    public EnemyInformation GetEnemyAsTarget()
    {

        return target.GetComponent<EnemyInformation>();
    }
    public UnitInformation GetUnitAsTarget()
    {
        return target.GetComponent<UnitInformation>();
    }
    public void SetAttacker(Transform unit)
    {
        attacker = unit;
    }
    public Transform GetAttacker()
    {
        return attacker;
    }
    public int GetDamageFrom(bool attacker)
    {
        if (attacker)
        {
            return damageByAttacker;
        }
        else
        {
            return damageByTarget;
        }
    }
    public bool GetCounterInf()
    {
        return canCounter;
    }
    public bool GetFollowUpInf()
    {
        return canFollowUp;
    }
    public bool TargetAlreadySet()
    {
        return target != null;
    }
    public bool AttackerAlreadyChosen()
    {
        return attacker != null;
    }


    public void StartCombat(bool attackerIsEnemy)
    {
        
        if (attackerIsEnemy)
        {
           
            EnemyInformation attackerInf = attacker.GetComponent<EnemyInformation>();
            UnitInformation targetInf = target.GetComponent<UnitInformation>();

            if (canFollowUp) damageByAttacker /= 2;

            //CALCULATE DAMAGE WITH FORMULA
            int damage = damageByAttacker;



            targetInf.TakeDamage(damage);

            // check if target can counter attack
            if (!targetInf.isDead && canCounter)
            {
                damage = damageByTarget;
                attackerInf.TakeDamage(damage);

                

            }
            // check if unit can followup on its attack
            if (!targetInf.isDead && !attackerInf.isDead && canFollowUp)
            {
               
                damage = damageByAttacker;
                targetInf.TakeDamage(damage);
            }


        }
        else if (!attackerIsEnemy)
        {
           
            UnitInformation attackerInf = attacker.GetComponent<UnitInformation>();
            EnemyInformation targetInf = target.GetComponent<EnemyInformation>();


            //CALCULATE DAMAGE WITH FORMULA
            if (canFollowUp) damageByAttacker /= 2;

            int damage = damageByAttacker;



            targetInf.TakeDamage(damage);

            // check if target can counter attack
            if (!targetInf.isDead && canCounter)
            {
               
                damage = damageByTarget;
                attackerInf.TakeDamage(damage);

            }

            if (!targetInf.isDead && !attackerInf.isDead && canFollowUp)
            {
                
                damage = damageByAttacker;
                targetInf.TakeDamage(damage);
            }

        }

        LogMetrics();
        ResetCombat();
    }


    public bool CheckFollowUpAttack(UnitInformation player, EnemyInformation enemy, bool attackerIsEnemy)
    {
        // when there is at least a 5p difference between player and foe
        // the one with the speed advantage attacks twice
        int p_speed = player.spd;
        int e_speed = enemy.spd;

        if (attackerIsEnemy)
        {
            if (e_speed - p_speed >= 5)
            {
                // it means enemy attacks twice
                return true;
            }

        }
        else
        {
            if (p_speed - e_speed >= 5)
            {
                //means player attacks twice
                return true;
            }
        }
        return false;
    }

    public bool CheckCounterAttack(UnitInformation player, EnemyInformation enemy)
    {
        // when same attack range, targeted unit can counter attack
        int p_range = player.attackRange;
        int e_range = enemy.attackRange;

        if (p_range == e_range)
        {
            // counter attack
            return true;
        }

        return false;
    }

    public int CalculateDamage(UnitInformation player, EnemyInformation enemy, bool attackerIsEnemy)
    {
        int totalDamge = 0;
        if (attackerIsEnemy)
        {
            if (player == null)
            {
                Debug.Log("No player in env: " + envManager.transform.parent.gameObject.name);
            }
            if (enemy == null)
            {
                Debug.Log("No enemy in env: " + envManager.transform.parent.gameObject.name);
            }

            int ans = enemy.atk;
            int def = 0;



            float f_ans = ans * CheckAdvantage(player.unitWeapon, enemy.unitWeapon, attackerIsEnemy);
            ans = (int)(f_ans * CheckEffectiveness(player, enemy, attackerIsEnemy));

            // Use res as defense if magic usage
            if (enemy.unitWeapon == "magic")
            {
                def = player.res;
            }
            else
            {
                def = player.def;
            }

            totalDamge = ans - def;

        }

        else
        {
            int ans = player.atk;
            int def = 0;

            float f_ans = ans * CheckAdvantage(player.unitWeapon, enemy.unitWeapon, attackerIsEnemy);
            ans = (int)(f_ans * CheckEffectiveness(player, enemy, attackerIsEnemy));

            if (player.unitWeapon == "magic")
            {
                def = enemy.res;
            }
            else
            {
                def = enemy.def;
            }

            totalDamge = ans - def;

        }

        if (totalDamge > 0) return totalDamge;
        else return 0;

    }

    public float CheckAdvantage(string playerWeapon, string enemyWeapon, bool attackerIsEnemy)
    {
        float advantage = 1f;
        if (attackerIsEnemy)
        {
            // sword -> axe
            // axe -> lance
            // lance -> sword
            if ((enemyWeapon == "sword" && playerWeapon == "axe") || (enemyWeapon == "axe" && playerWeapon == "lance")
                || (enemyWeapon == "lance" && playerWeapon == "sword"))
            {

                advantage = 1.2f;
                // for metric logger, log combat information during mirror mode to see agent performance
                if (instantiateAttack && GameManager.Instance.SeeGameMode()) adv = true;
            }
            else if ((enemyWeapon == "axe" && playerWeapon == "sword") || (enemyWeapon == "lance" && playerWeapon == "axe")
                || (enemyWeapon == "sword" && playerWeapon == "lance"))
            {

                advantage = 0.8f;
                if (instantiateAttack && GameManager.Instance.SeeGameMode()) disadv = true;
            }
        }

        else
        {
            if ((playerWeapon == "sword" && enemyWeapon == "axe") || (playerWeapon == "axe" && enemyWeapon == "lance")
                || (playerWeapon == "lance" && enemyWeapon == "sword"))
            {

                advantage = 1.2f;
                if (instantiateAttack && !GameManager.Instance.SeeGameMode()) adv = true;
            }
            else if ((playerWeapon == "axe" && enemyWeapon == "sword") || (playerWeapon == "lance" && enemyWeapon == "axe")
                || (playerWeapon == "sword" && enemyWeapon == "lance"))
            {

                advantage = 0.8f;
                if (instantiateAttack && !GameManager.Instance.SeeGameMode()) disadv = true;
            }
        }


        return advantage;

    }

    public float CheckEffectiveness(UnitInformation player, EnemyInformation enemy, bool attackerIsEnemy)
    {
        float effectiveness = 1f;

        if (attackerIsEnemy)
        {
            if (enemy.unitWeapon == "bow" && player.unitType == "flying")
            {
                effectiveness = 1.5f;
                if (instantiateAttack && GameManager.Instance.SeeGameMode()) effective = true;
            }
        }
        else
        {
            if (player.unitWeapon == "bow" && enemy.unitType == "flying")
            {
                effectiveness = 1.5f;
                if (instantiateAttack && !GameManager.Instance.SeeGameMode()) effective = true;
            }
        }

        return effectiveness;
    }

    public void ResetCombat()
    {
        

        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining) textManager.ShowCombatUI(false);
        if (target)
        {
            if (target.gameObject.tag == "Enemy") target.GetComponent<EnemyInformation>().updateInformation();
            else if (target.gameObject.tag == "Player") target.GetComponent<UnitInformation>().updateInformation();

        }
        target = null;
        attacker = null;
        instantiateAttack = false;
        effective = false;
        adv = false;
        disadv = false;

    }

    IEnumerator WaitEndingTurn(float t)
    {

        yield return new WaitForSeconds(t);

        if (attacker.gameObject.tag == "Enemy")
        {
            EnemyInformation attackerInf = attacker.GetComponent<EnemyInformation>();
            attackerInf.endUnitTurn();

        }

    }
    
    void LogMetrics()
    {
        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining && !envManager.augmentated)
        {
            metricsLogger.LogCombatInfo(effective, adv, disadv);
        }
    }
}