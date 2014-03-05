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

    

	// Use this for initialization
	void Awake () 
    {
        anim = GetComponent<Animator>();
        CheckAnimationInfo();
	}
	
	// Update is called once per frame
	void Update () 
    {
        var a = anim.GetCurrentAnimatorStateInfo(0);
        var b = anim.GetCurrentAnimationClipState(0);
        //anim.
        //anim.
        animHash = a.nameHash.ToString();
        animName = b[0].clip.name;
        //Debug.Log(b[0].clip.name);

        if (AnimationsInfo.Where(an => an.ClipHash == a.nameHash && an.SyncAnimation).Count() > 0)
        {
            anim.ForceStateNormalizedTime(GlobalBeat.ProgressInMeasure() / GlobalBeat.Measures);
            Debug.Log(0);
        } 
        
        anim.ForceStateNormalizedTime(GlobalBeat.ProgressInMeasure() / GlobalBeat.Measures);
            //a.normalizedTime = GlobalBeat.ProgressInMeasure() / GlobalBeat.Measures;
        //b[0].clip.

        
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

    //private void CheckAnimationInfo()
    //{
    //    //if (AnimationsInfo != null && AnimationsInfo.Count > 0)
    //    //    return;

    //    AnimationsInfo = GetAllAnimInfo();// new List<AnimationGBInfo>();
    //    //List<string> animNames = GetAllNames();
    //    //foreach (string s in animNames)
    //    //{
    //    //    AnimationsInfo.Add(new AnimationGBInfo() { Name = s });
    //    //}
    //}

    private void CheckAnimationInfo()
    {
        UnityEditorInternal.AnimatorController ac = GetComponent<Animator>().runtimeAnimatorController as UnityEditorInternal.AnimatorController;
        int layerCount = anim.layerCount;

        if(AnimationsInfo == null)
            AnimationsInfo = new List<AnimationGBInfo>();

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

                var cur = AnimationsInfo.Where(a => a.Name == state.uniqueName);
                if (cur.Count() > 0)
                {
                    cur.First().ClipHash = Animator.StringToHash(state.uniqueName);
                    Debug.Log("REplace");
                }
                else
                    AnimationsInfo.Add(new AnimationGBInfo() { Name = state.uniqueName, ClipHash = state.uniqueNameHash});
            }
        }
    }
}
