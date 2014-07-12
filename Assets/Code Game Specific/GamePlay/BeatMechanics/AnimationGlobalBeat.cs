using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class AnimationGlobalBeat : MonoBehaviour 
{
    [System.Serializable]
    public class AnimationGBInfo
    {
        [ReadOnly] public string Name;
        [ReadOnly] public int ClipHash;
        public float SpeedPerMeasure = 1.0f;
        public bool SyncAnimation = false;
    }

    Animator anim;
    [SerializeField, ReadOnly]
    string animHash, animName;

    public List<AnimationGBInfo> AnimationsInfo;

	void Awake () 
    {
        anim = GetComponent<Animator>();
        CheckAnimationInfo();
	}
	
	void Update () 
    {
        
        #if UNITY_EDITOR
        // Check all the animations and update the animation info
        if(!UnityEditor.EditorApplication.isPlaying)
        {
            CheckAnimationInfo();
            return;
        }
        #endif

        // Only runs in playmode

        var a = anim.GetCurrentAnimatorStateInfo(0);
        var b = anim.GetCurrentAnimationClipState(0);

        animHash = a.nameHash.ToString();
        animName = b[0].clip.name;
        
        if (AnimationsInfo.Where(an => a.IsName(an.Name) && an.SyncAnimation).Count() >0)//an.ClipHash == a.nameHash && an.SyncAnimation).Count() > 0)
        {
            anim.ForceStateNormalizedTime(GlobalBeat.ProgressInMeasure() / GlobalBeat.Measures);
        } 
        
        anim.ForceStateNormalizedTime(GlobalBeat.ProgressInMeasure() / GlobalBeat.Measures);
	}

    private List<string> GetAllNames()
    {
        UnityEditorInternal.AnimatorController ac = GetComponent<Animator>().runtimeAnimatorController as UnityEditorInternal.AnimatorController;
        int layerCount = anim.layerCount;
        List<string> names = new List<string>();

        for (int i = 0; i < layerCount; i++)
        {
            UnityEditorInternal.AnimatorControllerLayer layer = ac.GetLayer(i);
            //Debug.Log(string.Format("Layer {0}: {1}", i, layer.name));

            UnityEditorInternal.StateMachine sm = layer.stateMachine;
            int smCount = sm.stateCount;
            for (int j = 0; j < smCount; j++)
            {
                UnityEditorInternal.State state = sm.GetState(j);
                Debug.Log(string.Format("State: {0}", state.uniqueName));
                names.Add(state.uniqueName);
            }
        }

        return names;
    }

    //
    private void CheckAnimationInfo()
    {
        UnityEditorInternal.AnimatorController ac = GetComponent<Animator>().runtimeAnimatorController as UnityEditorInternal.AnimatorController;
        int layerCount = anim.layerCount;

        if(AnimationsInfo == null)
            AnimationsInfo = new List<AnimationGBInfo>();

        // For each animation layer
        for (int i = 0; i < layerCount; i++)
        {
            UnityEditorInternal.AnimatorControllerLayer layer = ac.GetLayer(i);
            //Debug.Log(string.Format("Layer {0}: {1}", i, layer.name));

            UnityEditorInternal.StateMachine sm = layer.stateMachine;
            int smCount = sm.stateCount;
            for (int j = 0; j < smCount; j++)
            {
                UnityEditorInternal.State state = sm.GetState(j);
                //Debug.Log(string.Format("State: {0}", state.uniqueName));

                var cur = AnimationsInfo.Where(a => a.Name == state.uniqueName);
                
                // Replace hash if it exists, keep all the other info (boolean etc.)
                if (cur.Count() > 0)
                    cur.First().ClipHash = Animator.StringToHash(state.uniqueName);
                else // make a new one
                    AnimationsInfo.Add(new AnimationGBInfo() { Name = state.uniqueName, ClipHash = state.uniqueNameHash});
            }
        }
    }
}
