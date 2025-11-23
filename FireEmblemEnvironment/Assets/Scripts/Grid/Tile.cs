using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color baseColor, highlight, movementRange, attackColor, attackableEnemyColor;
    public Transform inRangeUnit = null;
    private GridManager gridManager;
    private EnvironmentManager envManager;
    private CombatManager combatManager;
    private TextManager textManager;
    private SpriteRenderer spriteRenderer;

    //now for testing to public later can go to private
    public int tileType = 0; // 0: path, 1: forest, 2: mountain, 3: water 

    private bool canClick = false;
    //private bool clicked = false;
    public bool moveable = false;
    public bool attackable = false;
    public bool launchAttack = false; // if you can attack from here
    private bool selected = false;
    public bool IsWalkable = true; //if nothing is blocking the tile

    void Awake()
    {

        gridManager = this.transform.parent.GetComponent<GridManager>();
        envManager = this.transform.parent.Find("EnvironmentManager").GetComponent<EnvironmentManager>();
        //turnGameManager = envManager.transform.GetComponent<TurnGameManager>();
        combatManager = envManager.transform.GetComponent<CombatManager>();
        //augmentationManager = GameObject.Find("AugmentationManager").GetComponent<AugmentationManager>();
        textManager = GameObject.Find("TextManager").GetComponent<TextManager>();

        spriteRenderer = this.transform.GetComponent<SpriteRenderer>();
        spriteRenderer.color = baseColor;
        //this.enabled = false;
        //Start();
    }

    public void SetTileType(int type)
    {
        tileType = type;
        //SetTileColor();
    }

    public int GetTileType()
    {
        return tileType;
    }

    
    void SetTileColor()
    {
        if (tileType == 0) // path
        {
    
            baseColor = new Color(1f, 0.88f, 0.72f);
        }
        else if (tileType == 1) // forest
        {
            baseColor = new Color(0.39f, 0.75f, 0.34f);
        }
        else if (tileType == 2) // mountain
        {
            baseColor = new Color(0.5f, 0.39f, 0.09f);
        }
        else if (tileType == 3) // water
        {
            baseColor = new Color(0.577f, 0.73f, 0.76f);
        }
        else if (tileType == 4) // defense terrain
        {
            baseColor = new Color(0.88f, 1f, 0.7f);
        }
        else
        {
            baseColor = Color.white;
        }
       

    }

    // void Update()
    // {

    // }

    void OnMouseDown()
    {
        if (!canClick) return;

        //check if other tile is selected for launch attack
        if (combatManager.instantiateAttack && !launchAttack)
        {
            combatManager.ResetCombat();
        }



        canClick = false;
        UnitInformation unit = envManager.selectedUnit.GetComponent<UnitInformation>(); // check the current selected unit
        if (!launchAttack)
        {
            //unit.performAction(this, 1); // just move
            unit.PushAction(this, 1);
            gridManager.ResetTileColors();
            textManager.ShowPlayerStats(false, unit.unitIndex);
        }
        else if (launchAttack)
        {
            //unit.performAction(this, 2); // attack
            textManager.ShowPlayerStats(false, unit.unitIndex);
            unit.PushAction(this, 2);
            gridManager.ResetTileColors();
           
        }
        


    }

    // call this function when clicked on a tile that is blocked by a unit
    public void OnMouseDownBlockedTile()
    {
        canClick = false;
        UnitInformation unit = envManager.selectedUnit.GetComponent<UnitInformation>(); // check the current selected unit
        if (!unit) return;
        // still implement wait if time

        if (!launchAttack)
        {
            //unit.performAction(this, 1); // just move
            unit.PushAction(this, 1);
            gridManager.ResetTileColors();
        } 
        else if (launchAttack) 
        {
            //unit.performAction(this, 2); // attack
            unit.PushAction(this, 2);
            gridManager.ResetTileColors();
        }

        // if (!launchAttack) unit.performAction(this, 1); // just move
        // else if (launchAttack) unit.performAction(this, 2); // attack
       
   
    }

    void OnMouseEnter()
    {

        if (moveable){
            canClick = true;
            HighlightSelected();
        }
        else if (attackable){
            canClick = true;
            HighlightSelected();
        }
      
    }

    void OnMouseExit()
    {
        
        if (moveable){
            canClick = false;
            HighlightMoveRange();
        }
        else if (attackable){
            canClick = false;
            HighlightAttackRange();
        }
        
    }


    public Transform checkUnit(Vector2 tilePos)
    {
        int x = (int)tilePos.x;
        int y = (int)tilePos.y;

        Transform unit = gridManager.GetUnit(x, y);

        if (unit != null)
        {
            //Debug.Log("Contains Unit");
            return unit;
        }
        else
        {
            //Debug.Log("No Unit here");
            return null;
        }
    }

   
    public bool IsWithinMovementRange(Vector2 start, Vector2 target, int steps)
    {
        int dx = Mathf.Abs((int)target.x - (int)start.x);
        int dy = Mathf.Abs((int)target.y - (int)start.y);

        return dx + dy <= steps;
    }

    public bool IsWithinAttackRange(Vector2 start, Vector2 target, int steps, int attack)
    {
        int dx = Mathf.Abs((int)target.x - (int)start.x);
        int dy = Mathf.Abs((int)target.y - (int)start.y);

        return dx + dy == steps + attack;
    }

    public void HighlightMoveRange(){
        spriteRenderer.color = movementRange;
    }

    public void HighlightAttackRange(){
        spriteRenderer.color = attackColor;
    }

    public void HighlightSelected(){
        spriteRenderer.color = highlight;
    }

    public void HighlightEnemies(){
        spriteRenderer.color = attackableEnemyColor;
    }

    public void ResetColor()
    {
        spriteRenderer.color = baseColor;

    }
    public void ResetInformation()
    {
        launchAttack = false;

    }
}