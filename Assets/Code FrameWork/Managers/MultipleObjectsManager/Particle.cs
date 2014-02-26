using UnityEngine;
using System.Collections;

public class Particle : ManagedObject
{
    public float LifeTime;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(DeactivateAfterLifeTime());
    }

    private IEnumerator DeactivateAfterLifeTime()
    {
        float startTime = Time.realtimeSinceStartup;
        float dt = 0;
        while (dt < LifeTime)
        {
            if (!gameObject.activeSelf)
                startTime = Time.realtimeSinceStartup;
            dt = Time.realtimeSinceStartup - startTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
