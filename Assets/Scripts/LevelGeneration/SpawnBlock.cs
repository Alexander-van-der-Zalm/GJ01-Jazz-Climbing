using UnityEngine;
using System.Collections;

public class SpawnBlock : MonoBehaviour 
{
    public int BlockWidth;
    public int BlockHeight;
    public TileSet TileSet;
    public int Seed = -1;

    private Vector3 MidOffset;
    
    // Use this for initialization
	void Start () 
    {
        LevelGenerator.CreateSimpleLevelBlock(TileSet, transform.position, BlockHeight, BlockWidth, Seed);
	}

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1,1);
        Vector3 size = Vector3.zero;
        
        if (TileSet != null)
        {
            size = TileSet.TileSize * new Vector3(BlockWidth, BlockHeight);
            MidOffset = 0.5f*size;
            size.y -= 1f;
        }

        //Gizmos.DrawWireCube(transform.position, Vector3.one);
        Gizmos.DrawWireCube(transform.position + MidOffset, size);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 1, 1);
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}
