# 👥 Project Nemesis
Cyberpunk Roguelike Hack & Slash  

> 데이터 기반 설계를 중심으로 구현한 팀 프로젝트입니다.  
> 저는 **스킬 시스템 및 플레이어 스탯 시스템 설계/구현, 서버시스템**을 담당했습니다.

---

## 🎮 프로젝트 개요

- 장르: 사이버펑크 로그라이크 핵 앤 슬래시
- 플랫폼: PC / Mobile
- 개발 인원: 4명
- 담당 파트: Skill System / Player Stat System / Server

🎥 Gameplay Video  
👉 [(소개 영상)](https://youtu.be/AbBBSkmhfGs?si=shfaVm-Ta9q2r0yD)

---

# 🧩 My Contribution

## 1️⃣ Skill System 설계 및 구현

### 주요 기능
- JSON 기반 스킬 데이터 로드
- 기업 단위 스킬 모듈화 구조 설계
- 콜라보 스킬 조건 검사 시스템 구현
- 가중치 기반 랜덤 스킬 선택 로직 구현
- 업그레이드 가능 스킬 리스트 분리 관리

### 스킬 선택 로직 예시

```csharp
public TechSelectPackType[] GetSkillPackTypes(int count)
{
    int totalChance = Random.Range(0, totalNum);
    ...
}
```

### 설계 의도

- 데이터와 로직 분리
- 스킬 추가 시 JSON 데이터만 수정
- 확장 시 기존 코드 수정 최소화

---

## 2️⃣ Player Stat System 설계 및 구현

### 주요 기능
- 서버 JSON 기반 스탯 초기화
- Reflection 기반 필드 매핑
- 공격 타입별 데미지 계산 분리
- 이벤트 기반 전투 처리 구조

### 데미지 처리 예시

```csharp
public void TakeDamage(WeaponType weaponType, ATTACKTYPE attackType, Transform monster)
{
    float damage = CalculateDamage(weaponType, attackType);
    monster.GetComponent<MonsterBase>().TakeDamage(damage);
}
```

---

# 🏗 Architecture Overview

- SkillManager 중심 구조
- JSON 기반 Data-driven 설계
- 이벤트 기반 전투 처리
- 책임 분리 (Skill / Stat / UI)

---

# 🔧 Trouble Shooting

### 문제
DontDestroyOnLoad 객체로 인해 재시작 시 스킬 데이터가 초기화되지 않음

### 해결
- 재시작 전용 초기화 메서드 분리
- 보유 스킬 리스트 및 업그레이드 리스트 명확히 초기화

---

# 🛠 Tech Stack

- Unity 6000.0.59f2
- C#
- Newtonsoft JSON
- Git

---

> 확장 가능한 구조 설계와 데이터 중심 설계를 목표로 구현했습니다.
