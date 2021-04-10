using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Info
{
  public Info()
  {
    growth_ = 0;
    density_ = 0;
    startPopulation_ = 0;

    population_ = 0;
    steps_ = 0;
    buildings_ = new List<TileBuildings>();
    roads_ = new List<Road>();
    cost_ = 0;
    gridScore_ = 0;
    maxScore_ = 0;
    residential_ = 0;
    business_ = 0;
    utility_ = 0;
  }

  public Info(float growth, int density, int startPopulation)
  {
    growth_ = growth;
    density_ = density;
    startPopulation_ = startPopulation;

    population_ = 0;
    steps_ = 0;
    cost_ = 0;
    gridScore_ = 0;
    maxScore_ = 0;
    residential_ = 0;
    business_ = 0;
    utility_ = 0;
  }

  public float growth_ { get; set; }
  public int density_ { get; set; }
  public int startPopulation_ { get; set; }

  public int population_ { get; set; }
  public int steps_ { get; set; }
  public List<TileBuildings> buildings_ { get; set; }
  public List<Road> roads_ { get; set; }
  public int cost_ { get; set; }
  public int gridScore_ { get; set; }
  public int maxScore_ { get; set; }
  public int residential_ { get; set; }
  public int business_ { get; set; }
  public int utility_ { get; set; }

}