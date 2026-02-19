using System.Linq;
using UnityEngine;

public class RandomSpawnPoints : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    private Transform[] selectedSpawnPoints;
    void Start()
    {
        //ChooseSpawnPoints(3);
    }

   public  Transform[] ChooseSpawnPoints(int count)
    {
        int[] index = new int[spawnPoints.Length];

        // 채우기
        for (int i = 0; i < index.Length; i++)
        {
            index[i] = i;
        }

        // 랜덤 섞기
        for (int i = 0; i < index.Length; i++)
        {
            // 랜덤 인덱스 선택
            int rand = Random.Range(i, index.Length);

            // 자리 바꾸기
            int temp = index[i];
            index[i] = index[rand];
            index[rand] = temp;
        }

        // count개 선택
        selectedSpawnPoints = new Transform[count];
        for (int i = 0; i < count; i++)
        {
            selectedSpawnPoints[i] = spawnPoints[index[i]];
        }

        return selectedSpawnPoints;
    }
    
}
