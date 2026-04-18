// 전처리 지시문 : 이후의 코드가 Unity Editor에서만 컴파일되고 실행되도록 지정
#if UNITY_EDITOR

// UnityEditor 네임스페이스 포함
using UnityEditor;

// 이 특성(Attribute)는 Room 클래스(true가 있으면 상속된 클래스도 포함됨)에 대한 커스텀 인스펙터를 적용하겠다는 의미
[CustomEditor(typeof(Room), true)]
// RoomEditor 클래스는 Unity의 Editor 클래스를 상속받아 커스텀 인스펙터를 구현
public class RoomEditor : Editor
{
    // OnInspectorGUI 메서드는 인스펙터 창을 그릴 때 호출됨
    // 그 함수를 재정의하여 커스텀 인스펙터 UI를 구현
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 GUI를 그림
        base.OnInspectorGUI();
        // 현재 인스펙터에서 편집 중인 대상 객체를 Room 타입으로 캐스팅
        Room room = (Room)target;
        // DoorSpawnPointsLeft 배열이 null이거나 길이가 3이 아닌 경우 경고 메시지를 인스펙터에 표시
        if (room.DoorSpawnPointsLeft == null || room.DoorSpawnPointsLeft.Length != 3)
        {
            EditorGUILayout.HelpBox("DoorSpawnPointsLeft에 반드시 3개의 Transform이 할당되어야 합니다.", MessageType.Error);
        }
        // DoorSpawnPointsRight 배열이 null이거나 길이가 3이 아닌 경우 경고 메시지를 인스펙터에 표시
        if (room.DoorSpawnPointsRight == null || room.DoorSpawnPointsRight.Length != 3)
        {
            EditorGUILayout.HelpBox("DoorSpawnPointsRight에 반드시 3개의 Transform이 할당되어야 합니다.", MessageType.Error);
        }
    }
}
#endif