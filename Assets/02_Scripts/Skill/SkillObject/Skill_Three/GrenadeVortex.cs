using System.Collections;
using UnityEngine;

public class GrenadeVortexData
{
    public float time;
    public float extent;

    public GrenadeVortexData(float time, float extent)
    {
        this.time = time;
        this.extent = extent;
    }
}


public class GrenadeVortex : vortex
{
    private float _time;

    public override void Initialize(object data)
    {
        base.Initialize(data);
        if(data is GrenadeVortexData vortexData)
        {
            _time = vortexData.time;
            SetRadius(vortexData.extent);
        }
    }

    public void Initialize()
    {
        StartCoroutine(ReleaseCoroutine());
    }

    public IEnumerator ReleaseCoroutine()
    {
        yield return new WaitForSeconds(_time);
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);
    }
}
