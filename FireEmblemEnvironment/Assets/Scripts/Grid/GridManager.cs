using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int width, height;
    [SerializeField] private float cellSize;
    [SerializeField] private Tile tile;
    [SerializeField] private Transform map;
    [SerializeField] private Transform background;
    [SerializeField] private Transform cam;
    //public int[] tileIndices;
    private Transform[,] gridArray; // unit positions on map
    private Transform[] flattenGrid;
    private Dictionary<Vector2, Tile> tilesDict;
    public bool augmentated = false;


    void Awake()
    {
        cam.localPosition = new Vector3(((float)width) / 2.0f - 0.5f, ((float)height) / 2.0f - 0.5f, -10f) + new Vector3(0f, 0.5f, 0f);
        background.localPosition = new Vector3(((float)width) / 2.0f - 0.5f, ((float)height) / 2.0f - 0.5f, 0f);
        map.localPosition = new Vector3(((float)width)/2.0f -0.5f, ((float)height)/2.0f - 0.5f, 0f);

    }
    private void Start(){
        
        //GenerateGrid(width, height);
        
    }
    public void GenerateGrid(){
        gridArray =  new Transform[width, height];
        tilesDict = new Dictionary<Vector2, Tile>();
        for (int x = 0; x < gridArray.GetLength(0); x++){
            for (int y = 0; y < gridArray.GetLength(1); y++){
                Vector3 localPos = new Vector3(x, y, 0);
                Vector3 worldPos = this.transform.TransformPoint(localPos);

                var spawnedTile = Instantiate(tile, worldPos, Quaternion.identity, this.transform);
                spawnedTile.name = $"Tile_{x}_{y}";
                // NOTE TO SELF: assign tile type based on map coordinates
                if ((x == 0 || x == gridArray.GetLength(0)-1) && (y <= 5 && y >= 2))
                {
                    spawnedTile.SetTileType(2);
                }
                else if ((x ==  1 && y == 3) || (x == 1 && y == 4) || (x ==  4 && y == 3) || (x == 4 && y == 4))
                {
                    spawnedTile.SetTileType(1);
                }
                else
                {
                    spawnedTile.SetTileType(0);
                }
                

                tilesDict[new Vector2(x,y)] = spawnedTile;
               
            }
        }

        //this.transform.position = new Vector3(width/2.0f - cellSize/2.0f, height/2.0f - cellSize/2.0f);
        //this.transform.position = new Vector3()
        flattenGrid = gridArray.Cast<Transform>().ToArray();
        
      
        //if (!augmentated) GameManager.Instance.SwitchState(GameState.setUnits);

    }

    
    public Vector2 MirrorPosition(Vector2 originalPos, bool mirrorX, bool mirrorY)
    {
        float centerX = (width - 1) / 2f;
        float centerY = (height - 1) / 2f;

        float x = originalPos.x;
        float y = originalPos.y;

        if (mirrorX)
        {
            x = 2 * centerX - x;
        }

        if (mirrorY)
        {
            y = 2 * centerY - y;
        }

        return new Vector2(x, y);
    }

    public List<int> GetAllTileIndices()
    {
        List<int> tileIndices = Enumerable.Range(0, width * height).ToList();
        return  tileIndices;
    }

    private Vector3 GetWorldPosition(int x, int y){
        return new Vector3(x,y) * cellSize;
    }
    public int GetMaxGridWidth()
    {
        return width -1;
    }
    public int GetMaxGridHeight()
    {
        return height -1;
    }

    // get the walkable grid tiles from one team's half
    public List<Tile> GetGridHalf(bool enemySide)
    {
        List<Tile> gridHalf = new List<Tile>();
        int half = height/2;

        if (enemySide)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = half; y < half*2; y++)
                {
                    
                    Tile tile = GetTileAtPosition(x, y);
                    // units can only start on path tile
                    if (tile != null && tile.GetTileType() == 0)
                    {
                        gridHalf.Add(tile);
                    }
                    
                }
            }
        }

        else
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < half; y++)
                {
                    
                    Tile tile = GetTileAtPosition(x, y);
                    // units can only start on path tile
                    if (tile != null && tile.GetTileType() == 0)
                    {
                        gridHalf.Add(tile);
                    }
                    
                }
            }
        }
        
        return gridHalf;
    }
    
    public Vector2 GetPositionFromIndex(int index)
    {
        int x = index % width;
        int y = index / width;

        Vector2 position = new Vector2(x, y);

        return position;
    }
    public int GetIndexFromPosition(int x, int y)
    {
        int index = y * width + x;

        return index;
    }
    public int GetIndexFromPosition(Tile tile)
    {
        int x = (int)tile.transform.localPosition.x;
        int y = (int)tile.transform.localPosition.y;

        int index = y * width + x;

        return index;
    }

    public Tile GetTileAtPosition(Vector2 pos){

        if (tilesDict.TryGetValue(pos, out var tile)){
            return tile;
        }

        else {
            return null;
        }
    }

    public Tile GetTileAtPosition(int x, int y){
        Vector2 pos = new Vector2(x, y);

        if (tilesDict.TryGetValue(pos, out var tile)){
            return tile;
        }

        else {
            return null;
        }
    }


    public Vector2 GetPositionFromTile(Tile tile){
        return tilesDict.FirstOrDefault(x => x.Value == tile).Key;
    }

    public List<Tile> GetAdjacentTiles(Tile tile){
        List<Tile> adjacentTiles = new List<Tile>();
    
        // Get the tile's position
        int x = (int)tile.transform.localPosition.x;
        int y = (int)tile.transform.localPosition.y;

        // Define the four possible directions (up, down, left, right)
        Vector2[] directions = new Vector2[]
        {
            new Vector2(x, y + 1), // Up
            new Vector2(x, y - 1), // Down
            new Vector2(x - 1, y), // Left
            new Vector2(x + 1, y)  // Right
        };

        // Loop through each direction and get the tile
        foreach (Vector2 pos in directions)
        {
            Tile adjacentTile = GetTileAtPosition((int)pos.x, (int)pos.y);
            if (adjacentTile != null) // Ensure the tile exists before adding
            {
                adjacentTiles.Add(adjacentTile);
            }
            else{
                adjacentTiles.Add(null);
            }
        }

        return adjacentTiles;


    }

    public List<Tile> GetTilesInRange(Tile tile, int range)
    {
        List<Tile> tilesInRange = new List<Tile>();

        int startX = (int)tile.transform.localPosition.x;
        int startY = (int)tile.transform.localPosition.y;

        // Loop through all positions within range (Manhattan distance)
        for (int x = startX - range; x <= startX + range; x++)
        {
            for (int y = startY - range; y <= startY + range; y++)
            {
                int distance = Mathf.Abs(x - startX) + Mathf.Abs(y - startY);
                if (distance <= range) // Only include tiles within the exact range
                {
                    Tile foundTile = GetTileAtPosition(x, y);
                    if (foundTile != null) // Make sure tile exists
                    {
                        tilesInRange.Add(foundTile);
                    }
                }
            }
        }

        return tilesInRange;
    }


    // Get all the tiles that are in range to attack from
    public List<Tile> GetAttackTilesInRange(Tile tile, int range, List<Tile> reach)
    {
        List<Tile> tilesInRange = new List<Tile>();

        int startX = (int)tile.transform.localPosition.x;
        int startY = (int)tile.transform.localPosition.y;

        // Loop through all positions within range (Manhattan distance)
        for (int x = startX - range; x <= startX + range; x++)
        {
            for (int y = startY - range; y <= startY + range; y++)
            {
                int distance = Mathf.Abs(x - startX) + Mathf.Abs(y - startY);
                if (distance == range) // Only include tiles within the exact range
                {
                    Tile foundTile = GetTileAtPosition(x, y);
                    if (foundTile != null && reach.Contains(foundTile)) // Make sure tile exists
                    {
                        tilesInRange.Add(foundTile);
                    }
                }
            }
        }

        return tilesInRange;
    }

    public void SetUnit(int x, int y, Transform unit){
        if (x >= 0 && x < width && y < height){
            gridArray[x, y] = unit;
            
            Tile tile = GetTileAtPosition(x, y);
            if (tile != null)
            {
                tile.enabled = true;
            }
           
        }
    }

    public void RemoveUnit(int x, int y){
        if (x >= 0 && x < width && y < height){
            gridArray[x, y] = null;     
           
        }
    }

    public Transform GetUnit(int x, int y){
        if (x >= 0 && x < width && y < height){
            return gridArray[x, y];
        }
        else {
            return null;
        }
    }

    public int GetDistance(Transform unit, Transform target)
    {
        float x = unit.localPosition.x;
        float y = unit.localPosition.y;

        float xTarget = target.localPosition.x;
        float yTarget = target.localPosition.y;

        //float distance = Vector2.Distance(new Vector2(x, y), new Vector2(xTarget, yTarget));
        int distance = (int)(Mathf.Abs(x - xTarget) + Mathf.Abs(y - yTarget));

        return distance;

    }

    public int GetMaxDistance()
    {
        return (width - 1) + (height - 1);
    }

    public int GetDistance(Tile unitTile, Tile targetTile)
    {
        Vector2 unitPosition = GetPositionFromTile(unitTile);
        Vector2 targetPosition = GetPositionFromTile(targetTile);

        int distance = (int)(Mathf.Abs(unitPosition.x - targetPosition.x) + Mathf.Abs(unitPosition.y - targetPosition.y));

        return distance;
    }


    public void ResetTileColors()
    {
        foreach (var tile in tilesDict.Values)
        {
            tile.ResetColor();
        }
    }
}
