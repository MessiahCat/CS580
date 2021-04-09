using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileProperties
{
  public TileProperties(GridPos pos)
  {
    pos_ = pos;
    totalScore_ = 0;
    connectionScore_ = 0;
    densityScore_ = 0;
    roadsideScore_ = 0;
    alignmentScore_ = 0;
    isDirty_ = false;
  }

  public void Update()
  {
    if(isDirty_)
    {
      totalScore_ = connectionScore_ + densityScore_ + roadsideScore_ + alignmentScore_;
    }
  }

  public GridPos pos_ { get; set; }
  public int totalScore_ { get; set; }
  public int connectionScore_ { get; set; }
  public int densityScore_ { get; set; }
  public int roadsideScore_ { get; set; }
  public int alignmentScore_ { get; set; }
  public bool isDirty_ { get; set; }
}