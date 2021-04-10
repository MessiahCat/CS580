using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles: MonoBehaviour
{
    [SerializeField]
    int count;
  public Tiles()
  {
    x = 0;
    y = 0;
    isRoad = false;
  }

  public int x { get; set; }
  public int y { get; set; }

  public TileProperties tileProperties_ { get; set; }
  public TileBuildings tileBuildings_ { get; set; }

    //for road use
  public List<int> InRoad;

  public bool isRoad { get; set; }
    
  private void Start()
  { 
  }
  private void update()
  {
        count = InRoad.Count;
  }

}
