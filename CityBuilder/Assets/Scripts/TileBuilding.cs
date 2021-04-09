using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBuildings
{
  public TileBuildings(GridPos pos, int type, int number)
  {
    number_ = number;
    pos_ = pos;
    type_ = type;
    number++;
  }

  public int number_ { get; set; }
  public GridPos pos_ { get; set; }
  public int type_ { get; set; }
}
