using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum PieceType
{
    Smiling_Face,
    Smiling_Face_with_Tear,
    Angry_Face,
    Laughing_Face,
    Smiling_Face_With_Heart_Eyes,
    Sleeping_Face,
    Surprised_Face,
    Crying_Face,
}

public enum SpecialPieceType
{
    //Special Pieces
    Bomb,
    Coloumn_Clear,
    Row_Clear,
    Colour_Clear
}


public class Piece : MonoBehaviour
{
    public int X;// X position in the grid
    public int Y; // Y position in the grid

    //Input related variables
    public Vector2 firstTouchPosition; // Position of the first touch
    public Vector2 finalTouchPosition; // Position of the last touch
    //public float touchAngle; // Angle of the touch movement

    public GameObject otherPiece; // The Piece that will be swapped with the current Piece
    private Vector2 tempPosition; // Temporary position for moving the Piece

    public float swipeAngle; // Angle of the swipe gesture
    //public float swipeTime = 0.3f; // Duration for the tween animation


    public PieceType pieceType; // Type of the piece

    public SpecialPieceType specialPieceType; // Type of the special piece

    public bool IsSpecialPiece = false;

    public GridManager gridManager; // Reference to the PieceMatch script for matching logic

    public bool isMatched = false; // Flag to check if the piece is matched

    private Vector2 originalWorldPosition;
    private int originalX, originalY;

    private LevelData levelData;

