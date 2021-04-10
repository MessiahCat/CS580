using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGrid
{
  public TileGrid(int rows, int columns)
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

    foreach (GameObject g in gameObjects_)
    {
      g.GetComponent<Tiles>().tileProperties_.Update();
      info_.gridScore_ += g.GetComponent<Tiles>().tileProperties_.totalScore_;
    }
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
