using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tiles: MonoBehaviour
{
  public Tiles()
  {
    x = 0;
    y = 0;
  }

  // Start is called before the first frame update
  public int x { get; set; }
  public int y { get; set; }

  public TileProperties tileProperties_ { get; set; }
  public TileBuildings tileBuildings_ { get; set; }


  private void Start()
  { 
  }


}
