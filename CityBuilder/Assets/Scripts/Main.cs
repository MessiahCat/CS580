using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
  //Please Keep the Ratio of Col::Row to 16::9
  //rows
  [SerializeField]
  private int row_y;
  //columns
  [SerializeField]
  private int col_x;
  //tile prefab
  [SerializeField]
  private GameObject tile;
  // 0 is nothing, 1 is a single step, 2 is all steps
  [SerializeField]
  private int mode;
  //seed for rng
  [SerializeField]
  private int seed;
  [SerializeField]
  private int growth;
  [SerializeField]
  private int density;
  [SerializeField]
  private int startPopulation;

  //whether what the grid is drawing should be updated
  static bool redraw = true;

  public class LoopHolder
  {
    public LoopHolder(int hold)
    {
      value_ = hold;
    }

    public int value_ { get; set; }
  }

  //for single steps in step 2
  static LoopHolder step2SetRoadLoop = new LoopHolder(0);
  static LoopHolder step2RemoveRoadLoop = new LoopHolder(0);
  static LoopHolder step2ConnectSingleBuildingLoop = new LoopHolder(0);
  static LoopHolder step2CleanUpRoadsLoop = new LoopHolder(0);
  static LoopHolder step5CleanUpRoadsLoop = new LoopHolder(0);
  static bool step2complete = false;

  //for single steps in step 3
  static LoopHolder step3SetRoadLoop = new LoopHolder(0);
  static LoopHolder step3RemoveRoadLoop = new LoopHolder(0);
  static LoopHolder step3ConnectSingleBuildingLoop = new LoopHolder(0);
  static LoopHolder step3CleanUpRoadsLoop = new LoopHolder(0);

  static bool once = true;

  //camera stuff
  static Vector3 camRotation = new Vector3();
  static int cameraStatus = 1;
  //top view
  //pos 140, -80, -130
  //rot 0,0,0

  //ideal everything
  //pos 250, -190, -174
  //rot -30,-20,20
  static Vector3 topPos = new Vector3(140,-80,-135);
  static Vector3 topRot = new Vector3(0,0,0);
  static Vector3 angledPos = new Vector3(250,-190,-174);
  static Vector3 angledRot = new Vector3(-30,-20,20);

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
    if (mode == 0)
      mode = 0;
    if (seed == 0)
      seed = 42;
    if (growth == 0)
      growth = 1;
    if (density == 0)
      density = 1;
    if (startPopulation == 0)
      startPopulation = 10;

    //build the grid
    grid = new TileGrid(col_x, row_y);
    //Set info
    grid.info_.growth_ = growth;
    grid.info_.density_ = density;
    grid.info_.startPopulation_ = startPopulation;
    grid.info_.population_ = startPopulation;

    SetTileMap();
    SetCamera();

    //init random seed
    Random.InitState(seed);
    Random.Range(1, 4);

    //x from left to right is 0 to n;
    //y from top to bot is 0 to n;

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

        newTile.transform.position = new Vector3(startpoint.x + tileSize * j, startpoint.y - tileSize * i, 0);
        newTile.GetComponent<Tiles>().x = j;
        newTile.GetComponent<Tiles>().y = i;
        newTile.GetComponent<Tiles>().tileProperties_ = new TileProperties(new GridPos(j, i));
        grid.gameObjects_.Add(newTile);
      }
    }

    //Updates the scores of the grid and total
    grid.UpdateGridScore();
  }

  void SetCamera()
  {
    Camera.main.transform.Translate(140, -80, -125);
    //Camera.main.orthographicSize = 27 * col_x / 16;

    //float rotateX = 50;
    //Camera.main.transform.eulerAngles = new Vector3(0, 0, 0);

    //Vector3 rotateValue = rotateValue = new Vector3(x, y * -1, 0);
    //transform.eulerAngles = transform.eulerAngles - rotateValue;
  }

  //Draws the changes to grid
  void Draw()
  {
    //draw new grid
    if (redraw)
    {
      int x = 0;
      int y = 0;
      int i = 0;

      //Debug.Log("max score = " + grid.info_.maxScore_);

      foreach (GameObject g in grid.gameObjects_)
      {
        float scorePercent = (float)g.GetComponent<Tiles>().tileProperties_.totalScore_ / (grid.info_.maxScore_ * 2);
        if (scorePercent > 1.0f)
          scorePercent = 1;
        g.GetComponent<SpriteRenderer>().color = new Color(1.0f - scorePercent, 1.0f - scorePercent, 1.0f);

        //Give tiles with buildings a unique color
        if (g.GetComponent<Tiles>().tileBuildings_ != null)
        {
          if (g.GetComponent<Tiles>().tileBuildings_.type_ == 1)
            g.GetComponent<SpriteRenderer>().color = Color.yellow;
          else if (g.GetComponent<Tiles>().tileBuildings_.type_ == 2)
            g.GetComponent<SpriteRenderer>().color = Color.magenta;
          else if (g.GetComponent<Tiles>().tileBuildings_.type_ == 3)
            g.GetComponent<SpriteRenderer>().color = Color.red;
        }
        if (g.GetComponent<Tiles>().InRoad.Count != 0)
        {
          g.GetComponent<SpriteRenderer>().color = Color.black;
        }

        ++i;
        x = i % grid.columns_;
        y = i / grid.rows_;
      }
      redraw = false;

      //grid.GetTile(47, 26).GetComponent<SpriteRenderer>().color = Color.black;
    }
  }

  void InputManager()
  {
    //A single step
    if (Input.GetKeyDown(KeyCode.N))
    {
      mode = 1;
    }

    if (Input.GetKeyDown(KeyCode.D))
      Camera.main.transform.Translate(10, 0, 0);
    if (Input.GetKeyDown(KeyCode.A))
      Camera.main.transform.Translate(-10, 0, 0);

    if (Input.GetKeyDown(KeyCode.W))
      Camera.main.transform.Translate(0, 10, 0);
    if (Input.GetKeyDown(KeyCode.S))
      Camera.main.transform.Translate(0, -10, 0);

    if (Input.GetKeyDown(KeyCode.T))
      Camera.main.transform.Translate(0, 0, 10);
    if (Input.GetKeyDown(KeyCode.Y))
      Camera.main.transform.Translate(0, 0, -10);

    if (Input.GetKeyDown(KeyCode.Q))
    {
      camRotation.z -= 10;
      camRotation.y += 10;
      Camera.main.transform.rotation = Quaternion.Euler(camRotation);
    }
    if (Input.GetKeyDown(KeyCode.E))
    {
      camRotation.z += 10;
      camRotation.y -= 10;
      Camera.main.transform.rotation = Quaternion.Euler(camRotation);
    }
    
    if (Input.GetKeyDown(KeyCode.Z))
    {
      camRotation.x -= 10;
      Camera.main.transform.rotation = Quaternion.Euler(camRotation);
    }
    
    if (Input.GetKeyDown(KeyCode.X))
    {
      camRotation.x += 10;
      Camera.main.transform.rotation = Quaternion.Euler(camRotation);
    }

    if (Input.GetKeyDown(KeyCode.B))
    {
      if (cameraStatus == 1)
      {
        cameraStatus = 2;
        Camera.main.transform.Translate(angledPos - Camera.main.transform.position);
        Camera.main.transform.rotation = Quaternion.Euler(angledRot);
      }  
      else if (cameraStatus == 2)
      {
        cameraStatus = 1;
        Camera.main.transform.rotation = Quaternion.Euler(topRot);
        Camera.main.transform.Translate(topPos - Camera.main.transform.position);
        
      }

    }
  }

  void Steps()
  {
    if (mode == 1)
    {
      //Carry on one step
      SingleStep();
      mode = 0;

      redraw = true;
    }
    else if (mode == 2)
    {
      //Continuously do steps until finished
      if (SingleStep())
        mode = 0;

      redraw = true;
    }


  }

  bool SingleStep()
  {
    //Debug.Log("Starting Single Step");

    //keep doing a step until it's finished, then move on
    if (Step1())
      if (Step2())
        if (Step3())
          if (Step4())
            if (Step5())
              return true;

    //Debug.Log("Finishing Single Step");

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
      //Debug.Log("Finished Step1");
      return true;
    }

    //random tile to chose, higher score, higher chance
    int totalScore = Random.Range(0, grid.info_.gridScore_);
    //Debug.Log("totalScore is " + totalScore);

    foreach (GameObject g in grid.gameObjects_)
    {
      if ((totalScore -= g.GetComponent<Tiles>().tileProperties_.totalScore_) <= 0)
      {
        //Debug.Log("Placing a building");

        //total score of 0 cannot be picked

        //declare occupied
        g.GetComponent<Tiles>().tileProperties_.occupied_ = true;

        //place resident->business->utility
        if (grid.info_.residential_ < grid.info_.population_)
        {
          g.GetComponent<Tiles>().tileBuildings_ = new TileBuildings(g.GetComponent<Tiles>().tileProperties_.pos_, 1, ++grid.info_.residential_);
          grid.info_.buildings_.Add(g.GetComponent<Tiles>().tileBuildings_);
          //Debug.Log("Resident Placed");
        }
        else if (grid.info_.business_ < grid.info_.population_ / 3)
        {
          g.GetComponent<Tiles>().tileBuildings_ = new TileBuildings(g.GetComponent<Tiles>().tileProperties_.pos_, 2, ++grid.info_.business_);
          grid.info_.buildings_.Add(g.GetComponent<Tiles>().tileBuildings_);
        }
        else if (grid.info_.utility_ < grid.info_.population_ / 5)
        {
          g.GetComponent<Tiles>().tileBuildings_ = new TileBuildings(g.GetComponent<Tiles>().tileProperties_.pos_, 3, ++grid.info_.utility_);
          grid.info_.buildings_.Add(g.GetComponent<Tiles>().tileBuildings_);
        }

        grid.info_.buildingCount_++;
        grid.SpreadDensity(g.GetComponent<Tiles>().tileProperties_.pos_);
        grid.SpreadAlignment(g.GetComponent<Tiles>().tileProperties_.pos_);

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
    if (!step2complete)
    {
      if (SetRoads(step2SetRoadLoop))
        if (Road_Remover(step2RemoveRoadLoop))
          if (Connect_SingleBuilding(step2ConnectSingleBuildingLoop))
            if (CleanUpRoads(step2CleanUpRoadsLoop))
            {
              Debug.Log("completed step2");
              step2complete = true;
              return true;
            }
      return false;
    }
    return true;
  }
  bool CleanUpRoads(LoopHolder looper)
  {
    //if gone through all buildings, step complete
    if (looper.value_ == grid.info_.roads_.Count)
    {
      //do once
      looper.value_++;
      grid.SpreadRoadSide();
      grid.UpdateGridScore();
      return true;
    }
    if (looper.value_ == grid.info_.roads_.Count + 1)
    {
      return true;
    }

    bool nothingHappened = true;

    Road r = grid.info_.roads_[looper.value_];
    if (r.type_ == 0 && r.startP == 0)
    {
      int i = 0;
      int type = r.type_;
      int y = r.pos_;
      int x = r.startP;
      bool loop = true;
      while (loop == true)
      {
        if (Deletable(grid.Tile(x, y), type))
        {
          grid.Tile(x, y).GetComponent<Tiles>().InRoad.Clear();
          x++;
        }
        else
        {
          loop = false;
        }
      }
      r.startP = x;

      nothingHappened = false;
    }
    if (r.type_ == 0 && r.endP == col_x - 1)
    {
      int i = 0;
      int type = r.type_;
      int y = r.pos_;
      int x = r.endP;
      bool loop = true;
      while (loop == true)
      {
        if (Deletable(grid.Tile(x, y), type))
        {
          grid.Tile(x, y).GetComponent<Tiles>().InRoad.Clear();
          x--;
        }
        else
        {
          loop = false;
        }
      }
      r.endP = x;

      nothingHappened = false;
    }
    if (r.type_ == 1 && r.endP == row_y - 1)
    {
      int i = 0;
      int type = r.type_;
      int x = r.pos_;
      int y = r.endP;
      bool loop = true;
      while (loop == true)
      {
        if (Deletable(grid.Tile(x, y), type))
        {
          grid.Tile(x, y).GetComponent<Tiles>().InRoad.Clear();
          y--;
        }
        else
        {
          loop = false;
        }
      }
      r.endP = y;

      nothingHappened = false;
    }
    if (r.type_ == 1 && r.startP == 0)
    {
      int i = 0;
      int type = r.type_;
      int x = r.pos_;
      int y = r.startP;
      bool loop = true;
      while (loop == true)
      {
        if (Deletable(grid.Tile(x, y), type))
        {
          grid.Tile(x, y).GetComponent<Tiles>().InRoad.Clear();
          y++;
        }
        else
        {
          loop = false;
        }
      }
      r.startP = y;

      nothingHappened = false;
    }

    if(nothingHappened)
    {
      looper.value_++;
      return CleanUpRoads(looper);
    }

    looper.value_++;
    return false;
  }
  bool Deletable(GameObject tile, int type)
  {
    int x = tile.GetComponent<Tiles>().x;
    int y = tile.GetComponent<Tiles>().y;
    if (tile.GetComponent<Tiles>().InRoad.Count > 1)
    {
      return false;
    }
    if (type == 0)
    {
      if (y - 1 >= 0 && grid.Tile(x, y - 1).GetComponent<Tiles>().tileBuildings_ != null)
      {
        return false;
      }
      if (y + 1 < row_y && grid.Tile(x, y + 1).GetComponent<Tiles>().tileBuildings_ != null)
      {
        return false;
      }
    }
    if (type == 1)
    {
      if (x - 1 >= 0 && grid.Tile(x - 1, y).GetComponent<Tiles>().tileBuildings_ != null)
      {
        return false;
      }
      if (x + 1 < col_x && grid.Tile(x + 1, y).GetComponent<Tiles>().tileBuildings_ != null)
      {
        return false;
      }
    }
    return true;
  }
  bool Connect_SingleBuilding(LoopHolder loop)
  {
    //if gone through all buildings, step complete
    if (loop.value_ == grid.info_.buildingCount_)
    {
      return true;
    }

    //Debug.Log("step2RemoveRoadLoop = " + step2RemoveRoadLoop);
    if (is_single(grid.GetTile(grid.info_.buildings_[loop.value_].pos_)))
    {
      //retieve x and y from building
      int x = grid.info_.buildings_[loop.value_].pos_.x;
      int y = grid.info_.buildings_[loop.value_].pos_.y;

      int shortest = 1;
      //0:up, 1:down, 2:left, 3:right
      int direction = 0;
      bool found = false;
      while (found == false)
      {
        if (x - shortest >= 0 && grid.Tile(x - shortest, y).GetComponent<Tiles>().InRoad.Count != 0)
        {
          direction = 2;
          break;
        }
        if (x + shortest < col_x && grid.Tile(x + shortest, y).GetComponent<Tiles>().InRoad.Count != 0)
        {
          direction = 3;
          break;
        }
        if (y - shortest >= 0 && grid.Tile(x, y - shortest).GetComponent<Tiles>().InRoad.Count != 0)
        {
          direction = 0;
          break;
        }
        if (y + shortest < row_y && grid.Tile(x, y + shortest).GetComponent<Tiles>().InRoad.Count != 0)
        {
          direction = 1;
          break;
        }

        shortest++;
      }
      int index = generate_index();
      if (direction == 0)
      {
        for (int i = 1; i <= shortest; i++)
        {
          grid.Tile(x, y - i).GetComponent<Tiles>().InRoad.Add(index);
          
        }
        grid.info_.roads_.Add(new Road(index, 1, y-shortest, y, x, 0));
      }
      if (direction == 1)
      {
        for (int i = 1; i <= shortest; i++)
        {
          grid.Tile(x, y + i).GetComponent<Tiles>().InRoad.Add(index);
        }
          grid.info_.roads_.Add(new Road(index, 1, y, y + shortest, x, 0));
      }
      if (direction == 2)
      {
        for (int i = 1; i <= shortest; i++)
        {
          grid.Tile(x - i, y).GetComponent<Tiles>().InRoad.Add(index);
        }
        grid.info_.roads_.Add(new Road(index, 0, x- shortest, x, y, 0));
      }
      if (direction == 3)
      {
        for (int i = 1; i <= shortest; i++)
        {
          grid.Tile(x + i, y).GetComponent<Tiles>().InRoad.Add(index);
        }
          grid.info_.roads_.Add(new Road(index, 0, x, x + shortest, y, 0));
      }

    }
    else
    {
      loop.value_++;
      return Connect_SingleBuilding(loop);
    }

    loop.value_++;
    return false;
  }
  int generate_index()
  {
    for (int i = 0; i < 1000000; i++)
    {
      bool inList = false;
      foreach (Road r in grid.info_.roads_)
      {
        if (r.number_ == i)
        {
          inList = true;
        }
      }
      if (inList == false)
      {
        return i;
      }
    }
    return 999;
  }
  bool is_single(GameObject tile)
  {
    int x = tile.GetComponent<Tiles>().x;
    int y = tile.GetComponent<Tiles>().y;
    if (x - 1 >= 0 && grid.Tile(x - 1, y).GetComponent<Tiles>().InRoad.Count != 0)
    {
      return false;
    }
    if (x + 1 < col_x && grid.Tile(x + 1, y).GetComponent<Tiles>().InRoad.Count != 0)
    {
      return false;
    }
    if (y - 1 >= 0 && grid.Tile(x, y - 1).GetComponent<Tiles>().InRoad.Count != 0)
    {
      return false;
    }
    if (y + 1 < row_y && grid.Tile(x, y + 1).GetComponent<Tiles>().InRoad.Count != 0)
    {
      return false;
    }
    return true;
  }
  void Caculate_Conection()
  {
    foreach (Road r in grid.info_.roads_)
    {
      int total = 0;
      int num = r.number_;
      int start = r.startP;
      int end = r.endP;
      int type = r.type_;
      int pos = r.pos_;
      if (type == 0)
      {
        for (int i = start; i <= end; i++)
        {
          foreach (int j in grid.Tile(i, pos).GetComponent<Tiles>().InRoad)
          {
            if (j != num)
            {
              total += 1;
            }
          }
        }
      }
      if (type == 1)
      {
        for (int i = start; i <= end; i++)
        {
          foreach (int j in grid.Tile(pos, i).GetComponent<Tiles>().InRoad)
          {
            if (j != num)
            {
              total += 1;
            }
          }
        }
      }
      r.connected_ = total;
    }
  }
  bool Road_Remover(LoopHolder loop)
  {
    if (loop.value_ == grid.info_.buildingCount_)
    {
      //this is to only call it once
      loop.value_++;
      
      return true;
    }
    if (loop.value_ == grid.info_.buildingCount_ + 1)
    {
      return true;
    }

    //Debug.Log("step2RemoveRoadLoop = " + step2RemoveRoadLoop);

    //retieve x and y from building
    List<int> road = new List<int>();
    int x = grid.info_.buildings_[loop.value_].pos_.x;
    int y = grid.info_.buildings_[loop.value_].pos_.y;

    //Debug.Log("grid pos = " + x + "," + y);

    if (x - 1 >= 0)
    {
      road.AddRange(grid.Tile(x - 1, y).GetComponent<Tiles>().InRoad);
    }
    if (x + 1 < col_x)
    {
      road.AddRange(grid.Tile(x + 1, y).GetComponent<Tiles>().InRoad);
    }
    if (y + 1 < row_y)
    {
      road.AddRange(grid.Tile(x, y + 1).GetComponent<Tiles>().InRoad);
    }
    if (y - 1 >= 0)
    {
      road.AddRange(grid.Tile(x, y - 1).GetComponent<Tiles>().InRoad);
    }

    if (road.Count != 0)
    {
      int max = 0;
      int roadTokeep = -1;
      foreach (int rn in road)
      {
        foreach (Road r in grid.info_.roads_)
        {
          if (r.number_ == rn)
          {
            if (r.Connect_Buildings + r.connected_ > max)
            {
              max = r.Connect_Buildings + r.connected_;
              roadTokeep = r.number_;
            }
          }
        }

      }
      if (max == 0)
      {
        foreach (int rn in road)
        {
          delete_Road(rn);

        }
      }
      else
      {
        foreach (int rn in road)
        {
          if (rn != roadTokeep)
          {
            delete_Road(rn);
          }
        }
      }
    }

    loop.value_++;
    return false;
  }
  void delete_Road(int Rnum)
  {
    Road rRmove = null;
    foreach (Road r in grid.info_.roads_)
    {
      if (r.number_ == Rnum)
      {
        int start = r.startP;
        int end = r.endP;
        int type = r.type_;
        int pos = r.pos_;
        if (type == 0)
        {
          for (int i = start; i <= end; i++)
          {
            grid.Tile(i, pos).GetComponent<Tiles>().InRoad.Remove(Rnum);
          }
        }
        if (type == 1)
        {
          for (int i = start; i <= end; i++)
          {
            grid.Tile(pos, i).GetComponent<Tiles>().InRoad.Remove(Rnum);
          }
        }

        rRmove = r;

      }
    }
    if (rRmove != null)
    {
      grid.info_.roads_.Remove(rRmove);
    }

  }
  bool SetRoads(LoopHolder loop)
  {
    //if gone through all buildings, step complete
    if (loop.value_ == grid.info_.buildingCount_)
    {
      //this is to only call it once
      loop.value_++;
      Caculate_Conection();
      return true;
    }
    if(loop.value_ == grid.info_.buildingCount_ + 1)
    {
      return true;
    }

    //retieve x and y from building
    int x = grid.info_.buildings_[loop.value_].pos_.x;
    int y = grid.info_.buildings_[loop.value_].pos_.y;

    //vertical left
    if (Road_vertical(x - 1, y, grid.info_.roadCount_) == true)
    {
      grid.info_.roadCount_++;
    }
    //vertical right
    if (Road_vertical(x + 1, y, grid.info_.roadCount_) == true)
    {
      grid.info_.roadCount_++;
    }
    //Horizon bot
    if (Road_Horrizon(x, y + 1, grid.info_.roadCount_) == true)
    {
      grid.info_.roadCount_++;
    }
    //Horizon Top
    if (Road_Horrizon(x, y - 1, grid.info_.roadCount_) == true)
    {
      grid.info_.roadCount_++;
    }

    //move loop
    loop.value_++;

    //step is not complete
    return false;
  }
  bool Road_Horrizon(int x, int y, int i)
  {
    int xs = x - 1;//to left, min
    int xe = x;//to right +
    int start = 0;
    int end = col_x - 1;
    int builds = 0;
    if (y >= 0 && y < row_y && grid.Tile(x, y).GetComponent<Tiles>().tileBuildings_ == null)
    {
      while (xs >= 0 || xe < col_x)
      {
        if (xs >= 0)
        {
          if (grid.Tile(xs, y).GetComponent<Tiles>().tileBuildings_ == null /*&& grid.Tile(xs, y).GetComponent<Tiles>().InRoad.Count == 0*/)
          {
            if (y + 1 < row_y && grid.Tile(xs, y + 1).GetComponent<Tiles>().tileBuildings_ != null)
            {
              builds++;
            }
            if (y - 1 >= 0 && grid.Tile(xs, y - 1).GetComponent<Tiles>().tileBuildings_ != null)
            {
              builds++;
            }
            grid.Tile(xs, y).GetComponent<Tiles>().InRoad.Add(i);
            xs--;
          }
          //else if (grid.Tile(xs, y).GetComponent<Tiles>().tileBuildings_ == null && grid.Tile(xs, y).GetComponent<Tiles>().InRoad.Count != 0)
          //{
          //    grid.Tile(xs, y).GetComponent<Tiles>().InRoad.Add(i);
          //    //start = xs;
          //    //xs = -1;
          //    xs--;
          //}
          else
          {
            start = xs + 1;
            xs = -1;
          }
        }

        if (xe < col_x)
        {
          if (grid.Tile(xe, y).GetComponent<Tiles>().tileBuildings_ == null /*&& grid.Tile(xe, y).GetComponent<Tiles>().InRoad.Count == 0*/)
          {
            if (y + 1 < row_y && grid.Tile(xe, y + 1).GetComponent<Tiles>().tileBuildings_ != null)
            {
              builds++;
            }
            if (y - 1 >= 0 && grid.Tile(xe, y - 1).GetComponent<Tiles>().tileBuildings_ != null)
            {
              builds++;
            }
            grid.Tile(xe, y).GetComponent<Tiles>().InRoad.Add(i);
            xe++;
          }
          //else if (grid.Tile(xe, y).GetComponent<Tiles>().tileBuildings_ == null && grid.Tile(xe, y).GetComponent<Tiles>().InRoad.Count != 0)
          //{
          //    grid.Tile(xe, y).GetComponent<Tiles>().InRoad.Add(i);
          //    //end = xe;
          //    //xe = 99999;
          //    xe++;
          //}
          else
          {
            end = xe - 1;
            xe = 99999;
          }
        }
      }
      grid.info_.roads_.Add(new Road(i, 0, start, end, y, builds));
      return true;

    }
    return false;
  }
  bool Road_vertical(int x, int y, int i)
  {
    int x1 = x;
    int ys = y - 1;//to top, min
    int ye = y;//to bot +
    int start = 0;
    int end = row_y - 1;
    int builds = 0;
    if (x >= 0 && x < col_x && grid.Tile(x1, y).GetComponent<Tiles>().tileBuildings_ == null)
    {
      while (ys >= 0 || ye < row_y)
      {
        if (ys >= 0)
        {
          if (grid.Tile(x1, ys).GetComponent<Tiles>().tileBuildings_ == null /*&& grid.Tile(x1, ys).GetComponent<Tiles>().InRoad.Count == 0*/)
          {
            if (x + 1 < col_x && grid.Tile(x + 1, ys).GetComponent<Tiles>().tileBuildings_ != null)
            {
              builds++;
            }
            if (x - 1 >= 0 && grid.Tile(x - 1, ys).GetComponent<Tiles>().tileBuildings_ != null)
            {
              builds++;
            }
            grid.Tile(x1, ys).GetComponent<Tiles>().InRoad.Add(i);
            ys--;
          }
          //else if (grid.Tile(x1, ys).GetComponent<Tiles>().tileBuildings_ == null && grid.Tile(x1, ys).GetComponent<Tiles>().InRoad.Count != 0)
          //{
          //    grid.Tile(x1, ys).GetComponent<Tiles>().InRoad.Add(i);
          //    //start = ys;
          //    //ys = -1;
          //    ys--;
          //}
          else
          {
            start = ys + 1;
            ys = -1;
          }
        }

        if (ye < row_y)
        {
          if (grid.Tile(x1, ye).GetComponent<Tiles>().tileBuildings_ == null /*&& grid.Tile(x1, ye).GetComponent<Tiles>().InRoad.Count == 0*/)
          {
            if (ye + 1 < row_y && grid.Tile(x, ye + 1).GetComponent<Tiles>().tileBuildings_ != null)
            {
              builds++;
            }
            if (ye - 1 >= 0 && grid.Tile(x, ye - 1).GetComponent<Tiles>().tileBuildings_ != null)
            {
              builds++;
            }
            grid.Tile(x1, ye).GetComponent<Tiles>().InRoad.Add(i);
            ye++;
          }
          //else if (grid.Tile(x1, ye).GetComponent<Tiles>().tileBuildings_ == null && grid.Tile(x1, ye).GetComponent<Tiles>().InRoad.Count != 0)
          //{
          //    grid.Tile(x1, ye).GetComponent<Tiles>().InRoad.Add(i);
          //    //end = ye;
          //    //ye = 99999;
          //    ye++;
          //}
          else
          {
            end = ye - 1;
            ye = 99999;
          }
        }
      }
      grid.info_.roads_.Add(new Road(i, 1, start, end, x, builds));
      return true;

    }

    return false;
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
    //to use previous steps, call this
    if(once == true)
    {
      grid.info_.population_ += 10;
      once = true;
      //step2complete = false;

      //step2SetRoadLoop.value_ = 0;
      //step2RemoveRoadLoop.value_ = 0;
      //step2ConnectSingleBuildingLoop.value_ = 0;
      //step2CleanUpRoadsLoop.value_ = 0;
      //return false;
      while (!Step1()) ;
    }

    //while (!SetRoads(step3SetRoadLoop)) ;
    //while (!Road_Remover(step3RemoveRoadLoop)) ;
    //while (!Connect_SingleBuilding(step3ConnectSingleBuildingLoop)) ;
    //while (!CleanUpRoads(step3CleanUpRoadsLoop)) ;

    Step5Helper();

    return true;
  }

  void Step5Helper()
  {
        extentRoad();
        connect_singlebuilding5();
        CleanUpRoads5();
        CleanData();
  }
    void CleanData() 
    {

    }
    void CleanUpRoads5() 
    {
        foreach (Road r in grid.info_.roads_)
        {


            if (r.type_ == 0 && r.startP == 0)
            {
                int i = 0;
                int type = r.type_;
                int y = r.pos_;
                int x = r.startP;
                bool loop = true;
                while (loop == true)
                {
                    if (Deletable(grid.Tile(x, y), type))
                    {
                        grid.Tile(x, y).GetComponent<Tiles>().InRoad.Clear();
                        x++;
                    }
                    else
                    {
                        loop = false;
                    }
                }
                r.startP = x;

            }
            if (r.type_ == 0 && r.endP == col_x - 1)
            {
                int i = 0;
                int type = r.type_;
                int y = r.pos_;
                int x = r.endP;
                bool loop = true;
                while (loop == true)
                {
                    if (Deletable(grid.Tile(x, y), type))
                    {
                        grid.Tile(x, y).GetComponent<Tiles>().InRoad.Clear();
                        x--;
                    }
                    else
                    {
                        loop = false;
                    }
                }
                r.endP = x;
            }
            if (r.type_ == 1 && r.endP == row_y - 1)
            {
                int type = r.type_;
                int x = r.pos_;
                int y = r.endP;
                bool loop = true;
                while (loop == true)
                {
                    if (y < 0)
                    {
                        break;
                    }
                    if (Deletable(grid.Tile(x, y), type))
                    {
                        grid.Tile(x, y).GetComponent<Tiles>().InRoad.Clear();
                        y--;
                    }
                    else
                    {
                        loop = false;
                    }
                }
                r.endP = y;

            }
            if (r.type_ == 1 && r.startP == 0)
            {
                int i = 0;
                int type = r.type_;
                int x = r.pos_;
                int y = r.startP;
                bool loop = true;
                while (loop == true)
                {
                    if (Deletable(grid.Tile(x, y), type))
                    {
                        grid.Tile(x, y).GetComponent<Tiles>().InRoad.Clear();
                        y++;
                    }
                    else
                    {
                        loop = false;
                    }
                }
                r.startP = y;

            }
        }
    }
    void extentRoad() 
    {
        foreach (Road r in grid.info_.roads_)
        {
            if (r.type_ == 0)
            {
                if (r.startP > 0 && grid.Tile(r.startP - 1, r.pos_).GetComponent<Tiles>().tileBuildings_ == null)
                {
                    int x = r.startP;
                    bool until_end = true;
                    for (int i = x; i >= 0; i--)
                    {
                        if (grid.Tile(i, r.pos_).GetComponent<Tiles>().tileBuildings_ == null)
                        {
                            grid.Tile(i, r.pos_).GetComponent<Tiles>().InRoad.Add(r.number_);
                        }

                        else 
                        {
                            r.startP = i+1;
                            until_end = false;
                            break;
                        }
                    }
                    if (until_end)
                    {
                        r.startP = 0;
                    }
                }

                if (r.endP < col_x-1 && grid.Tile(r.endP + 1, r.pos_).GetComponent<Tiles>().tileBuildings_ == null)
                {
                    int x = r.endP;
                    bool until_end = true;
                    for (int i = x; i < col_x; i++)
                    {
                        if (grid.Tile(i, r.pos_).GetComponent<Tiles>().tileBuildings_ == null)
                        {
                            grid.Tile(i, r.pos_).GetComponent<Tiles>().InRoad.Add(r.number_);
                        }

                        else
                        {
                            r.endP = i-1;
                            until_end = false;
                            break;
                        }


                    }
                    if (until_end)
                    {
                        r.endP = col_x-1;
                    }
                }
            }
            if (r.type_ == 1)
            {
                if (r.startP > 0 && grid.Tile(r.pos_, r.startP - 1).GetComponent<Tiles>().tileBuildings_ == null)
                {
                    int y = r.startP;
                    bool until_end = true;
                    for (int i = y; i >= 0; i--)
                    {
                        if (grid.Tile(r.pos_, i).GetComponent<Tiles>().tileBuildings_ == null)
                        {
                            grid.Tile(r.pos_, i).GetComponent<Tiles>().InRoad.Add(r.number_);
                        }

                        else
                        {
                            r.startP = i+1;
                            until_end = false ;
                            break;
                        }
                    }
                    if (until_end)
                    {
                        r.startP = 0;
                    }
                }

                if (r.endP <row_y-1 && grid.Tile(r.pos_, r.endP + 1).GetComponent<Tiles>().tileBuildings_ == null)
                {
                    int y = r.endP;
                    bool until_end = true;
                    for (int i = y; i < row_y; i++)
                    {
                        if (grid.Tile(r.pos_, i).GetComponent<Tiles>().tileBuildings_ == null)
                        {
                            grid.Tile(r.pos_, i).GetComponent<Tiles>().InRoad.Add(r.number_);
                        }

                        else
                        {
                            r.endP = i-1;
                            until_end = false;
                            break;
                        }
                    }
                    if (until_end)
                    {
                        r.endP = row_y - 1;
                    }
                }
            }
        }
    }
    void connect_singlebuilding5() 
    {
        foreach (GameObject g in grid.gameObjects_)
        {
            if (g.GetComponent<Tiles>().tileBuildings_ != null)
            {

                if (is_single(g))
                {
                    //retieve x and y from building
                    int x = g.GetComponent<Tiles>().x;
                    int y = g.GetComponent<Tiles>().y;
                    int shortest = 999;
                    //0:up, 1:down, 2:left, 3:right
                    int direction = 0;
                    // to left
                    #region Solu1
                    if (x >= 2)
                    {
                        int target_x = x-1;
                        bool able = true;
                        while (true)
                        {
                            if (grid.Tile(target_x, y).GetComponent<Tiles>().tileBuildings_ != null)
                            {
                                able = false;
                                break;
                            }
                            if (target_x == 0)
                            {
                                able = false;
                                break;
                            }
                            if (grid.Tile(target_x, y).GetComponent<Tiles>().InRoad.Count != 0)
                            {
                                break;
                            }
                            if (grid.Tile(target_x, y).GetComponent<Tiles>().InRoad.Count == 0)
                            {
                                target_x -= 1;
                            }
                        }
                        if (able == true)
                        {
                            int distance = Mathf.Abs( target_x - x); 
                            if (distance < shortest)
                            {
                                shortest = target_x;
                                direction = 2;
                            }
                        }
                        
                    }
                    // to right
                    if (x < col_x-2)
                    {
                        int target_x = x + 1;
                        bool able = true;
                        while (true)
                        {
                            if (grid.Tile(target_x, y).GetComponent<Tiles>().tileBuildings_ != null)
                            {
                                able = false;
                                break;
                            }
                            if (target_x == col_x-1)
                            {
                                able = false;
                                break;
                            }
                            if (grid.Tile(target_x, y).GetComponent<Tiles>().InRoad.Count != 0)
                            {
                                break;
                            }
                            if (grid.Tile(target_x, y).GetComponent<Tiles>().InRoad.Count == 0)
                            {
                                target_x ++;
                            }
                        }
                        if (able == true)
                        {
                            int distance = Mathf.Abs( target_x - x);
                            if (distance < shortest)
                            {
                                shortest = target_x;
                                direction = 3;
                            }
                        }
                    }
                    // to bot
                    if (y < row_y - 2)
                    {
                        int target_y = y + 1;
                        bool able = true;
                        while (true)
                        {
                            if (grid.Tile(x, target_y).GetComponent<Tiles>().tileBuildings_ != null)
                            {
                                able = false;
                                break;
                            }
                            if (target_y == row_y - 1)
                            {
                                able = false;
                                break;
                            }
                            if (grid.Tile(x, target_y).GetComponent<Tiles>().InRoad.Count != 0)
                            {
                                break;
                            }
                            if (grid.Tile(x, target_y).GetComponent<Tiles>().InRoad.Count == 0)
                            {
                                target_y++;
                            }
                        }
                        if (able == true)
                        {
                            int distance =Mathf.Abs (target_y - y);
                            if (distance < shortest)
                            {
                                shortest = target_y;
                                direction = 1;
                            }
                        }
                    }
                    // to top
                    if (y >= 2)
                    {
                        int target_y = y - 1;
                        bool able = true;
                        while (true)
                        {
                            if (grid.Tile(x, target_y).GetComponent<Tiles>().tileBuildings_ != null)
                            {
                                able = false;
                                break;
                            }
                            if (target_y == 0)
                            {
                                able = false;
                                break;
                            }
                            if (grid.Tile(x, target_y).GetComponent<Tiles>().InRoad.Count != 0)
                            {
                                break;
                            }
                            if (grid.Tile(x, target_y).GetComponent<Tiles>().InRoad.Count == 0)
                            {
                                target_y--;
                            }
                        }
                        if (able == true)
                        {
                            int distance = Mathf.Abs( target_y - y);
                            if (distance < shortest)
                            {
                                shortest = target_y;
                                direction = 0;
                            }
                        }
                    }
                    #endregion

                    

                    int index = generate_index();
                    if (direction == 0)
                    {
                        for (int i = y-1; i >= shortest; i--)
                        {
                            grid.Tile(x, i).GetComponent<Tiles>().InRoad.Add(index);

                        }
                        grid.info_.roads_.Add(new Road(index, 1, shortest, y, x, 0));
                    }
                    if (direction == 1)
                    {
                        for (int i = y+1; i <= shortest; i++)
                        {
                            grid.Tile(x, i).GetComponent<Tiles>().InRoad.Add(index);
                        }
                        grid.info_.roads_.Add(new Road(index, 1, y, shortest, x, 0));
                    }
                    if (direction == 2)
                    {
                        for (int i = x-1; i >= shortest; i++)
                        {
                            grid.Tile(i, y).GetComponent<Tiles>().InRoad.Add(index);
                        }
                        grid.info_.roads_.Add(new Road(index, 0, shortest, x, y, 0));
                    }
                    if (direction == 3)
                    {
                        for (int i = x+1; i <= shortest; i++)
                        {
                            grid.Tile(i, y).GetComponent<Tiles>().InRoad.Add(index);
                        }
                        grid.info_.roads_.Add(new Road(index, 0, x, shortest, y, 0));
                    }

                }
            }
        }
    }
}
