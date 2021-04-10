using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid
{
  public TileGrid(int columns, int rows)
  {
    rows_ = rows;
    columns_ = columns;
    info_ = new Info();
    gameObjects_ = new List<GameObject>();
  }

  // Start is called before the first frame update
  public int rows_ { get; set; }
  public int columns_ { get; set; }
  public List<GameObject> gameObjects_ { get; set; }
  public Info info_ { get; set; }

  public void UpdateGridScore()
  {
    info_.gridScore_ = 0;
    info_.maxScore_ = 0;

    foreach (GameObject g in gameObjects_)
    {
      g.GetComponent<Tiles>().tileProperties_.Update();
      info_.gridScore_ += g.GetComponent<Tiles>().tileProperties_.totalScore_;
      if (g.GetComponent<Tiles>().tileProperties_.totalScore_ > info_.maxScore_) 
        info_.maxScore_ = g.GetComponent<Tiles>().tileProperties_.totalScore_;
    }
  }

  public bool InGrid(int x, int y)
  {
    if (x < 0 || x >= columns_)
      return false;
    if (y < 0 || y >= rows_)
      return false;

    return true;
  }

  public void SpreadDensity(GridPos spot)
  {
    foreach (GameObject g in gameObjects_)
    {
      int x2 = g.GetComponent<Tiles>().tileProperties_.pos_.x;
      int y2 = g.GetComponent<Tiles>().tileProperties_.pos_.y;

      float x = (float)(Mathf.Pow(spot.x - x2, 2.0f));
      float y = (float)(Mathf.Pow(spot.y - y2, 2.0f));
      int d = (int)Mathf.Sqrt(x + y);

      int affect =  info_.density_ - d;

      if(affect > 0)
      {
        g.GetComponent<Tiles>().tileProperties_.isDirty_ = true;
        g.GetComponent<Tiles>().tileProperties_.densityScore_ += affect;
      }
    }
  }

  public void SpreadAlignment(GridPos spot)
  {
    for (int i = 0; i != 4; ++i)
    {
      GridPos temp = new GridPos(spot);
      GridPos expansion = new GridPos();

      switch (i)
      {
        case 0:
          expansion.x += 1;
          break;
        case 1:
          expansion.x -= 1;
          break;
        case 2:
          expansion.y += 1;
          break;
        default:
          expansion.y -= 1;
          break;
      }

      temp.x += expansion.x;
      temp.y += expansion.y;
      
      while (InGrid(temp.x,temp.y))
      {
        

        GameObject g = GetTile(temp);
        g.GetComponent<Tiles>().tileProperties_.alignmentScore_ += 5;
        g.GetComponent<Tiles>().tileProperties_.isDirty_ = true;
        temp.x += expansion.x;
        temp.y += expansion.y;
      }

    }
  }

  public GameObject GetTile(GridPos spot)
  {
    int index = spot.x + spot.y * columns_;
    return gameObjects_[index];
  }

  public GameObject GetTile(int x, int y)
  {
    int index = x + y * columns_;
    return gameObjects_[index];
  }

  public GameObject Tile(int x, int y) 
    {
        foreach (GameObject g in gameObjects_) 
        {
            if (g.GetComponent<Tiles>().x == x && g.GetComponent<Tiles>().y == y)
            {
                return g;
            }
        }

        return null;

    }

    private void Start()
    { 
    }


}
