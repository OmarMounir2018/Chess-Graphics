using UnityEngine;
using System.Collections.Generic;

public class Coord

{
    public Coord() { }
    public Coord(int i, int j)
    {
        x = i;
        y = j;
    }

    public static Coord operator +(Coord c1, Coord c2)
    {
        int newX = c1.x + c2.x;
        int newY = c1.y + c2.y;
        return new Coord(newX, newY);
    }

    public static Coord operator *(Coord c1, int i)
    {
        int newX = c1.x * i;
        int newY = c1.y * i;
        return new Coord(newX, newY);
    }

    public override bool Equals(object obj)
    {
        return (obj as Coord == this);
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }
    public static Coord operator -(Coord c1, Coord c2)
    {
        int newX = c1.x - c2.x;
        int newY = c1.y - c2.y;
        return new Coord(newX, newY);
    }

    public static Coord operator *(int k, Coord c)
    {
        int newX = c.x * k;
        int newY = c.y * k;
        return new Coord(newX, newY);
    }

    public static bool operator ==(Coord c1, Coord c2)
    {
        return (((c1.x) == (c2.x)) && ((c1.y) == (c2.y)));
    }

    public static bool operator !=(Coord c1, Coord c2)
    {
        return (!(((c1.x) == (c2.x)) && ((c1.y) == (c2.y))));
    }

    public override string ToString()
    {
        string str = "(" + x + "," + y + ")";
        return (str);
    }

    public double abs()
    {
        double absValue = Mathf.Sqrt((Mathf.Pow(x, 2) + (Mathf.Pow(y, 2))));
        return absValue;
    }


    public int x { get; set; }
    public int y { get; set; }
}


public class Piece
{

    protected Coord location;
    public bool specialUsed = false;
    protected GameObject gameObject;
    public virtual void moveTo(Coord newLoc)
    {
        //if (willSelfCheck(newLoc)) { LogicMaster.messageString = "Move would cause you to be in check"; return; }
        Coord origLoc = location;
        Transform transform = gameObject.GetComponent<Transform>();
        Coord transCoord = newLoc - location;
        if (LogicMaster.currentBoard[newLoc].piece != null && LogicMaster.currentBoard[newLoc].piece.targeted && LogicMaster.currentBoard[newLoc].piece.GetType() != typeof(King))
        {
            UnityEngine.Object.Destroy(LogicMaster.currentBoard[newLoc].actor);
            LogicMaster.currentBoard[newLoc].actor = null;
        }
        else if (LogicMaster.currentBoard[newLoc].piece != null && LogicMaster.currentBoard[newLoc].piece.GetType() == typeof(King))
        {
            LogicMaster.messageString = "One does not simply kill the king... glitches may ensue...";
            return;
        }
        if (LogicMaster.currentBoard[newLoc].actor == null)
        {
            Vector3 transVector = new Vector3();
            transVector.x = transform.position.x + transCoord.x * Constants.oneRight.x;
            transVector.z = transform.position.z + transCoord.y * Constants.oneForward.y;
            transVector.y = transform.position.y;
            transform.position = transVector;
            LogicMaster.currentBoard[newLoc].actor = gameObject;
            LogicMaster.currentBoard[newLoc].piece = this;
            LogicMaster.currentBoard[newLoc].piece.location = newLoc;
            LogicMaster.currentBoard[origLoc].actor = null;
            LogicMaster.currentBoard[origLoc].piece = null;
            LogicMaster.selectedPiece = null;
            LogicMaster.pieceSelected = false;
            LogicMaster.currentBoard.resetColours();
            LogicMaster.currentBoard.hideMarkers();
            foreach (SquareState square in LogicMaster.currentBoard)
            {

                if (square.piece != null)
                {
                    square.piece.unTarget();
                }
            }
            /*if (LogicMaster.check)
            {
                bool checkMate = true;
                foreach (SquareState square in LogicMaster.currentBoard)
                {
                    if (square.piece.canUnCheck())
                    {
                        checkMate = false;
                    }
                }
                if (checkMate)
                {
                    LogicMaster.checkMate();
                }
            }*/
            LogicMaster.whiteTurn = !LogicMaster.whiteTurn;
        }
    }

