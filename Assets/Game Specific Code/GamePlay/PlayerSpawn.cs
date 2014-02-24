using UnityEngine;
using System.Collections;

public class PlayerSpawn : Singleton<PlayerSpawn> 
{
    private Transform player;
    public float CameraHeightOffset = 5;
    public float GizmoRadius = 2;

	// Use this for initialization
	void Start () 
    {
        player = GameObject.Find("PlayerCharacter").transform;
        //Debug.Log(player.name);
        Respawn();
	}

    public static void Respawn()
    {
        //Debug.Log(Instance.player.transform.position);
        //Debug.Log(Instance.transform.position);
        Vector3 pos = Instance.transform.position;
        Instance.player.position = pos;
        pos.y+=Instance.CameraHeightOffset;
        pos.z = Camera.main.transform.position.z;
        Camera.main.transform.position = pos;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 1);
        DrawGizmo();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 1);
        DrawGizmo();
    }

    private void DrawGizmo()
    {
        Gizmos.DrawWireSphere(transform.position, GizmoRadius);
        Gizmos.DrawIcon(transform.position + new Vector3(0, GizmoRadius), "icon_spawn.png",true);
    }
}
