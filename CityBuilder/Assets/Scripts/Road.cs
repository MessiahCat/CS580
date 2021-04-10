using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road
{
  public int number_ { get; set; }

  public int startP { get; set; }
  public int endP { get; set; }
  public GridPos pos_ { get; set; }
  public int type_ { get; set; }
  public int connected_ { get; set; }

  public Road(int number,int type, int start, int end) 
  {
        this.number_ = number;
        this.type_ = type;
        this.startP = start;
        this.endP = end;
  }
}