    private bool willSelfCheck(Coord newLoc)
    {
        bool selfCheck = false;
        if (GetType() == typeof(King)) //Special case for checking king movement
        {
            Piece tempPiece = LogicMaster.currentBoard[newLoc].piece;
            LogicMaster.currentBoard[this].piece = null;
            LogicMaster.currentBoard[newLoc].piece = this;
            (this as King).checkCheck();
            if ((this as King).isChecked)
            {
                selfCheck = true;
            }
            LogicMaster.currentBoard[location].piece = this;
            LogicMaster.currentBoard[newLoc].piece = tempPiece;
            (this as King).checkCheck();
            return selfCheck;
        }
        foreach (GameObject king in GameObject.FindGameObjectsWithTag("King"))
        {
            if (LogicMaster.currentBoard[king].piece.isWhite && LogicMaster.whiteTurn)
            {
               
                Piece tempPiece = LogicMaster.currentBoard[newLoc].piece;
                LogicMaster.currentBoard[this].piece = null;
                LogicMaster.currentBoard[newLoc].piece = this;
                (LogicMaster.currentBoard[king].piece as King).checkCheck();
                if ((LogicMaster.currentBoard[king].piece as King).isChecked)
                {
                    selfCheck = true;
                }
                LogicMaster.currentBoard[location].piece = this;
                LogicMaster.currentBoard[newLoc].piece = tempPiece;
            }
        }
        
        return selfCheck;
    }
    public bool canUnCheck()
    {
        List<Coord> escapes = new List<Coord>();
        escapes.AddRange(canBlock());
        escapes.AddRange(canKill());
        escapes.AddRange(canEscape());
        if (escapes.Count != 0)
        {
            foreach (Coord location in escapes)
            {
                if (LogicMaster.currentBoard[location] != null)
                {
                    if (LogicMaster.currentBoard[location].piece != null)
                    {
                        LogicMaster.currentBoard[location].targetPiece();
                    }
                    else
                    {
                        LogicMaster.currentBoard[location].showMarker();
                    }
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    private List<Coord> canBlock()
    {
        if (GetType() == typeof(King)) { return new List<Coord>(); }
        showMoves();
        List<Coord> escapes = new List<Coord>();
        List<Coord> moves = new List<Coord>();
        foreach (SquareState square in LogicMaster.currentBoard)
        {
            if (square.marked)
            {
                moves.Add(square.location);
            }
        }
        LogicMaster.currentBoard.hideMarkers();
        LogicMaster.currentBoard.resetColours();
        LogicMaster.currentBoard.untargetAll();
        foreach (Coord move in moves)
        {
            foreach (GameObject king in GameObject.FindGameObjectsWithTag("King"))
            {
                if (!(LogicMaster.currentBoard[king].piece as King).isChecked) { continue; }
                if (LogicMaster.currentBoard[move] != null && LogicMaster.currentBoard[move].piece == null)
                {
                    Coord origLoc = location;
                    LogicMaster.currentBoard[this].piece = null;
                    LogicMaster.currentBoard[move].piece = this;
                    (LogicMaster.currentBoard[king].piece as King).checkCheck();
                    bool simulCheck = (LogicMaster.currentBoard[king].piece as King).isChecked;
                    LogicMaster.currentBoard[move].piece = null;
                    LogicMaster.currentBoard[origLoc].piece = this;
                    (LogicMaster.currentBoard[king].piece as King).checkCheck();
                    if (!simulCheck)
                    {
                        escapes.Add(move);
                    }
                }
            }
        }
        return escapes;
    }


    private List<Coord> canEscape()
    {
        List<Coord> escapes = new List<Coord>();
        if (GetType() != typeof(King))
        {
            return escapes;
        }
        Coord origLoc = location;
        foreach (GameObject king in GameObject.FindGameObjectsWithTag("King"))
        {
            if ((LogicMaster.currentBoard[king].piece as King).isChecked)
            {
                foreach (Coord direction in (LogicMaster.currentBoard[king].piece as King).unitCoords)
                {
                    if (LogicMaster.currentBoard[origLoc + direction] != null && LogicMaster.currentBoard[origLoc + direction].piece == null)
                    {
                        LogicMaster.currentBoard[this].piece = null;
                        LogicMaster.currentBoard[origLoc + direction].piece = this;
                        location = origLoc + direction;
                        (this as King).checkCheck();
                        bool simulCheck = (this as King).isChecked;
                        location = origLoc;
                        LogicMaster.currentBoard[this].piece = null;
                        LogicMaster.currentBoard[origLoc].piece = this;
                        (this as King).checkCheck();
                        if (!simulCheck)
                        {
                            escapes.Add(origLoc + direction);
                        }
                    }
                }
            }
        }
        return escapes;
    }
    private List<Coord> canKill()
    {
        if (GetType() == typeof(King)) { return (this as King).execution(); }
        List<Coord> escapes = new List<Coord>();
        showMoves();
        foreach (GameObject king in GameObject.FindGameObjectsWithTag("King"))
        {
            if ((LogicMaster.currentBoard[king].piece as King).isChecked)
            {
                List<Piece> checkingPieces = new List<Piece>();
                checkingPieces = (LogicMaster.currentBoard[king].piece as King).checkCheck();
                if (checkingPieces.Count != 0)
                {
                    foreach (Piece piece in checkingPieces)
                    {
                        if (piece.targeted)
                        {
                            
                            LogicMaster.currentBoard[this].piece = null; //Simulate the piece being taken
                            LogicMaster.currentBoard[piece].piece = this;
                            
                            (LogicMaster.currentBoard[king].piece as King).checkCheck();
                            bool simulCheck = (LogicMaster.currentBoard[king].piece as King).isChecked;
                            LogicMaster.currentBoard[location].piece = this; //Return to actual positions
                            LogicMaster.currentBoard[piece.location].piece = piece;
                            (LogicMaster.currentBoard[king].piece as King).checkCheck(); //Recheck
                            if (!simulCheck)
                            {
                                LogicMaster.currentBoard.hideMarkers();
                                LogicMaster.currentBoard.resetColours();
                                escapes.Add(piece.location);
                            }
                        }
                    }
                }

            }
        }
        LogicMaster.currentBoard.hideMarkers();
        LogicMaster.currentBoard.resetColours();
        return escapes;
    }
    public bool selectPiece()
    {
        if ((isWhite && LogicMaster.whiteTurn || !isWhite && !LogicMaster.whiteTurn))
        {
            MeshRenderer mRenderer = gameObject.GetComponent<MeshRenderer>();
            mRenderer.material = Constants.Selected;
            return true;
        }
        return false;
    }
    public void resetColour()
    {
        MeshRenderer mRenderer = gameObject.GetComponent<MeshRenderer>();

        if (isWhite)
        {
            mRenderer.material = Constants.White;
        }
        else
        {
            mRenderer.material = Constants.Black;
        }


    }

    public void unTarget()
    {
        resetColour();
        targeted = false;
    }
    public virtual void showMoves()
    {

    }
    public bool isWhite;
    public bool targeted;
}
public class Pawn : Piece
{
    public Pawn(Coord loc, GameObject gameObj)
    {
        location = loc;
        if (loc.y < 4) { isWhite = true; } else { isWhite = false; }
        gameObject = gameObj;
    }

    public override void showMoves()
    {
        List<Coord> targetList = new List<Coord>();
        List<Coord> movesList = new List<Coord>();
        int direction = -1;
        if (isWhite) { direction = 1; }
        if (LogicMaster.currentBoard[location + new Coord(0, 1 * direction)].piece == null) //check if square is empty
        {
            movesList.Add(location + new Coord(0, 1 * direction));
            if (!specialUsed && LogicMaster.currentBoard[location + new Coord(0, 2 * direction)].piece == null)
            {
                movesList.Add(location + new Coord(0, 2 * direction));
            }
        }
        if (LogicMaster.currentBoard[location + new Coord(1, 1 * direction)] != null && LogicMaster.currentBoard[location + new Coord(1, 1 * direction)] != null)
        {
            targetList.Add(LogicMaster.currentBoard[location + new Coord(1, 1 * direction)].location);
        }
        if (LogicMaster.currentBoard[location + new Coord(-1, 1 * direction)] != null && LogicMaster.currentBoard[location + new Coord(-1, 1 * direction)] != null)
        {
            targetList.Add(LogicMaster.currentBoard[location + new Coord(-1, 1 * direction)].location);
        }
        foreach (Coord target in targetList)
        {
            if (LogicMaster.currentBoard[target].piece != null && (LogicMaster.currentBoard[target].piece.isWhite == !LogicMaster.whiteTurn))
            {
                LogicMaster.currentBoard[target].targetPiece();
            }
        }
        foreach (Coord move in movesList)
        {
            if (LogicMaster.currentBoard[move] != null)
            {
                LogicMaster.currentBoard[move].showMarker();
            }
        }
    }

    public override void moveTo(Coord newLoc)
    {
        base.moveTo(newLoc);
        specialUsed = true;
        if ((newLoc.y == 7 && isWhite) || (newLoc.y == 0 && !isWhite))
        {
            int j = location.x;
            int i = location.y;
            GameObject gameObj = Resources.Load("Queen") as GameObject;
            int rotator = 0;
            switch (Constants.DefaultBoard[i, j])
            {
                case "Bishop": { if (i < 4) { break; } else { rotator = 180; break; } }
                case "Knight": { if (i < 4) { rotator = -90; break; } else { rotator = 90; break; } }
            }
            Transform transform = gameObject.GetComponent<Transform>();
            UnityEngine.Object.Destroy(LogicMaster.currentBoard[newLoc].actor);
            LogicMaster.currentBoard[newLoc].actor = UnityEngine.Object.Instantiate(gameObj, transform.position, Quaternion.Euler(0, rotator, 0)) as GameObject;
            LogicMaster.currentBoard[newLoc].piece = new Queen(newLoc, LogicMaster.currentBoard[newLoc].actor);
            LogicMaster.currentBoard[newLoc].piece.isWhite = isWhite;
            LogicMaster.currentBoard.resetColours();
        }
    }
}
public class Bishop : Piece
{
    public override void showMoves()
    {
        List<Coord> targetList = new List<Coord>();
        List<Coord> movesList = new List<Coord>();
        Coord testCoord = new Coord(location.x, location.y);
        while (testCoord.x < 8 && testCoord.y < 8)
        {
            testCoord.x++;
            testCoord.y++;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.x < 8 && testCoord.y >= 0)
        {
            testCoord.x++;
            testCoord.y--;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.x >= 0 && testCoord.y >= 0)
        {
            testCoord.x--;
            testCoord.y--;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.x >= 0 && testCoord.y < 8)
        {
            testCoord.x--;
            testCoord.y++;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        foreach (Coord target in targetList)
        {
            if (LogicMaster.currentBoard[target].piece != null && (LogicMaster.currentBoard[target].piece.isWhite == !LogicMaster.whiteTurn))
            {
                LogicMaster.currentBoard[target].targetPiece();
            }
        }
        foreach (Coord move in movesList)
        {
            if (LogicMaster.currentBoard[move] != null)
            {
                LogicMaster.currentBoard[move].showMarker();
            }
        }
    }
    public Bishop(Coord loc, GameObject gameObj)
    {
        specialUsed = true;
        location = loc;
        if (loc.y < 4) { isWhite = true; } else { isWhite = false; }
        gameObject = gameObj;
    }
}
public class Knight : Piece
{
    public override void showMoves()
    {
        List<Coord> targetList = new List<Coord>();
        List<Coord> movesList = new List<Coord>();

        if ((LogicMaster.currentBoard[location + new Coord(2, 1)] != null) && (LogicMaster.currentBoard[location + new Coord(2, 1)].piece != null))
        {
            targetList.Add(LogicMaster.currentBoard[location + new Coord(2, 1)].location);
        }
        else if (LogicMaster.currentBoard[location + new Coord(2, 1)] != null)
        {
            movesList.Add(LogicMaster.currentBoard[location + new Coord(2, 1)].location);
        }

        if ((LogicMaster.currentBoard[location + new Coord(2, -1)] != null) && (LogicMaster.currentBoard[location + new Coord(2, -1)].piece != null))
        {
            targetList.Add(LogicMaster.currentBoard[location + new Coord(2, -1)].location);
        }
        else if (LogicMaster.currentBoard[location + new Coord(2, -1)] != null)
        {
            movesList.Add(LogicMaster.currentBoard[location + new Coord(2, -1)].location);
        }

        if ((LogicMaster.currentBoard[location + new Coord(-2, 1)] != null) && (LogicMaster.currentBoard[location + new Coord(-2, 1)].piece != null))
        {
            targetList.Add(LogicMaster.currentBoard[location + new Coord(-2, 1)].location);
        }
        else if (LogicMaster.currentBoard[location + new Coord(-2, 1)] != null)
        {
            movesList.Add(LogicMaster.currentBoard[location + new Coord(-2, 1)].location);
        }

        if ((LogicMaster.currentBoard[location + new Coord(-2, -1)] != null) && (LogicMaster.currentBoard[location + new Coord(-2, -1)].piece != null))
        {
            targetList.Add(LogicMaster.currentBoard[location + new Coord(-2, -1)].location);
        }

        else if (LogicMaster.currentBoard[location + new Coord(-2, -1)] != null)
        {
            movesList.Add(LogicMaster.currentBoard[location + new Coord(-2, -1)].location);
        }

        if ((LogicMaster.currentBoard[location + new Coord(1, 2)] != null) && (LogicMaster.currentBoard[location + new Coord(1, 2)].piece != null))
        {
            targetList.Add(LogicMaster.currentBoard[location + new Coord(1, 2)].location);
        }
        else if (LogicMaster.currentBoard[location + new Coord(1, 2)] != null)
        {
            movesList.Add(LogicMaster.currentBoard[location + new Coord(1, 2)].location);
        }

        if ((LogicMaster.currentBoard[location + new Coord(-1, 2)] != null) && (LogicMaster.currentBoard[location + new Coord(-1, 2)].piece != null))
        {
            targetList.Add(LogicMaster.currentBoard[location + new Coord(-1, 2)].location);
        }
        else if (LogicMaster.currentBoard[location + new Coord(-1, 2)] != null)
        {
            movesList.Add(LogicMaster.currentBoard[location + new Coord(-1, 2)].location);
        }

        if ((LogicMaster.currentBoard[location + new Coord(1, -2)] != null) && (LogicMaster.currentBoard[location + new Coord(1, -2)].piece != null))
        {
            targetList.Add(LogicMaster.currentBoard[location + new Coord(1, -2)].location);
        }
        else if (LogicMaster.currentBoard[location + new Coord(1, -2)] != null)
        {
            movesList.Add(LogicMaster.currentBoard[location + new Coord(1, -2)].location);
        }

        if ((LogicMaster.currentBoard[location + new Coord(-1, -2)] != null) && (LogicMaster.currentBoard[location + new Coord(-1, -2)].piece != null))
        {
            targetList.Add(LogicMaster.currentBoard[location + new Coord(-1, -2)].location);
        }
        else if (LogicMaster.currentBoard[location + new Coord(-1, -2)] != null)
        {
            movesList.Add(LogicMaster.currentBoard[location + new Coord(-1, -2)].location);
        }
        foreach (Coord target in targetList)
        {
            if (LogicMaster.currentBoard[target].piece != null && (LogicMaster.currentBoard[target].piece.isWhite == !LogicMaster.whiteTurn))
            {
                LogicMaster.currentBoard[target].targetPiece();
            }
        }
        foreach (Coord move in movesList)
        {
            if (LogicMaster.currentBoard[move] != null)
            {
                LogicMaster.currentBoard[move].showMarker();
            }
        }
    }
    public Knight(Coord loc, GameObject gameObj)
    {

        specialUsed = true;
        location = loc;
        if (loc.y < 4) { isWhite = true; } else { isWhite = false; }
        gameObject = gameObj;
    }

}
public class Rook : Piece
{
    public override void showMoves()
    {
        List<Coord> targetList = new List<Coord>();
        List<Coord> movesList = new List<Coord>();
        Coord testCoord = new Coord(location.x, location.y);
        while (testCoord.x < 8)
        {
            testCoord.x++;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.x >= 0)
        {
            testCoord.x--;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.y < 8)
        {
            testCoord.y++;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.y >= 0)
        {
            testCoord.y--;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        foreach (Coord target in targetList)
        {
            if (LogicMaster.currentBoard[target].piece != null && (LogicMaster.currentBoard[target].piece.isWhite == !LogicMaster.whiteTurn))
            {
                LogicMaster.currentBoard[target].targetPiece();
            }
        }
        foreach (Coord move in movesList)
        {
            if (LogicMaster.currentBoard[move] != null)
            {
                LogicMaster.currentBoard[move].showMarker();
            }
        }
    }

    public override void moveTo(Coord newLoc)
    {
        base.moveTo(newLoc);
        specialUsed = true;
    }
    public Rook(Coord loc, GameObject gameObj)
    {
        location = loc;
        if (loc.y < 4) { isWhite = true; } else { isWhite = false; }
        gameObject = gameObj;
    }

}
public class Queen : Piece
{
    public override void showMoves()
    {
        List<Coord> targetList = new List<Coord>();
        List<Coord> movesList = new List<Coord>();
        Coord testCoord = new Coord(location.x, location.y);
        while (testCoord.x < 8 && testCoord.y < 8)
        {
            testCoord.x++;
            testCoord.y++;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.x < 8 && testCoord.y >= 0)
        {
            testCoord.x++;
            testCoord.y--;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.x >= 0 && testCoord.y >= 0)
        {
            testCoord.x--;
            testCoord.y--;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.x >= 0 && testCoord.y < 8)
        {
            testCoord.x--;
            testCoord.y++;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.x < 8)
        {
            testCoord.x++;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.x >= 0)
        {
            testCoord.x--;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.y < 8)
        {
            testCoord.y++;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        while (testCoord.y >= 0)
        {
            testCoord.y--;
            if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece == null))
            {
                movesList.Add(LogicMaster.currentBoard[testCoord].location);
            }
            else if ((LogicMaster.currentBoard[testCoord] != null) && (LogicMaster.currentBoard[testCoord].piece != null))
            {
                targetList.Add(LogicMaster.currentBoard[testCoord].location);
                break;
            }
        }
        testCoord = new Coord(location.x, location.y);
        foreach (Coord target in targetList)
        {
            if (LogicMaster.currentBoard[target].piece != null && (LogicMaster.currentBoard[target].piece.isWhite == !LogicMaster.whiteTurn))
            {
                LogicMaster.currentBoard[target].targetPiece();
            }
        }
        foreach (Coord move in movesList)
        {
            if (LogicMaster.currentBoard[move] != null)
            {
                LogicMaster.currentBoard[move].showMarker();
            }
        }
    }
    public Queen(Coord loc, GameObject gameObj)
    {
        specialUsed = true;
        location = loc;
        if (loc.y < 4) { isWhite = true; } else { isWhite = false; }
        gameObject = gameObj;
    }

}
public class King : Piece
{
    public Coord[] unitCoords = { new Coord(1, 1), new Coord(0, 1), new Coord(1, 0), new Coord(-1, 0), new Coord(0, -1), new Coord(-1, 1), new Coord(1, -1), new Coord(-1, -1) };
    public bool isChecked = false;
    public override void showMoves()
    {
        List<Coord> targetList = new List<Coord>();
        List<Coord> movesList = new List<Coord>();
           foreach (Coord direction in unitCoords)
            {
                if ((LogicMaster.currentBoard[location + direction] != null) && (LogicMaster.currentBoard[location + direction].piece == null))
                {
                    movesList.Add(location + direction);
                }
                else if ((LogicMaster.currentBoard[location + direction] != null) && (LogicMaster.currentBoard[location + direction].piece != null))
                {
                    targetList.Add(location + direction);
                }
            }
            if (!specialUsed && !isChecked)
            {
                int direction = 1;
                if (!isWhite) { direction = -1; }
                if (LogicMaster.currentBoard[location + new Coord(3, 0) * direction].piece != null && !LogicMaster.currentBoard[location + new Coord(3, 0) * direction].piece.specialUsed)
                {
                    if ((LogicMaster.currentBoard[location + new Coord(1, 0) * direction].piece == null) && (LogicMaster.currentBoard[location + new Coord(2, 0) * direction].piece == null))
                    {
                        movesList.Add(location + new Coord(2, 0) * direction);
                    }
                }
                if (LogicMaster.currentBoard[location + new Coord(-4, 0) * direction].piece != null && !LogicMaster.currentBoard[location + new Coord(-4, 0) * direction].piece.specialUsed)
                {
                    if ((LogicMaster.currentBoard[location + new Coord(-1, 0) * direction].piece == null) && (LogicMaster.currentBoard[location + new Coord(-2, 0) * direction].piece == null) && (LogicMaster.currentBoard[location + new Coord(-3, 0) * direction].piece == null))
                    {
                        movesList.Add(location + new Coord(-3, 0) * direction);
                    }
                }
            }
        
        foreach (Coord target in targetList)
        {
            if (LogicMaster.currentBoard[target].piece != null && (LogicMaster.currentBoard[target].piece.isWhite == !LogicMaster.whiteTurn))
            {
                LogicMaster.currentBoard[target].targetPiece();
            }
        }
        foreach (Coord move in movesList)
        {
            if (LogicMaster.currentBoard[move] != null)
            {
                LogicMaster.currentBoard[move].showMarker();
            }
        }
    }
    public override void moveTo(Coord newLoc)
    {
        int direction = 1;
        if (!isWhite) { direction = -1; }
        Coord transLoc = newLoc - location;
        if (transLoc.abs() > Mathf.Sqrt(2))
        {
            if (transLoc.abs() < 2.5)
            {
                LogicMaster.currentBoard[location + new Coord(3, 0) * direction].piece.moveTo(location + new Coord(1, 0) * direction);
                LogicMaster.whiteTurn = !LogicMaster.whiteTurn; //Needed to correct for extra flip due to rook move
            }
            else
            {
                LogicMaster.currentBoard[location + new Coord(-4, 0) * direction].piece.moveTo(location + new Coord(-2, 0) * direction);
                LogicMaster.whiteTurn = !LogicMaster.whiteTurn;
            }
        }
        base.moveTo(newLoc);
        specialUsed = true;
    }
    public King(Coord loc, GameObject gameObj)
    {
        specialUsed = false;
        location = loc;
        if (loc.y < 4) { isWhite = true; } else { isWhite = false; }
        gameObject = gameObj;
    }
    public List<Piece> checkCheck()
    {
        List<Piece> checkingPieces = new List<Piece>();
        List<Piece>[] lists = new List<Piece>[6] { checkPawns(), checkBishops(), checkKnights(), checkRooks(), checkQueen(), checkKing() };
        foreach (List<Piece> list in lists)
        {
            if (list != null)
            {
                checkingPieces.AddRange(list);
            }
        }
        foreach (Piece piece in checkingPieces)
        {
            Debug.Log(piece.GetType());
        }
        if (checkingPieces.Count != 0)
        {
            check();
        }
        else
        {
            unCheck();
        }
        return checkingPieces;
    }
    List<Piece> checkPawns()
    {
        List<Piece> returnList = new List<Piece>();
        int direction = 1;
        if (!isWhite) { direction = -1; }

        if ((LogicMaster.currentBoard[location + new Coord(1, 1 * direction)] != null) && (LogicMaster.currentBoard[location + new Coord(1, 1 * direction)].piece != null) && (LogicMaster.currentBoard[location + new Coord(1, 1 * direction)].piece.GetType() == typeof(Pawn)) && (isWhite != LogicMaster.currentBoard[location + new Coord(1, 1 * direction)].piece.isWhite))
        {
            returnList.Add(LogicMaster.currentBoard[location + new Coord(1, 1 * direction)].piece);
        }
        if ((LogicMaster.currentBoard[location + new Coord(-1, 1 * direction)] != null) && (LogicMaster.currentBoard[location + new Coord(-1, 1 * direction)].piece != null) && (LogicMaster.currentBoard[location + new Coord(-1, 1 * direction)].piece.GetType() == typeof(Pawn)) && (isWhite != LogicMaster.currentBoard[location + new Coord(-1, 1 * direction)].piece.isWhite))
        {
            returnList.Add(LogicMaster.currentBoard[location + new Coord(-1, 1 * direction)].piece);
        }
        return returnList;
    }
    List<Piece> checkBishops()
    {
        List<Piece> returnList = new List<Piece>();
        Coord[] dVects = new Coord[4] { new Coord(1, 1), new Coord(-1, 1), new Coord(-1, -1), new Coord(1, -1) };
        foreach (Coord direction in dVects)
        {
            Coord testLoc = location;
            while (testLoc.x >= 0 && testLoc.x < 8 && testLoc.y >= 0 && testLoc.y < 8)
            {
                testLoc += direction;
                if ((LogicMaster.currentBoard[testLoc] != null) && (LogicMaster.currentBoard[testLoc].piece != null) && (LogicMaster.currentBoard[testLoc].piece.GetType() == typeof(Bishop)) && (isWhite != LogicMaster.currentBoard[testLoc].piece.isWhite))
                {
                    returnList.Add(LogicMaster.currentBoard[testLoc].piece);
                    break;
                }
                else if ((LogicMaster.currentBoard[testLoc] != null) && LogicMaster.currentBoard[testLoc].piece != null)
                {
                    break;
                }
            }
        }
        return returnList;
    }
    List<Piece> checkKnights()
    {
        List<Piece> returnList = new List<Piece>();
        Coord[] dVects = new Coord[8] { new Coord(2, 1), new Coord(2, -1), new Coord(-2, -1), new Coord(-2, 1), new Coord(1, 2), new Coord(1, -2), new Coord(-1, -2), new Coord(-1, 2) };
        foreach (Coord direction in dVects)
        {
            Coord testLoc = location;
            testLoc += direction;
            if ((LogicMaster.currentBoard[testLoc] != null) && (LogicMaster.currentBoard[testLoc].piece != null) && (LogicMaster.currentBoard[testLoc].piece.GetType() == typeof(Knight)) && (isWhite != LogicMaster.currentBoard[testLoc].piece.isWhite))
            {
                returnList.Add(LogicMaster.currentBoard[testLoc].piece);
                break;
            }
            else if ((LogicMaster.currentBoard[testLoc] != null) && LogicMaster.currentBoard[testLoc].piece != null)
            {
                break;
            }
        }
        return returnList;
    }
    List<Piece> checkRooks()
    {
        List<Piece> returnList = new List<Piece>();
        Coord[] dVects = new Coord[4] { new Coord(1, 0), new Coord(0, 1), new Coord(-1, 0), new Coord(0, -1), };
        foreach (Coord direction in dVects)
        {
            Coord testLoc = location;
            while (testLoc.x >= 0 && testLoc.x < 8 && testLoc.y >= 0 && testLoc.y < 8)
            {
                testLoc += direction;
                if ((LogicMaster.currentBoard[testLoc] != null) && (LogicMaster.currentBoard[testLoc].piece != null) && (LogicMaster.currentBoard[testLoc].piece.GetType() == typeof(Rook)) && (isWhite != LogicMaster.currentBoard[testLoc].piece.isWhite))
                {
                    returnList.Add(LogicMaster.currentBoard[testLoc].piece);
                    break;
                }
                else if ((LogicMaster.currentBoard[testLoc] != null) && LogicMaster.currentBoard[testLoc].piece != null)
                {
                    break;
                }
            }
        }
        return returnList;
    }
    List<Piece> checkQueen()
    {
        List<Piece> returnList = new List<Piece>();
        Coord[] dVects = new Coord[8] { new Coord(1, 1), new Coord(1, -1), new Coord(-1, -1), new Coord(-1, 1), new Coord(1, 0), new Coord(0, -1), new Coord(-1, 0), new Coord(0, 1) };
        foreach (Coord direction in dVects)
        {
            Coord testLoc = location;
            while (testLoc.x >= 0 && testLoc.x < 8 && testLoc.y >= 0 && testLoc.y < 8)
            {
                testLoc += direction;
                if ((LogicMaster.currentBoard[testLoc] != null) && (LogicMaster.currentBoard[testLoc].piece != null) && (LogicMaster.currentBoard[testLoc].piece.GetType() == typeof(Queen)) && (isWhite != LogicMaster.currentBoard[testLoc].piece.isWhite))
                {
                    returnList.Add(LogicMaster.currentBoard[testLoc].piece);
                    break;
                }
                else if ((LogicMaster.currentBoard[testLoc] != null) && LogicMaster.currentBoard[testLoc].piece != null)
                {
                    break;
                }
            }
        }
        return returnList;
    }
    List<Piece> checkKing()
    {
        return null;
    }
    public List<Coord> execution()
    {
        List<Coord> returnList = new List<Coord>();
        foreach (Coord direction in unitCoords)
        {
            if (LogicMaster.currentBoard[location + direction]!= null && LogicMaster.currentBoard[location + direction].piece != null && isWhite != LogicMaster.currentBoard[location + direction].piece.isWhite)
            {
                Coord newLoc = location + direction;
                Coord origLoc = location;
                Piece tempPiece = LogicMaster.currentBoard[newLoc].piece;
                LogicMaster.currentBoard[newLoc].piece = this;
                LogicMaster.currentBoard[origLoc].piece = null;
                checkCheck();
                bool simulcheck = isChecked;
                LogicMaster.currentBoard[newLoc].piece = tempPiece;
                LogicMaster.currentBoard[origLoc].piece = this;
                if (!simulcheck)
                {
                    returnList.Add(newLoc);
                }
            }
        }
        return returnList;
    }
    private void check()
    {
        isChecked = true;
        LogicMaster.check = true;
        MeshRenderer mRend = gameObject.GetComponent<MeshRenderer>();
        mRend.material = Constants.Check;
        if (isWhite)
        {
            LogicMaster.messageString = "White King Is In Check";
        }
        else
        {
            LogicMaster.messageString = "Black King Is In Check";
        }
    }
    private void unCheck()
    {
        LogicMaster.messageString = "";
        isChecked = false;
        LogicMaster.check = false;
        resetColour();
    }
}
