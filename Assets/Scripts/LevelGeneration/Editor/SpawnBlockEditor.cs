using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SpawnBlock))]
public class SpawnBlockEditor : Editor 
{

    public override void OnInspectorGUI()
    {
        SpawnBlock sb = target as SpawnBlock;
        
        

        DrawDefaultInspector();

        if(GUI.changed||GUILayout.Button("Generate New Block Elements"))
            CreateNewBlock(sb);
    }

    private void CreateNewBlock(SpawnBlock sb)
    {
        if (sb.SpawnedBlock != null)
            GameObject.DestroyImmediate(sb.SpawnedBlock);

        sb.SpawnedBlock = LevelGenerator.CreateSimpleLevelBlock(sb.TileSet, sb.transform.position, sb.BlockHeight, sb.BlockWidth, sb.Seed);
        sb.SpawnedBlock.transform.parent = sb.transform;
    }
}
