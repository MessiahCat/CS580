using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road
{
  public int number_ { get; set; }

  public int startP { get; set; }
  public int endP { get; set; }
  public int pos_ { get; set; }
  public int type_ { get; set; }
  public int connected_ { get; set; }

  public int Connect_Buildings { get; set; }

  public Road(int number,int type, int start, int end,int pos,int build) 
  {
        this.number_ = number;
        this.type_ = type;
        this.startP = start;
        this.endP = end;
        this.pos_ = pos;
        this.Connect_Buildings = build;
  }
}
