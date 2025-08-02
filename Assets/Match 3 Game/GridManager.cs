using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridWidth = 5; // Width of the grid
    public int gridHeight = 10; // Height of the grid
    public GameObject[] piecePrefabs;// Array of piece prefabs to instantiate
    public GameObject[,] grid; // 2D array to hold the grid pieces
    //public Piece[] pieces; // Array to hold all pieces in the game

    //define currentPiece
    //private Piece currentPiece; // Reference to the currently selected piece

    //public enum PieceType { Blue, Green, Orange, Pink, Purple, Red, SkyBlue, Yellow }; // Enum for piece types




    // Start is called before the first frame update
    void Start()
    {
        grid = new GameObject[gridWidth, gridHeight];
        CreateGrid(); // Call the method to create the grid and place pieces

        /*//pieces will be the piecePrefabs those are spawned in the grid
        pieces = new Piece[gridWidth * gridHeight]; // Initialize the pieces array with the total number of pieces
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                pieces[x + y * gridWidth] = grid[x, y].GetComponent<Piece>(); // Store each piece in the pieces array
            }
        }
        CheckForMatches(); // Call the method to check for matches in the grid*/


    }

    // Update is called once per frame
    void Update()
    {
        //HandleTouchInput(); // Call the method to handle touch input
    }

    //create grid and place pieces position based on piecePrefabs attactched script Piece.SetPosition(x, y)
    public void CreateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int randomIndex = Random.Range(0, piecePrefabs.Length);
                GameObject piece = Instantiate(piecePrefabs[randomIndex], new Vector2(x, y), Quaternion.identity);
                piece.GetComponent<Piece>().SetPosition(x, y); // Set the position of the piece in the grid
                grid[x, y] = piece; // Store the piece in the grid array

                piece.transform.SetParent(transform);
                
                piece.name = piece.GetComponent<Piece>().pieceType.ToString() + " (" + x + ", " + y + ")";
            }
        }

        
    }

    public void UpdateGrid()
    {
        StartCoroutine(RefillGridCoroutine());
    }

    private IEnumerator RefillGridCoroutine()
    {
        yield return new WaitForSeconds(0.7f); // Reduced wait, feel free to adjust

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[x, y] == null)
                {
                    int randomIndex = Random.Range(0, piecePrefabs.Length);
                    GameObject newPiece = Instantiate(
                        piecePrefabs[randomIndex],
                        new Vector2(x, y + 1f), // Slightly above for fall effect
                        Quaternion.identity
                    );

                    Piece pieceScript = newPiece.GetComponent<Piece>();
                    pieceScript.SetPosition(x, y);
                    newPiece.transform.SetParent(transform);
                    newPiece.name = pieceScript.pieceType.ToString() + " (" + x + ", " + y + ")";

                    // Start with zero scale (invisible)
                    newPiece.transform.localScale = Vector3.zero;

                    // Store in grid before animating
                    grid[x, y] = newPiece;

                    // Animate scale (appear) then move down
                    newPiece.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
                    newPiece.transform.DOMove(new Vector2(x, y), 0.3f).SetEase(Ease.OutBounce);
                }
            }
        }

        Debug.Log("Refill complete.");
    }



}
