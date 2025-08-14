using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    //define currentPiece
    //private Piece currentPiece; // Reference to the currently selected piece

    //public enum PieceType { Blue, Green, Orange, Pink, Purple, Red, SkyBlue, Yellow }; // Enum for piece types

    // Start is called before the first frame update
    void Start()
    {
        grid = new GameObject[levelData.gridWidth, levelData.gridHeight];
        SpawnGridBackgroundBlock(); // Call the method to spawn background blocks
        CreateGrid(); // Call the method to create the grid and place pieces

        //timescale will be 1
        //Time.timeScale = 0.2f;



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

        //Debug.Log("Grid created with seed: " + levelData.GridSeed);
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


    public void UpdateGrid()
    {
        StartCoroutine(RefillGridCoroutine());
    }

    private IEnumerator RefillGridCoroutine()
    {
        yield return new WaitForSeconds(0.2f);

        // pieces fall down to fill empty spaces
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

                            // Disable grid sticking during fall
                            pieceScript.stickToGrid = false;

                            // Update grid references
                            grid[x, y] = fallingPiece;
                            grid[x, upperY] = null;

                            // Update logical position
                            pieceScript.X = x;
                            pieceScript.Y = y;

                            // Animate fall
                            Vector2 targetPos = new Vector2(x, y);
                            float fallTime = 0.5f;
                            float delay = fallDelayIndex * 0.06f;

                            fallingPiece.transform.DOMove(targetPos, fallTime)
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

        // Refill empty cells with new pieces
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                if (grid[x, y] == null && !IsBlocked(x, y))
                {
                    int randomIndex = Random.Range(0, piecePrefabs.Length);
                    GameObject newPiece = Instantiate(
                        piecePrefabs[randomIndex],
                        new Vector2(x, levelData.gridHeight + 1f), // Spawn above grid
                        Quaternion.identity
                    );
                    Piece pieceScript = newPiece.GetComponent<Piece>();

                    // Disable grid sticking during spawn animation
                    pieceScript.stickToGrid = false;

                    pieceScript.SetPosition(x, y);
                    newPiece.transform.SetParent(transform);
                    newPiece.name = pieceScript.pieceType.ToString() + " (" + x + ", " + y + ")";
                    newPiece.transform.localScale = Vector3.zero;
                    grid[x, y] = newPiece;

                    newPiece.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
                    newPiece.transform.DOMove(new Vector2(x, y), 0.3f).SetEase(Ease.OutBounce);
                }
            }
        }

        // Wait for all animations to finish before re-enabling grid sticking
        yield return new WaitForSeconds(0.5f);

        // Re-enable stickToGrid for all pieces
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                if (grid[x, y] != null)
                {
                    Piece pieceScript = grid[x, y].GetComponent<Piece>();
                    pieceScript.stickToGrid = true;
                }
            }
        }

        //Debug.Log("Refill complete.");

        yield return new WaitForSeconds(0.1f); // Optional delay before next refill
        // Call FindMatches of piece after refill is complete
        foreach (var piece in pieces)
        {
            if (piece != null)
            {
                piece.FindMatches(); // Call FindMatches on each piece
            }
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


    // Method to spawn a particle effect at a specific(X,Y) position of grid
    public void SpawnParticleEffect(int x, int y)
    {
        /*if (IsBlocked(x, y))
        {
            Debug.LogWarning($"Trying to spawn particle effect at blocked cell ({x},{y})");
            return;
        }
        GameObject particle = Instantiate(particlePrefab, new Vector2(x, y), Quaternion.identity);
        particle.transform.SetParent(transform);
        particle.name = "Particle (" + x + ", " + y + ")";
        Destroy(particle, 1f); // Destroy after 1 second*/
    }



    public void RegisterNewPiece(GameObject newPiece, int x, int y)
    {
        if (IsBlocked(x, y))
        {
            //Debug.LogWarning($"Trying to register piece at blocked cell ({x},{y})");
            return;
        }

        // Update grid array
        grid[x, y] = newPiece;

        // Set the piece position in its script
        Piece pieceScript = newPiece.GetComponent<Piece>();
        if (pieceScript != null)
        {
            pieceScript.SetPosition(x, y);
            pieceScript.stickToGrid = true; // Enable grid sticking once registered
        }

        // Optionally update pieces array if you want (to keep it synced)
        int index = x + y * levelData.gridWidth;
        if (index >= 0 && index < pieces.Length)
        {
            pieces[index] = pieceScript;
        }
    }

}
