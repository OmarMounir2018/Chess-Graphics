using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SquareState
{
    public SquareState() { }
    public SquareState(Coord loc, GameObject act, GameObject mark, Piece pc)
    {
        location = loc;
        actor = act;
        marker = mark;
        piece = pc;
    }
    public void hideMarker()
    {
        Renderer renderer = marker.GetComponent<Renderer>();
        renderer.enabled = false;
        marked = false;
    }
    public void showMarker()
    {
        Renderer renderer = marker.GetComponent<Renderer>();
        renderer.enabled = true;
        marked = true;
    }
    public void targetPiece()
    {
        piece.targeted = true;
        MeshRenderer mRenderer = actor.GetComponent<MeshRenderer>();
        mRenderer.material = Constants.Target;
    }
   public Coord location;
   public GameObject actor;
   public GameObject marker;
   public Piece piece;
   public bool marked = false;
}
/// <remarks></remarks>
public class BoardState : IEnumerable
{
    //Private
    public BoardState()
    {
        innerList = new List<SquareState>();
    }
    public BoardState(SquareState[] squares)
    {
        innerList = new List<SquareState>();
        innerList.AddRange(squares);
    }
    internal string[,] defaultBoard = Constants.DefaultBoard;

    List<SquareState> innerList;

    internal SquareState findCoord(Coord location)
    {
        foreach (SquareState square in innerList)
        {
            if (square.location == location)
            {
                return square;
            }
        }
        return null;
    }

    internal SquareState findMarker(GameObject marker)
    {
        foreach (SquareState square in innerList)
        {
            if (square.marker == marker)
            {
                return square;
            }
        }
        return null;
    }

    internal SquareState findActor(GameObject actor)
    {
        foreach (SquareState square in innerList)
        {
            if (square.actor == actor)
            {
                return square;
            }
        }
        return null;
    }

    internal SquareState findPiece(Piece piece)
    {
        foreach (SquareState square in innerList)
        {
            if (square.piece == piece)
            {
                return square;
            }
        }
        return null;
    }
    //Public
    public void add(SquareState item)
    {
        innerList.Add(item);
    }

    public void remove(SquareState item)
    {
        innerList.Remove(item);
    }

    public void addRange(SquareState[] items)
    {
        innerList.AddRange(items);
    }

    public IEnumerator GetEnumerator()
    {
        return ((IEnumerable)innerList).GetEnumerator();
    }

    public SquareState this[Coord coordinate]
    {
        get
        {
            return findCoord(coordinate);
        }
    }

    public SquareState this[GameObject gameObj]
    {
        get
        {
            if (gameObj == null) { return null; }
            if (gameObj.tag == "Marker")
            {
                return findMarker(gameObj);
            }
            else if (gameObj.tag != "Board")
            {
                return findActor(gameObj);
            }
            else { return null; }
        }
    }

    public SquareState this[Piece piece]
    {
        get
        {
            return findPiece(piece);
        }
    }

    public void hideMarkers()
    {
        foreach (SquareState square in innerList)
        {
            square.hideMarker();
        }
    }

    public void resetColours()
    {
        foreach (SquareState square in innerList)
        {
            if (square.piece != null)
            {
                square.piece.resetColour();
            }
        }
    }

    public void untargetAll()
    {
        foreach (SquareState square in innerList)
        {
            if (square.piece != null)
            {
                square.piece.unTarget();
            }
            
        }
    }
}
