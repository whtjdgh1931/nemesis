using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "RoomData", menuName = "ScriptableObjects/Map/RoomData")]
public class RoomDataSO : ScriptableObject
{
    [Header("Identification")]
    [SerializeField] RoomType _roomType;

    [Header("Prefab / Visuals")]
    [SerializeField] GameObject _roomPrefab;
    [SerializeField] Sprite _previewIcon;

    [Header("Gameplay Metadata")]
    [SerializeField, Min(0f)] float _baseChance = 1f; // 기본 등장 확률 등

    [TextArea][SerializeField] string _description;

    public RoomType RoomType => _roomType;
    public GameObject RoomPrefab => _roomPrefab;
    public Sprite PreviewIcon => _previewIcon;
    public float BaseChance => _baseChance;
    public string Description => _description;

#if UNITY_EDITOR
    void OnValidate()
    {
        // Prefab이 Room 컴포넌트를 가지고 있는지 검사 (디자이너가 실수했을 때 빠르게 알림)
        if (_roomPrefab != null && !_roomPrefab.TryGetComponent<Room>(out _))
        {
            Debug.LogWarning($"RoomDataSO '{name}': assigned prefab '{_roomPrefab.name}' has no Room component.");
        }
        EditorUtility.SetDirty(this);
    }
#endif

    public bool IsValid(out string reason)
    {
        if (_roomPrefab == null) { reason = "RoomPrefab is null"; return false; }
        if (!_roomPrefab.TryGetComponent<Room>(out _)) { reason = "Prefab missing Room component"; return false; }
        reason = "";
        return true;
    }
}