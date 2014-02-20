using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SpawnBlock))]
public class SpawnBlockEditor : Editor 
{
    int oldHeight;
    int oldWidth;
    string originalName;

    public override void OnInspectorGUI()
    {
        SpawnBlock sb = target as SpawnBlock;
        
        if(GUILayout.Button("Create New Block"))
            CreateNewBlock(sb);

        DrawDefaultInspector();

        if(GUI.changed)//sb.BlockHeight != oldHeight || sb.BlockWidth != oldWidth)
            CreateNewBlock(sb);

        oldHeight = sb.BlockHeight;
        oldWidth = sb.BlockWidth;
    }

    private void CreateNewBlock(SpawnBlock sb)
    {
        if (sb.SpawnedBlock != null)
            GameObject.DestroyImmediate(sb.SpawnedBlock);

        sb.SpawnedBlock = LevelGenerator.CreateSimpleLevelBlock(sb.TileSet, sb.transform.position, sb.BlockHeight, sb.BlockWidth, sb.Seed);
        sb.SpawnedBlock.transform.parent = sb.transform;
    }
}
