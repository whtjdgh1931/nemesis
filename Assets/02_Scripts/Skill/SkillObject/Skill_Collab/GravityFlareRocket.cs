using JetBrains.Annotations;
using Unity.AppUI.UI;
using Unity.VisualScripting;
using UnityEngine;


public class GravityFlareRocketData
{
    
    public GravityFlareRocketExplosionData explosionData;
    public GravityFlareRocketExplosion explosionPrefab;

    public GravityFlareRocketData(GravityFlareRocketExplosionData data, GravityFlareRocketExplosion explosionPrefab)
    {
        
        this.explosionData = data;
        this.explosionPrefab = explosionPrefab;
    }
}

public class GravityFlareRocket : PoolableObject,IInitializePoolable
{
   
    [SerializeField]
    private float moveSpeed;

    private GravityFlareRocketExplosionData explosionData;
    [SerializeField]
    private GravityFlareRocketExplosion _explosionPrefab;
    public void Initialize(object data)
    {
       if(data is GravityFlareRocketData rocketData)
        {
            explosionData = rocketData.explosionData;
            _explosionPrefab = rocketData.explosionPrefab;
        }
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.TAG_PLAYER))
        {
            return;
        }
        Vector3 position = transform.position;
        position.y = 0;
        GameManager.Instance.PoolManager.GetFromPool(
            _explosionPrefab, position, Quaternion.identity, null, explosionData).GetComponent<GravityFlareRocketExplosion>().Initialize();
        GameManager.Instance.PoolManager.ReleaseToPool(gameObject);

        //효과음
        GameManager.Instance.SoundManager.PlaySfxAt("GravityFlare", position);
    }

}
