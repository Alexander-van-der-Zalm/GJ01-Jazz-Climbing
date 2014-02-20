using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SpawnBlock))]
public class SpawnBlockEditor : Editor 
{
	public override void OnInspectorGUI()
    {
        SpawnBlock sb = target as SpawnBlock;
        
        if(GUILayout.Button("Create New Block"))
        {
            if(sb.SpawnedBlock != null)
                GameObject.DestroyImmediate(sb.SpawnedBlock);

            sb.SpawnedBlock = LevelGenerator.CreateSimpleLevelBlock(sb.TileSet,sb.transform.position,sb.BlockHeight,sb.BlockWidth,sb.Seed);
            sb.SpawnedBlock.transform.parent = sb.transform;
        }
        DrawDefaultInspector();
    }
}
