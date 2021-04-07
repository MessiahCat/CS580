using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    //Please Keep the Ratio of Col::Row to 16::9
    [SerializeField]
    private int row;
    [SerializeField]
    private int col;
    [SerializeField]
    private GameObject tile;

    // Start is called before the first frame update
    void Start()
    {
        SetCamera();
        SetTileMap();

        //x from left to right is 0 to n;
        //y from top to bow is 0 to n;
        //All the tile with tag "Tile"

        //Test
        #region Test & Delete
        GameObject[] Tiles = GameObject.FindGameObjectsWithTag("Tile");
        for (int i = 0; i < Tiles.Length; i++)
        {
            if (Tiles[i].GetComponent<Tiles>().y == Tiles[i].GetComponent<Tiles>().x)
            {
                Tiles[i].GetComponent<SpriteRenderer>().color = Color.red;
            }

            if (Tiles[i].GetComponent<Tiles>().TileProperties == 1)
            {
                Tiles[i].GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }
        #endregion
        //Endtest
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void SetTileMap() 
    {
        float tileSize = tile.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        Vector3 startpoint = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height));
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                GameObject newTile = Instantiate(tile);
                newTile.transform.position = new Vector3(startpoint.x+ tileSize*j,startpoint.y - tileSize*i,0);
                newTile.GetComponent<Tiles>().x = j;
                newTile.GetComponent<Tiles>().y = i;

                //for test use
                if (i == 8)
                {
                    newTile.GetComponent<Tiles>().TileProperties = 1;
                }
                //end
                    
            }
        }
        
    }

    void SetCamera() 
    {
        Camera.main.orthographicSize = 27*col/16;
    }

}
