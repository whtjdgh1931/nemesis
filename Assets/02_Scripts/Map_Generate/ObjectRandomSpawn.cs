using UnityEngine;

public class ObjectRandomSpawn : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;   // 스폰 포인트
    [SerializeField] private GameObject[] objects;      // 스폰될 오브젝트
    void Start()
    {
        SpawnObjects();
    }

    private void SpawnObjects()
    {
        int[] array = new int[objects.Length];
        for (int i = 0; i < array.Length; i++)
            array[i] = i;

        
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }//랜덤 로직

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Transform objectPoint = objects[array[i]].transform.Find("Point");  //따로 지정해둔 포인트
            Instantiate(objects[array[i]], new Vector3(spawnPoints[i].position.x - objectPoint.position.x, 
                spawnPoints[i].position.y - objectPoint.position.y, 
                spawnPoints[i].position.z - objectPoint.position.z),
                spawnPoints[i].rotation, spawnPoints[i]);
            if (i ==0 || i ==1)
            {
                spawnPoints[i].Rotate(0, 45, 0);    //왼쪽 스폰 각도
            }
            else
            {
                spawnPoints[i].Rotate(0, -45, 0);   //오른쪽 스폰 각도

            }
        } 
    }
}
