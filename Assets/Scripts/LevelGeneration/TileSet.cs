using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileSet : MonoBehaviour 
{
    public string Name = "TileSet";
    public int TileSize = 2;
    public List<GameObject> TopCorners      = new List<GameObject>();
    public List<GameObject> BottomCorners   = new List<GameObject>();
    public List<GameObject> Sides           = new List<GameObject>();
    public List<GameObject> Tops            = new List<GameObject>();
    public List<GameObject> Bottoms         = new List<GameObject>();
    public List<GameObject> Fills           = new List<GameObject>();
}
