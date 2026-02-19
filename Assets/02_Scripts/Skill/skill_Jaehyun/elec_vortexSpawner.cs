using UnityEngine;

public class elec_vortexSpawner : MonoBehaviour
{
    [SerializeField] private GameObject elecvortexPrefab;
    private elecVortex vortex;
    void Start()
    {
        SpawnElecvortex(6, 8, 1, 2);
    }

    private void SpawnElecvortex(float radius, float radius_magnet, int damage, float speed)
    {
        elecVortex vortex = elecvortexPrefab.GetComponent<elecVortex>();

        elecvortexPrefab.transform.Find("vortex_Range").localScale = new Vector3(radius*2, radius*2, radius*2);

        vortex.PullRadius = radius_magnet;

        vortex.Damage = damage;

        vortex.Speed = speed;

        Instantiate(elecvortexPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }

}
