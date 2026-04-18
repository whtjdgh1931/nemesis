using System;
using UnityEngine;


[RequireComponent(typeof(SkillChoose))]
public class TechItem : MonoBehaviour
{ 
    #region 스킬 습득

    private TechSelectPackType _TechCompany;
    /// <summary>
    /// 스킬 습득
    /// </summary>
    public void GetSkill(TechSelectPackType packType)
    {
        SkillChoose skillchoose = GetComponent<SkillChoose>();
        // 회사 지정
        skillchoose.SetSkillCompany(packType);
        skillchoose.SetBtn();
    }
    #endregion

    #region 스킬 업그레이드
   

    public void SkillUpgrade()
    {
        SkillChoose skillchoose = gameObject.GetComponent<SkillChoose>();
        skillchoose.SetUpgradeBtn();
    }
    #endregion

    #region 돌연변이 업그레이드

    public void Mutantupgrade()
    {
        SkillChoose skillchoose = gameObject.GetComponent<SkillChoose>();
        skillchoose.SetMutant();
    }
    #endregion
}