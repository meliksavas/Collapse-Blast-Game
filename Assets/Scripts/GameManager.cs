using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 10; 
    public int columns = 10;
    public int colors = 6;
    public Transform gridParent;
    public GameObject[] blockPrefabs; 

    [Header("Group Size Conditions")]
    public int A = 4; 
    public int B = 7; 
    public int C = 10;
    float blockSize = 0.7f;
    private GameObject[,] grid; 

    private bool isShifting = false; 
    void Start()
    {
        InitializeGrid();
        CheckForDeadlock();
    }

    
    void InitializeGrid()
    {
        
        float gridWidth = columns * blockSize;
        float gridHeight = rows * blockSize;

        
        if (gridParent != null)
        {
            gridParent.position = new Vector3(-(gridWidth / 2) + (blockSize / 2), (gridHeight / 2) - (blockSize / 2), 0);
        }

       
        grid = new GameObject[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                
                Vector2 position = new Vector2(
                    col * blockSize - (gridWidth / 2) + (blockSize / 2),
                    -(row * blockSize) + (gridHeight / 2) - (blockSize / 2)
                );

                int randomColorIndex = Random.Range(0, colors);
                GameObject block = Instantiate(blockPrefabs[randomColorIndex], position, Quaternion.identity, gridParent);
                block.name = $"Block_{row}_{col}";

                grid[row, col] = block;

                Block blockScript = block.GetComponent<Block>();
                blockScript.SetGameManager(this); 
                blockScript.SetColor(randomColorIndex); 
                UpdateAllBlockIcons();
            }
        }
    }
    public void UpdateBlockIconsBasedOnGroupSize(int groupSize, Block block)
    {
        block.UpdateIcon(groupSize); 
    }
    private List<Block> DetectGroup(int row, int col, int color)
    {
        List<Block> group = new List<Block>();
        bool[,] visited = new bool[rows, columns];
        Queue<Vector2Int> toCheck = new Queue<Vector2Int>();

        toCheck.Enqueue(new Vector2Int(row, col));

        while (toCheck.Count > 0)
        {
            Vector2Int current = toCheck.Dequeue();
            int r = current.x;
            int c = current.y;

            if (r < 0 || r >= rows || c < 0 || c >= columns || visited[r, c]) continue;

            GameObject blockObj = grid[r, c];
            if (blockObj == null) continue;

            Block block = blockObj.GetComponent<Block>();
            if (block.GetColor() == color)
            {
                group.Add(block);
                visited[r, c] = true;

               
                toCheck.Enqueue(new Vector2Int(r + 1, c));
                toCheck.Enqueue(new Vector2Int(r - 1, c));
                toCheck.Enqueue(new Vector2Int(r, c + 1));
                toCheck.Enqueue(new Vector2Int(r, c - 1));
            }
        }

        return group;
    }
    public void HandleBlockClick(Block clickedBlock)
    {
        if (isShifting) return; 

        int row = -1, column = -1;

        
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (grid[r, c] == clickedBlock.gameObject)
                {
                    row = r;
                    column = c;
                    break;
                }
            }
            if (row != -1) break;
        }

        if (row != -1 && column != -1)
        {
            List<Block> group = DetectGroup(row, column, clickedBlock.GetColor());

            if (group.Count >= 2)
            {
                CollapseGroup(group);
            }
        }
        
    }
   
    
    
    private void UpdateAllBlockIcons()
    {
        bool[,] visited = new bool[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (grid[row, col] != null && !visited[row, col])
                {
                    
                    Block block = grid[row, col].GetComponent<Block>();
                    List<Block> group = DetectGroup(row, col, block.GetColor());

                    
                    foreach (Block groupBlock in group)
                    {
                        
                        for (int r = 0; r < rows; r++)
                        {
                            for (int c = 0; c < columns; c++)
                            {
                                if (grid[r, c] == groupBlock.gameObject)
                                {
                                    visited[r, c] = true;
                                    break;
                                }
                            }
                        }

                        
                        groupBlock.UpdateIcon(group.Count);
                    }
                }
            }
        }
    }
    
    private IEnumerator EnableClickingAfterShift()
    {
        
        yield return new WaitForSeconds(0.9f); 
        isShifting = false; 
    }
    

    private void CheckForDeadlock()
    {
        if (IsDeadlock())
        {
            Debug.Log("Deadlock detected! Shuffling the board...");
            ShuffleBoard();
        }
    }

    private bool IsDeadlock()
    {
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject blockObj = grid[row, col];
                if (blockObj == null) continue;

                Block block = blockObj.GetComponent<Block>();
                int color = block.GetColor();

                
                if (IsSameColor(row + 1, col, color) || 
                    IsSameColor(row - 1, col, color) || 
                    IsSameColor(row, col + 1, color) || 
                    IsSameColor(row, col - 1, color))   
                {
                    return false; 
                }
            }
        }
        return true; 
    }

    private void ShuffleBoard()
    {
        List<Block> allBlocks = new List<Block>();

        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (grid[row, col] != null)
                {
                    allBlocks.Add(grid[row, col].GetComponent<Block>());
                }
            }
        }

        
        bool validShuffle = false;
        int maxAttempts = 100; 
        int attempts = 0;

        while (!validShuffle && attempts < maxAttempts)
        {
            attempts++;

            allBlocks = allBlocks.OrderBy(x => Random.value).ToList();

            int index = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    
                    if (grid[row, col] != null)
                    {
                        Block block = allBlocks[index];
                        grid[row, col] = block.gameObject;

                        
                        block.transform.position = GetBlockPosition(row, col);
                        index++;
                    }
                }
            }

            
            validShuffle = !IsDeadlock();
        }

        if (validShuffle)
        {
            Debug.Log($"Deadlock resolved after {attempts} shuffle(s).");
        } else
        {
            Debug.LogError("Failed to resolve deadlock after maximum shuffle attempts.");
        }
    }

    private Vector2 GetBlockPosition(int row, int col)
    {
        float x = col * blockSize - (columns * blockSize / 2) + (blockSize / 2);
        float y = -row * blockSize + (rows * blockSize / 2) - (blockSize / 2);
        return new Vector2(x, y);
    }

    private bool IsSameColor(int row, int col, int color)
    {
       
        if (row < 0 || row >= rows || col < 0 || col >= columns) return false;

        GameObject blockObj = grid[row, col];
        if (blockObj == null) return false;

        Block block = blockObj.GetComponent<Block>();
        return block.GetColor() == color;
    }

    private void CollapseGroup(List<Block> group)
    {
       
        foreach (Block block in group)
        {
            
            int row = -1, column = -1;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (grid[r, c] == block.gameObject)
                    {
                        row = r;
                        column = c;
                        break;
                    }
                }
                if (row != -1) break;
            }

            
            if (row != -1 && column != -1)
            {
                grid[row, column] = null;
            }
            Destroy(block.gameObject);
        }

        
        isShifting = true; 

        
        StartCoroutine(ShiftBlocksAndSpawn());
     
    }

    private IEnumerator ShiftBlocksAndSpawn()
    {
        ShiftBlocksDown();

        
        SpawnNewBlocks();

        StartCoroutine(EnableClickingAfterShift());  


        
        UpdateAllBlockIcons();

        yield return new WaitForSecondsRealtime(1f);

        CheckForDeadlock();


    }

    private void ShiftBlocksDown()
    {
        for (int col = 0; col < columns; col++)
        {
            for (int row = rows - 1; row >= 0; row--)
            {
                if (grid[row, col] == null)
                {
                    
                    for (int r = row - 1; r >= 0; r--)
                    {
                        if (grid[r, col] != null)
                        {
                            
                            GameObject block = grid[r, col];
                            grid[row, col] = block;
                            grid[r, col] = null;

                          
                            StartCoroutine(DropBlock(block, row, col));

                            break;
                        }
                    }
                }
            }
        }
    }

    private void SpawnNewBlocks()
    {
        for (int col = 0; col < columns; col++)
        {
            for (int row = rows - 1; row >= 0; row--)
            {
                if (grid[row, col] == null)
                {
                    
                    int randomColorIndex = Random.Range(0, colors);
                    GameObject block = Instantiate(blockPrefabs[randomColorIndex], new Vector2(
                        col * blockSize - (columns * blockSize / 2) + (blockSize / 2),
                        rows * blockSize - row), Quaternion.identity, gridParent);

                    grid[row, col] = block;

                    Block blockScript = block.GetComponent<Block>();
                    blockScript.SetGameManager(this); 
                    blockScript.SetColor(randomColorIndex); 
                    
                    StartCoroutine(DropBlock(block, row, col));
                }       
            }
        }
    }

    private IEnumerator DropBlock(GameObject block, int targetRow, int targetCol)
    {
        float targetY = -targetRow * blockSize + (rows * blockSize / 2) - (blockSize / 2); 
        Vector2 targetPosition = new Vector2(
            targetCol * blockSize - (columns * blockSize / 2) + (blockSize / 2),
            targetY
        );

        
        float fallSpeed = 5f; 

        while (Vector2.Distance((Vector2)block.transform.position, targetPosition) > 0.01f)
        {
            
            block.transform.Translate(Vector2.down * Time.deltaTime * fallSpeed);

           
            yield return null;
        }

        block.transform.position = targetPosition;
    }

 

}
