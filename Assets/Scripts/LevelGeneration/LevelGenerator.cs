using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour 
{
    public TileSet TileSet;
    public Vector3 SpawnLocation;
    public int BlockHeight;
    public int BlockWidth;
    public int Seed;

	// Use this for initialization
	void Start () 
    {
        CreateSimpleLevelBlock(TileSet, SpawnLocation, BlockHeight, BlockWidth, Seed);
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public static void CreateSimpleLevelBlock(TileSet tileSet, Vector3 botLeftCorner, int blockHeight, int blockWidth, int seed = -1)
    {
        // Random?
        if (seed >= 0)
            Random.seed = seed;

        if(blockHeight%2 == 1)
        {
            blockHeight--;
            Debug.Log("LevelGenerator.CreateSimpleLevelBlock: height must be divideable by two");
        }

        GameObject parent = new GameObject();
        parent.name = tileSet.name + " " + blockHeight + " x " + blockWidth + " seed: " + Random.seed;
        

        for (int x = 0; x < blockWidth; x++)
        {
            for (int y = 0; y < blockHeight; y++)
            {
                GameObject Element;
                Vector3 Offset = Vector3.zero;
                // Bottom
                if (y == 0)
                {
                    Element = LeftMiddleRight(0, blockWidth - 1, x, tileSet.BottomCorners, tileSet.Bottoms);
                }// Top
                else if (y == blockHeight - 1)
                {
                    Element = LeftMiddleRight(0, blockWidth - 1, x, tileSet.TopCorners, tileSet.Tops);
                }
                else
                {
                    Element = LeftMiddleRight(0, blockWidth - 1, x, tileSet.Sides, tileSet.Fills);
                    // Ignore all the even height side objects
                    if (x == 0 || x == blockWidth-1)
                    {
                        if(y % 2 == 0 ) 
                        {
                            GameObject.DestroyImmediate(Element);
                            continue;
                        }   
                        else 
                            Offset.y += 0.5f;
                    }
                }
               
                // Set position
                Element.transform.position = tileSet.TileSize * (new Vector3(x + 0.5f , y + 0.5f) + Offset);
                // Parent
                Element.transform.parent = parent.transform;
            }
        }

        parent.transform.position = botLeftCorner;
    }
    private static GameObject LeftMiddleRight(int min, int max, int index, List<GameObject> Side, List<GameObject> Middle)
    {
        // BotCorner left
        if (index == min)
        {
            return RandomGameObjectFromList(Side);
        } // BotCorner right
        else if (index == max)
        {
            GameObject toFlip = RandomGameObjectFromList(Side);
            Debug.Log(toFlip.name+ " "+ toFlip.transform.localScale);
            Vector3 localScale = toFlip.transform.localScale;
            localScale.x *= -1;
            toFlip.transform.localScale = localScale;
           
            return toFlip;
        } // Bottom
        else
        {
            return RandomGameObjectFromList(Middle);
        }
    }

    private static GameObject RandomGameObjectFromList(List<GameObject> gos)
    {
        return GameObject.Instantiate(gos[Random.Range(0, gos.Count)]) as GameObject;
    }
}
