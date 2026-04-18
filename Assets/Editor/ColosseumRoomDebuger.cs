using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColosseumRoom))]
public class ColosseumRoomDebugger : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ColosseumRoom colosseumRoom = (ColosseumRoom)target;
        if (GUILayout.Button("Spawn Rewards"))
        {
            colosseumRoom.SpawnReward();
        }
    }
}