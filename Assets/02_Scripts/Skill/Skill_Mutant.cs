using UnityEngine;

/// <summary>
/// 돌연변이 스킬
/// </summary>
public class Skill_Mutant : SkillBase
{
    public override void ActivateSkill(SkillData choosedSkill)
    {
        switch (choosedSkill.skillIdx)
        {
            // 폭격 클러스터
            case 70:
                EventBus.HasMutant1 = true;
                break;

                // 로켓런쳐
            case 71:
                EventBus.HasMutant2 = true;

                break;

                // 살육전차
            case 72:

                break;

                // 유도 탄환
            case 73:
                EventBus.HasMutant3 = true;

                break;

            case 74:

                // 프로토콜 : 도미노

                break;

                // 목베기
            case 75:

                break;

                // 에너지 구체
            case 76:
                EventBus.HasMutant4 = true;

                break;

                // 프로토콜 : 제우스
            case 77:

                break;
           
            default:
                Debug.LogError("에러, 배정되지 않은 idx");
                break;

        }
    }

    
}
