# Project Nemesis 🎮

> Unity 기반의 3D 액션 로그라이크 슈팅 게임

TeamNemesis의 ProjectNemesis 작업용 Repository입니다.

**프로젝트 노션 링크**: https://economic-kettle-c2e.notion.site/26fc01e9d6ba80b498dde6d3fc2cc36e?source=copy_link

---

## 📌 프로젝트 소개

**Project Nemesis**는 Unity 엔진으로 제작된 3D 액션 로그라이크 슈팅 게임입니다. 플레이어는 다양한 무기와 스킬을 활용하여 몬스터들을 처치하고, 보스를 물리치며 스테이지를 클리어하는 것이 목표입니다.

### 주요 특징
- 🎯 **다양한 전투 시스템**: 일반 공격, 특수 공격, 대시, 유탄 발사 등 다채로운 액션
- 🔫 **무기 시스템**: 여러 종류의 무기를 획득하고 전환할 수 있는 시스템
- ⚡ **스킬 업그레이드**: 게임 진행 중 다양한 스킬을 획득하고 강화
- 🏪 **상호작용 시스템**: 상점, 아이템, 문, 보상 등 다양한 오브젝트와 상호작용
- 👾 **몬스터 AI**: 다양한 크기와 행동 패턴을 가진 몬스터 및 보스
- 🗺️ **맵 생성 시스템**: 동적 맵 생성 및 방 구조
- 🎵 **사운드 시스템**: 몰입감 있는 게임플레이를 위한 사운드 관리

---

## 🎮 게임 플레이 및 기능

### 플레이어 시스템
- **이동 & 대시**: 자유로운 3D 공간에서의 이동과 회피를 위한 대시
- **전투 메커니즘**:
  - 일반 공격 (Normal Attack)
  - 특수 공격 (Special Attack)
  - 유탄 공격 (Grenade Attack)
- **상태 머신**: 플레이어의 상태를 효율적으로 관리하는 State Machine 패턴 적용
- **애니메이션 시스템**: 부드러운 액션을 위한 애니메이션 블렌딩

### 스킬 시스템
- 다양한 스킬 획득 및 강화
- JSON 기반 스킬 데이터 관리
- 스킬 조합 및 시너지 효과
- 실시간 스킬 UI 업데이트

### 몬스터 시스템
- **몬스터 크기**: SMALL, MIDDLE, BIG
- **AI 상태**: Idle, Move, Attack, Die
- **몬스터 스포너**: 동적 몬스터 생성 시스템
- **보스 시스템**: 강력한 보스 몬스터와의 전투

### 상호작용 시스템
- **상점**: 아이템 구매 및 업그레이드
- **보상 상자**: 랜덤 보상 획득
- **무기**: 새로운 무기 획득
- **기술 업그레이드**: 플레이어 능력 향상
- **힐 팩**: 체력 회복
- **문**: 다음 방으로 이동

---

## 🗂️ 프로젝트 구조

```
ProjectNemesis/
├── Assets/
│   ├── 01_Scenes/              # 씬 파일
│   │   ├── Play.unity          # 메인 게임 플레이 씬
│   │   ├── Intro/              # 인트로 씬
│   │   ├── Player/             # 플레이어 테스트 씬
│   │   ├── Monsters/           # 몬스터 테스트 씬
│   │   └── Skill/              # 스킬 테스트 씬
│   ├── 02_Scripts/             # C# 스크립트
│   │   ├── 00_Manager/         # 게임 매니저들
│   │   │   ├── GameManager.cs
│   │   │   ├── SkillManager.cs
│   │   │   ├── PoolManager.cs
│   │   │   └── ...
│   │   ├── Player/             # 플레이어 관련
│   │   │   ├── Player.cs
│   │   │   ├── PlayerModel.cs
│   │   │   ├── PlayerView.cs
│   │   │   ├── Move/           # 이동 시스템
│   │   │   ├── Attack/         # 공격 시스템
│   │   │   └── StateMachine/   # 상태 머신
│   │   ├── Monster/            # 몬스터 관련
│   │   │   ├── MonsterBase.cs
│   │   │   ├── MonsterSpawner/
│   │   │   ├── Boss/
│   │   │   └── NormalMonsters/
│   │   ├── Skill/              # 스킬 시스템
│   │   ├── InteractableObjects/ # 상호작용 오브젝트
│   │   ├── Interaction/        # 상호작용 컨트롤러
│   │   └── UI/                 # 사용자 인터페이스
│   ├── 03_Prefabs/             # 프리팹
│   ├── 04_Animations/          # 애니메이션
│   ├── 05_Materials/           # 머티리얼
│   └── 99_StoreAssets/         # 외부 에셋
├── ProjectSettings/            # Unity 프로젝트 설정
└── Packages/                   # Unity 패키지
```

