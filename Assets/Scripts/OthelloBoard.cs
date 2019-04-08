using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OthelloBoard : MonoBehaviour
{
    enum PieceColor { White, Black };

    public Piece[,] pieces = new Piece[8, 8];
    public GameObject piecePrefab;

    private Vector3 boardOffset = new Vector3(-4.0f, 1.5f, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    public bool isWhiteTurn;

    private Vector2 mouseOver;

    public int whiteScore;
    public int blackScore;

    public int numPossible;

    public bool gameOver;

    // Start is called before the first frame update
    void Start()
    {
        GenerateBoard(pieces);
        GetScore(pieces);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
            List<Move> possibleMoves = getPossibleMoves(pieces, isWhiteTurn);
            numPossible = possibleMoves.Count;
            if (possibleMoves.Count == 0)
            {
                EndTurn(true, pieces);
            }

            if (isWhiteTurn)
            {
                UpdateMouseOver();

                int x = (int)mouseOver.x;
                int y = (int)mouseOver.y;

                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 selectedSquare = SelectSquare(x, y, pieces);
                    TryMove(pieces, isWhiteTurn, selectedSquare);
                }
            }
            else if (!isWhiteTurn)
            {
                StartCoroutine(Wait(possibleMoves, pieces, isWhiteTurn));
            }
        }  
    }

    IEnumerator Wait(List<Move> possibleMoves, Piece[,] board, bool isItWhiteTurn)
    {
        yield return new WaitForSeconds(.5f);
        Move m = getMaxScoreMove(possibleMoves);
        int x = (int)m.getSquare().x;
        int y = (int)m.getSquare().y;
        Vector2 selectedSquare = SelectSquare(x, y, board);
        TryMove(board, isItWhiteTurn, selectedSquare);
    }

    private List<Move> getPossibleMoves(Piece[,] board, bool isItWhiteTurn)
    {
        List<Move> res = new List<Move>();
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                List<Vector2> tilesToFlip = new List<Vector2>();
                Vector2 square = new Vector2(i, j);
                if (IsValidMove(i, j, tilesToFlip, board, isItWhiteTurn))
                {
                    Move m = new Move(tilesToFlip, square);
                    res.Add(m);
                }
            }
        }
        return res;
    }

    private void TryMove(Piece[,] board, bool isItWhiteTurn, Vector2 chosenSquare)
    {
        int x = (int)chosenSquare.x;
        int y = (int)chosenSquare.y;
        List<Vector2> tilesToFlip = new List<Vector2>();
        if(IsValidMove(x, y, tilesToFlip, board, isItWhiteTurn))
        {
            GeneratePiece(x, y, isItWhiteTurn ? PieceColor.White : PieceColor.Black, board);
            FlipTiles(tilesToFlip, board);
            EndTurn(false, board);
        }       
    }

    private void MakeMove(Piece[,] board, bool isItWhiteTurn, Vector2 chosenSquare)
    {
        int x = (int)chosenSquare.x;
        int y = (int)chosenSquare.y;
        List<Vector2> tilesToFlip = new List<Vector2>();
        if (IsValidMove(x, y, tilesToFlip, board, isItWhiteTurn))
        {
            GeneratePiece(x, y, isItWhiteTurn ? PieceColor.White : PieceColor.Black, board);
            FlipTiles(tilesToFlip, board);
            EndTurn(false, board);
        }
    }

    private void FlipTiles(List<Vector2> tilesToFlip, Piece[,] board)
    {
        for(int i = 0; i < tilesToFlip.Count; i++)
        {
            Vector2 squareToFlip = tilesToFlip[i];
            int x = (int)squareToFlip.x;
            int y = (int)squareToFlip.y;
            FlipPiece(x, y, board);
        }
    }

    //Algorithm logic found on https://inventwithpython.com/chapter15.html
    private bool IsValidMove(int selectedX, int selectedY, List<Vector2> tilesToFlip, Piece[,] board, bool isItWhiteTurn)
    {
        if (board[selectedX, selectedY] || !isOnBoard(selectedX, selectedY, board))
        {
            return false;
        }

        Vector2[] offsets = { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 1) };

        for (int direction = 0; direction < offsets.Length; direction++)
        {
            int x = selectedX;
            int y = selectedY;
            int xOffset = (int)offsets[direction].x;
            int yOffset = (int)offsets[direction].y;

            x = x + xOffset;
            y = y + yOffset;
            if (isOnBoard(x, y, board) && board[x, y] && board[x, y].isWhite != isItWhiteTurn)
            {
                //We know there is an opponent piece in the current direction that is next to our player
                x = x + xOffset;
                y = y + yOffset;

                //check the next square after that to see if it is on the board
                if (!isOnBoard(x, y, board))
                {
                    continue;
                }

                //while the pieces are not the same color
                while (board[x, y] && board[x, y].isWhite != isItWhiteTurn)
                {
                    x = x + xOffset;
                    y = y + yOffset;
                    if (!isOnBoard(x, y, board))
                    {
                        break;
                    }
                }

                if (!isOnBoard(x, y, board))
                {
                    continue;
                }
                if (board[x, y] && board[x, y].isWhite == isItWhiteTurn)
                {
                    while (true)
                    {
                        x -= xOffset;
                        y -= yOffset;
                        if (x == selectedX && y == selectedY)
                        {
                            break;
                        }
                        Vector2 toFlip = new Vector2(x, y);
                        tilesToFlip.Add(toFlip);
                    }
                }
            }
        }

        board[selectedX, selectedY] = null;
        if (tilesToFlip.Count == 0)
        {
            return false;
        }
        return true;
    }


    private bool isOnBoard(int x, int y, Piece[,] board)
    {
        return x >= 0 && x < board.GetLength(0) && y >= 0 && y < board.GetLength(0);
    }

    private void EndTurn(bool wasPassed, Piece[,] board)
    {
        isWhiteTurn = !isWhiteTurn;
        GetScore(board);

        if (wasPassed)
        {
            List<Move> possibleMoves = getPossibleMoves(board, isWhiteTurn);
            if (possibleMoves.Count == 0) {
                EndGame();
            }
        }
    }

    private Vector2 GetScore(Piece[,] board)
    {
        Vector2 scores = new Vector2();
        int white = 0;
        int black = 0;
        for(int i = 0; i < board.GetLength(0); i++)
        {
            for(int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i,j] && board[i, j].isWhite)
                {
                    white++;
                } else if(board[i,j] && !board[i, j].isWhite)
                {
                    black++;
                }
            }
        }
        whiteScore = white;
        blackScore = black;

        scores.x = whiteScore;
        scores.y = blackScore;

        if(whiteScore == 0 || blackScore == 0)
        {
            EndGame();
            return scores;
        }
        return scores;
    }

    private void EndGame()
    {
        gameOver = true;
    }

    private void UpdateMouseOver()
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera.");
            return;
        }

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
        {
            mouseOver.x = (int)(hit.point.x - boardOffset.x);
            mouseOver.y = (int)(hit.point.z - boardOffset.z);
        }
        else
        {
            mouseOver.x = -1;
            mouseOver.y = -1;
        }
    }

    private Vector2 SelectSquare(int x, int y, Piece[,] board)
    {
        //Out of bounds
        if(x < 0 || x >= board.GetLength(0) || y < 0 || y >= board.GetLength(1))
        {
            return new Vector2();
        }

        Vector2 chosenSquare = new Vector2(x, y);
        return chosenSquare;
    }

    private void GenerateBoard(Piece[,] board)
    {
        GeneratePiece(3, 3, PieceColor.White, board);
        GeneratePiece(4, 3, PieceColor.Black, board);
        GeneratePiece(3, 4, PieceColor.Black, board);
        GeneratePiece(4, 4, PieceColor.White, board);
    }

    private void GeneratePiece(int x, int y, PieceColor color, Piece[,] board)
    {
        GameObject go = Instantiate(piecePrefab) as GameObject;
        go.transform.SetParent(transform);
        Piece p = go.GetComponent<Piece>();
        MovePiece(p, x, y);
        if (color == PieceColor.White)
        {
            FlipPiece(p);
            p.isWhite = true;
        }
        else
        {
            p.isWhite = false;
        }

        board[x, y] = p;
    }

    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }
    private void FlipPiece(Piece p)
    {
        p.transform.Rotate(0, 0, 180);
    }

    private void FlipPiece(int x, int y, Piece[,] board)
    {
        board[x, y].isWhite = !board[x, y].isWhite;
        board[x, y].transform.Rotate(0, 0, 180);
    }

    private int heuristic(Piece[,] board, bool isWhiteTurn)
    {
        Vector2 scores = GetScore(board);
        int whiteScore = (int)scores.x;
        int blackScore = (int)scores.y;

        if (isWhiteTurn)
        {
            return whiteScore - blackScore;
        }
        else
        {
            return blackScore - whiteScore;
        }
    }

    private Vector2 minimaxDecision(Piece[,] board, bool isWhiteTurn)
    {
        Vector2 res = new Vector2();
        List<Move> curPossibleMoves = getPossibleMoves(board, isWhiteTurn);
        int numMoves = curPossibleMoves.Count;
        
        if(numMoves == 0)
        {
            res.x = -1;
            res.y = -1;
        }
        else
        {
            //Remember the best move
            int bestMoveVal = System.Int32.MinValue;
            Vector2 bestMove = curPossibleMoves[0].getSquare();

            //Try out every single move
            for(int i = 0; i < numMoves; i++)
            {
                //Apply the move to a new board;
                Piece[,] tmpBoard = CopyBoard(board);

            }
        }
        return res;
    }

    private Piece[,] CopyBoard(Piece[,] source)
    {
        Piece[,] result = new Piece[source.GetLength(0), source.GetLength(1)];

        for(int i = 0; i < source.GetLength(0); i++)
        {
            for(int j = 0; j < source.GetLength(1); j++)
            {
                if(source[i,j] != null)
                {
                    result[i, j] = new Piece(source[i, j]);
                }
            }
        }

        return result;
    }

    private class Move
    {
        List<Vector2> tilesToFlip;
        Vector2 square;

        public Move(List<Vector2> tiles, Vector2 vec)
        {
            tilesToFlip = tiles;
            square = vec;
        }

        public List<Vector2> getTiles()
        {
            return tilesToFlip;
        }

        public int getScore()
        {
            return tilesToFlip.Count;
        }

        public Vector2 getSquare()
        {
            return square;
        }
    }

    private Move getMaxScoreMove(List<Move> possibleMoves)
    {      
        Move res = possibleMoves[0];
        for (int i = 0; i < possibleMoves.Count; i++)
        {
            if(possibleMoves[i].getScore() > res.getScore())
            {
                res = possibleMoves[i];
            }
        }
        return res;
    }

    public class GameState
    {
        Piece[,] pieces;
        bool isWhiteTurn;
    }
}
