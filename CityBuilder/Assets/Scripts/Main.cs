using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
  //Please Keep the Ratio of Col::Row to 16::9
  [SerializeField]
  private int row_y;
  [SerializeField]
  private int col_x;
  [SerializeField]
  private GameObject tile;

  //whether what the grid is drawing should be updated
  static bool redraw = true;

  // 0 is nothing, 1 is a single step, 2 is all steps
  static int mode = 0;

  //holds the main grid
  static TileGrid grid;

  // Start is called before the first frame update
  void Start()
  {
    //insure everything is given values
    if (row_y == 0)
      row_y = 1;
    if (col_x == 0)
      col_x = 1;
    if (tile == null)
    {
      tile = Instantiate(Resources.Load("Tile")) as GameObject;
      Debug.Log("Tile never set");
    }

    //build the grid
    grid = new TileGrid(col_x, row_y);
    //test
    grid.info_.population_ = 10;

    SetCamera();
    SetTileMap();

    //init random seed
    Random.InitState(42);
    Random.Range(1, 4);

    //x from left to right is 0 to n;
    //y from top to bot is 0 to n;
    //All the tiles with tag "Tile"

    //Test
    //#region Test & Delete
    //GameObject[] Tiles = GameObject.FindGameObjectsWithTag("Tile");
    //for (int i = 0; i < Tiles.Length; i++)
    //{
    //  if (Tiles[i].GetComponent<Tiles>().y == Tiles[i].GetComponent<Tiles>().x)
    //  {
    //   Tiles[i].GetComponent<SpriteRenderer>().color = Color.red;
    // }
    //
    // if (Tiles[i].GetComponent<Tiles>().tileProperties_.totalScore_ == 1)
    // {
    //   Tiles[i].GetComponent<SpriteRenderer>().color = Color.blue;
    // }
    //}
    //#endregion
    //Endtest

    Debug.Log("Init Complete");
  }

  // Update is called once per frame
  void Update()
  {
    //everything involving updating the grid
    Steps();
    //everything involving drawing an updated grid
    Draw();
    //manages input
    InputManager();
  }


  void SetTileMap() 
  {
    float tileSize = tile.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
    Vector3 startpoint = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height));
    for (int i = 0; i < row_y; i++)
    {
      for (int j = 0; j < col_x; j++)
      {
        GameObject newTile = Instantiate(tile);

        newTile.transform.position = new Vector3(startpoint.x+ tileSize*j,startpoint.y - tileSize*i,0);
        newTile.GetComponent<Tiles>().x = j;
        newTile.GetComponent<Tiles>().y = i;
        newTile.GetComponent<Tiles>().tileProperties_ = new TileProperties(new GridPos(i,j));

        newTile.GetComponent<Tiles>().tileProperties_.totalScore_ = 1;

        //for test use
        //if (j % 2 == 1 && i % 2 == 1 || j % 2 == 0 && i % 2 == 0)
        // {
        //  newTile.GetComponent<Tiles>().tileProperties_.totalScore_ = 1;
        // }
        //end
        grid.gameObjects_.Add(newTile);           
      }
    }

    //Updates the scores of the grid and total
    grid.UpdateGridScore();
  }

  void SetCamera() 
  {
    Camera.main.orthographicSize = 27*col_x/16;

    //float rotateX = 50;
    //Camera.main.transform.eulerAngles = new Vector3(0, 0, 0);

    //Vector3 rotateValue = rotateValue = new Vector3(x, y * -1, 0);
    //transform.eulerAngles = transform.eulerAngles - rotateValue;
  }
  
  //Draws the changes to grid
  void Draw()
  {
    //draw new grid
    if(redraw)
    {
      int x = 0;
      int y = 0;
      int i = 0;
      foreach(GameObject g in grid.gameObjects_)
      {
        //if (g.GetComponent<Tiles>().tileProperties_.totalScore_ == 1)
        //{
          //g.GetComponent<SpriteRenderer>().color = Color.blue;
          float scorePercent = g.GetComponent<Tiles>().tileProperties_.totalScore_ / grid.info_.gridScore_ * 52;
          if (scorePercent > 1.0f)
            scorePercent = 1;
          g.GetComponent<SpriteRenderer>().color = new Color(1.0f, 1.0f, 1.0f - scorePercent);
        //}

        //Give tiles with buildings a unique color
        if (g.GetComponent<Tiles>().tileBuildings_ != null)
        {
          if(g.GetComponent<Tiles>().tileBuildings_.type_ == 1)
            g.GetComponent<SpriteRenderer>().color = Color.yellow;
          else if (g.GetComponent<Tiles>().tileBuildings_.type_ == 2)
            g.GetComponent<SpriteRenderer>().color = Color.magenta;
          else if (g.GetComponent<Tiles>().tileBuildings_.type_ == 3)
            g.GetComponent<SpriteRenderer>().color = Color.red;
        }

        ++i;
        x = i % grid.columns_;
        y = i / grid.rows_;
      }
      redraw = false;
    }
  }
  
  void InputManager()
  {
    //A single step
    if(Input.GetKeyDown(KeyCode.N))
    {
      mode = 1;
    }
  }
  
  void Steps()
  {
    if(mode == 1)
    {
      //Carry on one step
      SingleStep();
      mode = 0;
    }
    else if(mode == 2)
    {
      //Continuously do steps until finished
      if (SingleStep())
        mode = 0;
    }

    redraw = true;
  }

  bool SingleStep()
  {
    Debug.Log("Starting Single Step");

    //keep doing a step until it's finished, then move on
    if (Step1())
      if (Step2())
        if (Step3())
          if (Step4())
            if (Step5())
              return true;

    Debug.Log("Finishing Single Step");

    return false;
  }

  //Step 1 Building placement