    public bool stickToGrid = true; // Whether the piece should stick to the grid


    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
        transform.position = new Vector2(x, y); // Set the position of the piece in the grid
    }

    /*public void SetPieceType(PieceType type)
    {
        pieceType = type;
        
    }*/

    // Start is called before the first frame update
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>(); // Find the PieceMatch script in the scene
        //Invoke(nameof(TriggerGridUpdate), 0.2f); // Call the method to find matches at the start

        //FindMatches(); // Call the method to find matches at the start

        Invoke(nameof(FindMatches), 0.5f); // Call the method to find matches after a short delay

        //take levelData from the gridManager
        if (gridManager != null)
        {
            levelData = gridManager.levelData; // Get the LevelData from the GridManager
        }
        else
        {
            Debug.LogError("GridManager not found in the scene.");
        }

        stickToGrid = true; // Enable sticking to grid by default

    }

    // Update is called once per frame
    void Update()
    {
        //CalculateAngle();


        Vector2Int snapped = Vector2Int.RoundToInt(transform.position);
        transform.position = new Vector2(snapped.x, snapped.y); // Snap the piece to the grid position

        // Check if the piece is a special piece is bomb and handle double-click on collider for bomb actions
        if (IsSpecialPiece && specialPieceType == SpecialPieceType.Bomb)
        {
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null && collider.enabled && Input.GetMouseButtonDown(0))
            {
                // Check if the mouse is over the collider
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (collider.OverlapPoint(mousePosition))
                {
                    // Handle double-click action for bomb
                    Debug.Log("Bomb piece clicked! Triggering explosion...");
                    Bomb(X, Y); // Call the Bomb method with the current piece's position
                }
            }
        }

        if(IsSpecialPiece && specialPieceType == SpecialPieceType.Coloumn_Clear)
        {
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null && collider.enabled && Input.GetMouseButtonDown(0))
            {
                // Check if the mouse is over the collider
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (collider.OverlapPoint(mousePosition))
                {
                    // Handle double-click action for column clear
                    Debug.Log("Column Clear piece clicked! Clearing column...");
                    ClearColoumn(X); // Call the ClearColoumn method with the current piece's X position
                }
            }
        }

        if (IsSpecialPiece && specialPieceType == SpecialPieceType.Row_Clear)
        {
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null && collider.enabled && Input.GetMouseButtonDown(0))
            {
                // Check if the mouse is over the collider
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (collider.OverlapPoint(mousePosition))
                {
                    // Handle double-click action for row clear
                    Debug.Log("Row Clear piece clicked! Clearing row...");
                    ClearRow(Y); // Call the ClearRow method with the current piece's Y position
                }
            }
        }


    }

    private void FixedUpdate()
    {
        UpdateTargetPosition();
        //FindMatches(); // Call the method to find matches at the start
    }

    void UpdateTargetPosition()
    {
        if (finalTouchPosition != Vector2.zero)
        {
            RaycastHit2D hit = Physics2D.Raycast(finalTouchPosition, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                otherPiece = hit.collider.gameObject;
                Piece other = otherPiece.GetComponent<Piece>();

                Vector2Int currentGridPos = Vector2Int.RoundToInt(transform.position);
                Vector2Int otherGridPos = Vector2Int.RoundToInt(otherPiece.transform.position);
                Vector2Int difference = otherGridPos - currentGridPos;

                if ((Mathf.Abs(difference.x) == 1 && difference.y == 0) ||
                    (Mathf.Abs(difference.y) == 1 && difference.x == 0))
                {
                    Debug.Log("Swiped to: " + otherPiece.name + " from: " + gameObject.name);

                    Vector2 myTarget = otherPiece.transform.position;
                    Vector2 otherTarget = transform.position;

                    float swipeTime = 0.3f;

                    originalWorldPosition = transform.position;
                    originalX = X;
                    originalY = Y;

                    other.originalWorldPosition = otherPiece.transform.position;
                    other.originalX = other.X;
                    other.originalY = other.Y;

                    // Only update grid and logical positions if sticking to grid
                    if (stickToGrid)
                    {
                        // Animate movement
                        transform.DOMove(myTarget, swipeTime);
                        otherPiece.transform.DOMove(otherTarget, swipeTime);

                        // Swap in grid
                        gridManager.grid[X, Y] = otherPiece;
                        gridManager.grid[other.X, other.Y] = this.gameObject;

                        // Swap logical coordinates
                        int tempX = X;
                        int tempY = Y;
                        X = other.X;
                        Y = other.Y;
                        other.X = tempX;
                        other.Y = tempY;

                        // Trigger match check
                        Invoke(nameof(FindMatches), 0.5f);
                    }
                    else
                    {
                        // If not sticking to grid, just animate freely
                        transform.DOMove(myTarget, swipeTime);
                        otherPiece.transform.DOMove(otherTarget, swipeTime);
                    }

                    finalTouchPosition = Vector2.zero;
                }
            }
        }
    }


    void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        //Debug.Log("Swipe Angle : " + swipeAngle);
        //gridManager.CheckForMatches();

        //Invoke(nameof(FindMatches), 0.2f); // Call the method to find matches after a short delay

        //FindMatches();



    }


    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(firstTouchPosition);
    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Debug.Log(finalTouchPosition);
        CalculateAngle();
    }


    public void FindMatches()
    {
        //Debug.Log("Finding matches for piece at (" + X + "," + Y + ") with type " + pieceType);

        if (gridManager == null || gridManager.grid == null) return;

        List<Piece> horizontalMatches = new List<Piece>();
        List<Piece> verticalMatches = new List<Piece>();

        // 🔹 Horizontal Match Check
        horizontalMatches.Add(this);

        // Check Left
        for (int i = 1; X - i >= 0; i++)
        {
            Piece next = gridManager.grid[X - i, Y]?.GetComponent<Piece>();
            if (next != null && next.pieceType == pieceType)
                horizontalMatches.Add(next);
            else
                break;
        }

        // Check Right
        for (int i = 1; X + i < levelData.gridWidth; i++)
        {
            Piece next = gridManager.grid[X + i, Y]?.GetComponent<Piece>();
            if (next != null && next.pieceType == pieceType)
                horizontalMatches.Add(next);
            else
                break;
        }

        if (horizontalMatches.Count >= 3)
        {
            foreach (var piece in horizontalMatches)
            {
                if (piece != null && !piece.isMatched)
                {
                    piece.isMatched = true;
                    MarkAsMatched(piece);
                }
                else
                {
                    // If the piece is already matched, we need to reset the other piece
                }
            }
            TriggerGridUpdate();
            //Debug.Log($"Horizontal match of {horizontalMatches.Count} at ({X},{Y})");
        }

        //if horizontalMatches count 4 or more , then call Bomb(int x, int y)
        if (horizontalMatches.Count >= 4)
        {
            gridManager.SpawnSpecialPiece(X, Y, GridManager.SpecialPieceType.Bomb);

        }

        //if horizontalMatches count 5 or more , then call ClearRow
        if (horizontalMatches.Count >= 5)
        {
            gridManager.SpawnSpecialPiece(X, Y, GridManager.SpecialPieceType.Column_Clear);
        }

        //if horizontalMatches count 6 or more , then call ClearColour
        if (horizontalMatches.Count >= 6 || verticalMatches.Count >= 6)
        {
            gridManager.SpawnSpecialPiece(X, Y, GridManager.SpecialPieceType.Row_Clear);
        }



        // 🔹 Vertical Match Check
        verticalMatches.Add(this);

        // Check Down
        for (int i = 1; Y - i >= 0; i++)
        {
            Piece next = gridManager.grid[X, Y - i]?.GetComponent<Piece>();
            if (next != null && next.pieceType == pieceType)
                verticalMatches.Add(next);
            else
                break;
        }

        // Check Up
        for (int i = 1; Y + i < levelData.gridHeight; i++)
        {
            Piece next = gridManager.grid[X, Y + i]?.GetComponent<Piece>();
            if (next != null && next.pieceType == pieceType)
                verticalMatches.Add(next);
            else
                break;
        }

        if (verticalMatches.Count >= 3)
        {
            foreach (var piece in verticalMatches)
            {
                if (piece != null && !piece.isMatched)
                {
                    piece.isMatched = true;
                    MarkAsMatched(piece);
                }
                else
                {
                    // If the piece is already matched, we need to reset the other piece
                }
            }
            TriggerGridUpdate();
            //Debug.Log($"Vertical match of {verticalMatches.Count} at ({X},{Y})");
        }
        else
        {
            //Debug.Log("No matches found.");
            isMatched = false; // Reset the matched state if no matches found
            //CheckMoveCoHelper(); // Reverse the swap if no match
            StartCoroutine(SwipeBackAfterDelay()); // Reset the other piece if already matched



        }
        //if verticalMatches count 4 or more , then call Bomb(int x, int y)
        if (verticalMatches.Count >= 4)
        {
            gridManager.SpawnSpecialPiece(X, Y, GridManager.SpecialPieceType.Bomb);

        }

        //if verticalMatches count 4 or more , then call ClearColoumn
        if (verticalMatches.Count >= 5)
        {
            gridManager.SpawnSpecialPiece(X, Y, GridManager.SpecialPieceType.Column_Clear);
        }

        //if horizontalMatches count 6 or more , then call ClearColour
        if (horizontalMatches.Count >= 6 || verticalMatches.Count >= 6)
        {
            gridManager.SpawnSpecialPiece(X, Y, GridManager.SpecialPieceType.Row_Clear);
        }
    }




    void MarkAsMatched(Piece piece)
    {
        
        Collider2D collider = piece.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        TriggerGridUpdate();

        /*//destroy the piece after marking it as matched
        if (piece == null || gridManager == null) return;
        Destroy(piece.gameObject); // Destroy the matched piece*/



        // Clear grid reference immediately
        gridManager.grid[piece.X, piece.Y] = null;

        // Animate scale down to zero before destroying the piece
        piece.transform.DOScale(Vector2.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            //FindMatches(); // Call the method to find matches at the start
            Destroy(piece.gameObject);
        });
    }






    void TriggerGridUpdate()
    {
        //Invoke(nameof(gridManager.UpdateGrid), 0.1f); // Delay to allow destruction to complete

        //gridManager.UpdateGrid(); // Let GridManager handle collapsing and refilling
        //Debug.Log("Grid updated after match.");

        gridManager.UpdateGrid(false);
    }





    // Helper method to check if no matches were found after a swap. If no matches found, reverse the swap wait for 1 sec and reset the positions using dotween
    private IEnumerator SwipeBackAfterDelay(float delay = 1f)
    {
        Debug.Log("No matches found, reversing swap...");

        yield return new WaitForSeconds(delay);

        if (otherPiece == null) yield break;

        Piece other = otherPiece.GetComponent<Piece>();
        if (other == null) yield break;

        float swipeTime = 0.3f;

        // Move both pieces back to their original positions
        transform.DOMove(originalWorldPosition, swipeTime);
        otherPiece.transform.DOMove(other.originalWorldPosition, swipeTime);

        // Restore grid references
        X = originalX;
        Y = originalY;
        other.X = other.originalX;
        other.Y = other.originalY;

        gridManager.grid[X, Y] = this.gameObject;
        gridManager.grid[other.X, other.Y] = otherPiece;


    }




    void ClearColoumn(int coloumnIndex)
    {
        for (int y = 0; y < levelData.gridHeight; y++)
        {
            Piece piece = gridManager.grid[coloumnIndex, y]?.GetComponent<Piece>();
            if (piece != null && !piece.isMatched)
            {
                piece.isMatched = true;
                MarkAsMatched(piece);
            }
        }
    }

    void ClearRow(int rowIndex)
    {
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            Piece piece = gridManager.grid[x, rowIndex]?.GetComponent<Piece>();
            if (piece != null && !piece.isMatched)
            {
                piece.isMatched = true;
                MarkAsMatched(piece);
            }
        }
    }

    void ClearColour(PieceType type)
    {
        for (int x = 0; x < levelData.gridWidth; x++)
        {
            for (int y = 0; y < levelData.gridHeight; y++)
            {
                Piece piece = gridManager.grid[x, y]?.GetComponent<Piece>();
                if (piece != null && piece.pieceType == type && !piece.isMatched)
                {
                    piece.isMatched = true;
                    MarkAsMatched(piece);
                }
            }
        }
    }

    void Bomb(int x, int y)
    {
        // Clear surrounding pieces in a 3x3 area
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int targetX = x + i;
                int targetY = y + j;
                if (targetX >= 0 && targetX < levelData.gridWidth && targetY >= 0 && targetY < levelData.gridHeight)
                {
                    Piece piece = gridManager.grid[targetX, targetY]?.GetComponent<Piece>();
                    if (piece != null && !piece.isMatched)
                    {
                        piece.isMatched = true;
                        MarkAsMatched(piece);
                    }
                }
            }
        }
    }


    void StickToTheGrid()
    {
        if (stickToGrid)
        {
            // Snap the piece to the grid position
            Vector2 snappedPosition = new Vector2(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
            transform.position = snappedPosition;

            /*Vector2Int snapped = Vector2Int.RoundToInt(transform.position);
            transform.position = new Vector2(snapped.x, snapped.y); // Snap the piece to the grid position
            */
        }

    }


    //stickToGrid = false; // Disable sticking to grid for this operation

    //stickToGrid will be false for a certain float time. after that it will be true both for this piece and the other piece
    public void SetStickToGrid(float duration)
    {
        stickToGrid = false; // Disable sticking to grid for this operation
        Invoke(nameof(EnableStickToGrid), duration); // Re-enable after the specified duration
    }
    private void EnableStickToGrid()
    {
        stickToGrid = false; // Re-enable sticking to grid
        if (otherPiece != null)
        {
            Piece other = otherPiece.GetComponent<Piece>();
            if (other != null)
            {
                other.stickToGrid = false; // Also enable for the other piece
            }
        }
    }



}