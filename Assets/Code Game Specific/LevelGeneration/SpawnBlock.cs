using UnityEngine;
using System.Collections;

public class SpawnBlock : MonoBehaviour 
{
    public int BlockWidth;
    public int BlockHeight;
    public TileSet TileSet;
    public int Seed = -1;

    [HideInInspector]
    public GameObject SpawnedBlock;

    // Use this for initialization
	void Start () 
    {
        //LevelGenerator.CreateSimpleLevelBlock(TileSet, transform.position, BlockHeight, BlockWidth, Seed);
	}

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1,1);
        DrawGizmo();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 1, 1);
        DrawGizmo();
    }

    private void DrawGizmo()
    {
        Vector3 size = Vector3.zero;
        Vector3 MidOffset = Vector3.zero;

        if (TileSet != null)
        {
            size = TileSet.TileSize * new Vector3(BlockWidth, BlockHeight);

            MidOffset = 0.5f * size;
            size.y -= 0.5f;
        }
        Vector3 offset = new Vector3(0, -0.25f, 0);
        //Gizmos.DrawWireCube(transform.position, Vector3.one);
        Gizmos.DrawWireCube(transform.position + MidOffset + offset, size);
    }
}
