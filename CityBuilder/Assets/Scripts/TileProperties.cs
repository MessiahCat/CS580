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
    occupied_ = false;
    isDirty_ = true;
  }

  public void Update()
  {
    if(isDirty_)
    {
      if (occupied_)
        totalScore_ = 0;
      else 
        totalScore_ = connectionScore_ + densityScore_ + roadsideScore_ + alignmentScore_ + 1;
      isDirty_ = false;
    }
  }

  public GridPos pos_ { get; set; }
  public int totalScore_ { get; set; }
  public int connectionScore_ { get; set; }
  public int densityScore_ { get; set; }
  public int roadsideScore_ { get; set; }
  public int alignmentScore_ { get; set; }
  public bool occupied_ { get; set; }
  public bool isDirty_ { get; set; }
}