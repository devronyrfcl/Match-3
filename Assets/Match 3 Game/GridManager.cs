using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    //public int gridWidth = 5; // Width of the grid
    //public int gridHeight = 10; // Height of the grid
    public GameObject[] piecePrefabs;// Array of piece prefabs to instantiate
    public GameObject[,] grid; // 2D array to hold the grid pieces
    public Piece[] pieces; // Array to hold all pieces in the game
    public LevelData levelData; // Reference to the LevelData ScriptableObject

    public GameObject brickPrefab; // Prefab for the brick piece

    public GameObject particlePrefab; // Prefab for the particle effect

    public GameObject GridBackgroundBlock; // Array of background block prefabs for the grid

    // Enum for identifying special piece types
    public enum SpecialPieceType
    {
        Bomb,
        Column_Clear,
        Row_Clear,
        Colour_Clear
    }

    [System.Serializable]
    public struct SpecialPieceData
    {
        public int x;
        public int y;
        public SpecialPieceType type;
    }

    // Prefab references
    public GameObject Bomb;
    public GameObject Coloumn_Clear;
    public GameObject Row_Clear;
    public GameObject Colour_Clear;

    // Start is called before the first frame update
    void Start()
    {
        grid = new GameObject[levelData.gridWidth, levelData.gridHeight];
        SpawnGridBackgroundBlock(); // Call the method to spawn background blocks
        CreateGrid(); // Call the method to create the grid and place pieces

    }

    private void FixedUpdate()
    {
        //pieces objects will be the spawned pieces in the game
        pieces = new Piece[levelData.gridWidth * levelData.gridHeight];
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    Piece pieceScript = grid[x, y].GetComponent<Piece>();
                    pieces[x + y * levelData.gridWidth] = pieceScript; // Store the piece in the pieces array
                }
            }
        }
    }



    //the grid and place pieces using seed from LevelData
    private void CreateGrid()
    {
        // Use the seed from LevelData to ensure consistent piece placement
        Random.InitState(levelData.GridSeed);

        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                if (IsBlocked(x, y))
                {
                    grid[x, y] = null; // Explicitly mark as blocked
                    //spawn brick prefab in blocked cells
                    GameObject brick = Instantiate(brickPrefab, new Vector2(x, y), Quaternion.identity);
                    //brick.transform.SetParent(transform);
                    //brick.transform.localScale = new Vector2(x, y);
                    brick.name = "Brick (" + x + ", " + y + ")";
                    continue; // Skip to the next cell


                }

                int randomIndex = Random.Range(0, piecePrefabs.Length);
                GameObject newPiece = Instantiate(
                    piecePrefabs[randomIndex],
                    new Vector2(x, y + 1f),
                    Quaternion.identity
                );

                Piece pieceScript = newPiece.GetComponent<Piece>();
                pieceScript.SetPosition(x, y); //GameObject.SetPosition(Vector2)
                newPiece.transform.SetParent(transform);
                newPiece.name = pieceScript.pieceType.ToString() + " (" + x + ", " + y + ")";
                newPiece.transform.localScale = Vector3.zero;
                grid[x, y] = newPiece;
                newPiece.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
                newPiece.transform.DOMove(new Vector2(x, y), 0.3f).SetEase(Ease.OutBounce);
            }
        }

        Debug.Log("Grid created with seed: " + levelData.GridSeed);
    }

    void SpawnGridBackgroundBlock()
    {
        //spawn background block prefabs in the grid and it will be used to fill the grid background
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                GameObject block = Instantiate(GridBackgroundBlock, new Vector2(x, y), Quaternion.identity);
                block.transform.SetParent(transform);
                block.name = "Block (" + x + ", " + y + ")";
                block.transform.localScale = Vector3.one; // Set scale to one for visibility
            }
        }
    }

    // Public method to refill with optional special pieces
    public void UpdateGrid(bool withSpecialPieces, List<SpecialPieceData> specialPieces = null)
    {
        StartCoroutine(RefillGridCoroutine(withSpecialPieces, specialPieces));
    }

    private IEnumerator RefillGridCoroutine(bool withSpecialPieces, List<SpecialPieceData> specialPieces)
    {
        yield return new WaitForSeconds(0.2f);

        // 1. Pieces fall down to fill empty spaces
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            int fallDelayIndex = 0;

            for (int y = 0; y < levelData.gridHeight; y++)
            {
                if (grid[x, y] == null && !IsBlocked(x, y))
                {
                    for (int upperY = y + 1; upperY < levelData.gridHeight; upperY++)
                    {
                        if (grid[x, upperY] != null && !IsBlocked(x, upperY))
                        {
                            GameObject fallingPiece = grid[x, upperY];
                            Piece pieceScript = fallingPiece.GetComponent<Piece>();

                            pieceScript.stickToGrid = false;

                            grid[x, y] = fallingPiece;
                            grid[x, upperY] = null;

                            pieceScript.X = x;
                            pieceScript.Y = y;

                            Vector2 targetPos = new Vector2(x, y);
                            float delay = fallDelayIndex * 0.06f;

                            fallingPiece.transform.DOMove(targetPos, 0.5f)
                                .SetEase(Ease.InQuad)
                                .SetDelay(delay);

                            fallDelayIndex++;
                            break;
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.35f);

        // 2. If special refill, spawn special pieces first
        if (withSpecialPieces && specialPieces != null)
        {
            foreach (var sp in specialPieces)
            {
                if (grid[sp.x, sp.y] == null && !IsBlocked(sp.x, sp.y))
                {
                    GameObject prefabToSpawn = null;
                    switch (sp.type)
                    {
                        case SpecialPieceType.Bomb:
                            prefabToSpawn = Bomb;
                            break;
                        case SpecialPieceType.Column_Clear:
                            prefabToSpawn = Coloumn_Clear;
                            break;
                        case SpecialPieceType.Row_Clear:
                            prefabToSpawn = Row_Clear;
                            break;
                        case SpecialPieceType.Colour_Clear:
                            prefabToSpawn = Colour_Clear;
                            break;
                    }

                    if (prefabToSpawn != null)
                    {
                        GameObject specialPiece = Instantiate(prefabToSpawn,
                            new Vector2(sp.x, levelData.gridHeight + 1f),
                            Quaternion.identity);

                        Piece pieceScript = specialPiece.GetComponent<Piece>();
                        pieceScript.stickToGrid = false;
                        pieceScript.SetPosition(sp.x, sp.y);

                        specialPiece.transform.SetParent(transform);
                        specialPiece.transform.localScale = Vector3.zero;
                        grid[sp.x, sp.y] = specialPiece;

                        specialPiece.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
                        specialPiece.transform.DOMove(new Vector2(sp.x, sp.y), 0.3f).SetEase(Ease.OutBounce);
                    }
                }
            }
        }

        // 3. Spawn remaining normal pieces
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                if (grid[x, y] == null && !IsBlocked(x, y))
                {
                    int randomIndex = Random.Range(0, piecePrefabs.Length);
                    GameObject newPiece = Instantiate(piecePrefabs[randomIndex],
                        new Vector2(x, levelData.gridHeight + 1f),
                        Quaternion.identity);

                    Piece pieceScript = newPiece.GetComponent<Piece>();
                    pieceScript.stickToGrid = false;
                    pieceScript.SetPosition(x, y);

                    newPiece.transform.SetParent(transform);
                    newPiece.transform.localScale = Vector3.zero;
                    grid[x, y] = newPiece;

                    newPiece.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
                    newPiece.transform.DOMove(new Vector2(x, y), 0.3f).SetEase(Ease.OutBounce);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        // Re-enable sticking
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    grid[x, y].GetComponent<Piece>().stickToGrid = true;
                }
            }
        }

        Debug.Log("Refill complete.");

        // Call FindMatches after refill
        foreach (var piece in pieces)
        {
            if (piece != null)
                piece.FindMatches();
        }
    }

    private bool IsBlocked(int x, int y)
    {
        foreach (var blockedCell in levelData.blockedCells)
        {
            if (blockedCell.x == x && blockedCell.y == y)
            {
                return true; // Cell is blocked
            }
        }
        return false; // Cell is not blocked
    }


    // Method to spawn a particle effect at a specific position of grid
    public void SpawnParticleEffect(Vector2 position)
    {
        GameObject particle = Instantiate(particlePrefab, position, Quaternion.identity);
        Destroy(particle, 1f); // Destroy after 1 second to clean up
    }

    public void SpawnSpecialPiece(int x, int y, SpecialPieceType type)
    {
        if (grid[x, y] != null) Destroy(grid[x, y]); // Remove old piece

        GameObject prefabToSpawn = null;
        switch (type)
        {
            case SpecialPieceType.Bomb:
                prefabToSpawn = Bomb;
                break;
            case SpecialPieceType.Column_Clear:
                prefabToSpawn = Coloumn_Clear;
                break;
            case SpecialPieceType.Row_Clear:
                prefabToSpawn = Row_Clear;
                break;
            case SpecialPieceType.Colour_Clear:
                prefabToSpawn = Colour_Clear;
                break;
        }

        if (prefabToSpawn != null)
        {
            GameObject specialPiece = Instantiate(prefabToSpawn, new Vector2(x, levelData.gridHeight + 1f), Quaternion.identity);
            Piece pieceScript = specialPiece.GetComponent<Piece>();
            pieceScript.stickToGrid = false;
            pieceScript.SetPosition(x, y);

            specialPiece.transform.SetParent(transform);
            grid[x, y] = specialPiece;

            // Animate drop
            specialPiece.transform.DOMove(new Vector2(x, y), 0.3f).SetEase(Ease.OutBounce);
        }
    }


}
