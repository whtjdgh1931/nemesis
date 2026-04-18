using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillChoose))]
public class SkillChooseEditor : Editor
{
    private int skillIndexToActivate = 0;

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 표시
        DrawDefaultInspector();

        SkillChoose skillChoose = (SkillChoose)target;

        GUILayout.Space(10);
        GUILayout.Label("🔧 디버그 기능", EditorStyles.boldLabel);

        if (GUILayout.Button("업그레이드 버튼"))
        {
            skillChoose.SetUpgradeBtn();
        }

        if (GUILayout.Button("OnClick_DrawMutant() - 돌연변이 스킬"))
        {
            skillChoose.OnClick_DrawMutant();
        }

        if (GUILayout.Button("OnClick_skillCompanyOne() - Company1"))
        {
            skillChoose.OnClick_skillCompanyOne();
            skillChoose.SetBtn();
        }

        if (GUILayout.Button("OnClick_skillCompanyTwo() - Company2"))
        {
            skillChoose.OnClick_skillCompanyTwo();
            skillChoose.SetBtn();

        }

        if (GUILayout.Button("OnClick_skillCompanyThree() - Company3"))
        {
            skillChoose.OnClick_skillCompanyThree();
            skillChoose.SetBtn();

        }

        if (GUILayout.Button("OnClick_skillCompanyFour() - Company4"))
        {
            skillChoose.OnClick_skillCompanyFour();
            skillChoose.SetBtn();

        }

        if (GUILayout.Button("OnClick_skillCompanyFive() - Company5"))
        {
            skillChoose.OnClick_skillCompanyFive();
            skillChoose.SetBtn();

        }

        if (GUILayout.Button("OnClick_ListBtn() - 현재 스킬 리스트 생성"))
        {
            skillChoose.OnClick_ListBtn();
        }

        GUILayout.Space(10);
        GUILayout.Label("🎯 스킬 인덱스로 Activate", EditorStyles.boldLabel);

        skillIndexToActivate = EditorGUILayout.IntField("스킬 인덱스", skillIndexToActivate);

        if (GUILayout.Button($"ActivateSkillByIndex({skillIndexToActivate})"))
        {
            GameManager.Instance.skillManager.ActivateSkillByIndex(skillIndexToActivate);
        }
    }
}
