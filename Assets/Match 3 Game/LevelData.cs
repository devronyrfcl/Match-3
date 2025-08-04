using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BlockedCell
{
    public int x; // X coordinate of the blocked position
    public int y; // Y coordinate of the blocked position
}



[CreateAssetMenu(fileName = "Level Data", menuName = "Epic Loop/Level Data")]

public class LevelData : ScriptableObject
{
    public int gridWidth = 5; // Width of the grid
    public int gridHeight = 10; // Height of the grid
    public int GridSeed = 1; // Speed of the grid pieces falling
    public int SpecialPiecesAmount = 2; //

    public BlockedCell[] blockedCells; //Array of blocked cells in the grid

    
}
