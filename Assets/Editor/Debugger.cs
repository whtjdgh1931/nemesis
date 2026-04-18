using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Player))]
public class EventBusEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("돌연변이 1 획득"))
        {
            EventBus.HasMutant1 = true;
        }
        if (GUILayout.Button("돌연변이 2 획득"))
        {
            EventBus.HasMutant2 = true;
        }
        if (GUILayout.Button("돌연변이 3 획득"))
        {
            EventBus.HasMutant3 = true;
        }
        if(GUILayout.Button("돌연변이 4 획득"))
        {
            EventBus.HasMutant4 = true;
        }
        if (GUILayout.Button("돌연변이 모두 제거"))
        {
            EventBus.HasMutant1 = false;
            EventBus.HasMutant2 = false;
            EventBus.HasMutant3 = false;
            EventBus.HasMutant4 = false;
        }
        if (GUILayout.Button("플레이어 데미지 20배 증가"))
        {
            GameManager.Instance.PlayerStatManager.AddTotalMultiDamage(20f);
        }
        if (GUILayout.Button("플레이어 데미지 20배 감소"))
        {
            GameManager.Instance.PlayerStatManager.AddTotalMultiDamage(-20f);
        }
    }
}