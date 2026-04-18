using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMover : MonoBehaviour
{
    [SerializeField] Vector3 _offset;
    [SerializeField] float _cameraSpeed = 10f;

    [Header("카메라 제한 영역")]
    [SerializeField] float _minX = -50f;
    [SerializeField] float _maxX = 50f;
    [SerializeField] float _minZ = -50f;
    [SerializeField] float _maxZ = 50f;

    [Header("투명화 설정")]
    [SerializeField, Range(0f, 1f)] float _transparentAlpha = 0.3f;
    [SerializeField] LayerMask _obstacleMask;

    [Header("박스 검사 옵션")]
    [SerializeField] bool _useBoxOverlap = true;
    [SerializeField] float _paddingFactor = 1.05f;

    [Header("서서히 페이드 속도")]
    [SerializeField] float _fadeSpeed = 5f;

    [Header("----- 읽기 전용 -----")]
    [SerializeField] Transform _target;

    // 내부 상태
    class OccluderInfo
    {
        public Color originalColor;
        public Color originalEmission;
        public float currentAlpha;
        public float targetAlpha;
    }


    Dictionary<Renderer, OccluderInfo> _occluderInfos = new Dictionary<Renderer, OccluderInfo>();
    HashSet<Renderer> _currentOccluders = new HashSet<Renderer>();
    MaterialPropertyBlock _mpb;

    Collider[] _overlapResults = new Collider[256];
    RaycastHit[] _rayHits = new RaycastHit[256];

    public void Initialize(Player player)
    {
        _target = player?.transform;
        _mpb = new MaterialPropertyBlock();
        if (_target == null)
            Debug.LogError("CameraMover.Initialize: target이 null입니다!");
    }

    private void OnEnable()
    {
        transform.position = new Vector3(0, 2, -5); // 초기 위치 설정
        transform.rotation = Quaternion.Euler(30f, 0f, 0f);
    }

    void Update()
    {
        if (_target == null) return;

        // 카메라 목표 위치 이동
        Vector3 desiredPos = _target.position + _offset;
        float clampX = Mathf.Clamp(desiredPos.x, _minX, _maxX);
        float clampZ = Mathf.Clamp(desiredPos.z, _minZ, _maxZ);
        Vector3 targetPos = new Vector3(clampX, desiredPos.y, clampZ);
        transform.position = Vector3.Lerp(transform.position, targetPos, _cameraSpeed * Time.deltaTime);

        _currentOccluders.Clear();

        Vector3 origin = transform.position;
        Vector3 dir = _target.position - origin;
        float dist = dir.magnitude;
        if (dist <= 0.001f) return;

        if (_useBoxOverlap)
        {
            Vector3 boxCenter = origin + dir * 0.5f;
            float midDist = dist * 0.5f;
            Camera cam = GetComponent<Camera>();
            float halfHeight = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * midDist;
            float halfWidth = halfHeight * cam.aspect;
            Vector3 halfExtents = new Vector3(halfWidth * _paddingFactor, halfHeight * _paddingFactor, dist * 0.5f);
            Quaternion orientation = Quaternion.LookRotation(dir.normalized, cam.transform.up);

            int hitCount = Physics.OverlapBoxNonAlloc(boxCenter, halfExtents, _overlapResults, orientation, _obstacleMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _overlapResults[i];
                if (col == null || col.isTrigger) continue;

                Renderer rend = GetRelevantRenderer(col);
                if (rend == null || IsTargetRenderer(rend)) continue;

                EnsureOccluderInfo(rend);
                SetRendererTargetAlpha(rend, _transparentAlpha);
                _currentOccluders.Add(rend);
            }
        }
        else
        {
            int hitCount = Physics.RaycastNonAlloc(new Ray(origin, dir.normalized), _rayHits, dist, _obstacleMask, QueryTriggerInteraction.Ignore);
            if (hitCount > 0)
            {
                System.Array.Sort(_rayHits, 0, hitCount, new RayHitDistanceComparer());
                for (int i = 0; i < hitCount; i++)
                {
                    var hit = _rayHits[i];
                    var col = hit.collider;
                    if (col == null || col.isTrigger) continue;

                    Renderer rend = GetRelevantRenderer(col);
                    if (rend == null || IsTargetRenderer(rend)) continue;

                    EnsureOccluderInfo(rend);
                    SetRendererTargetAlpha(rend, _transparentAlpha);
                    _currentOccluders.Add(rend);
                }
            }
        }

        // 이번 프레임 투명화 대상이 아닌 렌더러는 원래 알파 목표값으로
        foreach (var kv in _occluderInfos)
        {
            if (!_currentOccluders.Contains(kv.Key))
                kv.Value.targetAlpha = kv.Value.originalColor.a;
        }

        // 알파 Lerp & 적용
        // 알파 Lerp & 적용
        foreach (var kv in _occluderInfos)
        {
            Renderer rend = kv.Key;
            OccluderInfo info = kv.Value;
            if (rend == null) continue;

            info.currentAlpha = Mathf.Lerp(info.currentAlpha, info.targetAlpha, Time.deltaTime * _fadeSpeed);

            var shared = rend.sharedMaterial;
            if (shared == null) continue;
            string colorProp = GetColorPropertyName(shared);
            if (colorProp == null) continue;

            rend.GetPropertyBlock(_mpb);
            Color newColor = info.originalColor;
            newColor.a = info.currentAlpha;
            _mpb.SetColor(colorProp, newColor);

            if (shared.HasProperty("_EmissionColor"))
            {
                Color newEmission = info.originalEmission * info.currentAlpha;
                _mpb.SetColor("_EmissionColor", newEmission);
            }

            rend.SetPropertyBlock(_mpb);
        }

    }

    Renderer GetRelevantRenderer(Collider col)
    {
        if (col == null) return null;
        Renderer rend = col.GetComponent<Renderer>();
        if (rend != null) return rend;
        rend = col.GetComponentInParent<Renderer>();
        if (rend != null) return rend;
        rend = col.GetComponentInChildren<Renderer>();
        return rend;
    }

    bool IsTargetRenderer(Renderer r)
    {
        if (r == null || _target == null) return false;
        return r.transform.IsChildOf(_target);
    }

    void EnsureOccluderInfo(Renderer rend)
    {
        if (rend == null || _occluderInfos.ContainsKey(rend)) return;

        Color origColor = Color.white;
        Color origEmission = Color.black;
        var shared = rend.sharedMaterial;
        if (shared != null)
        {
            string colorProp = GetColorPropertyName(shared);
            if (colorProp != null) origColor = shared.GetColor(colorProp);
            if (shared.HasProperty("_EmissionColor")) origEmission = shared.GetColor("_EmissionColor");
        }

        _occluderInfos[rend] = new OccluderInfo
        {
            originalColor = origColor,
            originalEmission = origEmission,
            currentAlpha = origColor.a,
            targetAlpha = origColor.a
        };
    }


    void SetRendererTargetAlpha(Renderer rend, float alpha)
    {
        if (rend == null) return;
        EnsureOccluderInfo(rend);
        _occluderInfos[rend].targetAlpha = alpha;
    }

    string GetColorPropertyName(Material mat)
    {
        if (mat == null) return null;
        if (mat.HasProperty("_BaseColor")) return "_BaseColor";
        if (mat.HasProperty("_Color")) return "_Color";
        return null;
    }

    private void OnDisable()
    {
        foreach (var kv in _occluderInfos)
        {
            Renderer rend = kv.Key;
            var info = kv.Value;
            if (rend == null) continue;

            var shared = rend.sharedMaterial;
            if (shared == null) continue;

            string colorProp = GetColorPropertyName(shared);
            if (colorProp != null)
            {
                rend.GetPropertyBlock(_mpb);
                _mpb.SetColor(colorProp, info.originalColor);

                if (shared.HasProperty("_EmissionColor"))
                    _mpb.SetColor("_EmissionColor", info.originalEmission);

                rend.SetPropertyBlock(_mpb);
            }
        }
        _occluderInfos.Clear();
    }


    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || _target == null) return;

        Vector3 origin = transform.position;
        Vector3 dir = _target.position - origin;
        float dist = dir.magnitude;
        if (dist <= 0.001f) return;

        Vector3 boxCenter = origin + dir * 0.5f;
        float midDist = dist * 0.5f;
        Camera cam = GetComponent<Camera>();
        float halfHeight = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * midDist * _paddingFactor;
        float halfWidth = halfHeight * cam.aspect * _paddingFactor;
        Vector3 halfExtents = new Vector3(halfWidth, halfHeight, dist * 0.5f);
        Quaternion orientation = Quaternion.LookRotation(dir.normalized, cam.transform.up);

        Gizmos.color = new Color(1f, 0f, 0f, 0.12f);
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, orientation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, halfExtents * 2f);
        Gizmos.matrix = old;
    }

    class RayHitDistanceComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit a, RaycastHit b)
        {
            return a.distance.CompareTo(b.distance);
        }
    }
}
