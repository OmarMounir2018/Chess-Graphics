using UnityEngine;

public class SetUp : MonoBehaviour
{
    public static GameObject refCircle;

    public Material setBlack, setWhite, setSelected, setTarget, setCheck;
    public Texture2D setBTurn, setWTurn;

    void Awake()
    {
        Constants.Black = setBlack;
        Constants.White = setWhite;
        Constants.Selected = setSelected;
        Constants.Target = setTarget;
        Constants.Check = setCheck;
        Constants.whiteTurn = setWTurn;
        Constants.blackTurn = setBTurn;
        refCircle = GameObject.FindGameObjectWithTag("Marker");
        LogicMaster.currentBoard = new BoardState(generateBoard());
        setColours();
    }

    SquareState[] generateBoard()
    {
        SquareState[] squareArray = new SquareState[64];
        for (int i = 0; i < 64; i++)
        {
            Coord location = intToLoc(i);
            GameObject gameObj = spawnPiece(location);
            GameObject marker = makeMarker(location);
            Piece piece = makePiece(location, gameObj);
            squareArray[i] = new SquareState(location, gameObj, marker, piece);
        }
        Destroy(refCircle);
        return squareArray;
    }

    Coord intToLoc(int i)
    {
        return new Coord(i % 8, i / 8);
    }

    GameObject spawnPiece(Coord location)
    {
        int j = location.x;
        int i = location.y;
        GameObject gameObj = Resources.Load(Constants.DefaultBoard[i, j]) as GameObject;
        if (gameObj == null) { return null; }
        int rotator = 0;
        switch (Constants.DefaultBoard[i, j])
        {
            case "Bishop": { if (i < 4) {break; } else { rotator = 180; break; } }
            case "Knight": { if (i < 4) {rotator = -90; break; } else {rotator = 90; break; } }
        }
        Transform transform = refCircle.GetComponent<Transform>();
        Vector3 PieceLocation = transform.position + new Vector3(j * 1.41f, 1f, i * 1.34f);
        return Instantiate(gameObj, PieceLocation, Quaternion.Euler(0, rotator, 0)) as GameObject;
    }

    GameObject makeMarker(Coord location)
    {
        int i = location.y;
        int j = location.x;
        GameObject gameObj = refCircle;
        Renderer renderer = gameObj.GetComponent<Renderer>();
        renderer.enabled = false;
        Transform transform = refCircle.GetComponent<Transform>();
        Vector3 PieceLocation = transform.position + new Vector3(j * 1.41f, 0, i * 1.34f);
        return Instantiate(gameObj, PieceLocation, Quaternion.Euler(90, 0, 0)) as GameObject;
    }

    public Piece makePiece(Coord location, GameObject gameObj)
    {
        if (gameObj == null) { return null; }
        switch(gameObj.tag)
        {
            case "Pawn":
                {
                    return new Pawn(location, gameObj);
                }
            case "Bishop":
                {
                    return new Bishop(location, gameObj);
                }
            case "Knight":
                {
                    return new Knight(location, gameObj);
                }
            case "Rook":
                {
                    return new Rook(location, gameObj);
                }
            case "Queen":
                {
                    return new Queen(location, gameObj);
                }
            case "King":
                {
                    return new King(location, gameObj);
                }
        }
        return null;
    }
 
    void setColours()
    {
        foreach (SquareState square in LogicMaster.currentBoard)
        {
            if (square.actor == null) { continue; }
            square.piece.resetColour();
        }
    }

}