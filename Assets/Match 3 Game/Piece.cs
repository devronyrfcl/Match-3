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
    Crying_Face





}


public class Piece : MonoBehaviour
{
    public int X;// X position in the grid
    public int Y; // Y position in the grid

    //Input related variables
    public Vector2 firstTouchPosition; // Position of the first touch
    public Vector2 finalTouchPosition; // Position of the last touch
    public float touchAngle; // Angle of the touch movement

    public GameObject otherPiece; // The Piece that will be swapped with the current Piece
    private Vector2 tempPosition; // Temporary position for moving the Piece

    public float swipeAngle; // Angle of the swipe gesture
    //public float swipeTime = 0.3f; // Duration for the tween animation


    public PieceType pieceType; // Type of the piece

    public GridManager gridManager; // Reference to the PieceMatch script for matching logic

    public bool isMatched = false; // Flag to check if the piece is matched

    private Vector2 originalWorldPosition;
    private int originalX, originalY;


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



    }

    // Update is called once per frame
    void Update()
    {
        //CalculateAngle();
        

        Vector2Int snapped = Vector2Int.RoundToInt(transform.position);
        transform.position = new Vector2(snapped.x, snapped.y); // Snap the piece to the grid position

        


    }

    private void FixedUpdate()
    {
        UpdateTargetPosition();
        //FindMatches(); // Call the method to find matches at the start
    }

    /*void UpdateTargetPosition()
    {

        if (finalTouchPosition != Vector2.zero)
        {
            RaycastHit2D hit = Physics2D.Raycast(finalTouchPosition, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                otherPiece = hit.collider.gameObject;
                tempPosition = otherPiece.transform.position;
                otherPiece.transform.position = transform.position;
                transform.position = tempPosition;
                finalTouchPosition = Vector2.zero; // Reset final touch position after swap
            }
        }






    }*/


    void UpdateTargetPosition()
    {
        if (finalTouchPosition != Vector2.zero)
        {
            RaycastHit2D hit = Physics2D.Raycast(finalTouchPosition, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                otherPiece = hit.collider.gameObject;

                Piece other = otherPiece.GetComponent<Piece>();

                // Get grid position difference
                Vector2Int currentGridPos = Vector2Int.RoundToInt(transform.position);
                Vector2Int otherGridPos = Vector2Int.RoundToInt(otherPiece.transform.position);
                Vector2Int difference = otherGridPos - currentGridPos;

                // Check if difference is one unit in only one axis (orthogonal move
                if ((Mathf.Abs(difference.x) == 1 && difference.y == 0) ||
                    (Mathf.Abs(difference.y) == 1 && difference.x == 0))   
                {


                    if (Mathf.Abs(difference.x) == 1 || Mathf.Abs(difference.y) == 1)

                    {
                        Debug.Log("Swiped to: " + otherPiece.name + " from: " + gameObject.name);

                    }


                    //tempPosition = otherPiece.transform.position;
                    //otherPiece.transform.position = transform.position;
                    //transform.position = tempPosition;
                    Vector2 myTarget = otherPiece.transform.position;
                    Vector2 otherTarget = transform.position;

                    float swipeTime = 0.3f;

                    originalWorldPosition = transform.position;
                    originalX = X;
                    originalY = Y;

                    otherPiece.GetComponent<Piece>().originalWorldPosition = otherPiece.transform.position;
                    otherPiece.GetComponent<Piece>().originalX = otherPiece.GetComponent<Piece>().X;
                    otherPiece.GetComponent<Piece>().originalY = otherPiece.GetComponent<Piece>().Y;



                    Invoke(nameof(FindMatches), swipeTime);


                    transform.DOMove(myTarget, swipeTime);
                    otherPiece.transform.DOMove(otherTarget, swipeTime);

                    // Swap positions in grid and data
                    gridManager.grid[X, Y] = otherPiece;
                    gridManager.grid[other.X, other.Y] = this.gameObject;


                    int tempX = X;
                    int tempY = Y;
                    X = other.X;
                    Y = other.Y;
                    other.X = tempX;
                    other.Y = tempY;

                    //Debug.Log for both horizontal and vertical transformations
                    if (Mathf.Abs(difference.x) == 1)
                    {
                        //gridManager.HorizontalMatch();
                        // Debug.Log("Horizontal swipe detected.");
                        Invoke(nameof(FindMatches), 0.5f); // Call the method to find matches after a short delay


                    }
                    else if (Mathf.Abs(difference.y) == 1)
                    {
                        //gridManager.VerticalMatch();
                        //Debug.Log("Vertical swipe detected.");
                        Invoke(nameof(FindMatches), 0.5f); // Call the method to find matches after a short delay


                    }


                }

                finalTouchPosition = Vector2.zero; // Reset after checking

                
            }
        }


        

    }

    /*void UpdateTargetPosition()
    {
        if (finalTouchPosition == Vector2.zero) return;

        RaycastHit2D hit = Physics2D.Raycast(finalTouchPosition, Vector2.zero);
        if (hit.collider == null || hit.collider.gameObject == gameObject) return;

        otherPiece = hit.collider.gameObject;
        Piece other = otherPiece.GetComponent<Piece>();

        Vector2Int currentGridPos = Vector2Int.RoundToInt(transform.position);
        Vector2Int otherGridPos = Vector2Int.RoundToInt(otherPiece.transform.position);
        Vector2Int difference = otherGridPos - currentGridPos;

        // Check if it's a valid adjacent move
        if ((Mathf.Abs(difference.x) == 1 && difference.y == 0) ||
            (Mathf.Abs(difference.y) == 1 && difference.x == 0))
        {
            Debug.Log("Swiped to: " + otherPiece.name + " from: " + gameObject.name);

            Vector2 myTarget = otherPiece.transform.position;
            Vector2 otherTarget = transform.position;
            float swipeTime = 0.3f;

            Sequence swapSequence = DOTween.Sequence();
            swapSequence.Append(transform.DOMove(myTarget, swipeTime));
            swapSequence.Join(otherPiece.transform.DOMove(otherTarget, swipeTime));

            swapSequence.OnComplete(() =>
            {
                // Swap grid references
                gridManager.grid[X, Y] = otherPiece;
                gridManager.grid[other.X, other.Y] = this.gameObject;

                // Swap X, Y values
                int tempX = X;
                int tempY = Y;
                X = other.X;
                Y = other.Y;
                other.X = tempX;
                other.Y = tempY;

                // Start checking for matches AFTER movement is complete
                FindMatches();
                CheckMoveCoHelper(); // Handles reverse swap if no match
                StartCoroutine(ResetOtherPieceAfterDelay());
            });
        }

        finalTouchPosition = Vector2.zero;
    }*/






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
        for (int i = 1; X + i < gridManager.gridWidth; i++)
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
        for (int i = 1; Y + i < gridManager.gridHeight; i++)
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
                } else
                {
                    // If the piece is already matched, we need to reset the other piece
                }
            }
            TriggerGridUpdate();
            //Debug.Log($"Vertical match of {verticalMatches.Count} at ({X},{Y})");
        } else
        {
            //Debug.Log("No matches found.");
            isMatched = false; // Reset the matched state if no matches found
            //CheckMoveCoHelper(); // Reverse the swap if no match
            StartCoroutine(SwipeBackAfterDelay()); // Reset the other piece if already matched



        }
    }




    void MarkAsMatched(Piece piece)
    {
        //SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();
        SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();

        //sr.GetComponent<SpriteRenderer>().enabled = false;

        //if (sr != null) sr.color = Color.gray;

        //Disable the collider to prevent further interaction
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
        piece.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            //FindMatches(); // Call the method to find matches at the start
            Destroy(piece.gameObject);
        });
    }


    

    

    void TriggerGridUpdate()
    {
        //Invoke(nameof(gridManager.UpdateGrid), 0.1f); // Delay to allow destruction to complete

        gridManager.UpdateGrid(); // Let GridManager handle collapsing and refilling
        //Debug.Log("Grid updated after match.");
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

}
