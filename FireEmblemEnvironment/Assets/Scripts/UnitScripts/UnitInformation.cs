using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class UnitInformation : MonoBehaviour
{
    [SerializeField] public Vector3 initialPosition;
    [SerializeField] private Color initialColor;
    [SerializeField] private Color highlightedColor;
    [SerializeField] private Color greyedOutColor;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer weaponSprite;
    public Vector3 newPosition;
    private Vector3 prevPosition;
    public Tile currentTile;
    [SerializeField] public string unitType;
    [SerializeField] public string unitWeapon;
    private readonly string[] weaponTypes = {"sword", "lance", "axe", "bow", "magic"};
    private readonly string[] unitTypes = {"infantry", "cavalry", "flying", "heavyArmor"};

    public int attackRange;
    public int stepSize;
    public bool isEnemy = false;
    public int unitIndex;

    [Header("Unit Stats")]
    public int atk = 10;
    public int def = 10;
    public int spd = 10;
    public int res = 10;
    public int health = 30;    
    public int initialHP;
   
    private GridManager gridManager;
    private EnvironmentManager envManager;
    private TurnGameManager turnGameManager;
    private CombatManager combatManager;
    public StrategyUnitAgent trainingAgent;
    private AugmentationManager augmentationManager;
    private TextManager textManager;
    private Transform DemoRecorderManager;

    private List<Tile> moveTilesList = new List<Tile>();
    private List<Tile> attackTilesList = new List<Tile>();
    private List<Tile> attackableUnitTiles = new List<Tile>();
    private List<Tile> tilesInRangeAttack = new List<Tile>();

    public bool canClick = false;
    private bool clicked = false;
    public bool isTurn = false;
    public bool isSelected = false; // only true for the unit that is selected
    public bool isVulnerable = false;
    public bool isDead = false;

  

    void Awake()
    {
        gridManager = this.transform.parent.GetComponent<GridManager>();
        envManager = this.transform.parent.Find("EnvironmentManager").GetComponent<EnvironmentManager>();
        turnGameManager = this.transform.parent.Find("EnvironmentManager").GetComponent<TurnGameManager>();
        combatManager = this.transform.parent.Find("EnvironmentManager").GetComponent<CombatManager>();
        augmentationManager = GameObject.Find("AugmentationManager").GetComponent<AugmentationManager>();
        textManager = GameObject.Find("TextManager").GetComponent<TextManager>();
        DemoRecorderManager = GameObject.Find("DemoRecorderManager").transform;
      
        trainingAgent = this.GetComponent<StrategyUnitAgent>();
        unitIndex = envManager.GetUnitIndex(this.transform, isEnemy);
        Start();
    }


    void Start()
    {

        spriteRenderer = this.transform.GetChild(0).GetComponent<SpriteRenderer>();
        weaponSprite = this.transform.GetChild(1).GetComponent<SpriteRenderer>();
        spriteRenderer.color = initialColor;
        initialHP = health;

        SetTypeEffects();
  

    }

    private void SetTypeEffects()
    {
        if (unitType == null)
        {
            Debug.LogWarning("No Unit Type given!");
            unitType = "infantry";

        }

        if (unitType == "infantry")
        {
            stepSize = 2;
            spriteRenderer.sprite = textManager.GetCharacter(GetUnitType());
        }

        else if (unitType == "cavalry")
        {
            stepSize = 3;
            spriteRenderer.sprite = textManager.GetCharacter(GetUnitType());
        }
        else if (unitType == "flying")
        {
            stepSize = 2;
            spriteRenderer.sprite = textManager.GetCharacter(GetUnitType());
        }

        else if (unitType == "heavyArmor")
        {
            stepSize = 1;
            spriteRenderer.sprite = textManager.GetCharacter(GetUnitType());
        }


        else
        {
            unitType = "infantry";
            stepSize = 2;
            spriteRenderer.sprite = textManager.GetCharacter(GetUnitType());
        }

        if (unitWeapon == null)
        {
            unitWeapon = "sword";
        }

        if (unitWeapon == "sword")
        {
            attackRange = 1;
            weaponSprite.sprite = textManager.GetWeaponSprite(GetWeaponType());
        }
        else if (unitWeapon == "axe")
        {
            attackRange = 1;
            weaponSprite.sprite = textManager.GetWeaponSprite(GetWeaponType());
        }
        else if (unitWeapon == "lance")
        {
            attackRange = 1;
            weaponSprite.sprite = textManager.GetWeaponSprite(GetWeaponType());
        }
        else if (unitWeapon == "bow")
        {
            attackRange = 2;
            weaponSprite.sprite = textManager.GetWeaponSprite(GetWeaponType());
        }
        else if (unitWeapon == "magic")
        {
            attackRange = 2;
            weaponSprite.sprite = textManager.GetWeaponSprite(GetWeaponType());
        }
        else
        {
            attackRange = 1;
            weaponSprite.sprite = textManager.GetWeaponSprite(GetWeaponType());
        }
    }

    // when starting a new episode
    public List<Tile> RandomInitialPosition(List<Tile> gridHalfUnit)
    {

        List<Tile> leftTiles = gridHalfUnit;
        int ind = Random.Range(0, gridHalfUnit.Count);
        Tile startTile = gridHalfUnit[ind];
        initialPosition = gridManager.GetPositionFromTile(startTile);
        newPosition = initialPosition;
        leftTiles.Remove(startTile);

        int thisInd = envManager.GetUnitIndex(this.transform, isEnemy);
        augmentationManager.ForwardNewPosition(initialPosition, thisInd, isEnemy);

        if (DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining) RandomizeStats();
        else
        {
            if (!envManager.augmentated) RandomizeStats();
        }
   

        return leftTiles;
    }

    public void SetStats(string unittype, string weapontype, int hp, int attack, int defense, int resistance, int speed)
    {
        unitType = unittype;
        unitWeapon = weapontype;
        health = hp;
        atk = attack;
        def = defense;
        res = resistance;
        spd = speed;

        SetTypeEffects();
    }

    public void RandomizeStats()
    {
        health = Random.Range(35, 51);
        if (unitType == "heavyArmor")
        {
            def = Random.Range(35, 46);
            atk = Random.Range(40, 51);
            res = Random.Range(15, 25);
            spd = Random.Range(15, 25);
        }
        else if (unitWeapon == "magic")
        {
            def = Random.Range(15, 21);
            atk = Random.Range(35, 41);
            res = Random.Range(40, 50);
            spd = Random.Range(20, 36);
        }
        else if (unitWeapon == "bow")
        {
            def = Random.Range(20, 31);
            atk = Random.Range(30, 41);
            res = Random.Range(20, 31);
            spd = Random.Range(35, 46);
        }
        else
        {
            def = Random.Range(20, 31);
            atk = Random.Range(35, 41);
            res = Random.Range(20, 31);
            spd = Random.Range(20, 36);
        }
        initialHP = health;

        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining)
        {
            if (!envManager.augmentated)
            {
                textManager.SetPlayerStats(unitIndex, health, initialHP, atk, def, spd, res, GetUnitType(), GetWeaponType());
                augmentationManager.EqualizeStats(unitIndex, isEnemy);
            }
        }
        
            
    }

    public int GetWeaponType()
    {
        return System.Array.IndexOf(weaponTypes, unitWeapon);
    }

    public int GetUnitType()
    {
        return System.Array.IndexOf(unitTypes, unitType);
    }

    void Update()
    {
        // Check if it's your turn
        isTurn = turnGameManager.checkTurn(this.gameObject.transform, isEnemy);

    }

     public void updateInformation()
    {
        
        if (isDead) return;
        // set unit on new tile
        gridManager.SetUnit((int)newPosition.x, (int)newPosition.y, this.transform);
        envManager.setTilesInRange(this.transform, new Vector2((int)newPosition.x, (int)newPosition.y), stepSize, attackRange, isEnemy);
      
        moveTilesList = envManager.GetMovementTiles(this.transform);
        attackTilesList = envManager.GetAttackTiles(this.transform);
        attackableUnitTiles = envManager.GetAttackableUnitsTiles(this.transform);
        trainingAgent.movementTiles = moveTilesList;
        trainingAgent.attackTiles = attackTilesList;
        trainingAgent.attackableUnits = attackableUnitTiles;

        currentTile = gridManager.GetTileAtPosition((int)newPosition.x, (int)newPosition.y);
    }

    public void ResetInformation()
    {
        gridManager.RemoveUnit((int)this.transform.localPosition.x, (int)this.transform.localPosition.y);
        envManager.ResetTileInformation(this.transform);
        moveTilesList = envManager.GetMovementTiles(this.transform);
        attackTilesList = envManager.GetAttackTiles(this.transform);
        attackableUnitTiles = envManager.GetAttackableUnitsTiles(this.transform);
        tilesInRangeAttack.Clear();
        currentTile = null;
        newPosition = new Vector3(0f, 0f, 0f);
    }

    // get tiles in range on request
    // 0: tiles that are in movement range
    // 1: tiles that are in attack range
    // 2: tiles that have units that can be attacked
    // 3: tiles that can be moved to to launch an attack
    public List<Tile> GetTilesInRange(int request)
    {
        switch(request){
            case 0:
            return moveTilesList;

            case 1:

            return attackTilesList;
            
            case 2:
            return attackableUnitTiles;

            case 3:
            return tilesInRangeAttack;

            default:
            return moveTilesList;

        }
        
    }

    public void StartRound()
    {
        this.transform.localPosition = newPosition;

        turnGameManager.disableTurn(this.gameObject.transform, isEnemy);

    }

    public void MoveUnit(int x, int y){
        newPosition = new Vector3(x, y); 
        gridManager.ResetTileColors();
        prevPosition = this.transform.localPosition;
        this.transform.localPosition = newPosition;
        spriteRenderer.color = greyedOutColor;


        //reset moveable tiles
        foreach (Tile tile in moveTilesList){

            tile.moveable = false;

        }
       

        gridManager.RemoveUnit((int)prevPosition.x, (int)prevPosition.y);

    }

    public void performAction(Tile tile, int actionType)
    {
        Vector2 selectedTile = new Vector2();


        switch (actionType)
        {
            case 0: // wait
          
                selectedTile = gridManager.GetPositionFromTile(currentTile);
                MoveUnit((int)selectedTile.x, (int)selectedTile.y);
                break;
                
            case 1: // move
          
                selectedTile = gridManager.GetPositionFromTile(tile);
                MoveUnit((int)selectedTile.x, (int)selectedTile.y);
                break;

            case 2: // attack
               
                selectedTile = gridManager.GetPositionFromTile(tile);
                MoveUnit((int)selectedTile.x, (int)selectedTile.y);
                combatManager.StartCombat(isEnemy);
                break;

        } 
    }

    // push action through to the training agent
    public void PushAction(Tile tile, int actionType)
    {
        

        Vector2 selectedTile = gridManager.GetPositionFromTile(tile);
        int index = gridManager.GetIndexFromPosition((int)selectedTile.x, (int)selectedTile.y);// get index of tile action
        int targetInd = -1;

        if (actionType == 2) {
            EnemyInformation targetScript = combatManager.GetEnemyAsTarget();
            if (targetScript == null)
            {
                Debug.Log("NO TARGET SCRIPT");
            }
            targetInd = envManager.GetUnitIndex(combatManager.GetTarget(), targetScript.isEnemy);
      
        }
        

        if(trainingAgent) {
            trainingAgent.PushActionThrough(index, actionType, targetInd);

            int thisInd = envManager.GetUnitIndex(this.transform, isEnemy);

            // only the original environment should forward the actions to the other environments
            if (augmentationManager.enableAugmentation)
            {
                if (!envManager.augmentated) augmentationManager.ForwardAction(selectedTile, actionType, thisInd, targetInd, isEnemy);
            }
            
            
        }

     
    }

    public void startUnitTurn()
    {
    
        if (trainingAgent) trainingAgent.StartAgentTurn();
    }

    public void endUnitTurn()
    {
       
        
        gridManager.ResetTileColors();
        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining) textManager.ShowPlayerStats(false, unitIndex);
         // reset attackable tiles
        foreach (Tile tile in attackableUnitTiles)
        {
            tile.attackable = false;
            tile.launchAttack = false;

            Transform containEnemy = tile.checkUnit(gridManager.GetPositionFromTile(tile));
            if (!containEnemy) continue;

            EnemyInformation enemyInfo = containEnemy.GetComponent<EnemyInformation>();

            if (enemyInfo) enemyInfo.isVulnerable = false;

        }

        turnGameManager.disableTurn(this.gameObject.transform, isEnemy);
        attackableUnitTiles.Clear();


        foreach (Tile tile in tilesInRangeAttack)
        {
            
            tile.launchAttack = false;
        }
        
        tilesInRangeAttack.Clear();
        envManager.InitializeInformation(0);
        envManager.InitializeInformation(1);
       
        StartCoroutine(WaitBetweenTurns(1f));   

    }

    // Selects a unit
    void OnMouseDown()
    {
        if (isDead || !isTurn || !canClick) return;

        if (tilesInRangeAttack.Contains(currentTile)){
            if (currentTile.launchAttack){
                
                currentTile.OnMouseDownBlockedTile();
                return;
            }
        }

        canClick = false;
        clicked = !clicked;
        isSelected = ! isSelected;

        envManager.unitSelected = !envManager.unitSelected;
        envManager.setSelectedUnit(this.transform);

        foreach (Tile tile in moveTilesList)
        {

            tile.moveable = !tile.moveable;

        }

        foreach(Tile tile in attackableUnitTiles)
        {
            Transform containEnemy = tile.checkUnit(gridManager.GetPositionFromTile(tile));
            if (!containEnemy) continue;

            EnemyInformation enemyInfo = containEnemy.GetComponent<EnemyInformation>();
            tile.attackable = !tile.attackable;
        
            enemyInfo.isVulnerable = !enemyInfo.isVulnerable;
      
        }

        if (combatManager.instantiateAttack)
        {
            CancelAttack();
        }
    }

    void OnMouseEnter()
    {

        // do something with disabling mouse enter when action is still processed
        if (isDead || envManager.unitSelected && !isSelected || !isTurn)
        {
            return;
        }

        if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining) textManager.ShowPlayerStats(true, unitIndex);

    
        if (!clicked)
        {

            spriteRenderer.color = highlightedColor;


            foreach (Tile tile in moveTilesList)
            {
                tile.HighlightMoveRange();

            }

            foreach (Tile tile in attackTilesList)
            {
                tile.HighlightAttackRange();
            }

            foreach (Tile tile in attackableUnitTiles)
            {
                tile.HighlightEnemies();
            }

            canClick = true;

        }

        

        if (clicked && isSelected)
        {
            canClick = true; // you can deselect the unit
        }
    
    }

    void OnMouseExit(){
        if (isDead || envManager.unitSelected && !isSelected || !isTurn){
            return;
        }

        if (!clicked)
        {
            gridManager.ResetTileColors();
            spriteRenderer.color = initialColor;
            canClick = false;
            if (!DemoRecorderManager.GetComponent<DemoRecorderDummyAgent>().startTraining) textManager.ShowPlayerStats(false, unitIndex);
        }

        if (clicked && isSelected)
        {
            canClick = false; // you can deselect the unit
        }
    }

    public void WantToAttack(EnemyInformation enemy)
    {
        Tile enemyTile = enemy.currentTile;
        tilesInRangeAttack = gridManager.GetAttackTilesInRange(enemyTile, attackRange, moveTilesList);
        foreach (Tile tile in tilesInRangeAttack)
        {
            tile.HighlightSelected();
            tile.launchAttack = true;
        }
        combatManager.SetAttacker(this.transform);
        combatManager.SetTarget(enemy.transform, isEnemy);

    }

  
    public void CancelAttack()
    {
        foreach (Tile tile in tilesInRangeAttack)
        {
            
            tile.launchAttack = false;
        }
        combatManager.ResetCombat();

        tilesInRangeAttack.Clear();
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        textManager.AdjustStatValuePlayer(unitIndex, 0, health);
        if (health <= 0)
        {
            Die();
        }
    }
    public void ResetSpriteColor()
    {
        spriteRenderer.color = initialColor;
    }

    private void Die()
    {
        GetComponent<BoxCollider2D>().enabled = false;
        isDead = true;
        gridManager.RemoveUnit((int)this.transform.localPosition.x, (int)this.transform.localPosition.y);
        turnGameManager.FallUnit(isEnemy);
        currentTile = null;
        trainingAgent.AddReward(-0.1f);

        spriteRenderer.enabled = false;
        weaponSprite.enabled = false;


    }

    public void Respawn()
    {

        GetComponent<BoxCollider2D>().enabled = true;
        isDead = false;
        health = initialHP;

        spriteRenderer.enabled = true;
        weaponSprite.enabled = true;
     
    }

    private void FadeOut()
    {
        Color color = spriteRenderer.color;
        float alpha = color.a;
        float fadeRate = 0.1f;

        while (spriteRenderer.color.a > 0.1f){
          
            alpha -= fadeRate * Time.deltaTime;
            alpha = Mathf.Clamp01(alpha);

            color.a = alpha;
            spriteRenderer.color = color;
           
        }
        color.a = 0f;
        spriteRenderer.color = color;
        
        
    }

    IEnumerator WaitBetweenTurns(float t)
    {

        yield return new WaitForSeconds(t);

        isSelected = false;
        clicked = false;
        canClick = false;
        envManager.setSelectedUnit(null);
        envManager.unitSelected = false;

        if (turnGameManager.gameEnded)
        {

            if (augmentationManager.enableAugmentation)
            {

                if (!envManager.augmentated) turnGameManager.EndGame();
            }
            // end the game per individual environment if training is happening
            else
            {
                if (turnGameManager.gameWon)
                {
                    trainingAgent.AddReward(1f);
                }
                else
                {
                    trainingAgent.AddReward(-1f);
                }

                turnGameManager.EndGame();
            }
        }
        
 
        
    }

   


}