---

## 🚀 시작하기

### 필요 사항
- **Unity 버전**: `6000.0.59f2` (Unity 6)
- **운영 체제**: Windows, macOS, Linux
- **최소 사양**:
  - RAM: 8GB 이상
  - GPU: DirectX 11 지원
  - 저장 공간: 5GB 이상

### 설치 및 실행

1. **저장소 클론**
   ```bash
   git clone https://github.com/TeamNemesis/ProjectNemesis.git
   cd ProjectNemesis
   ```

2. **Unity Hub 설치**
   - Unity Hub 다운로드: https://unity.com/download
   - Unity 6000.0.59f2 버전 설치

3. **프로젝트 열기**
   - Unity Hub에서 프로젝트 추가
   - `/ProjectNemesis` 폴더 선택

4. **게임 실행**
   - `Assets/01_Scenes/Intro/IntroScene.unity` 씬을 열어 인트로부터 시작
   - 또는 `Assets/01_Scenes/Play.unity` 씬을 열어 바로 게임 플레이
   - Unity 에디터에서 재생 버튼 클릭

---

## 💡 사용 기술 및 패키지

### Unity 패키지
- **Unity Input System**: 새로운 입력 시스템
- **TextMesh Pro**: 고품질 텍스트 렌더링
- **Unity Addressables**: 효율적인 에셋 관리
- **NavMesh**: AI 길찾기
- **Firebase**: 백엔드 서비스 (선택적)

### 외부 에셋
- **캐릭터 모델**: Robot & Pilot, Stella Girl, Free Test Character
- **무기 에셋**: Modern Warfare 팩
- **이펙트**: FX Kandol Pack, Kyeoms FX, Rolling Balls Sci-fi Pack
- **UI 에셋**: Japanese Cyberpunk GUI
- **맵 에셋**: Sci-Fi Warehouse Kit, Cosmic Retro Station Props
- **사운드**: Casual Game Sounds, Sci-Fi Small Sound Pack
- **애니메이션**: RPG Animations Pack, Melee Animation Pack

### 디자인 패턴
- **Singleton Pattern**: 매니저 클래스들
- **State Machine Pattern**: 플레이어 및 몬스터 상태 관리
- **Object Pool Pattern**: 오브젝트 재사용을 통한 성능 최적화
- **MVC Pattern**: 플레이어 구조 (Model-View-Controller)
- **Observer Pattern**: 이벤트 시스템

---

## 👥 팀원 및 역할

### Team Nemesis 구성원

- **endsun1234**: 상호작용 시스템 개발
  - 상호작용 오브젝트 시스템 구현
  - 상점, 아이템, 보상 시스템
  
- **minji**: 캐릭터 움직임 시스템
  - 플레이어 이동 및 대시 메커니즘
  - 캐릭터 컨트롤러 최적화
  
- **hyunwoo**: 스킬 업그레이드 시스템
  - 스킬 매니저 및 데이터 관리
  - 스킬 강화 및 선택 UI

- **기타 기여자들**:
  - 몬스터 AI 시스템
  - 무기 및 전투 시스템
  - 맵 생성 시스템
  - UI/UX 디자인
  - 사운드 및 이펙트

---

## 🎯 개발 가이드

### 코드 스타일
- C# 네이밍 컨벤션 준수
- 주석을 통한 코드 문서화
- 메서드 및 클래스에 XML 문서 주석 사용

### 브랜치 전략
- `main`: 안정적인 릴리즈 버전
- `feature/*`: 새로운 기능 개발
- `fix/*`: 버그 수정
- `develop`: 개발 통합 브랜치

### 커밋 컨벤션
- `feat:` 새로운 기능
- `fix:` 버그 수정
- `docs:` 문서 수정
- `refactor:` 코드 리팩토링
- `test:` 테스트 코드

---

## 📄 추가 문서

- [평가 및 분석 문서](./EVALUATION.md)
- [발표 자료](./PRESENTATION.md)
- [프로젝트 노션](https://economic-kettle-c2e.notion.site/26fc01e9d6ba80b498dde6d3fc2cc36e?source=copy_link)

---

## 📝 라이선스

이 프로젝트는 교육 목적으로 제작되었습니다.

---

## 🤝 문의 및 기여

프로젝트에 대한 문의사항이나 기여를 원하시면 이슈를 등록해주세요.

**Repository**: https://github.com/TeamNemesis/ProjectNemesis
