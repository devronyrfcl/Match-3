using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;


[System.Serializable]
public class SpriteData
{
    public Sprite sprite;
    public Vector3 targetScale = Vector3.one; // Default (1,1,1), you can customize per sprite
}


public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject[] piecePrefabs;// Array of piece prefabs to instantiate
    public GameObject[,] grid; // 2D array to hold the grid pieces
    public Piece[] pieces; // Array to hold all pieces in the game
    public LevelData levelData; // Reference to the LevelData ScriptableObject

    public LevelData[] levelDatas; // Array of LevelData ScriptableObjects for different levels

    public int currentLevelIndex = 0;
    private const string SelectedLevelIndexKey = "SelectedLevelIndex";


    public GameObject brickPrefab; // Prefab for the brick piece
    public GameObject particlePrefab; // Prefab for the particle effect
    public GameObject GridBackgroundBlock; // Array of background block prefabs for the grid
    public bool isPlacingBomb = false;
    public bool isPlacingColor = false;
    public bool canControl = true;




    [Header("Main Game Visuals")]
    public GameObject EmojisImage;

    public TextMeshProUGUI timeText;
    //public int currentTime; // Current time in seconds
    public TextMeshProUGUI movesCountText;

    //Ability UI Elements
    public TextMeshProUGUI Ability_bombCountText;
    public TextMeshProUGUI Ability_ColorBombCountText;
    public TextMeshProUGUI Ability_extraMovesCountText;
    /*public int Ability_bombStartAmount;
    public int Ability_colorBombStartAmount;
    public int Ability_extraMovesStartAmount;*/
    public int Ability_bombCurrentAmount;
    public int Ability_colorBombCurrentAmount;
    public int Ability_extraMovesCurrentAmount;
    public SpriteData[] sprites; // Each sprite has its own target scale
    public Image targetImage; // Target Image component
    public float scaleDuration = 0.3f; // How fast it scales
    public float holdDuration = 0.5f;  // Hold time before scaling back
    private Sequence currentSequence; // Track current tween sequence


    /*private int bombAmount;
    private int colorAmount;
    private int extraMovesAmount;*/

    public float currentTime;
    private int currentMoves;
    private int currentTarget1;
    private int currentTarget2;

    public GameObject GameOverPanel;
    public TMP_Text gameOverText; // Text to display game over message
    public TMP_Text level_Count;
    public GameObject itemWarningPanel;
    public int stars = 0;
    public int XP = 0; // XP earned in the level
    public GameObject[] normalStars; // Empty stars
    public GameObject[] glowStars;   // Filled stars
    public TMP_Text xpAmount;




    [Header("Targets Section")]
    public Sprite smilingFaceSprite;
    public Sprite smilingFaceWithTearSprite;
    public Sprite angryFaceSprite;
    public Sprite laughingFaceSprite;
    public Sprite smilingFaceWithHeartEyesSprite;
    public Sprite sleepingFaceSprite;
    public Sprite surprisedFaceSprite;
    public Sprite cryingFaceSprite;

    public Image target1Image; // Image to represent target1
    public Image target2Image; // Image to represent target2
    public TMP_Text target1CountText; // TextMeshPro text for target1 count
    public TMP_Text target2CountText; // TextMeshPro text for target2 count

    private int currentTarget1Count;
    private int currentTarget2Count;

    public GameObject horizontalClearParticle;
    public GameObject verticalClearParticle;

    private bool isTimerRunning = false;
    public string fileName = "playerdata.json";

    private string SavePath; // = Path.Combine(Application.persistentDataPath, "playerdata.json");



    #region "Common Region"
    // Start is called before the first frame update
    void Start()
    {
        grid = new GameObject[levelData.gridWidth, levelData.gridHeight];
        SavePath = Path.Combine(Application.persistentDataPath, "playerdata.json");

        LoadLevel();

        //AudioManager.Instance.PlayMusic("MenuBG");


        SpawnGridBackgroundBlock(); // Call the method to spawn background blocks
        //CreateGrid(); // Call the method to create the grid and place pieces

        //timescale will be 1
        //Time.timeScale = 0.2f;

        StartCoroutine(EmojiLoading()); // Start the emoji loading coroutine

        currentTime = levelData.timeLimit;
        currentMoves = levelData.movesCount;
        currentTarget1 = levelData.target1Count;
        currentTarget2 = levelData.target2Count;



        UpdateUI();

        StartTimer();

        /*//ability start value
        Ability_bombCurrentAmount = Ability_bombStartAmount;
        Ability_colorBombCurrentAmount = Ability_colorBombStartAmount;
        Ability_extraMovesCurrentAmount = Ability_extraMovesStartAmount;*/



        if (levelData == null)
        {
            Debug.LogError("LevelData not found in GridManager!");
            return;
        }

        // Initialize counts from LevelData
        currentTarget1Count = levelData.target1Count;
        currentTarget2Count = levelData.target2Count;

        // Assign sprites based on LevelData piece types
        target1Image.sprite = GetSpriteForPiece(levelData.target1Piece);
        target2Image.sprite = GetSpriteForPiece(levelData.target2Piece);

        UpdateUI();


        LoadPlayerAbilities();



    }

    private void Awake()
    {
        if (targetImage != null)
            targetImage.transform.localScale = Vector3.zero; // Start hidden
    }

    private void UpdateUI()
    {
        movesCountText.text = currentMoves.ToString();

        //ability UI
        Ability_bombCountText.text = Ability_bombCurrentAmount.ToString();
        Ability_ColorBombCountText.text = Ability_colorBombCurrentAmount.ToString();
        Ability_extraMovesCountText.text = Ability_extraMovesCurrentAmount.ToString();

        if (target1CountText != null)
            target1CountText.text = currentTarget1Count.ToString();
        if (target2CountText != null)
            target2CountText.text = currentTarget2Count.ToString();

        


    }

    private void Update()
    {
        if (!isTimerRunning)
        {
            return;
        }

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            isTimerRunning = false;
            OnTimeUp();
        }

        UpdateTimeText();

        //if currentTime is less than or equal to 0, then call a function name GameOver()
        if (currentTime <= 0)
        {
            //GameOver();
            StartCoroutine(GameOver());
            Debug.Log("Game Over! Time is up.");
            // You can call a GameOver function here if needed
        }

        if(currentMoves <= 0)
        {
            StartCoroutine(GameOver());
            Debug.Log("Game Over! No moves left.");
            // You can call a GameOver function here if needed
        }

        if (currentTarget1Count <= 0 && currentTarget2Count <= 0)
        {
            // You can call a function to handle level completion here
            Debug.Log("Level Completed!");
            StartCoroutine(GameOver());
        }


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


    void LoadLevel()
    {
        // Safety check: Ensure we have level data assigned
        if (levelDatas == null || levelDatas.Length == 0)
        {
            Debug.LogError("No LevelData assigned in GridManager!");
            return;
        }

        // Load the selected level index from PlayerPrefs (default to 0)
        currentLevelIndex = PlayerPrefs.GetInt(SelectedLevelIndexKey, 0);

        // Clamp the index to ensure it's valid
        if (currentLevelIndex < 0 || currentLevelIndex >= levelDatas.Length)
        {
            Debug.LogWarning($"Invalid level index ({currentLevelIndex}). Resetting to 0.");
            currentLevelIndex = 0;
        }

        // Assign the selected LevelData
        levelData = levelDatas[currentLevelIndex];

        // Recreate the grid with the new LevelData
        CreateGrid();

        

    }

    void LoadPlayerAbilities()
    {
        //savePath = 

        // Load player abilities from the JSON file
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);
            Ability_bombCurrentAmount = playerData.PlayerBombAbilityCount;
            Ability_colorBombCurrentAmount = playerData.PlayerColorBombAbilityCount;
            Ability_extraMovesCurrentAmount = playerData.PlayerExtraMoveAbilityCount;
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("Save file not found. Using default ability values.");
        }
    }


    // Method to save new ability counts and update the UI. just change the values of ability counts.
    void SaveNewAbilityCounts(int bombCount, int colorBombCount, int extraMovesCount)
    {
        PlayerDataManager.Instance.SetPlayerBombAbilityCount(bombCount);
        PlayerDataManager.Instance.SetPlayerColorBombAbilityCount(colorBombCount);
        PlayerDataManager.Instance.SetPlayerExtraMoveAbilityCount(extraMovesCount);
        PlayerDataManager.Instance.SavePlayerData(); // Save the updated player data to the JSON file

    }



    IEnumerator GameOver()
    {
        //wait for 1 second
        yield return new WaitForSeconds(1f);

        //gameOverText will be = level + currentLevelIndex + 1
        gameOverText.text = "Level :" + (currentLevelIndex + 1);
        level_Count.text = (currentLevelIndex + 1).ToString(); // Update level count text

        // Handle game over logic here
        // For example, show a game over screen or reset the game
        Debug.Log("Game Over! You can implement your game over logic here.");
        isTimerRunning = false; // Stop the timer
        canControl = false; // Disable player controls
        //game over panel will be shown using do tweening
        GameOverPanel.SetActive(true);
        GameOverPanel.transform.localScale = Vector3.zero; // Start from scale 0
        GameOverPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack); // Scale to normal size
        // Optionally, you can also reset the game state or show options to restart or exit
        // Reset the grid and pieces
        //CalculateStarAndShow();
        //delay CalculateStarAndShow() for 0.5 seconds
        Invoke("CalculateStarAndShow", 0.5f);

        

        SaveNewAbilityCounts(Ability_bombCurrentAmount, Ability_colorBombCurrentAmount, Ability_extraMovesCurrentAmount);
        
    }

    public void CalculateStarAndShow()
    {
        //if  level datas Target1Count and Target2Count are 0, then no stars and no XP
        if (levelData.target1Count == 0 && levelData.target2Count == 0)
        {
            stars = 0; // No stars
            XP = 0; // No XP
            Debug.Log("No stars and no XP because targets are 0.");
        }
        // Calculate stars based on moves and time left. more moves and move time left, more stars

        else if (currentMoves > 0 && currentTime > 0)
        {
            if (currentMoves >= levelData.movesCount * 0.25f && currentTime >= levelData.timeLimit * 0.25f)
            {
                stars = 3; // Full stars
                Debug.Log("3 stars and 100XP earned!");

                //calculate XP based on moves left multiplied by left times
                XP = Mathf.FloorToInt((currentMoves / (float)levelData.movesCount) * 100) + Mathf.FloorToInt((currentTime / (float)levelData.timeLimit) * 100);
                
            }
            else if (currentMoves >= levelData.movesCount * 0.2f && currentTime >= levelData.timeLimit * 0.2f)
            {
                stars = 2; // Two stars
                Debug.Log("2 stars and 50 XP earned!");
                XP = Mathf.FloorToInt((currentMoves / (float)levelData.movesCount) * 100) + Mathf.FloorToInt((currentTime / (float)levelData.timeLimit) * 100);
            }
            else
            {
                stars = 1; // One star
                Debug.Log("1 star and 20 XP earned!");
                XP = Mathf.FloorToInt((currentMoves / (float)levelData.movesCount) * 100) + Mathf.FloorToInt((currentTime / (float)levelData.timeLimit) * 100);
            }
        }




        // Show the stars UI. By default, all Glowing stars are hidden and normal stars are shown
        for (int i = 0; i < 3; i++)
        {
            if (i < stars)
            {
                glowStars[i].SetActive(true); // Show glowing stars
                normalStars[i].SetActive(false); // Hide normal stars
            }
            else
            {
                glowStars[i].SetActive(false); // Hide glowing stars
                normalStars[i].SetActive(true); // Show normal stars
                Debug.Log("No stars earned for star index: " + i);
            }
        }

        // Update XP UI
        xpAmount.text = XP.ToString();
        Debug.Log("Stars: " + stars + ", XP: " + XP);

        //send star and xp data to PlayerDataManager
        SendStarXpDataToPlayerDataManager(currentLevelIndex + 1, 0, stars, XP);
        
        //PlayerDataManager.Instance.SendLeaderboardScore(stars, XP); // Send the score to the leaderboard

    }

    void SendStarXpDataToPlayerDataManager(int levelId, int lockedValue, int stars, int xp)
    {

        PlayerDataManager.Instance.SetLevelStars(levelId, stars, xp);
        PlayerDataManager.Instance.SendXP(levelId, xp);

        PlayerDataManager.Instance.SetAllData(currentLevelIndex + 2, 0, 0, 0);// Set the next level data to 0 stars and 0 XP, and unlock it
        PlayerDataManager.Instance.SetCurrentLevel(currentLevelIndex + 2);

        //PlayerDataManager.Instance.SetLevelLocked(currentLevelIndex + 2, 0); // Unlock the next level (currentLevelIndex + 2 because levels are 1-based in PlayerDataManager)


        PlayerDataManager.Instance.SavePlayerData(); // Save the updated player data to the JSON file

        

    }
    

    

    public void BackToMainMenu()
    {
        // Load the main menu scene
        StartCoroutine(EmojiLoading_2());

        PlayerDataManager.Instance.GetCurrentLevel(); // Initialize current level after creating new player
        
    }


    IEnumerator EmojiLoading()
    {
        RectTransform emojiRect = EmojisImage.GetComponent<RectTransform>();

        canControl = false; // Disable player controls during loading
        // Move EmojisImage into view (Y: -1250 to 2500)
        yield return emojiRect.DOAnchorPosY(2500f, 1f).SetEase(Ease.InOutQuad).WaitForCompletion();
        canControl = true; // Re-enable player controls after loading
    }

    IEnumerator EmojiLoading_2()
    {
        RectTransform emojiRect = EmojisImage.GetComponent<RectTransform>();
        // Move EmojisImage into view (Y: 2500 to -1250)
        canControl = false;
        yield return emojiRect.DOAnchorPosY(-1250f, 1f).SetEase(Ease.InOutQuad).WaitForCompletion();


        SceneManager.LoadScene("MainMenu");
    }


    #endregion

    #region "Grid System"


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


    public void UnregisterPiece(GameObject piece, int x, int y)
    {
        if (IsBlocked(x, y))
        {
            //Debug.LogWarning($"Trying to unregister piece at blocked cell ({x},{y})");
            return;
        }
        // Clear the grid array at the specified position
        grid[x, y] = null;
        // Optionally clear the pieces array if you want
        int index = x + y * levelData.gridWidth;
        if (index >= 0 && index < pieces.Length)
        {
            pieces[index] = null;
        }
    }

    public void OnBombButtonClick()
    {
        isPlacingBomb = true;
    }

    public void OnColorButtonClick()
    {
        isPlacingColor = true;
    }

    #endregion

    #region "Game Management"

    private void StartTimer()
    {
        isTimerRunning = true;
        UpdateTimeText();
    }

    private void UpdateTimeText()
    {
        int min = Mathf.FloorToInt(currentTime / 60);
        int sec = Mathf.FloorToInt(currentTime % 60);

        timeText.text = string.Format("{0:00}:{1:00}", min, sec);
    }

    

    public void DeductTarget1(int amount = 1)
    {
        currentTarget1 -= amount;
        if (currentTarget1 < 0)
        {
            currentTarget1 = 0;
            
        }
        UpdateUI();
    }

    public void DeductTarget2(int amount = 1)
    {
        currentTarget2 -= amount;
        if (currentTarget2 < 0)
        {
            currentTarget2 = 0;
            
        }
        UpdateUI();
    }

    public void DeductMove(int amount = 1)
    {
        currentMoves -= amount;
        if (currentMoves < 0)
        {
            currentMoves = 0;
            
        }
        UpdateUI();
    }

    

    private void OnTimeUp()
    {
        Debug.Log("Time is up!");
        // maybe add fail screen logic here or something
    }

    public void ResetUI()
    {
        currentTime = levelData.timeLimit;
        currentMoves = levelData.movesCount;
        currentTarget1 = levelData.target1Count;
        currentTarget2 = levelData.target2Count;



        UpdateUI();
        StartTimer();
    }


    //ability methods
    public void AddAbility_Bomb(int amount = 1)
    {
        Ability_bombCurrentAmount += amount;
        if (Ability_bombCurrentAmount < 0)
        {
            Ability_bombCurrentAmount = 0;
        }
        UpdateUI();
    }

    public void DeductAbility_Bomb(int amount = 1)
    {
        Ability_bombCurrentAmount -= amount;
        if (Ability_bombCurrentAmount < 0)
        {
            Ability_bombCurrentAmount = 0;
            ItemWarningPanel();
        }
        UpdateUI();
    }

    public void AddAbility_ColorBomb(int amount = 1)
    {
        Ability_colorBombCurrentAmount += amount;
        if (Ability_colorBombCurrentAmount < 0)
        {
            Ability_colorBombCurrentAmount = 0;

        }
        UpdateUI();
    }

    public void DeductAbility_ColorBomb(int amount = 1)
    {
        Ability_colorBombCurrentAmount -= amount;
        if (Ability_colorBombCurrentAmount < 0)
        {
            Ability_colorBombCurrentAmount = 0;
            ItemWarningPanel();
        }
        UpdateUI();
    }

    public void AddAbility_ExtraMoves(int amount = 1)
    {
        Ability_extraMovesCurrentAmount += amount;
        if (Ability_extraMovesCurrentAmount < 0)
        {
            Ability_extraMovesCurrentAmount = 0;

        }
        UpdateUI();
    }

    public void DeductAbility_ExtraMoves(int amount = 1)
    {
        Ability_extraMovesCurrentAmount -= amount;
        if (Ability_extraMovesCurrentAmount < 0)
        {
            Ability_extraMovesCurrentAmount = 0;
            ItemWarningPanel();
        }
        UpdateUI();
    }


    public void ItemWarningPanel()
    {
        // Show the item warning panel
        itemWarningPanel.SetActive(true);
        /*itemWarningPanel.transform.localScale = Vector3.zero; // Start from scale 0
        itemWarningPanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack); // Scale to normal size*/
    }




    #endregion

    #region "Target Management"

    private Sprite GetSpriteForPiece(PieceType type)
    {
        switch (type)
        {
            case PieceType.Smiling_Face: return smilingFaceSprite;
            case PieceType.Smiling_Face_with_Tear: return smilingFaceWithTearSprite;
            case PieceType.Angry_Face: return angryFaceSprite;
            case PieceType.Freeze_Face: return laughingFaceSprite;
            case PieceType.SunGlass_Face: return smilingFaceWithHeartEyesSprite;
            case PieceType.Jumbo_Angry: return sleepingFaceSprite;
            case PieceType.Surprised_Face: return surprisedFaceSprite;
            case PieceType.Sad_Face: return cryingFaceSprite;
            default: return null;
        }
    }


    // Call this when a piece is matched
    public void DeductTarget(PieceType type)
    {
        if (type == levelData.target1Piece)
        {
            currentTarget1Count = Mathf.Max(0, currentTarget1Count - 1);
        }
        else if (type == levelData.target2Piece)
        {
            currentTarget2Count = Mathf.Max(0, currentTarget2Count - 1);
        }

        UpdateUI();
    }

    // Individual functions for each PieceType (optional)
    public void Smiling_Face() => DeductTarget(PieceType.Smiling_Face);
    public void Smiling_Face_with_Tear() => DeductTarget(PieceType.Smiling_Face_with_Tear);
    public void Angry_Face() => DeductTarget(PieceType.Angry_Face);
    public void Laughing_Face() => DeductTarget(PieceType.Freeze_Face);
    public void Smiling_Face_With_Heart_Eyes() => DeductTarget(PieceType.SunGlass_Face);
    public void Sleeping_Face() => DeductTarget(PieceType.Jumbo_Angry);
    public void Surprised_Face() => DeductTarget(PieceType.Surprised_Face);
    public void Crying_Face() => DeductTarget(PieceType.Sad_Face);
    #endregion



    #region "Visual Effects and Sounds"
    public void SpawnHorizontalClear(int y)
    {

        GameObject particle = Instantiate(horizontalClearParticle, new Vector2(levelData.gridWidth / 2f - 0.5f, y), Quaternion.identity);
        particle.transform.SetParent(transform);
        particle.name = "HorizontalClear (" + y + ")";
        Destroy(particle, 1f); // Destroy after 1 second
    }

    public void SpawnVerticalClear(int x)
    {
        GameObject particle = Instantiate(verticalClearParticle, new Vector2(x, levelData.gridHeight / 2f - 0.5f), Quaternion.identity);
        particle.transform.SetParent(transform);
        particle.name = "VerticalClear (" + x + ")";
        Destroy(particle, 1f); // Destroy after 1 second
    }


    //play random sfx sound(Pop_1, Pop_2, Pop_3, Pop_4) from AudioManager
    public void PlayRandomSFX()
    {
        int randomIndex = Random.Range(1, 5); // Random index between 1 and 4
        string sfxName = "Pop_" + randomIndex;
        AudioManager.Instance.PlaySFX(sfxName);
    }


    public void PlayEffect()
    {
        if (sprites.Length == 0 || targetImage == null) return;

        // If a previous tween is running, tween it back to zero immediately
        if (currentSequence != null && currentSequence.IsActive() && currentSequence.IsPlaying())
        {
            currentSequence.Kill();
            targetImage.transform.DOScale(Vector3.zero, scaleDuration * 0.5f).SetEase(Ease.InBack);
        }

        // Pick a random sprite
        SpriteData data = sprites[Random.Range(0, sprites.Length)];
        targetImage.sprite = data.sprite;

        // Reset scale
        targetImage.transform.localScale = Vector3.zero;

        // Start new tween sequence
        currentSequence = DOTween.Sequence();
        currentSequence.Append(targetImage.transform.DOScale(data.targetScale, scaleDuration).SetEase(Ease.OutBack));
        currentSequence.AppendInterval(holdDuration);
        currentSequence.Append(targetImage.transform.DOScale(Vector3.zero, scaleDuration).SetEase(Ease.InBack));
    }

    #endregion

}