//-------
//This is one of the main processes.
//-loop through all tiles to update their properties
//-place a build based on values and seed
//-update surrounding tiles
//-repeat prior two steps
//Building order: place all of one type before moving to the next
//Residential -> Business -> Utility

//Residential 
//-houses
//- 1-4 spaces
//- cannot be placed in occupied spaces nor surrounded by road


//Business 
//-generic store 
//- 1-4 spaces
//- cannot be placed in occupied spaces
//- must be able to reach a % of residential building

//Utility 
//-Fire Station
//-Police Station
//-cannot be placed in occupied spaces
//-must be able to reach every building
  bool Step1()
  {
    //means all buildings are placed
    if (grid.info_.utility_ == grid.info_.population_ / 5)
    {
      Debug.Log("Finished Step1");
      return true;
    }

    //random tile to chose, higher score, higher chance
    int totalScore = Random.Range(0, grid.info_.gridScore_);
    Debug.Log("totalScore is " + totalScore);

    foreach (GameObject g in grid.gameObjects_)
    {
      if((totalScore -= g.GetComponent<Tiles>().tileProperties_.totalScore_) <= 0)
      {
        Debug.Log("Placing a building");

        //total score of 0 cannot be picked
        g.GetComponent<Tiles>().tileProperties_.totalScore_ = 0;

        //place resident->business->utility
        if (grid.info_.residential_ < grid.info_.population_  )
        {
          g.GetComponent<Tiles>().tileBuildings_ = new TileBuildings(g.GetComponent<Tiles>().tileProperties_.pos_, 1, ++grid.info_.residential_);
          grid.info_.buildings_.Add(g.GetComponent<Tiles>().tileBuildings_);
          Debug.Log("Resident Placed");
        }
        else if(grid.info_.business_ < grid.info_.population_/3 )
        {
          g.GetComponent<Tiles>().tileBuildings_ = new TileBuildings(g.GetComponent<Tiles>().tileProperties_.pos_, 2, ++grid.info_.business_);
          
          grid.info_.buildings_.Add(g.GetComponent<Tiles>().tileBuildings_);
        }
        else if (grid.info_.utility_ < grid.info_.population_/5 )
        {
          g.GetComponent<Tiles>().tileBuildings_ = new TileBuildings(g.GetComponent<Tiles>().tileProperties_.pos_, 3, ++grid.info_.utility_);
          
          grid.info_.buildings_.Add(g.GetComponent<Tiles>().tileBuildings_);
        }

        //update grid
        grid.UpdateGridScore();
        break;
      }
    }

    return false;
  }

//Step 2 Connect all buildings with roads
//---
//This is a main process.
//Start a road one each side of a building
//-Roads to the left and right extend vertically until hitting another building or end of grid
//-Roads above and below extend horizontally until hitting a building or end of grid
//-Intersections of roads are considered ‘connected’
//-repeat until all buildings have roads

//-check the number of roads next to a building
//-remove all roads except the one with the most buildings next to it or its‘connected’ roads
//-if build has no roads next to it, create one to the nearest road
//-repeat until done to all buildings

//-trim all roads leading off the grid
  bool Step2()
  {

    return true;
  }

//Step 3: Optimize
//---
//No idea
  bool Step3()
  {

    return true;
  }


//Step 4: Prediction
//---
//Try to extend roads based on rules and population density
  bool Step4()
  {

    return true;
  }

//Step 5: Grow City
//---
//Place more buildings or grow based a growth value
  bool Step5()
  {

    return true;
  }


}
