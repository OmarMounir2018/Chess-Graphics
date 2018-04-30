using UnityEngine;

public class LogicMaster : MonoBehaviour
{
    public static bool pieceSelected = false;
    public static GameObject selectedPiece;
    public static bool whiteTurn = true;
    public static BoardState currentBoard;
    public static bool check = false;
    public static string messageString = "";
    public void clickListener()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ClickHandler(SelectObject.GetClickedGameObject()); // Needs assigning to something
        }
    }

    private void ClickHandler(GameObject gameObject)
    {
        if (gameObject == null) { return; }
        if (currentBoard[gameObject].piece != null && gameObject!=selectedPiece)
        {
            currentBoard.resetColours();
            currentBoard.hideMarkers();
        }

        if (currentBoard[gameObject].piece != null && currentBoard[gameObject].piece.selectPiece() && !currentBoard[gameObject].piece.targeted && !check)
        {
            pieceSelected = true;
            selectedPiece = gameObject;
            currentBoard[gameObject].piece.showMoves();
        }
        else if (currentBoard[gameObject].piece != null && currentBoard[gameObject].piece.selectPiece() && (currentBoard[gameObject].piece.GetType() != typeof(King)) && !currentBoard[gameObject].piece.targeted && check)
        {
            currentBoard.untargetAll();
            if (currentBoard[gameObject].piece.canUnCheck())
            {
                pieceSelected = true;
                selectedPiece = gameObject;
                currentBoard[gameObject].piece.selectPiece();
            }
        }
        else if (currentBoard[gameObject].piece != null && currentBoard[gameObject].piece.selectPiece() && (currentBoard[gameObject].piece.GetType() == typeof(King)) && check)
        {
            currentBoard.untargetAll();
            if (currentBoard[gameObject].piece.canUnCheck())
            {
                pieceSelected = true;
                selectedPiece = gameObject;
                currentBoard[gameObject].piece.selectPiece();
            }
        }

        else if (pieceSelected == true && currentBoard[gameObject].marked)
        {
            currentBoard[selectedPiece].piece.moveTo(currentBoard[gameObject].location);
            
        }
        else if (pieceSelected == true && currentBoard[gameObject].piece != null && currentBoard[gameObject].piece.targeted)
        {
            currentBoard[selectedPiece].piece.moveTo(currentBoard[gameObject].location);
        }
        else
        {
            currentBoard.resetColours();
            currentBoard.hideMarkers();
            currentBoard.untargetAll();
            pieceSelected = false;
            selectedPiece = null;
        }
        foreach (GameObject king in GameObject.FindGameObjectsWithTag("King"))
        {
            (currentBoard[king].piece as King).checkCheck();
        }
        if ((currentBoard[GameObject.FindGameObjectsWithTag("King")[0]].piece as King).isChecked || (currentBoard[GameObject.FindGameObjectsWithTag("King")[1]].piece as King).isChecked)
        {
            check = true;
        }
    }
    public static void checkMate()
    {
        messageString = "Check Mate";
    }

    void Update()
    {
            clickListener();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(20, 350, 300, 50), messageString);
        if (whiteTurn)
        {
            GUI.Box(new Rect(new Vector2(1000, 10), new Vector2(80, 80)), Constants.whiteTurn);
        }
        else
        {
            GUI.Box(new Rect(new Vector2(1000, 10), new Vector2(80, 80)), Constants.blackTurn);
        }
    }
}