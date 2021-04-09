using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridPos
{
  public GridPos()
  {
    x = 0;
    y = 0;
  }

  public GridPos(int X, int Y)
  {
    x = X;
    y = Y;
  }

  public GridPos(GridPos gridPos)
  {
    x = gridPos.x;
    y = gridPos.y;
  }

  public static bool operator ==(GridPos a, GridPos b)
  {
    if (a.x == b.x && a.y == b.y)
      return true;
    else
      return false;
  }

  public static bool operator !=(GridPos a, GridPos b)
  {
    if (a.x != b.x || a.y != b.y)
      return true;
    else
      return false;
  }

  public int x { get; set; }
  public int y { get; set; }
}