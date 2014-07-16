using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpawn : Singleton<PlayerSpawn> 
{
    private Transform player;
    private List<Rigidbody2D> rigids;
    public float CameraHeightOffset = 5;
    public float GizmoRadius = 2;

	// Use this for initialization
	void Start () 
    {
        player = GameObject.Find("PlayerCharacter").transform;
        rigids = player.GetComponentsInChildren<Rigidbody2D>().ToList();
        rigids.AddRange(player.GetComponents<Rigidbody2D>());
        //Debug.Log(player.name);
        Respawn();
	}

    public static void Respawn()
    {
        //Debug.Log(Instance.player.transform.position);
        //Debug.Log(Instance.transform.position);
        
        // Move player
        Vector3 pos = Instance.transform.position;
        Instance.player.position = pos;

        // Reset rigidbodies
        foreach (Rigidbody2D rigid in Instance.rigids)
        {
            Instance.StartCoroutine(Instance.ResetRigidBody(rigid));
        }

        // Move camera
        pos.y+=Instance.CameraHeightOffset;
        pos.z = Camera.main.transform.position.z;
        Camera.main.transform.position = pos;
    }

    private IEnumerator ResetRigidBody(Rigidbody2D rigid)
    {
        //bool wasKinematic = rigid.isKinematic;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        rigid.isKinematic = true;
        yield return null;
        rigid.isKinematic = false;
        //yield return null;
        //rigid.isKinematic = wasKinematic;
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
