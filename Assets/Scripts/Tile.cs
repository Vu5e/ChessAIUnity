using UnityEngine;
using System;

public class Tile : MonoBehaviour
{
    public Color boardColor1, boardColor2, playerColor, aiColor, selectedColor;
    public Color defaultColor;
    
    public SpriteRenderer pieceRenderer;
    
    // Reordering from player King to AI King
    public Sprite[] pieceSprites;
    
    // Convert the value for Sprite Index 
    private int[] pieceValues = {-10000, -27, -15, -10, -9, -3, 0, 3, 9, 10, 15, 27, 10000};

    private BoardPosition position;
    private Chess chessAI;
    
    // For clicking the chess
	void OnMouseDown()
    {
        chessAI.PositionClicked(position);
    }

    // To intialize
    public void Initialize(BoardPosition position, Chess chessAI)
    {
        this.position = position;
        this.chessAI = chessAI;
        defaultColor = (position.x + position.y) % 2 == 0 ? boardColor1 : boardColor2;
        GetComponent<SpriteRenderer>().color = defaultColor;
    }

    // Set piece position
    public void SetPiece(int piece)
    {
        pieceRenderer.sprite = pieceSprites[Array.IndexOf<int>(pieceValues, piece)];
        pieceRenderer.color = piece < 0 ? playerColor : aiColor;
    }

    // Changes color for Selected and Unselected
    public void Selected(bool selected)
    {
        GetComponent<SpriteRenderer>().color = selected ? selectedColor : defaultColor;
    }

}