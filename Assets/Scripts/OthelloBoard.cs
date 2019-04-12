using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OthelloBoard : MonoBehaviour
{
    enum PieceColor { White, Black };

    public Piece[,] pieces;
    public GameObject piecePrefab;

    private Vector3 boardOffset = new Vector3(-4.0f, 1.5f, -4.0f);
    private Vector3 pieceOffset = new Vector3(0.5f, 0, 0.5f);

    public bool isWhiteTurn;

    public TextMeshProUGUI blackScoreBoard;
    public TextMeshProUGUI whiteScoreBoard;

    public TextMeshProUGUI gameOverText;

    public TextMeshProUGUI indicator;

    public Button newSoloGameButton;

    public Button newTwoPlayerGameButton;

    public Dropdown levelSelect;

    public Button quitGame;

    public Image gameOverBackground;

    private Vector2 mouseOver;

    public int whiteScore;
    public int blackScore;

    private bool aiFinished;

    public int numPossible;

    public int minimaxLevels = 1;

    public bool gameOver;

    private bool aiGame;

    // Start is called before the first frame update
    void Start()
    {
        gameOver = true;
        quitGame.gameObject.SetActive(false);
        //InitializeGame();
    }

    void InitializeGame()
    {
        if(pieces != null)
            ClearBoard();

        pieces = new Piece[8, 8];
        GenerateBoard(pieces);
        GetScore(pieces);
        System.Random rand = new System.Random();
        isWhiteTurn = rand.Next(2) == 0;
        blackScoreBoard.SetText("");
        whiteScoreBoard.SetText("");
        aiFinished = true;

        if (isWhiteTurn)
            indicator.SetText("White's Turn");
        else
            indicator.SetText("Black's Turn");

        gameOverText.enabled = false;
        gameOverBackground.enabled = false;
        newSoloGameButton.gameObject.SetActive(false);
        newTwoPlayerGameButton.gameObject.SetActive(false);
        levelSelect.gameObject.SetActive(false);
        quitGame.gameObject.SetActive(true);
        gameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
            if (aiGame) { 
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
                else if (!isWhiteTurn && aiFinished)
                {
                    aiFinished = false;
                    StartCoroutine(Wait(possibleMoves, pieces, isWhiteTurn));
                }
            }
            else
            {
                List<Move> possibleMoves = getPossibleMoves(pieces, isWhiteTurn);
                numPossible = possibleMoves.Count;
                if (possibleMoves.Count == 0)
                {
                    EndTurn(true, pieces);
                }

                UpdateMouseOver();

                int x = (int)mouseOver.x;
                int y = (int)mouseOver.y;

                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 selectedSquare = SelectSquare(x, y, pieces);
                    TryMove(pieces, isWhiteTurn, selectedSquare);
                }
            }
        }
    }

    void ClearBoard()
    {
        for(int i = 0; i < pieces.GetLength(0); i++)
        {
            for(int j = 0; j < pieces.GetLength(1); j++)
            {
                if (pieces[i, j])
                {
                    Destroy(pieces[i, j].gameObject);
                }
            }
        }
    }

    public void SelectLevel()
    {
        minimaxLevels = levelSelect.value + 1;
    }

    IEnumerator Wait(List<Move> possibleMoves, Piece[,] board, bool isItWhiteTurn)
    {
        yield return new WaitForSeconds(.5f);
        Move m = MinimaxDecision(board, isItWhiteTurn);
        //Move m = getMaxScoreMove(possibleMoves);
        int x = (int)m.getSquare().x;
        int y = (int)m.getSquare().y;
        Vector2 selectedSquare = SelectSquare(x, y, board);
        TryMove(board, isItWhiteTurn, selectedSquare);
        aiFinished = true;
    }

    public void ButtonNewSoloGame()
    {
        aiGame = true;
        InitializeGame();
    }

    public void ButtonNewTwoPlayerGame()
    {
        aiGame = false;
        InitializeGame();
    }

    public void ButtonEndGame()
    {
        gameOver = true;
        gameOverText.SetText("Game Over.");
        indicator.SetText("");
        blackScoreBoard.SetText("");
        whiteScoreBoard.SetText("");

        gameOverBackground.enabled = true;
        gameOverText.enabled = true;
        newSoloGameButton.gameObject.SetActive(true);
        newTwoPlayerGameButton.gameObject.SetActive(true);
        quitGame.gameObject.SetActive(false);
        levelSelect.gameObject.SetActive(true);
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

    private List<Move> getPossibleMovesTest(char[,] board, bool isItWhiteTurn)
    {
        List<Move> res = new List<Move>();
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                List<Vector2> tilesToFlip = new List<Vector2>();
                Vector2 square = new Vector2(i, j);
                if (IsValidMoveTest(i, j, tilesToFlip, board, isItWhiteTurn))
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
        if (IsValidMove(x, y, tilesToFlip, board, isItWhiteTurn))
        {
            GeneratePiece(x, y, isItWhiteTurn ? PieceColor.White : PieceColor.Black, board);
            FlipTiles(tilesToFlip, board);
            EndTurn(false, board);
        }
    }

    private char[,] MakeMove(char[,] board, bool isItWhiteTurn, Vector2 chosenSquare)
    {
        int x = (int)chosenSquare.x;
        int y = (int)chosenSquare.y;
        List<Vector2> tilesToFlip = new List<Vector2>();
        if (IsValidMoveTest(x, y, tilesToFlip, board, isItWhiteTurn))
        {
            char p = isItWhiteTurn ? 'W' : 'B';
            board[x, y] = p;
            //specify that we are flipping just in the board array, using the true boolean
            FlipTiles(tilesToFlip, board, true);
        }
        return board;
    }

    private void FlipTiles(List<Vector2> tilesToFlip, Piece[,] board)
    {
        for (int i = 0; i < tilesToFlip.Count; i++)
        {
            Vector2 squareToFlip = tilesToFlip[i];
            int x = (int)squareToFlip.x;
            int y = (int)squareToFlip.y;
            FlipPiece(x, y, board);
        }
    }

    private char[,] FlipTiles(List<Vector2> tilesToFlip, char[,] board, bool forMinimax)
    {
        for (int i = 0; i < tilesToFlip.Count; i++)
        {
            Vector2 squareToFlip = tilesToFlip[i];
            int x = (int)squareToFlip.x;
            int y = (int)squareToFlip.y;
            if(board[x,y] == 'W')
            {
                board[x, y] = 'B';
            } else if(board[x,y] == 'B')
            {
                board[x, y] = 'W';
            }
        }
        return board;
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

    private bool IsValidMoveTest(int selectedX, int selectedY, List<Vector2> tilesToFlip, char[,] board, bool isItWhiteTurn)
    {
        if (board[selectedX, selectedY] != '\0' || !isOnBoardTest(selectedX, selectedY, board))
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
            if (isOnBoardTest(x, y, board) && board[x, y] != '\0' && (board[x, y] == 'W') != isItWhiteTurn)
            {
                //We know there is an opponent piece in the current direction that is next to our player
                x = x + xOffset;
                y = y + yOffset;

                //check the next square after that to see if it is on the board
                if (!isOnBoardTest(x, y, board))
                {
                    continue;
                }

                //while the pieces are not the same color
                while (board[x, y] != '\0' && (board[x, y] == 'W') != isItWhiteTurn)
                {
                    x = x + xOffset;
                    y = y + yOffset;
                    if (!isOnBoardTest(x, y, board))
                    {
                        break;
                    }
                }

                if (!isOnBoardTest(x, y, board))
                {
                    continue;
                }
                if (board[x, y] != '\0' && (board[x, y] == 'W') == isItWhiteTurn)
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

        board[selectedX, selectedY] = '\0';
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

    private bool isOnBoardTest(int x, int y, char[,] board)
    {
        return x >= 0 && x < board.GetLength(0) && y >= 0 && y < board.GetLength(0);
    }

    private void EndTurn(bool wasPassed, Piece[,] board)
    {
        isWhiteTurn = !isWhiteTurn;
        if (isWhiteTurn)
            indicator.SetText("White's Turn");
        else
            indicator.SetText("Black's Turn");

        GetScore(board);
        blackScoreBoard.SetText("Black: " + blackScore);
        whiteScoreBoard.SetText("White: " + whiteScore);

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
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] && board[i, j].isWhite)
                {
                    white++;
                } else if (board[i, j] && !board[i, j].isWhite)
                {
                    black++;
                }
            }
        }
        whiteScore = white;
        blackScore = black;

        scores.x = whiteScore;
        scores.y = blackScore;

        if (whiteScore == 0 || blackScore == 0 || whiteScore + blackScore == 64)
        {
            EndGame();
            return scores;
        }
        return scores;
    }

    private Vector2 GetScoreTest(char[,] board)
    {
        Vector2 scores = new Vector2();
        int white = 0;
        int black = 0;
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] == 'W')
                {
                    white++;
                }
                else if (board[i, j] == 'B')
                {
                    black++;
                }
            }
        }
        whiteScore = white;
        blackScore = black;

        scores.x = whiteScore;
        scores.y = blackScore;

        return scores;
    }

    private void EndGame()
    {
        gameOver = true;
        if(blackScore == whiteScore)
        {
            gameOverText.SetText("Game Over. It's a Tie!");
        } else if(blackScore > whiteScore)
        {
            gameOverText.SetText("Game Over. Black Wins!");
        } else if(blackScore < whiteScore)
        {
            gameOverText.SetText("Game Over. White Wins!");
        }
        else
        {
            gameOverText.SetText("Game Over.");
        }
        indicator.SetText("");
        blackScoreBoard.SetText("");
        whiteScoreBoard.SetText("");

        gameOverBackground.enabled = true;
        gameOverText.enabled = true;
        newSoloGameButton.gameObject.SetActive(true);
        newTwoPlayerGameButton.gameObject.SetActive(true);
        quitGame.gameObject.SetActive(false);
        levelSelect.gameObject.SetActive(true);
    }

    private void UpdateMouseOver()
    {
        if (!Camera.main)
        {
            Debug.Log("Unable to find main camera.");
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("Board")))
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
        if (x < 0 || x >= board.GetLength(0) || y < 0 || y >= board.GetLength(1))
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
            p.isWhite = true;
            p.transform.Rotate(0, 0, 180);
            FlipPiece(p);
        }
        else
        {
            p.isWhite = false;
        }
        //FlipPiece(p);

        board[x, y] = p;
    }

    private void MovePiece(Piece p, int x, int y)
    {
        p.transform.position = (Vector3.right * x) + (Vector3.forward * y) + boardOffset + pieceOffset;
    }
    private void FlipPiece(Piece p)
    {
        Animator anim = p.GetComponentInChildren<Animator>();
        anim.SetBool("IsWhite", p.isWhite);
        //p.transform.Rotate(0, 0, 180);
    }

    private void FlipPiece(int x, int y, Piece[,] board)
    {
        board[x, y].isWhite = !board[x, y].isWhite;
        Piece p = board[x, y];
        Animator anim = p.GetComponentInChildren<Animator>();
        anim.SetBool("IsWhite", p.isWhite);

        
        //board[x, y].transform.Rotate(0, 0, 180);
    }

    private int Heuristic(char[,] board, bool isWhiteTurn)
    {
        Vector2 scores = GetScoreTest(board);
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

    private Move MinimaxDecision(Piece[,] board, bool isItWhiteTurn)
    {
        Move res = null;
        List<Move> curPossibleMoves = getPossibleMoves(board, isItWhiteTurn);
        int numMoves = curPossibleMoves.Count;

        if (numMoves == 0)
        {
            res = null;
        }
        else
        {
            //Remember the best move
            int bestMoveVal = System.Int32.MinValue;
            Move bestMove = curPossibleMoves[0];

            //Try out every single move
            for (int i = 0; i < numMoves; i++)
            {
                Vector2 chosenSquare = curPossibleMoves[i].getSquare();
                //Apply the move to a new board;
                char[,] tmpBoard = CopyBoard(board);
                tmpBoard = MakeMove(tmpBoard, isItWhiteTurn, chosenSquare);
                //Recursive call, initial search ply = 1
                int val = minimaxValue(tmpBoard, isItWhiteTurn, !isItWhiteTurn, 1);

                if (val > bestMoveVal)
                {
                    bestMoveVal = val;
                    bestMove = curPossibleMoves[i];
                }
            }
            //return the best move
            res = bestMove;
        }
        return res;
    }

    private int minimaxValue(char[,] board, bool originallyIsItWhiteTurn, bool currentlyIsItWhiteTurn, int searchPly)
    {
        if ((searchPly == minimaxLevels) || IsGameOver(board)){
            return Heuristic(board, originallyIsItWhiteTurn);
        }
        Vector2 resultMove = new Vector2();

        //this line might be a problem
        bool opponent = !currentlyIsItWhiteTurn;

        List<Move> curPossibleMoves = getPossibleMovesTest(board, currentlyIsItWhiteTurn);
        int numMoves = curPossibleMoves.Count;
        if(numMoves == 0)//if there are no moves, then skip to the next players turn
        {
            return minimaxValue(board, originallyIsItWhiteTurn, opponent, searchPly + 1);
        }
        else
        {
            //Rememeber the best move
            int bestMoveVal = System.Int32.MinValue; //for finding max

            if(originallyIsItWhiteTurn != currentlyIsItWhiteTurn)
            {
                bestMoveVal = System.Int32.MaxValue; //for finding min
            }

            //try out every single move
            for(int i = 0; i < numMoves; i++)
            {
                //Apply the move to a new board
                Vector2 chosenSquare = curPossibleMoves[i].getSquare();
                char[,] tmpBoard = (char[,])board.Clone();
                tmpBoard = MakeMove(tmpBoard, currentlyIsItWhiteTurn, chosenSquare);

                //Recursive call
                int val = minimaxValue(tmpBoard, originallyIsItWhiteTurn, opponent, searchPly + 1);

                //Remember the best move
                if(originallyIsItWhiteTurn == currentlyIsItWhiteTurn)
                {
                    //Remember max if its the originator's turn
                    if(val > bestMoveVal)
                    {
                        bestMoveVal = val;
                    }
                }
                else
                {
                    //Remember min if its opponent's turn
                    if (val < bestMoveVal)
                    {
                        bestMoveVal = val;
                    }
                }
            }
            return bestMoveVal;
        }
        return -1; //should never get here
    }

    private bool IsGameOver(char[,] board)
    {
        int white = 0;
        int black = 0;
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int j = 0; j < board.GetLength(1); j++)
            {
                if (board[i, j] == 'W')
                {
                    white++;
                }
                else if (board[i, j] == 'B')
                {
                    black++;
                }
            }
        }
        whiteScore = white;
        blackScore = black;

        if (whiteScore == 0 || blackScore == 0 || whiteScore + blackScore == 64)
        {
            return true;
        }
        return false;
    }

    private char[,] CopyBoard(Piece[,] source)
    {
        char[,] result = new char[source.GetLength(0), source.GetLength(1)];

        for(int i = 0; i < source.GetLength(0); i++)
        {
            for(int j = 0; j < source.GetLength(1); j++)
            {
                if(source[i,j] != null)
                {
                    result[i, j] = (source[i, j].isWhite) ? 'W' : 'B';
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
