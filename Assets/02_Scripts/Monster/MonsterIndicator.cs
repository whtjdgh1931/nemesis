using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyIndicator : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public RectTransform canvasRect;
    public GameObject arrowPrefab;
    public MonsterSpawner monsterSpawner;

    [Header("Arrow Sprites")]
    public Sprite smallArrowSprite;
    public Sprite middleArrowSprite;
    public Sprite bigArrowSprite;

    [Header("Settings")]
    public float edgeBuffer = 50f;

    private class EnemyTarget
    {
        public Transform enemyTransform;
        public MonsterSize size;
        public Image arrowUI;
    }

    [SerializeField]
    private readonly List<EnemyTarget> enemyTargets = new();

    void Update()
    {
        RefreshEnemyListIfNeeded();
        UpdateIndicators();
    }

    void RefreshEnemyListIfNeeded()
    {
        if (monsterSpawner == null) return;

        foreach (var target in enemyTargets)
        {
            if (target.arrowUI != null)
                Destroy(target.arrowUI.gameObject);
        }
        enemyTargets.Clear();

        foreach (GameObject monster in monsterSpawner.ActiveMonsters)
        {
            if (monster == null) continue;

            MonsterBase monsterBase = monster.GetComponent<MonsterBase>();
            if (monsterBase == null) continue;

            GameObject arrowObj = Instantiate(arrowPrefab, canvasRect);
            Image arrowImage = arrowObj.GetComponent<Image>();
            arrowImage.sprite = GetArrowSprite(monsterBase.GetMonsterSize());

            enemyTargets.Add(new EnemyTarget
            {
                enemyTransform = monster.transform,
                size = monsterBase.GetMonsterSize(),
                arrowUI = arrowImage
            });
        }
    }

    void UpdateIndicators()
    {
        foreach (var target in enemyTargets)
        {
            if (target.enemyTransform == null || target.arrowUI == null)
                continue;

            Vector3 screenPos = mainCamera.WorldToViewportPoint(target.enemyTransform.position);
            bool isOffScreen = screenPos.z < 0 || screenPos.x < 0 || screenPos.x > 1 || screenPos.y < 0 || screenPos.y > 1;

            if (isOffScreen)
            {
                target.arrowUI.gameObject.SetActive(true);

                Vector3 enemyScreenPos = mainCamera.WorldToScreenPoint(target.enemyTransform.position);
                Vector3 screenCenter = new(Screen.width / 2f, Screen.height / 2f);
                Vector3 dir = (enemyScreenPos - screenCenter).normalized;

                // 화면 끝 기준 거리 계산
                float maxX = (Screen.width / 2f) - edgeBuffer;
                float maxY = (Screen.height / 2f) - edgeBuffer;
                Vector3 arrowScreenPos = screenCenter + new Vector3(dir.x * maxX, dir.y * maxY, 0f);

                // 화면 경계 내로 제한
                arrowScreenPos.x = Mathf.Clamp(arrowScreenPos.x, edgeBuffer, Screen.width - edgeBuffer);
                arrowScreenPos.y = Mathf.Clamp(arrowScreenPos.y, edgeBuffer, Screen.height - edgeBuffer);

                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, arrowScreenPos, null, out Vector2 canvasPos);
                target.arrowUI.rectTransform.anchoredPosition = canvasPos;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                target.arrowUI.rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            }
            else
            {
                target.arrowUI.gameObject.SetActive(false);
            }
        }
    }

    Sprite GetArrowSprite(MonsterSize size)
    {
        return size switch
        {
            MonsterSize.SMALL => smallArrowSprite,
            MonsterSize.MIDDLE => middleArrowSprite,
            MonsterSize.BIG => bigArrowSprite,
            _ => smallArrowSprite
        };
    }
}
