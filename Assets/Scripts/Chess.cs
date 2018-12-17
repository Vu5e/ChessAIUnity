using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chess : MonoBehaviour
{

    //================//
    //   BOARD SETUP  //
    //================//


    // Pieces values
    public const int EMPTY = 0;
    public const int PAWN = 3;
    public const int BISHOP = 9;
    public const int KNIGHT = 10;
    public const int ROOK = 15;
    public const int QUEEN = 27;
    public const int KING = 10000;

    // Represents the board
    public int[,] board;

    public GameObject tilePrefab;
    private Tile[,] tiles;
    
    private bool colorsNotUpdated;

    // Sets up the game
    void Start()
    {
        Screen.fullScreen = false;
        ResetBoard();
    }

    public void ResetBoard()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        from = null;
        SetupBoard();
        CreateTiles();
        UpdateTiles();
        value = 0;
        ChangeDifficulty();
    }

    // Creates the inital board
    void SetupBoard()
    {
        board = new int[8, 8];
        int[] boardSetup = { ROOK, KNIGHT, BISHOP, QUEEN, KING, BISHOP, KNIGHT, ROOK };
        for (int x = 0; x < 8; x++)
        {
            board[x, 1] = -PAWN;
            board[x, 6] = PAWN;
            board[x, 0] = -boardSetup[x];
            board[x, 7] = boardSetup[x];
        }
    }

    // Creates the tiles
    void CreateTiles()
    {
        tiles = new Tile[8, 8];
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                GameObject tile = Instantiate<GameObject>(tilePrefab);
                tile.transform.SetParent(transform);
                tile.transform.localPosition = new Vector2(x, y);
                tiles[x, y] = tile.GetComponent<Tile>();
                Tile tileScript = tile.GetComponent<Tile>();
                tileScript.Initialize(new BoardPosition(x, y), this);

            }
        }
    }

    // Updates every tile to it's corresponding piece on the given board
    public void UpdateTiles()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                tiles[x, y].SetPiece(board[x, y]);
            }
        }
    }

    // Updates the tile at the given position
    public void UpdateTile(BoardPosition position)
    {
        tiles[position.x, position.y].SetPiece(GetPiece(position));
    }

    // Changes the color of a selected tile based on whether it was selected/unselected
    public void TileSelected(BoardPosition position, bool selected)
    {
        tiles[position.x, position.y].Selected(selected);
    }

    public void UpdateColors()
    {
        if (tiles == null)
        {
            return;
        }
        UpdateTiles();
    }

    //================//
    //   GAME LOGIC   //
    //================//

    const int PLAYER = -1;
    const int NEUTRAL = 0;
    const int AI = 1;

    // The piece the player has selected to move.
    // The piece goes FROM here TO somewhere else
    private BoardPosition from;

    // Handles the given position being clicked
    public void PositionClicked(BoardPosition position)
    {
        if (GameOver())
        {
            return;
        }

        if (from == null && WhatTeam(position) == PLAYER)
        {
            TileSelected(position, true);
            from = position;
        }
        else if (from != null)
        {
            MovePosition move = ValidMove(new MovePosition(from, position, GetPiece(position)));
            if (move != null)
            {
                CommitMove(move);
                if (!GameOver())
                {
                    StartCoroutine(AITurn());
                }
            }
            TileSelected(from, false);
            from = null;
        }
    }

    // Sets a move and updates it's tiles
    void CommitMove(MovePosition move)
    {
        SetMove(move);
        UpdateTile(move.from);
        UpdateTile(move.to);
    }

    // If the given move is valid, returns that move,
    // Otherwise returns null
    MovePosition ValidMove(MovePosition move)
    {
        foreach (MovePosition m in MovesFromposition(move.from))
        {
            if (move.Equals(m))
            {
                return m;
            }
        }
        return null;
    }

    // Returns every move the given team can make
    List<MovePosition> AllMoves(int team)
    {
        List<MovePosition> moves = new List<MovePosition>();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                BoardPosition from = new BoardPosition(x, y);
                if (WhatTeam(from) == team)
                {
                    moves.AddRange(MovesFromposition(from));
                }
            }
        }
        return moves;
    }

    // Returns all the moves from the given position
    List<MovePosition> MovesFromposition(BoardPosition from)
    {
        int piece = Mathf.Abs(GetPiece(from));
        int team = WhatTeam(from);
        if (piece == EMPTY)
        {
            return new List<MovePosition>();
        }
        else if (piece == PAWN)
        {
            return PawnMoves(from, team);
        }
        else if (piece == BISHOP)
        {
            return BishopMoves(from, team);
        }
        else if (piece == KNIGHT)
        {
            return KnightMoves(from, team);
        }
        else if (piece == ROOK)
        {
            return RookMoves(from, team);
        }
        else if (piece == QUEEN)
        {
            return QueenMoves(from, team);
        }
        else // piece == KING
        {
            return KingMoves(from, team);
        }
    }

    // Changes the board state to reflect the given move being played
    public void SetMove(MovePosition move)
    {
        if (move.HasSpecialRule())
        {
            int team = WhatTeam(move.from);
            SetPiece(move.to, QUEEN * team);
            SetPiece(move.from, EMPTY);
            value += (QUEEN - PAWN) * team;
            value -= move.removedPiece;
        }
        else
        {
            int piece = GetPiece(move.from);
            SetPiece(move.from, EMPTY);
            SetPiece(move.to, piece);
            value -= move.removedPiece;
        }
    }

    // Changes the board state to reflect the given move being undone
    public void UndoMove(MovePosition move)
    {
        if (move.HasSpecialRule())
        {
            int team = WhatTeam(move.to);
            SetPiece(move.from, PAWN * team);
            SetPiece(move.to, move.removedPiece);
            value -= (QUEEN - PAWN) * team;
            value += move.removedPiece;
        }
        else
        {
            SetPiece(move.from, GetPiece(move.to));
            SetPiece(move.to, move.removedPiece);
            value += move.removedPiece;
        }
    }

    //=========================//
    //   BASIC HELPER METHODS  //
    //=========================//

    // Returns which team a given position belongs to
    public int WhatTeam(BoardPosition position)
    {
        return WhatTeam(GetPiece(position));
    }

    // Returns which team a given int belongs to
    public int WhatTeam(int piece)
    {
        return piece == 0 ? NEUTRAL : (piece < 0 ? PLAYER : AI);
    }

    // Returns the piece at the given position
    public int GetPiece(BoardPosition position)
    {
        return board[position.x, position.y];
    }

    // Sets the given piece at the given position
    public void SetPiece(BoardPosition position, int piece)
    {
        board[position.x, position.y] = piece;
    }

    // Is the piece at the given position an enemy of the given team?
    bool IsEnemy(BoardPosition position, int team)
    {
        return WhatTeam(position) + team == 0;
    }

    // Is there no piece at the given position?
    bool IsEmpty(BoardPosition position)
    {
        return WhatTeam(position) == NEUTRAL;
    }

    // Is the given position on the map?
    bool IsOnMap(BoardPosition position)
    {
        return position.x >= 0 && position.x < 8 && position.y >= 0 && position.y < 8;
    }

    // Has someone won the game?
    public bool GameOver()
    {
        return Winner() != NEUTRAL;
    }

    // Returns the current winner.
    // If a king has not been taken, NEUTRAL is returned
    public int Winner()
    {
        if (Mathf.Abs(value) > WINSCORE)
        {
            return value < 0 ? PLAYER : AI;
        }
        else
        {
            return NEUTRAL;
        }
    }


    //===============//
    // PIECE MOVE    //
    //===============//

    // A list of moves that the pawn at the given position can make
    List<MovePosition> PawnMoves(BoardPosition from, int team)
    {
        int x = from.x;
        int y = from.y;
        List<MovePosition> moves = new List<MovePosition>();
        BoardPosition to = new BoardPosition(x, y - team);
        if (IsEmpty(to))
        {
            if (to.y % 7 == 0)
            {
                moves.Add(new MovePosition(from, to, EMPTY, "PROMOTION"));
            }
            else
            {
                AddMoveIf(from, to, moves, true);
            }
            if ((y + team) % 7 == 0)
            {
                to = new BoardPosition(x, y - team * 2);
                AddMoveIf(from, to, moves, IsEmpty(to));
            }
        }
        BoardPosition to1 = new BoardPosition(x - 1, y - team);
        BoardPosition to2 = new BoardPosition(x + 1, y - team);
        if ((y - team) % 7 == 0)
        {
            if (x != 0 && IsEnemy(to1, team))
            {
                moves.Add(new MovePosition(from, to1, GetPiece(to1), "PROMOTION"));
            }
            if (x != 7 && IsEnemy(to2, team))
            {
                moves.Add(new MovePosition(from, to2, GetPiece(to2), "PROMOTION"));
            }
        }
        else
        {
            AddMoveIf(from, to1, moves, x != 0 && IsEnemy(to1, team));
            AddMoveIf(from, to2, moves, x != 7 && IsEnemy(to2, team));
        }
        return moves;
    }

    // A list of moves that the knight at the given position can make
    List<MovePosition> KnightMoves(BoardPosition from, int team)
    {
        List<MovePosition> moves = new List<MovePosition>();
        int x = from.x;
        int y = from.y;
        for (int i = -2; i <= 2; i++)
        {
            for (int j = 3 - Mathf.Abs(i); j > -3; j -= 2)
            {
                if (i == 0 || j == 0)
                    continue;
                BoardPosition to = new BoardPosition(x + i, y + j);
                AddMoveIf(from, to, moves, IsOnMap(to) && WhatTeam(to) != team);
            }
        }
        return moves;
    }

    // A list of moves that the bishop at the given position can make
    List<MovePosition> BishopMoves(BoardPosition from, int team)
    {
        List<MovePosition> moves = new List<MovePosition>();
        AddMovesInDirection(1, 1, moves, from, team);
        AddMovesInDirection(1, -1, moves, from, team);
        AddMovesInDirection(-1, -1, moves, from, team);
        AddMovesInDirection(-1, 1, moves, from, team);
        return moves;
    }

    // A list of moves that the rook at the given position can make
    List<MovePosition> RookMoves(BoardPosition from, int team)
    {
        List<MovePosition> moves = new List<MovePosition>();
        AddMovesInDirection(0, 1, moves, from, team);
        AddMovesInDirection(1, 0, moves, from, team);
        AddMovesInDirection(0, -1, moves, from, team);
        AddMovesInDirection(-1, 0, moves, from, team);
        return moves;
    }

    // A list of moves that the queen at the given position can make
    List<MovePosition> QueenMoves(BoardPosition from, int team)
    {
        List<MovePosition> moves = BishopMoves(from, team);
        moves.AddRange(RookMoves(from, team));
        return moves;
    }

    // A list of moves that the king at the given position can make
    List<MovePosition> KingMoves(BoardPosition from, int team)
    {
        List<MovePosition> moves = new List<MovePosition>();
        int x = from.x;
        int y = from.y;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;
                BoardPosition to = new BoardPosition(x + i, y + j);
                AddMoveIf(from, to, moves, (IsOnMap(to) && WhatTeam(to) != team));
            }
        }
        return moves;
    }

    // Adds every empty/enemy space in the given (x,y) direction as a move
    void AddMovesInDirection(int xDir, int yDir, List<MovePosition> moves, BoardPosition from, int team)
    {
        BoardPosition to = new BoardPosition(from.x + xDir, from.y + yDir);
        while (IsOnMap(to))
        {
            if (!IsEmpty(to))
            {
                AddMoveIf(from, to, moves, IsEnemy(to, team));
                return;
            }
            AddMoveIf(from, to, moves, true);
            to = new BoardPosition(to.x + xDir, to.y + yDir);
        }
    }

    // Adds a move to moves if the given condition is true
    void AddMoveIf(BoardPosition from, BoardPosition to, List<MovePosition> moves, bool condition)
    {
        if (condition)
        {
            moves.Add(new MovePosition(from, to, GetPiece(to)));
        }
    }

    //===============//
    //   AI METHODS  //
    //===============//

    // The value of the board
    int value;

    public Slider difficultySlider;
    private int turnLookahead;

    public float switchOdds;

    const int WINSCORE = 5000;
    const int MINALPHA = -5001;

    // Changes the turn lookahead based on the difficulty slider
    public void ChangeDifficulty()
    {
        //turnLookahead = (int)Mathf.Clamp(difficultySlider.value, 1, 5);
        turnLookahead = (int)difficultySlider.value;
    }

    // Handles the AI's turn
    public IEnumerator AITurn()
    {
        yield return new WaitForSeconds(0);
        CommitMove(GetAIMove());
    }

    // Returns the next move for the AI
    public MovePosition GetAIMove()
    {
        MovePosition bestMoves = null;
        int alpha = MINALPHA;
        foreach (MovePosition move in AllMoves(AI))
        {
            SetMove(move);
            int score = GetMoveValue(move, AI, alpha, turnLookahead - 1);
            UndoMove(move);
            if (score > alpha || (score == alpha && Random.value < switchOdds))
            {
                alpha = score;
                bestMoves = move;
            }
        }
        return bestMoves;
    }

    // Returns the heuristic value of the given move
    int GetMoveValue(MovePosition move, int team, int alpha, int beta)
    {
        if (beta <= 0)
            return value;
        if (GameOver())
            return Winner() * WINSCORE;
        int enemyBestMove = MINALPHA * -team;
        foreach (MovePosition nextMove in AllMoves(-team))
        {
            SetMove(nextMove);
            int score = GetMoveValue(nextMove, -team, alpha, beta - 1);
            UndoMove(nextMove);
            if (team == AI && score < alpha)
                return alpha - 1;
            enemyBestMove = team == AI ?
                Mathf.Min(enemyBestMove, score) :
                Mathf.Max(enemyBestMove, score);
        }
        return enemyBestMove;
    }

}

// Positions represent BoardPosition
public class BoardPosition
{
    public int x;
    public int y;

    public BoardPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool Equals(BoardPosition position)
    {
        return this.x == position.x && this.y == position.y;
    }
}

// Moves represent a piece moving FROM one position TO another position
public class MovePosition
{
    public BoardPosition from;
    public BoardPosition to;

    // Remembers the piece that will be destroyed if this move goes through
    public int removedPiece;

    // escribes a special rule
    public string rule;

    public MovePosition(BoardPosition from, BoardPosition to, int removedPiece)
    {
        this.from = from;
        this.to = to;
        this.removedPiece = removedPiece;
    }

    public MovePosition(BoardPosition from, BoardPosition to, int removedPiece, string rule)
    {
        this.from = from;
        this.to = to;
        this.removedPiece = removedPiece;
        this.rule = rule;
    }

    // Does this move contain a rule exception?
    public bool HasSpecialRule()
    {
        return rule != null;
    }

    public bool Equals(MovePosition move)
    {
        return this.from.Equals(move.from) && this.to.Equals(move.to);
    }
}