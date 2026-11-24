# 1주차 체크리스트 — 세팅과 오프라인 프로토타입
 
> 이 문서는 새 대화방에서도 이 파일과 `GameDesign.md`, `ProjectGuidelines.md`만 보고 바로 작업을 이어갈 수 있도록 작성했다.
> 기획의 모든 결정 사항과 이유는 `GameDesign.md`에, 대화·코드 작성 규칙은 `ProjectGuidelines.md`에 있다.
> 이 문서는 1주차에 무엇을, 어떤 순서로, 어떤 기준으로 하는지만 다룬다.
>
> 작성일: 2026-10-09 / 최종 수정: 2026-10-10
 
---
 
## 0. 새 대화방을 시작할 때 읽을 것
 
### 0-1. 프로젝트 한눈에 보기
 
| 항목 | 내용 |
|---|---|
| 프로젝트 | 덕르코프 (코드명 ProjectD) |
| 목적 | 게임 클라이언트 프로그래머 취업용 포트폴리오 |
| 장르 | 3D 쿼터뷰 협동 익스트랙션 슈터 (Escape from Duckov 참고) |
| 플레이 | 2~4인 협동 PvE, 싱글 가능 |
| 개발 | 1인, 8주 |
| 엔진 | Unity 6000.3.10f1 (6.3), URP, New Input System |
| 네트워크 | Photon PUN 2, 마스터 클라이언트 권한 구조, 싱글은 오프라인 모드 |
| 버전 관리 | GitHub Desktop, 공개 저장소 `ProjectD`, Git LFS |
| 현재 위치 | 1~33번, 42번 완료. 35~37번 삭제, 38~41번 3주차로 이동, 34번·43번 보류. 1주차 작업은 2주차 마무리 커밋에 포함, `week1` 태그 생략 |
 
### 0-2. 작업 규칙 요약
 
자세한 규칙은 `ProjectGuidelines.md`를 따른다. 핵심만 요약한다.
 
- 대화는 한국어, 존댓말, 간결하게
- 결정이 필요할 때는 선택지별 장단점과 추천을 먼저 설명하고, 최종 결정은 사용자가 내린다
- 문서는 수정안을 먼저 보여주고 허락을 받은 뒤 갱신한다 ("바로 반영"이라고 하면 즉시 갱신)
- **스크립트는 사용자가 요청할 때만 작성한다**
- 스크립트는 `.cs` 파일로 제공, 네임스페이스 `ProjectD.하위모듈`, 한글 주석, 주석에 이모지 금지
- 파일 첫 줄에 경로 주석: `// 파일 경로: Assets/_Scripts/하위폴더/파일명.cs`
### 0-3. 1주차에 하지 않는 것
 
- 네트워크 동기화 (Photon은 접속 확인까지만) → 2주차
- 적 AI → 3주차
- 인벤토리, 아이템, 루팅 → 4주차
- 체력 0 이후 처리(다운, 사망) → 5주차 이후
- UI 화면(로비, HUD 전체) → 6~7주차. 1주차에는 조준점과 디버그 표시만
### 0-4. 1주차를 네트워크 없이 만드는 이유
 
1주차는 3D 조작을 익히는 단계라서 네트워크 없이 플레이어 조작만 만든다. 단, 2주차에 동기화를 바로 얹을 수 있도록 5번 "2주차를 위한 코드 원칙"을 지킨다. 3주차부터 적 AI, 아이템 등 모든 기능은 처음부터 마스터 권한 구조로 만든다.
 
---
 
## 1. 1주차 목표
 
네트워크 없이 혼자 움직이고, 피하고, 쏘고, 맞히는 캐릭터를 완성하고, 건물 안팎에서 화면이 제대로 보이는지 검증한다.
 
반드시 해소해야 하는 리스크
 
1. **에셋 캐릭터와 팩 클립이 제대로 재생되는가** (Generic, 제자리 동작 여부) → 애니메이션 작업(31번) 전까지 확인
2. **쿼터뷰 실내 표현(지붕 숨김, 벽 반투명)이 되는가** → 맵 제작 방식 전체가 여기에 달려 있다
### 진행 순서 원칙
 
- 조작 코드(입력, 카메라, 이동, 조준, 회피, 사격)는 캡슐 같은 임시 모양으로 먼저 만든다
- 에셋 리그 검증(29~30번)은 애니메이션 작업 전까지 끝낸다
- 그레이박스 제작(37번) 전에 MegaKit 벽 모듈 크기를 확인한다(35번)
- 나머지 에셋 점검과 테마 확정(41~42번)은 1주차 안에 끝낸다
---
 
## 2. 1주차 기술 결정 (확정)
 
| 항목 | 결정 | 이유 |
|---|---|---|
| 캐릭터 이동 | CharacterController | 움직임이 예측 가능하고 소유자 권한 이동과 잘 맞음. 물리로 밀려나는 요소가 없음 |
| 팀원끼리 충돌 | 없음 (레이어 충돌 표) | 좁은 곳 길막 방지, 원격 캐릭터 보간 시 덜컹거림 방지 |
| 카메라 | Cinemachine 3 | 추적, 화면 흔들림(Impulse), 관전 대상 전환을 기본 기능으로 해결 |
| 그레이박스 | ProBuilder | 에디터 안에서 벽·바닥·지붕을 따로 빠르게 제작, 3주차 맵에도 사용 |
| Assembly Definition | 사용 안 함 | 1인 2달 규모라 이득보다 참조 설정 비용이 큼 |
| 임시 에셋 | 플레이어·적: Quaternius Toon Shooter Game Kit / 건물: Quaternius Modular Sci-Fi MegaKit | CC0 라이선스, 검증 결과에 따라 변경 가능 |
| 애니메이션 | Generic, 팩 클립만 사용, Animator 하나·레이어 하나·블렌드 트리 없음, State 정수 파라미터 | 한 번에 클립 하나만 재생하는 단순한 구조. 우선순위를 코드 한 곳에서 관리 |
 
---
 
## 3. 할 일 (순서대로)
 
각 항목의 `확인`은 그 항목이 끝났다고 판단하는 기준이다.
 
### A. 환경 세팅 (1~11)
 
- [x] **1. Unity 설치**
  - 사용 버전: **6000.3.10f1 (Unity 6.3)**. 개발 중에는 버전을 올리지 않는다
- [x] **2. 프로젝트 생성**
  - 템플릿: Universal 3D (URP). Unity Cloud, Unity Version Control 옵션은 사용하지 않음
- [x] **3. 입력 시스템 설정**
  - Project Settings > Player > Other Settings > Active Input Handling: **Input System Package (New)**
  - 구 Input(`UnityEngine.Input`)은 사용하지 않는다
- [x] **4. 패키지 설치**
  - Package Manager > Unity Registry에서 Cinemachine(3.x), ProBuilder 설치. Input System 설치 확인
- [x] **5. PUN 2 임포트**
  - Asset Store의 PUN 2 - FREE를 Demos 폴더 제외하고 임포트
  - Script Updating Consent 창은 "Yes, for these and other files that might be found later" 선택 (Unity 6 API 변경 자동 수정)
  - **Photon 폴더는 `Assets/Photon/`에 그대로 둔다** (옮기면 업데이트와 설정 파일 경로 문제가 생길 수 있음)
- [x] **6. Photon 설정**
  - Photon 대시보드에서 PUN 타입 앱 생성, App ID를 PUN Wizard에 입력
  - PhotonServerSettings의 **Fixed Region을 `kr`로 고정** (클라이언트마다 리전이 다르면 방 코드로 방을 찾지 못함)
  - 참고: 공개 저장소에 App ID가 함께 올라간다. 포트폴리오에서는 허용
- [x] **7. 접속 확인**
  - 임시 스크립트 `Assets/_Scripts/Network/PhotonConnectionTester.cs` (네임스페이스 `ProjectD.Network`)
  - Sandbox 씬의 빈 오브젝트에 붙이고 플레이
  - 확인: Console에 "마스터 서버 접속 성공. 리전: kr" (kr로 시작하면 정상)
  - 2주차에 정식 네트워크 구조로 교체하며 삭제
- [x] **8. Git 세팅 (GitHub Desktop)**
  - 프로젝트 폴더(Assets와 같은 위치)에 `.gitattributes`를 **저장소 생성 전에** 먼저 만든다. 탐색기에서 파일 확장명 표시를 켜고 이름이 정확한지 확인
```
*.fbx filter=lfs diff=lfs merge=lfs -text
*.obj filter=lfs diff=lfs merge=lfs -text
*.blend filter=lfs diff=lfs merge=lfs -text
*.png filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text
*.psd filter=lfs diff=lfs merge=lfs -text
*.tga filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.mp3 filter=lfs diff=lfs merge=lfs -text
*.ogg filter=lfs diff=lfs merge=lfs -text
*.ttf filter=lfs diff=lfs merge=lfs -text
*.otf filter=lfs diff=lfs merge=lfs -text
```
 
  - GitHub Desktop: File > Add local repository → 프로젝트 폴더 → create a repository → Name `ProjectD`, Description `덕르코프 포트폴리오`, Git ignore **Unity**, License None → Create
  - LFS 초기화 안내가 뜨면 Initialize Git LFS. `.gitattributes`에 LFS 규칙이 남아 있는지 확인
  - Publish repository (Keep this code private 체크 해제)
  - 확인: GitHub 웹에서 Library 폴더가 없고, png 파일에 Stored with Git LFS 표시
  - 실패 시: GitHub 저장소 삭제, 프로젝트 폴더의 `.git` 폴더 삭제 후 처음부터 다시
  - Unity 설정 확인: Project Settings > Version Control의 Mode가 Visible Meta Files, Project Settings > Editor의 Asset Serialization Mode가 Force Text
  - 커밋 방법: Changes 탭에서 Summary(영어, Conventional Commits 형식)와 Description(한글) 작성 → Commit to main → Push origin. 형식은 `ProjectGuidelines.md` 8번
  - 태그 방법: History 탭에서 커밋 우클릭 > Create Tag → Push
- [x] **9. 폴더 구조 생성**
```
Assets/
├── _Scripts/
│   ├── AI/          적 (3주차부터)
│   ├── Combat/      무기, 투사체, 데미지
│   ├── Core/        공통 유틸, 이벤트, 매니저
│   │   └── Define/  열거형(Enum.cs), 상수 모음(AnimatorParams, ShaderPropertyIds 등)
│   ├── Data/        ScriptableObject 정의 클래스
│   ├── Inventory/   아이템, 인벤토리 (4주차부터)
│   ├── Network/     Photon 관련
│   ├── Player/      이동, 조준, 회피, 입력
│   └── UI/          조준점, HUD, 화면
├── Art/             모델 가공본, 애니메이션 등
├── Audio/
├── Data/            ScriptableObject 에셋, Input Actions 에셋
├── Materials/
├── Photon/          PUN 2 (임포트 위치 그대로)
├── Prefabs/
├── Scenes/
├── Settings/        URP 설정 (템플릿 생성, 삭제 금지)
└── ThirdParty/      외부 에셋 (Quaternius 등)
```
 
  - 네임스페이스는 `_Scripts` 하위 폴더와 맞춘다: `ProjectD.Player`, `ProjectD.Combat` 등
  - 템플릿의 TutorialInfo 폴더와 Readme 파일은 삭제해도 됨
  - 빈 폴더를 Git에 유지하려면 `.gitkeep` 파일 추가
- [x] **10. 씬 생성**
  - `Assets/Scenes/`에 Create > Scene으로 `Boot`, `Lobby`, `Raid`, `Sandbox` 생성
  - Build Profiles 씬 목록에 Boot, Lobby, Raid 추가 (Boot가 맨 위)
  - Sandbox: Main Camera, Directional Light, 바닥 Plane (Scale 5, 1, 5), 테스트용 벽 Cube 몇 개
- [x] **11. 레이어 설정**
  - Project Settings > Tags and Layers에 추가
| 번호 | 이름 |
|---|---|
| 6 | Player |
| 7 | Enemy |
| 8 | PlayerProjectile |
| 9 | EnemyProjectile |
| 10 | Environment |
| 11 | Roof |
| 12 | Interactable |
 
  - Project Settings > Physics > Layer Collision Matrix에서 체크 해제
    - Player ↔ Player (팀원 충돌 없음)
    - PlayerProjectile ↔ Player (아군 오사 없음)
    - PlayerProjectile ↔ PlayerProjectile
    - EnemyProjectile ↔ EnemyProjectile
  - 투사체는 레이캐스트로 판정하므로 실제 판정은 스크립트의 레이어 마스크가 결정한다. 이 표는 의도치 않은 물리 충돌을 막는 안전장치
  - 세팅이 끝나면 커밋 (예: `chore: set up packages, photon, folders, scenes, layers`)
### B. 입력, 카메라, 이동 (12~15)
 
- [x] **12. Input Actions 에셋 작성**
  - `Assets/Data/PlayerControls.inputactions`, Action Map 이름 `Player`
  - 템플릿 기본 `InputSystem_Actions`는 Project-wide Actions 지정 해제 후 삭제
  - 1주차에 쓰지 않는 액션도 전부 미리 정의한다
  - **입력 읽기 방식: Generate C# Class** → `Assets/_Scripts/Player/PlayerControls.cs`, 클래스 `PlayerControls`, 네임스페이스 `ProjectD.Player` (자동 생성 파일이라 주석 규칙 예외, 직접 수정 금지)
  - **길게 누르기는 에셋의 Interactions를 쓰지 않고 코드에서 누른 시간을 측정**한다 (상황마다 시간이 다르고 진행 UI가 필요하기 때문)
  - Move는 Up/Down/Left/Right Composite(WASD), Mode는 Digital Normalized
| 액션 | 타입 | 키 | 1주차 사용 |
|---|---|---|---|
| Move | Value (Vector2) | WASD | O |
| AimPosition | Value (Vector2) | 마우스 위치 | O |
| Sprint | Button | Left Shift | O |
| Dodge | Button | Space | O |
| Fire | Button | 마우스 왼쪽 | O |
| AimPrecise | Button | 마우스 오른쪽 | O |
| Reload | Button | R | O |
| Weapon1 | Button | 1 | O |
| Weapon2 | Button | 2 | O (테스트용으로 3종 순환도 허용) |
| Melee | Button | V | - |
| Interact | Button | F | - |
| Inventory | Button | Tab | - |
| Map | Button | M | - |
| Quick1 / Quick2 / Quick3 | Button | 3 / 4 / 5 | - |
| SelfKill | Button | X | - (길게 누르기는 코드에서 처리) |
| Pause | Button | Esc | - |
 
- [x] **13. 쿼터뷰 카메라** (스크립트 없음, 에디터 설정만)
  - 카메라 좌우 각도는 **0도 정면 시점**으로 고정 (월드 +Z가 화면 위쪽)
  - 임시 플레이어: Capsule `Player`, Position (0, 1, 0), Layer `Player`, **Capsule Collider 제거** (CharacterController와 충돌 방지)
  - **카메라 타깃은 `Player`의 자식 빈 오브젝트 `CameraTarget`**, Local Position (0, 0, 0). 캐릭터 모델로 바꾸면 (0, 1, 0) 정도로 조정
    - 자식으로 두면 코드 없이 위치가 따라가고, 실행 순서 문제로 인한 카메라 떨림이 없다
    - 카메라 타깃을 움직이는 스크립트는 17번 정밀 조준에서 작성
  - GameObject > Cinemachine > Cinemachine Camera → `CM_PlayerCamera`, Main Camera에 Cinemachine Brain 자동 추가 확인
  - Tracking Target: `CameraTarget`, Position Control: **Follow**, Rotation Control: **None**
  - Cinemachine Follow: Binding Mode **World Space**, Follow Offset **(0, 13, -9)**, Position Damping **(0.3, 0.3, 0.3)**
  - `CM_PlayerCamera` Rotation **(55, 0, 0)**. 오프셋과 각도는 짝이 맞아야 한다 (거리 변경 시 13과 9를 같은 비율로 조정)
  - Lens Field of View **40**
  - 확인: 카메라가 고정 각도로 따라가고, Player가 회전해도 카메라는 돌지 않음
- [x] **14. 걷기와 달리기 (CharacterController)**
  - 스크립트: `PlayerInputReader`(입력 담당), `PlayerMover`(이동 담당)
  - CharacterController: Center (0, 0, 0), Radius 0.5, Height 2
  - 이동 입력은 월드 기준 (정면 시점이라 입력 y = 월드 +Z)
  - 중력 적용 (기본 -20, 바닥에서는 -2 유지)
  - 시작값: 걷기 3.5 m/s, 달리기 6 m/s
  - 캡슐은 아직 회전하지 않음 (16번에서 조준 회전 구현)
- [x] **15. 스태미나**
  - 스크립트: `PlayerStamina`
  - `Drain`(달리기처럼 계속 소모), `TryConsume`(회피·근접처럼 한 번에 일정량, 부족하면 실패)
  - 값이 바뀌면 `OnStaminaChanged(현재, 최대)` 이벤트. UI는 구독만
  - 시작값: 최대 100, 달리기 초당 15 소모, 회복 초당 20, 회복 대기 1초
  - **스태미나가 0이 되면 Shift를 뗐다가 다시 눌러야 달리기 가능** (회복과 소모가 반복되며 달리기가 깜빡이는 현상 방지)
  - 로컬 전용 값 (네트워크 동기화 대상 아님)
  - 확인용: Inspector를 Debug 모드로 바꾸면 `_currentStamina` 확인 가능
  - 커밋 완료: 카메라, 이동, 스태미나
### C. 조준과 회피 (16~19)
 
- [x] **16. 마우스 조준**
  - 카메라에서 마우스 위치로 레이를 쏘고, 캐릭터 높이의 수평면과 교차하는 점을 조준점으로 사용
  - 캐릭터는 항상 조준점을 향해 수평 회전
  - 확인: 마우스를 어디에 두든 캐릭터가 그 방향을 바라봄
- [x] **17. 우클릭 정밀 조준**
  - 누르고 있는 동안: 이동 속도 감소, 탄퍼짐 감소(25번), 카메라 타깃을 마우스 방향으로 이동
  - 카메라 타깃 위치 = 캐릭터 위치 + (조준점 방향 × 거리 비율), 최대 이동 거리 제한
  - 시작값: 이동 속도 배율 0.6, 카메라 타깃 최대 4m
  - 정밀 조준 중에는 달리기 불가
- [x] **18. 회피**
  - 방향: 이동 입력 방향. 입력이 없으면 바라보는 방향
  - 앞부분 일부 구간만 무적
  - 스태미나가 회피 1회 비용보다 적으면 사용 불가
  - 시작값: 비용 25, 이동 거리 약 4m, 전체 0.4초, 무적 0.25초
  - **"지금 무적인가"를 외부에서 조회할 수 있게** 만든다 (3주차 피격자 최종 확인에서 사용)
  - 1주차에는 무적 구간을 디버그 표시(색 변화 등)로 확인
- [x] **19. 상호 배제 규칙**
  - 사격 또는 우클릭 조준 입력 → 달리기 즉시 해제
  - 회피 중 → 사격, 조준, 재장전 불가
  - 재장전 중 회피 → 재장전 취소
  - 확인: 위 조합을 하나씩 시험해서 규칙대로 동작
### D. 사격 (20~27)
 
- [x] **20. WeaponData ScriptableObject**
  - 필드: 이름, 데미지, 연사 속도, 탄속, 최대 사거리, 기본 탄퍼짐, 연사 시 확산 증가량, 최대 확산, 확산 회복 속도, 이동 시 확산 배율, 정밀 조준 시 확산 배율, 한 발당 탄 수, 탄창 크기, 재장전 시간, 탄 종류
  - 테스트 데이터 3개 (`Assets/Data/`). 아래는 시작값이며 플레이하며 조정
| 무기 | 데미지 | 분당 발사 | 탄속 | 사거리 | 기본 확산 | 한 발당 탄 수 | 탄창 | 재장전 |
|---|---|---|---|---|---|---|---|---|
| 권총 | 20 | 300 | 40 m/s | 25 m | 2도 | 1 | 12 | 1.2초 |
| 소총 | 15 | 600 | 60 m/s | 35 m | 3도 | 1 | 30 | 2.0초 |
| 산탄총 | 10 | 70 | 35 m/s | 12 m | 8도 | 8 | 6 | 2.5초 |
 
  - 참고: 적 탄속은 3주차에 플레이어 탄속의 1/3~1/2 (약 15~20 m/s)로 설정 예정
- [x] **21. 투사체 오브젝트 풀**
  - Unity 내장 `UnityEngine.Pool.ObjectPool<T>` 사용
  - 투사체는 네트워크 오브젝트가 아니다. 2주차에는 발사 정보(시작점, 방향, 무기)만 RPC로 보내고 각 클라이언트가 같은 풀에서 시각용 탄을 꺼내 재생한다
  - 확인: 연사해도 Instantiate와 Destroy가 반복되지 않음
- [x] **22. 관통 방지와 사거리**
  - 매 프레임 "이전 위치 → 현재 위치" 구간을 레이캐스트로 검사해서 그 사이에 벽이나 대상이 있으면 명중 처리
  - 이동 거리가 최대 사거리를 넘으면 풀로 반환
  - 확인: 탄속을 매우 높여도 얇은 벽을 뚫지 않음
- [x] **23. 탄퍼짐**
  - 최종 확산 = (기본 확산 + 누적 확산) × 이동 배율 × 정밀 조준 배율
  - 이동 중이면 증가, 달리기 직후 잠깐 더 크게 증가
  - 연사할수록 누적, 쏘지 않으면 서서히 회복
  - 발사 방향 = 조준 방향에서 확산 각도 안의 무작위 방향 (수평면 기준)
- [x] **24. 원형 조준점**
  - 마우스 위치에 원형 커서, 크기가 현재 확산에 비례
  - 시스템 마우스 커서는 숨김
- [x] **25. 탄창과 재장전**
  - 탄창 탄 수 관리, R로 재장전, 재장전 시간 동안 사격 불가
  - 탄창에 남은 탄은 보존
  - 예비탄은 임시로 무한 (4주차 인벤토리 연동 시 교체)
- [x] **26. 더미 적**
  - 체력, 피격 시 흰색 플래시, 데미지 숫자 표시
  - 체력 0이면 비활성화 후 일정 시간 뒤 다시 등장 (테스트 반복용)
  - 레이어는 `Enemy`
- [x] **27. 사격 화면 흔들림**
  - 무기에 Cinemachine Impulse Source, Cinemachine Camera에 Impulse Listener
  - 약하게. 쿼터뷰에서 과하면 조준이 어려워짐
  - 사격까지 끝나면 커밋
### E. 에셋 리그 검증 (28~30, 애니메이션 전에 반드시 완료)
 
- [x] **28. 에셋 받기와 넣기**
  - quaternius.com에서 Toon Shooter Game Kit, Modular Sci-Fi MegaKit 무료 버전 다운로드
  - 압축 파일 중 **FBX 폴더만** 넣는다 (다른 형식은 저장소 용량만 늘림)
  - 위치: `Assets/ThirdParty/Quaternius/ToonShooterGameKit/`, `Assets/ThirdParty/Quaternius/ModularSciFiMegaKit/`
- [x] **29. 캐릭터 리그 확인 (Generic)**
  - 리그는 **Generic 그대로** 사용 (팩에 포함된 클립만 쓰므로 Humanoid 불필요)
  - Avatar Definition은 기본값 그대로. 30번에서 이동 클립이 제자리가 아니면 그때 Create From This Model과 Root node 지정
  - 캐릭터 FBX를 Sandbox에 놓고 색·텍스처 확인 (하얗거나 분홍이면 메모 후 41번에서 처리)
  - 1m Cube와 비교해 키 약 1.7~1.8m인지 확인. 다르면 FBX Inspector > Model 탭 Scale Factor 조정 후 Apply
  - 키와 몸 폭을 메모 (31번 CharacterController 조정용)
  - FBX Inspector > Animation 탭에서 클립 목록 확인
  - 확인된 클립 (Toon Shooter Game Kit): idle, Death, 회피(대쉬), 피격, 근접 공격, 걷기, 달리기, idle 사격, 걷기 사격, 달리기 사격, 고개 끄덕임(재장전으로 사용)
- [x] **30. 팩 클립 재생 확인**
  - Idle 클립: Animation 탭에서 Loop Time 체크 → Apply
  - `Assets/Art/Animations/`에 Animator Controller `Test_Animator` 생성, Idle 클립을 끌어다 기본 상태로
  - Sandbox의 캐릭터 Animator: Controller `Test_Animator`, **Apply Root Motion 해제** → 플레이해서 자연스럽게 재생되는지 확인
  - 걷기 클립도 Loop Time 체크 후 Test_Animator 기본 상태로 바꿔서, **제자리에서 걷는지** 확인
  - 앞으로 미끄러져 나가거나 나갔다가 돌아오면: Rig 탭 Avatar Definition을 Create From This Model, Root node에 최상위 뼈 지정 → 해당 클립의 Root Transform Rotation / Position(Y) / Position(XZ) 모두 Bake Into Pose
  - 확인 후 Test_Animator는 삭제해도 됨 (32번에서 정식 Animator 생성)
### F. 애니메이션 (31~34)
 
- [x] **31. 클립 설정과 모델 교체**
  - Loop Time 체크 (이미 반복으로 되어 있으면 그대로): idle, 걷기, 달리기, idle 사격, 걷기 사격, 고개 끄덕임(재장전). 회피(대쉬)는 해제
  - Player 기준점을 발밑으로: Player Position Y를 바닥 높이(0)로
  - 캐릭터 모델을 Player의 자식으로 배치, Local Position (0, 0, 0)
  - 모델 앞쪽이 Player 앞쪽(+Z)과 다르면 자식 모델만 Y축 180도 회전
  - 캡슐의 Mesh Renderer와 Mesh Filter 제거
  - 함께 조정할 값
    - CharacterController: Height = 키, Radius = 몸 폭의 절반, Center Y = 키의 절반
    - CameraTarget Local Position Y: 몸 중심 높이 (약 1)
    - PlayerInputReader의 Aim Plane Height Offset: 총 높이 (약 1)
    - Muzzle Local Position: 총구 근처 (약 0, 1.2, 0.6)
    - PlayerDodgeDebugView의 Target Renderer: 모델 몸통의 Skinned Mesh Renderer (렌더러가 여러 개면 한 부분만 칠해져도 디버그용이라 허용)
  - 자식 모델의 Animator: Apply Root Motion 해제
  - 확인: 캡슐 때와 똑같이 이동, 조준, 회피, 사격이 동작하고 모델이 마우스 방향을 바라봄
- [x] **32. Animator Controller 구성**
  - `Assets/Art/Animations/`에 `PlayerAnimator` 생성, **Base Layer 하나만**, **블렌드 트리 없음**
  - 파라미터: `State`(int), `MoveDirection`(float, 기본값 1)
  - 상태 7개, 상태 하나에 클립 하나 (**상태 이름은 아래 표와 정확히 같게**)
| 상태 | 클립 | State 값 | 반복 | Speed Multiplier |
|---|---|---|---|---|
| Idle | idle | 0 | O | - |
| Walk | 걷기 | 1 | O | MoveDirection |
| Run | 달리기 | 2 | O | MoveDirection |
| IdleShoot | idle 사격 | 3 | O | - |
| WalkShoot | 걷기 사격 | 4 | O | MoveDirection |
| Dodge | 회피(대쉬) | 5 | X | - |
| Reload | 고개 끄덕임 | 6 | O (재장전 동안 반복) | - |
 
  - Speed Multiplier: 상태 Inspector의 Multiplier에서 Parameter 체크 후 `MoveDirection` 선택
  - 전환: **Any State에서 각 상태로 하나씩 (7개)**. 조건은 `State` Equals 해당 값, Has Exit Time 해제, Transition Duration 0, Can Transition To Self 해제
  - 발 미끄러짐 조정: Walk, Run, WalkShoot 상태의 Speed(기본 1)를 발 움직임이 실제 이동 속도와 맞을 때까지 조정. MoveDirection 배율과 별도로 곱해짐
  - 사용하지 않는 클립: 달리기 사격(달리며 사격 불가), 피격(3주차), 근접 공격(5주차), Death(5주차 이후)
  - 자식 모델의 Animator에 `PlayerAnimator` 연결
- [x] **33. 애니메이션 상태 결정 스크립트**
  - `Core/Define/Enum.cs`에 `PlayerAnimState` 추가. 값은 숫자를 직접 적고 Animator 전환 조건과 일치시킴
  - `Core/Define/AnimatorParams.cs`: `State`, `MoveDirection` 파라미터 해시
  - `Core/Define/AnimatorStates.cs`: `Dodge` 상태 이름 해시 (회피 재시작용)
  - Player 모듈 `PlayerAnimationController` (Player 루트에 부착, Animator는 자식에서 찾음)
    - 우선순위로 State 결정. **Animator에 값을 쓰는 유일한 컴포넌트**
      1. 회피 중 → Dodge
      2. 재장전 중이고 멈춰 있음 → Reload
      3. 사격 중이거나 정밀 조준 중 → 멈춤이면 IdleShoot, 이동이면 WalkShoot
      4. 그 외 → 멈춤이면 Idle, 걷기면 Walk, 달리기면 Run
    - MoveDirection: 이동 방향과 바라보는 방향이 반대쪽이면 -1, 그 외(앞, 옆) 1
    - 회피 시작 이벤트(`OnDodgeStarted`)를 받으면 `Animator.Play`로 Dodge 상태를 처음부터 재생 (연속 회피 시 State가 바뀌지 않아 클립이 처음부터 재생되지 않는 문제 대응. 파라미터 방식의 유일한 예외)
  - Combat 모듈 `WeaponAnimationBridge`: 사격 중·재장전 중 정보를 `SetShooting`, `SetReloading`으로 전달만 함 (Animator를 직접 건드리지 않음)
  - 확인: 우선순위대로 클립 전환, 뒤로 걸으면 역재생, 이동 중 재장전은 이동 클립 유지, 멈추면 끄덕임 반복, Space 연타 시 매번 대쉬가 처음부터 재생
- [ ] **34. 회피 클립 길이 맞추기**
  - Dodge 상태의 Speed = 회피 클립 길이(초) / 0.4
  - 대쉬 클립이 몸을 앞으로 이동시키는지 확인. 이동시키면 두 배로 나가거나 끝에 뒤로 튕겨 보이므로 알릴 것
  - 확인: 대쉬 동작과 실제 이동이 어긋나지 않음
  - 애니메이션까지 끝나면 커밋 대기 내용에 기록
### G. 실내 표현 검증 (35~40, 가장 큰 리스크)
 
- [ ] **35. MegaKit 모듈 크기 확인** [삭제: 에셋(Synty)을 바로 사용, 2026-11-06]
  - MegaKit의 벽, 바닥 모듈 하나를 Sandbox에 놓고 한 칸의 가로·세로·높이를 확인 (예: 2m, 4m 등)
  - 이 값을 그레이박스 격자 단위로 사용
- [ ] **36. ProBuilder 기본 사용법** [삭제: 에셋(Synty)을 바로 사용, 2026-11-06]
  - 익힐 것: 도형 생성(상자, 평면), 면·변·점 선택, 면 밀어내기(Extrude), 벽에 구멍 내기(문), 면별 머티리얼 지정
  - 그리드 스냅을 켜고 35번 단위로 작업
- [ ] **37. 그레이박스 건물 제작** [삭제: 에셋(Synty)을 바로 사용, 2026-11-06]
  - 방 2~3개, 문 2개, 좁은 복도 1개가 있는 단층 건물
  - **벽, 바닥, 지붕을 서로 다른 오브젝트로** 만들고, 지붕은 `Roof` 레이어, 벽과 바닥은 `Environment` 레이어
  - 머티리얼 색으로 구분 (예: 바닥 회색, 벽 흰색, 지붕 진한 색)
- [ ] **38. 지붕 숨기기** [3주차 A 섹션으로 이동, 방식 변경: 머리 위 레이 + Occluder]
  - 건물 내부를 덮는 트리거 볼륨 배치
  - 플레이어가 들어가면 그 건물의 지붕 렌더러를 숨기고, 나오면 복구
  - 팁: 지붕을 완전히 끄지 않고 그림자만 남기는 모드로 바꾸면 실내 조명 분위기가 유지된다
  - 멀티에서는 **로컬 플레이어 기준**으로만 동작해야 한다 (2주차에 확인)
  - 확인: 들어가면 실내가 보이고, 나오면 지붕이 다시 보임
- [ ] **39. 가리는 벽 반투명** [3주차 A 섹션으로 이동, 방식 변경: Occluder]
  - 카메라에서 플레이어 방향으로 검사해서 사이에 있는 벽을 찾음
  - 찾은 벽을 반투명 머티리얼로 교체하고, 더 이상 가리지 않으면 원래대로
  - 1차는 단순한 머티리얼 교체로 구현하고, 보기 나쁘면 이후 디더링 셰이더 등으로 개선
  - 확인: 벽 뒤로 가도 캐릭터가 보임
- [ ] **40. 맵 제작 규칙 정리** [3주차 9번으로 이동]
  - 37~39번 결과로 아래를 정해서 `GameDesign.md` 7번에 추가
    - 건물 하나의 단위 (트리거 볼륨 하나 = 건물 하나 등)
    - 지붕 오브젝트 구분 방식
    - 반투명 대상이 될 벽의 구분 방식
    - 그리드 단위 크기 (35번 값)
  - 3주차 그레이박스 맵이 이 규칙을 따른다
  - 실내 표현까지 끝나면 커밋
### H. 에셋 점검과 테마 확정 (41~42)
 
- [ ] **41. 나머지 에셋 기준 확인** [3주차 0번 에셋 준비로 대체]
| 기준 | 확인 방법 |
|---|---|
| 적 캐릭터 | 같은 톤의 적 2종 이상 + 보스로 쓸 만한 모델 (임시: Toon Shooter Game Kit 캐릭터 공용) |
| 무기 | 권총, 소총, 산탄총, 고급 무기(저격 소총 계열)로 쓸 모델 |
| 모듈형 건물 | 벽, 바닥, 천장(지붕 역할)이 분리되어 있고 격자 단위로 딱 맞게 붙는지 |
| 루팅 소품 | 상자, 사물함, 컨테이너 등 |
| URP | 머티리얼이 분홍색이 아닌지. 분홍이면 Window > Rendering > Render Pipeline Converter 또는 셰이더를 URP Lit으로 변경 |
| 텍스처 | 모델이 하얗게 나오면 텍스처 팩을 별도로 받았는지 확인 |
| 라이선스 | Quaternius는 CC0라 저장소에 함께 공개 가능. 유료 에셋을 섞으면 그 폴더를 `.gitignore`에 추가 |
 
- [x] **42. 테마 확정** [완료: 공사장 테마, 2026-11-06. 세부 컨셉은 미정]
  - 41번 결과를 바탕으로 테마 결정 (임시 테마: 폐쇄된 연구 기지에 잠입하는 회수 요원팀, 적은 보안 로봇)
  - `GameDesign.md` 0번 테마·한 줄 컨셉, 11번 에셋 최종 선택 항목 갱신
### I. 마무리 (43~45)
 
- [ ] **43. 디버그 패널 뼈대**
  - 토글: 무적, 스태미나 무한, 더미 적 스폰
  - 키 하나(예: F1)로 열고 닫기
  - 이후 주차에 아이템 지급, 즉시 탈출, 지연 시뮬레이션 등을 계속 추가
- [ ] **44. 완료 기준 점검** (4번 항목)
- [ ] **45. 개발 일지와 주차 마무리 커밋**
  - 6번 템플릿으로 1주차 개발 일지 작성
  - 주차 마무리 커밋 (Summary 예: `chore: complete week 1 - offline player prototype`, Description은 `ProjectGuidelines.md` 8번 형식) 후 태그 `week1`
  - 2026-11-06 변경: 1주차 개발 일지는 `DevLog_Week1.md`로 작성 완료. 1주차 작업은 2주차 마무리 커밋에 함께 포함했고 `week1` 태그는 생략 (사용자 결정)
---
 
## 4. 완료 기준
 
- [ ] 캐릭터가 걷고, 달리고, 피하고, 마우스 방향으로 조준한다
- [ ] 스태미나가 달리기와 회피에 맞게 줄고 회복된다
- [ ] 무기 3종을 바꿔 가며 쏠 수 있고, 탄퍼짐과 조준점이 상황에 따라 변한다
- [ ] 투사체가 벽을 뚫지 않고 더미 적에게 데미지가 들어간다
- [ ] 이동 방향과 조준 방향이 달라도 애니메이션이 자연스럽다
- [ ] 건물에 들어가면 지붕이 사라지고, 벽에 가려져도 캐릭터가 보인다
- [ ] 에셋이 확인 기준을 통과했고 테마가 정해졌다
- [ ] 모든 작업이 GitHub에 커밋되어 있고 `week1` 태그가 있다
---
 
## 5. 2주차를 위한 코드 원칙
 
1주차 코드는 2주차에 네트워크를 붙일 때 다시 쓰이므로 아래를 지킨다.
 
1. **입력과 동작을 분리한다.** 키 입력을 읽는 부분(입력 담당)과 "이동한다, 쏜다, 피한다"를 실행하는 부분(동작 담당)을 다른 클래스로 나눈다. 2주차에 다른 플레이어의 캐릭터는 입력 담당 없이 네트워크로 받은 값으로 동작 담당만 움직인다.
2. **로컬 플레이어 전용 처리를 구분한다.** 카메라 추적, 조준점, 지붕 숨기기, 화면 흔들림은 내 캐릭터에만 해당한다. 나중에 "내 캐릭터인가"로 켜고 끌 수 있게 한곳에 모은다.
3. **수치는 코드에 박지 않는다.** 이동 속도, 스태미나, 회피, 무기 수치는 인스펙터 값이나 ScriptableObject로 둔다.
4. **무적 여부를 외부에서 조회할 수 있게 한다.** 3주차 피격자 최종 확인에서 사용한다.
5. **발사는 "발사 정보"로 표현한다.** 사격 시 시작점, 방향, 무기 정보를 하나로 묶어 투사체를 생성하게 만들면 2주차에 그 정보를 그대로 RPC로 보낼 수 있다.
6. **시스템 간 통신 원칙을 지킨다.** 같은 오브젝트 안은 직접 참조, 시스템 사이는 C# 이벤트, UI는 이벤트를 구독만 한다.
---
 
## 6. 개발 일지 템플릿
 
```markdown
# N주차 개발 일지 (YYYY-MM-DD ~ YYYY-MM-DD)
 
## 한 일
-
 
## 해결한 기술 문제
### 문제 제목
- 증상:
- 원인:
- 해결:
- 배운 점:
 
## 하지 못한 일과 이유
-
 
## 다음 주에 할 일
-
 
## 측정 값 (있으면)
- 예: 프레임, 메모리, 네트워크 메시지 수
```
 
---
 
## 7. 2주차 미리보기
 
다음 주차 체크리스트는 `Week2_Checklist.md`로 작성한다. 2주차 목표는 PUN 2 도입이다.
 
- Boot, Lobby, Raid 씬 구성과 마스터가 씬을 전환하면 모두 따라오는 구조
- 방 코드로 방 만들기와 참가, PUN 2 오프라인 모드(싱글 플레이)
- 플레이어 스폰, 이동·조준·회피 상태 동기화 (위치 보간)
- 발사 이벤트 RPC, 플레이어→적 명중 판정과 마스터 검증
- 지연 시뮬레이션으로 테스트
- 임시 스크립트 `PhotonConnectionTester` 삭제
---
 
## 8. 현재까지 만든 스크립트
 
| 파일 | 네임스페이스 | 역할 | 비고 |
|---|---|---|---|
| `Assets/_Scripts/Player/PlayerControls.cs` | ProjectD.Player | Input Actions에서 자동 생성된 입력 클래스 | 직접 수정 금지 |
| `Assets/_Scripts/Player/PlayerInputReader.cs` | ProjectD.Player | 입력을 읽어 동작 담당에 전달 (입력 담당). 마우스 화면 좌표를 조준 평면에 투영해 월드 조준점 계산. 실행 순서 -10 | 로컬 플레이어 전용. 4주차: PlayerInputGate.IsAimBlocked면 조준 갱신과 정밀 조준을 멈춤 |
| `Assets/_Scripts/Player/PlayerMover.cs` | ProjectD.Player | CharacterController 걷기, 달리기, 중력. 정밀 조준 감속, 회피 중 수평 이동 정지, 조준·회피·사격 중 달리기 불가. `SetMoveInput`, `SetSprintRequested`, `SetSprintSuppressed`, `IsSprinting`, `IsMoving`, `MoveInputDirection` | 2주차 원격 플레이어도 사용 |
| `Assets/_Scripts/Player/PlayerStamina.cs` | ProjectD.Player | 스태미나 소모(`Drain`, `TryConsume`), 회복, `OnStaminaChanged` 이벤트 | 로컬 전용 |
| `Assets/_Scripts/Player/PlayerAimer.cs` | ProjectD.Player | 조준점 보관, 조준 방향으로 회전, 정밀 조준 상태. `SetAimPoint`, `SetPreciseAimRequested`, `AimPoint`, `AimDirection`, `IsAimingPrecise` | 2주차 원격 플레이어도 사용 |
| `Assets/_Scripts/Player/PlayerDodger.cs` | ProjectD.Player | 회피 이동, 무적 구간, 스태미나 소모. `TryStartDodge`, `IsDodging`, `IsInvincible`, `InvincibleEndTime`(3주차), `OnDodgeStarted` | 2주차 원격 플레이어도 사용 |
| `Assets/_Scripts/Player/PlayerCameraTargetOffset.cs` | ProjectD.Player | 정밀 조준 시 카메라 타깃을 조준점 쪽으로 이동. 실행 순서 10. CameraTarget 오브젝트에 부착 | 로컬 플레이어 전용 |
| `Assets/_Scripts/Player/PlayerDodgeDebugView.cs` | ProjectD.Player | 회피 중 캐릭터 색 변경 (무적 노랑, 무적 이후 하늘색) | 디버그용 |
| `Assets/_Scripts/Player/PlayerAnimationController.cs` | ProjectD.Player | 우선순위로 애니메이션 상태 하나를 정해 Animator의 State, MoveDirection 설정. Animator에 값을 쓰는 유일한 컴포넌트. 회피 시작 시 Animator.Play로 재시작. 외부 상태를 받으면 계산 대신 받은 값 재생(남의 캐릭터). `SetShooting`, `SetReloading`, `ApplyExternalState`, `PlayDodgeFromStart`, `CurrentState`, `CurrentMoveDirection`. 실행 순서 10, Animator는 자식에서 찾음 | Player 루트에 부착. 2주차 수정 |
| `Assets/_Scripts/Core/Define/Enum.cs` | ProjectD.Core.Define | 프로젝트의 모든 열거형 모음. `AmmoType`(Pistol, Rifle, Shotgun, Special), `PlayerAnimState`(Idle 0 ~ Reload 6, Animator 전환 조건과 일치), `OcclusionMode`(Transparent, ShadowsOnly, 3주차), `EnemyState`(Patrol 0 ~ Dead 5, byte로 동기화, 3주차), `EnemyAttackType`(Charge, Burst), `EnemyAttackPhase`(Telegraph, Strike, Recover), `ProjectilePoolType`(Player, Enemy), `ItemCategory`, `ItemRarity`, `ConsumableEffect`, `InputBlockReason`(Inventory, Chat) (4주차) | 새 열거형은 이 파일에 추가 |
| `Assets/_Scripts/Core/Define/ShaderPropertyIds.cs` | ProjectD.Core.Define | 셰이더 속성 ID 상수 모음. 현재 `BaseColor` | 셰이더 속성 문자열은 여기에만 |
| `Assets/_Scripts/Core/Define/AnimatorParams.cs` | ProjectD.Core.Define | Animator 파라미터 해시. `State`, `MoveDirection` | 파라미터 문자열은 여기에만 |
| `Assets/_Scripts/Core/Define/AnimatorStates.cs` | ProjectD.Core.Define | Animator 상태 이름 해시. `Dodge` (회피 재시작용) | 상태 이름 문자열은 여기에만 |
| `Assets/_Scripts/Data/WeaponData.cs` | ProjectD.Data | 무기 수치 ScriptableObject, ItemData 상속(ID·이름은 부모, 분류는 Weapon 고정). 메뉴 Create > ProjectD > Weapon Data. 탄퍼짐은 한쪽 최대 각도 | 4주차 ItemData 상속 (WeaponId, WeaponName 제거 → ItemId, DisplayName) |
| `Assets/_Scripts/Combat/FireInfo.cs` | ProjectD.Combat | 발사 정보 구조체 (Origin, Direction, Weapon). 2주차 RPC로 전송할 값 | |
| `Assets/_Scripts/Combat/IDamageable.cs` | ProjectD.Combat | 데미지를 받는 대상 인터페이스 `TakeDamage(amount, hitPoint)` | |
| `Assets/_Scripts/Combat/Projectile.cs` | ProjectD.Combat | 투사체. 프레임 간 레이캐스트로 관통 방지, 사거리 초과 시 종료. `Launch(fireInfo, hitMask, onHit, onFinished)` | 투사체 프리팹에 부착 |
| `Assets/_Scripts/Combat/ProjectilePool.cs` | ProjectD.Combat | 투사체 ObjectPool. `Get`, `Release`, `PoolType`(Player, Enemy) | 씬에 종류별로 하나씩 |
| `Assets/_Scripts/Combat/ProjectileLauncher.cs` | ProjectD.Combat | 발사 정보로 투사체 발사. `Launch(fireInfo)`는 판정용(명중 시 IHitHandler 호출, 기본 DirectDamageHitHandler), `LaunchVisualOnly(fireInfo)`는 시각용(명중 처리 없음), `SetHitHandler(handler)`. 풀이 비어 있으면 Pool Type이 같은 풀을 씬에서 찾음 | 2주차, 3주차 수정 |
| `Assets/_Scripts/Combat/PlayerWeaponHolder.cs` | ProjectD.Combat | 무기 목록, 현재 무기, 무기별 탄창. `SelectNextWeapon`, `SelectPreviousWeapon`, `TryConsumeAmmo`, `RefillMagazine`, `OnWeaponChanged`, `OnAmmoChanged` | 예비탄 임시 무한 |
| `Assets/_Scripts/Combat/PlayerWeaponSpread.cs` | ProjectD.Combat | 탄퍼짐 계산 (기본+누적, 이동·달리기 직후·정밀 조준 배율). `CurrentSpreadAngle`, `AddShotSpread` | |
| `Assets/_Scripts/Combat/PlayerReloader.cs` | ProjectD.Combat | 재장전 코루틴. 회피·무기 교체 시 취소. `TryStartReload`, `CancelReload`, `IsReloading`, 시작·완료·취소 이벤트 | |
| `Assets/_Scripts/Combat/PlayerShooter.cs` | ProjectD.Combat | 방아쇠, 발사 간격, 연사/단발, 탄 소모, 탄퍼짐 적용 발사, 빈 탄창 자동 재장전. `SetTriggerHeld`, `OnFired`, `OnShotFired`(발사 단위 FireInfo 목록), `MuzzlePosition` | 2주차 수정 |
| `Assets/_Scripts/Combat/WeaponInputReader.cs` | ProjectD.Combat | 사격·재장전·무기 교체 입력 (1 다음 무기, 2 이전 무기). 사격 중 달리기 막기. 실행 순서 -10 | 로컬 플레이어 전용 |
| `Assets/_Scripts/Combat/ShotCameraShake.cs` | ProjectD.Combat | 발사 시 Cinemachine Impulse 화면 흔들림 | 로컬 플레이어 전용 |
| `Assets/_Scripts/Combat/WeaponAnimationBridge.cs` | ProjectD.Combat | 사격(마지막 발사 후 0.5초 유지)·재장전 상태를 PlayerAnimationController에 전달. Animator는 건드리지 않음 | |
| `Assets/_Scripts/Combat/Health.cs` | ProjectD.Combat | 체력, IDamageable 구현. `OnDamaged`, `OnDied`, `ResetHealth`, `SetCurrentHealth`(중도 참가 시 체력 맞추기, 0이 되면 OnDied만), `SetMaxHealth`(남은 비율 유지) | 2주차, 3주차 수정 |
| `Assets/_Scripts/Combat/DamageFlash.cs` | ProjectD.Combat | 피격 시 색 깜빡임 (MaterialPropertyBlock) | |
| `Assets/_Scripts/Combat/DummyTarget.cs` | ProjectD.Combat | 더미 적. 사망 시 사라졌다가 재등장 | 3주차에 대체 |
| `Assets/_Scripts/UI/CrosshairView.cs` | ProjectD.UI | 마우스 위치 원형 조준점, 탄퍼짐에 따라 크기 변경. `SetTarget(aimer, weaponSpread)`, Find Target In Scene 설정(Raid에서는 끔) | 로컬 플레이어 전용 |
| `Assets/_Scripts/UI/DamageNumber.cs` | ProjectD.UI | 떠오르며 사라지는 데미지 숫자 (TextMeshPro) | 프리팹에 부착 |
| `Assets/_Scripts/UI/DamageNumberSpawner.cs` | ProjectD.UI | Health 데미지 이벤트로 데미지 숫자 표시, 풀 사용 | |
| `Assets/_Scripts/Core/Define/SceneNames.cs` | ProjectD.Core.Define | 씬 이름 상수 (Boot, Lobby, Raid) | 2주차. 씬 이름 문자열은 여기에만 |
| `Assets/_Scripts/Core/Define/RoomPropertyKeys.cs` | ProjectD.Core.Define | 방 속성 키 (DisplayName, StartPointSeed) | 2주차 |
| `Assets/_Scripts/Core/Define/PlayerPrefsKeys.cs` | ProjectD.Core.Define | PlayerPrefs 키 (PlayerName) | 2주차 |
| `Assets/_Scripts/Network/BootConnector.cs` | ProjectD.Network | 이름 검사·적용·저장, 마스터 서버 접속, 성공 시 Lobby 이동. AutomaticallySyncScene 끔. `TryConnect`, `TryApplyPlayerName`, `IsValidPlayerName`, `SavedPlayerName`, `MaxNameLength`, `OnConnectStarted`, `OnConnectFailed` | 2주차. Boot 씬 BootSystem |
| `Assets/_Scripts/Network/SoloPlayStarter.cs` | ProjectD.Network | 접속 끊기 → 오프라인 모드 → 오프라인 방 생성. `StartSoloPlay`, `IsStarting`, `OnSoloPlayFailed` | 2주차. Boot, Lobby 씬 |
| `Assets/_Scripts/Network/RaidSceneLoader.cs` | ProjectD.Network | 방에 들어가면 각자 Raid 씬 로드 (LoadLevel이 메시지 처리를 멈춰 중도 참가자의 기존 캐릭터 생성 정보가 Raid 씬에서 처리됨) | 2주차. Boot, Lobby 씬 |
| `Assets/_Scripts/Network/RoomListService.cs` | ProjectD.Network | 기본 로비 입장, 방 목록 딕셔너리 보관, 방 만들기·참가, 실패 이유. `CreateRoom`, `JoinRoom`, `Rooms`, `IsInLobby`, `LocalPlayerName`, `OnLobbyJoined`, `OnRoomListChanged`, `OnRequestFailed` | 2주차. Lobby 씬 LobbySystem |
| `Assets/_Scripts/Network/RoomListEntry.cs` | ProjectD.Network | 방 목록 한 줄 정보 구조체 (RoomName, DisplayName, PlayerCount, MaxPlayers, CanJoin) | 2주차. UI가 Photon 타입을 모르게 |
| `Assets/_Scripts/Network/RaidRoomProperties.cs` | ProjectD.Network | 레이드 방 옵션 생성(공개, 중도 참가, 표시 이름, 시작 지점 선택값), 방 속성 읽기. `CreateRoomOptions`, `GetDisplayName`, `GetStartPointSeed` | 2주차. static |
| `Assets/_Scripts/Network/RaidSessionWatcher.cs` | ProjectD.Network | Raid 참가자 목록, 나가기(온라인 Lobby, 혼자 Boot), 방장 이탈 시 방 닫고 전원 퇴장. `LeaveRaid`, `PlayerNames`, `MasterPlayerName`, `RoomDisplayName`, `MaxPlayers`, `IsOfflineMode`, `OnPlayerListChanged`, `OnMasterLeft` | 2주차. Raid 씬 RaidSystem |
| `Assets/_Scripts/UI/BootView.cs` | ProjectD.UI | Boot 화면 (이름 입력, 접속, 실패 시 혼자 플레이 버튼) | 2주차 |
| `Assets/_Scripts/UI/RoomListView.cs` | ProjectD.UI | Lobby 화면 (방 목록 다시 그리기, 방 만들기, 혼자 플레이, 요청 대기 중 버튼 막기) | 2주차 |
| `Assets/_Scripts/UI/RoomEntryView.cs` | ProjectD.UI | 방 목록 한 줄. `Setup(entry, allowJoin, onJoinClicked)` | 2주차. RoomEntry 프리팹 |
| `Assets/_Scripts/UI/RaidSessionView.cs` | ProjectD.UI | Raid 임시 방 정보 화면 (방 이름, 참가자, 나가기) | 2주차. 테스트용 |
| `Assets/_Scripts/Core/Define/PrefabPaths.cs` | ProjectD.Core.Define | 네트워크 생성 프리팹 경로 (Player, WorldItem) | 2주차. Resources 기준 |
| `Assets/_Scripts/Network/PlayerNetworkSetup.cs` | ProjectD.Network | 내 캐릭터/남의 캐릭터 판단 후 컴포넌트 켜고 끄기(인스펙터 목록), 방 밖(Sandbox)에서는 동기화 컴포넌트 끔. `IsLocalPlayer`. 실행 순서 -100 | 2주차. 플레이어 프리팹 루트 |
| `Assets/_Scripts/Network/RaidPlayerSpawner.cs` | ProjectD.Network | 시작 지점(방 속성)과 자리(ActorNumber)로 내 캐릭터 네트워크 생성. `OnLocalPlayerSpawned`, `LocalPlayer` | 2주차. Raid 씬 |
| `Assets/_Scripts/Network/LocalPlayerCameraBinder.cs` | ProjectD.Network | 내 캐릭터 생성 시 Cinemachine Camera 추적 대상을 CameraTarget으로 연결 | 2주차. Raid 씬 |
| `Assets/_Scripts/Network/PlayerAnimationSync.cs` | ProjectD.Network | IPunObservable. 내 캐릭터는 State(byte), 뒷걸음 여부(bool), 회피 횟수(byte) 전송, 남의 캐릭터는 받은 값을 PlayerAnimationController에 적용, 회피 횟수가 바뀌면 회피 재시작 | 2주차. 플레이어 프리팹 루트, PhotonView Observed Components에 등록 |
| `Assets/_Scripts/Network/PlayerFireSync.cs` | ProjectD.Network | 내 발사를 RPC(무기 ID, 방향 배열)로 다른 사람에게 전송, 받은 발사는 이 캐릭터 총구에서 LaunchVisualOnly. `OnShotObserved`(내 발사와 받은 발사 모두, 소음용) | 2주차. 3주차 수정. 4주차 ItemDatabase로 무기 찾기. 플레이어 프리팹 루트 |
| `Assets/_Scripts/Combat/IHitHandler.cs` | ProjectD.Combat | 판정용 투사체 명중 시 할 일 인터페이스 `HandleHit(fireInfo, hit)` | 2주차. 구현 2개 |
| `Assets/_Scripts/Combat/DirectDamageHitHandler.cs` | ProjectD.Combat | 맞은 IDamageable에 바로 데미지 (일반 클래스) | 2주차. Sandbox 기본 |
| `Assets/_Scripts/Network/PlayerHitReporter.cs` | ProjectD.Network | IHitHandler. 방 안에서만 Launcher 명중 처리 교체, 쏜 사람은 마스터에 보고(대상 ViewID, 무기 ID, 위치), 마스터는 검증 후 NetworkHealth로 적용하고 맞은 적의 EnemyPerception에 공격자(EnemyTarget) 알림. 디버그 컨텍스트 메뉴로 가짜 보고 | 2주차. 3주차 수정. 4주차 ItemDatabase로 무기 찾기. 플레이어 프리팹 루트 |
| `Assets/_Scripts/Network/NetworkHealth.cs` | ProjectD.Network | 마스터만 데미지와 최대 체력 결정(`ApplyDamageAsMaster`, `SetMaxHealthAsMaster`), RpcTarget.All로 모두 적용, 중도 참가자에게 현재·최대 체력 전송. `IsDead` | 2주차, 3주차 수정. 적·플레이어(Health, PhotonView와 함께) |
| `Assets/_Scripts/Data/EnemyData.cs` | ProjectD.Data | 적 종류 수치 SO (ID 2000번대, 프리팹 이름, 체력, 속도, 시야, 경계 게이지, 공격, 공격 방식, 돌진·점사 수치, 적 탄 WeaponData, 복귀, 시체 루팅 테이블(4주차)). 메뉴 ProjectD/Enemy Data | 3주차 |
| `Assets/_Scripts/Data/EnemyDatabase.cs` | ProjectD.Data | 모든 적 데이터 목록. `Enemies`, `TryGetEnemy(id, out enemy)`, 빈 칸·ID 0·중복·프리팹 이름 경고 | 3주차 |
| `Assets/_Scripts/Data/EnemyBalanceSettings.cs` | ProjectD.Data | 인원별 적 체력 배율. `GetHealthMultiplier(playerCount)` | 3주차. 39번에서 사용 |
| `Assets/_Scripts/Network/EnemyNetworkSetup.cs` | ProjectD.Network | AI 실행 쪽(IsMine, 방 오브젝트는 마스터)이 아니면 Authority Only 목록을 끔, 방 밖이면 Network Only 목록을 끔. `HasAuthority`. 실행 순서 -100 | 3주차. 적 프리팹 루트 |
| `Assets/_Scripts/Network/EnemySpawner.cs` | ProjectD.Network | 마스터가 스폰 지점마다 무작위 종류를 InstantiateRoomObject로 생성, 생성 데이터로 스폰 지점 번호 전달, 생성 직후 순찰 경로(스폰 지점 + 자식들) 전달 | 3주차. Raid 씬 |
| `Assets/_Scripts/World/Occluder.cs` | ProjectD.World | 내 캐릭터를 가리는 벽·천장에 붙임. 숨기는 방식(Transparent: 투명 머티리얼 교체, ShadowsOnly: 그림자만) 설정, 원래 sharedMaterials·그림자 설정 저장 후 복구. `Occlude`, `Reveal`, `IsOccluding` | 3주차. 벽·천장 프리팹 변형 |
| `Assets/_Scripts/World/OccluderProbe.cs` | ProjectD.World | 0.1초마다 카메라→캐릭터 SphereCast(벽)와 머리 위 Raycast(천장)로 Occluder를 찾아 숨기고, 복구 지연 후 되돌림. 콜라이더별 Occluder 조회 결과 캐싱 | 3주차. 플레이어 프리팹 루트, Local Only 목록 |
| `Assets/_Scripts/AI/EnemyStatus.cs` | ProjectD.AI | 화면에 보여 줄 적 상태 값 보관. `CurrentState`, `SetState`, `AlertLevel`(0~1), `SetAlertLevel`, `IsTelegraphing`, `SetTelegraphing`. 마스터는 AI가, 다른 화면은 EnemySync가 씀 | 3주차. 적 프리팹 루트, 항상 켬 |
| `Assets/_Scripts/AI/EnemyPerception.cs` | ProjectD.AI | 시야(0.2초마다 반경·각도·Linecast), 경계 게이지(가까울수록 빠름), 소음(게이지 +0.3, 상한 0.8), 피격 반응. INoiseListener 구현. `VisibleTarget`, `IsTargetVisible`, `AlertTarget`, `LastKnownTargetPosition`, `LastHeardNoisePosition`, `IsFullyAlerted`, `ReactToAttack`, `OnFullyAlerted`, `OnNoiseHeardAt`, `OnAttacked`, `OnSightLost`. 노릴 수 없는 대상(IsTargetable false)은 감지하지 않음. 시야 Gizmo | 3주차. 적 프리팹 루트, Authority Only 목록 |
| `Assets/_Scripts/AI/INoiseListener.cs` | ProjectD.AI | 소음 수신 인터페이스 `OnNoiseHeard(position)` | 3주차 |
| `Assets/_Scripts/AI/NoiseSystem.cs` | ProjectD.AI | 정적 유틸리티. `Emit(position, radius, listenerMask)`로 반경 안 INoiseListener에 전달 (OverlapSphereNonAlloc) | 3주차. static |
| `Assets/_Scripts/AI/EnemyTarget.cs` | ProjectD.AI | 적이 보고 쫓을 대상 표시. `AimPoint`(가슴 높이), `IsTargetable`, `SetTargetable`, `ActiveTargets`(켜진 대상 목록, static) | 3주차. 플레이어 프리팹 루트, 모든 캐릭터에서 켬 |
| `Assets/_Scripts/Network/EnemySync.cs` | ProjectD.Network | IPunObservable. 마스터가 경계 게이지와 상태(byte), 예고 여부(bool)를 전송, 다른 화면은 EnemyStatus에 적용 | 3주차. 적 프리팹 루트, PhotonView Observed Components에 등록 |
| `Assets/_Scripts/Network/PlayerNoiseEmitter.cs` | ProjectD.Network | 마스터(또는 방 밖)에서 애니메이션 상태로 0.3초마다 이동 소음(걷기 2, 달리기 10, 회피 8m), 발사 확인 시 사격 소음(20m) | 3주차. 플레이어 프리팹 루트, 모든 캐릭터에서 켬 |
| `Assets/_Scripts/UI/EnemyStatusDebugView.cs` | ProjectD.UI | 적 머리 위 상태 이름과 "경계 N%" 글자와 색, 카메라를 향해 회전. EnemyStatus를 매 프레임 읽음 | 3주차. 적 프리팹 자식 TextMeshPro, 디버그용 |
| `Assets/_Scripts/AI/IEnemyState.cs` | ProjectD.AI | 적 상태 인터페이스. `Id`, `Enter`, `Tick`, `Exit` | 3주차. 구현 5개 |
| `Assets/_Scripts/AI/EnemyStateMachine.cs` | ProjectD.AI | 현재 상태 보관, `ChangeState`(Exit → Enter, 같은 상태면 다시 시작), `Tick`, `OnStateChanged` | 3주차. 일반 클래스 |
| `Assets/_Scripts/AI/EnemyBrain.cs` | ProjectD.AI | 상태 생성, 감지 이벤트로 상태 전환, 추적 대상 선택(가장 가까운 대상, 피격 시 공격자, 2.5초 유지). `GoToPatrol`, `GoToInvestigate`, `GoToChase`, `GoToAttack`, `GoToReturn`, `RefreshTarget`, `GetFlatDistanceTo`, `CurrentTarget`, `CurrentStateId` | 3주차. 적 프리팹 루트, Authority Only 목록 |
| `Assets/_Scripts/AI/EnemyMovement.cs` | ProjectD.AI | NavMeshAgent 이동(목적지가 0.5m 이상 바뀔 때만 경로 재계산, NavMesh 밖 목적지는 가까운 곳으로), 정지, 제자리 회전. `MoveTo`, `Stop`, `MoveDirectly`(돌진용, 경로 없이 이동), `FaceTowards`, `FaceDirection`, `HasArrived` | 3주차. 적 프리팹 루트, Authority Only 목록 |
| `Assets/_Scripts/AI/EnemyPatrolRoute.cs` | ProjectD.AI | 순찰 지점 목록과 지금 향하는 지점. `SetPoints`, `AdvanceToNext`, `CurrentPoint`, `CurrentIndex`, `Count`. 받지 않으면 처음 위치 하나 | 3주차. 적 프리팹 루트 |
| `Assets/_Scripts/AI/States/EnemyPatrolState.cs` | ProjectD.AI.States | 순찰 지점을 돌며 도착 시 대기, 지점이 하나면 제자리 경계 | 3주차 |
| `Assets/_Scripts/AI/States/EnemyInvestigateState.cs` | ProjectD.AI.States | 조사 위치로 이동 후 좌우로 둘러보고 복귀. `SetDestination` | 3주차. GameDesign의 경계 상태 |
| `Assets/_Scripts/AI/States/EnemyChaseState.cs` | ProjectD.AI.States | 보이면 쫓고 사거리 안이면 공격, 안 보이면 마지막 위치로, 복귀 시간 넘으면 복귀 | 3주차 |
| `Assets/_Scripts/AI/States/EnemyReturnState.cs` | ProjectD.AI.States | 하던 순찰의 향하던 지점으로 이동 후 순찰 | 3주차 |
| `Assets/_Scripts/AI/EnemyWeapon.cs` | ProjectD.AI | 적의 공격 수단. 돌진 접촉 검사(`CollectTouchingTargets`), `ReportMeleeHit`, `Fire`, `MuzzlePosition`, `OnMeleeHit`, `OnFired` | 3주차. 적 프리팹 루트 |
| `Assets/_Scripts/AI/EnemyTelegraphView.cs` | ProjectD.AI | EnemyStatus.IsTelegraphing이 바뀌면 색(MaterialPropertyBlock)과 크기 변경 | 3주차. 적 프리팹 루트, 모든 화면 |
| `Assets/_Scripts/AI/States/EnemyChargeAttackState.cs` | ProjectD.AI.States | 돌격형 공격: 예고 → 직진 돌진(벽에 막히면 중단, 대상마다 한 번 명중) → 빈틈 → 추적 | 3주차 |
| `Assets/_Scripts/AI/States/EnemyBurstAttackState.cs` | ProjectD.AI.States | 사수형 공격: 예고 → 점사 → 쉬기(가까우면 물러남) 반복, 안 보이거나 멀면 추적 | 3주차 |
| `Assets/_Scripts/Network/EnemyAttackSync.cs` | ProjectD.Network | IHitHandler. 적 발사 시 마스터는 판정용 탄 + 다른 사람에게 방향 RPC(시각용), 돌진 명중·탄 명중 시 PlayerDamageReceiver로 피격 신호 | 3주차. 적 프리팹 루트, 모든 화면 |
| `Assets/_Scripts/Network/PlayerDamageReceiver.cs` | ProjectD.Network | 피격자 최종 확인. 마스터 `SendHitSignal(damage, hitPoint)` → 주인이 회피 무적(여유 0.1초) 판단 후 답 → 마스터가 NetworkHealth로 적용. 같은 공격 번호는 한 번만 | 3주차. 플레이어 프리팹 루트, 모든 캐릭터 |
| `Assets/_Scripts/Network/PlayerTemporaryDeath.cs` | ProjectD.Network | 체력 0이면 적 대상에서 빠지고, 마스터가 일정 시간 뒤 RPC로 회복. `OnDown`, `OnRevived` | 3주차. 플레이어 프리팹 루트, 모든 캐릭터. 다운·사망 구현 시 교체 |
| `Assets/_Scripts/UI/PlayerHealthDebugView.cs` | ProjectD.UI | 내 캐릭터 머리 위 "HP n / 최대", 쓰러지면 "사망(임시)" | 3주차. 플레이어 프리팹 자식 TextMeshPro, Local Only 목록 |
| `Assets/_Scripts/Network/EnemyHealthScaler.cs` | ProjectD.Network | 마스터가 생성 직후·입장·퇴장 때 최대 체력 = EnemyData 기본 체력 × 인원 배율로 다시 정함 | 3주차. 적 프리팹 루트 |
| `Assets/_Scripts/Network/EnemyDormancy.cs` | ProjectD.Network | 0.5초마다 가장 가까운 플레이어 거리로 휴면(25m 깨움, 30m 재움), 휴면 중 Brain·Perception과 PhotonView 전송 끔, 맞으면 깨움, 참가자 입장 시 0.3초 전송. `IsDormant` | 3주차. 적 프리팹 루트, Authority Only 목록 |
| `Assets/_Scripts/Network/EnemyDeath.cs` | ProjectD.Network | 체력 0이면 각 화면에서 시체 처리(목록의 컴포넌트·콜라이더 끔, 사망 상태, 눕힘), 마스터는 전송 끔, 참가자에게 시체 위치 RPC. `IsCorpse` | 3주차. 적 프리팹 루트, 모든 화면 |
| `Assets/_Scripts/UI/LocalPlayerCrosshairBinder.cs` | ProjectD.UI | 내 캐릭터 생성 시 CrosshairView에 PlayerAimer, PlayerWeaponSpread 연결 | 2주차. Raid 씬 |
| `Assets/_Scripts/Core/IInteractable.cs` | ProjectD.Core | F 상호작용 대상 인터페이스. `Priority`, `HoldDuration`, `Position`, `Prompt`, `CanInteract`, `Interact(interactor)` | 4주차. 구현: LootContainerInteractable, WorldItem |
| `Assets/_Scripts/Core/InteractionHighlight.cs` | ProjectD.Core | 상호작용 대상으로 골라지면 표시 오브젝트를 켜거나 렌더러 색 변경. `SetHighlighted` | 4주차. 상자, 시체, 월드 아이템 |
| `Assets/_Scripts/Data/ItemStack.cs` | ProjectD.Data | (아이템 ID, 개수) 읽기 전용 구조체. `IsEmpty`, `WithCount` | 4주차. 전송·상자·사본 단위 |
| `Assets/_Scripts/Data/ItemData.cs` | ProjectD.Data | 아이템 공통 SO (ID, 이름, 분류, 희귀도, 그리드 크기, 최대 스택, 아이콘, 월드 프리팹, 판매가). ID 대역 분류별 | 4주차. WeaponData·AmmoData·ConsumableData의 부모 |
| `Assets/_Scripts/Data/AmmoData.cs` | ProjectD.Data | 탄약 아이템. `AmmoType` | 4주차 |
| `Assets/_Scripts/Data/ConsumableData.cs` | ProjectD.Data | 소모품 아이템. `Effect`(Health, Stamina), `Amount`, `UseSeconds` | 4주차. 사용은 5주차 |
| `Assets/_Scripts/Data/ItemDatabase.cs` | ProjectD.Data | 모든 아이템 목록, `TryGetItem`, `TryGetWeapon`, 빈 칸·ID 0·중복 경고 | 4주차. WeaponDatabase 대체 |
| `Assets/_Scripts/Data/LootTableEntry.cs` | ProjectD.Data | 루팅 테이블 한 줄 (아이템, 가중치, 개수 범위) | 4주차 |
| `Assets/_Scripts/Data/LootTable.cs` | ProjectD.Data | 루팅 테이블 SO. `Roll(results)`: 횟수 범위만큼 가중치로 뽑기 | 4주차. 상자 등급별, 적 종류별 |
| `Assets/_Scripts/Inventory/InventoryEntry.cs` | ProjectD.Inventory | 그리드에 놓인 아이템 (아이템, 개수, 위치, 회전, 예약 수). `Size`, `SpaceLeft`, `IsReserved`, `GetSize` | 4주차. 일반 클래스 |
| `Assets/_Scripts/Inventory/InventoryReservation.cs` | ProjectD.Inventory | 줍기 승인 대기 중 잡아 둔 자리 (합친 묶음과 개수, 새 칸) | 4주차. 일반 클래스 |
| `Assets/_Scripts/Inventory/InventoryGrid.cs` | ProjectD.Inventory | 그리드 계산. `CanPlace`, `TryFindFreeSpot`, `TryReserve`, `Commit`, `Cancel`, `TryMove`, `TryMerge`, `Remove`, `GetEntryAt`, `OnChanged` | 4주차. 일반 클래스 |
| `Assets/_Scripts/Inventory/PlayerInventory.cs` | ProjectD.Inventory | 내 인벤토리(4×4). Grid를 감싸 예약·확정·취소·옮기기·합치기·빼기, `OnChanged` | 4주차. 플레이어 프리팹 루트, Local Only |
| `Assets/_Scripts/Inventory/LootContainer.cs` | ProjectD.Inventory | 상자·시체 내용물 목록 (꺼낸 항목은 개수 0으로 번호 유지). `SetContents`, `TryGetEntry`, `RemoveEntry`, `HasContents`, `IsEmpty`, `DisplayName`, `OnChanged` | 4주차. 상자·적 루트, 모든 화면 |
| `Assets/_Scripts/Inventory/LootContainerInteractable.cs` | ProjectD.Inventory | IInteractable. F 1초 → 상호작용한 플레이어의 PlayerLootSession.Open | 4주차. 상자·적 루트 |
| `Assets/_Scripts/Inventory/PlayerLootSession.cs` | ProjectD.Inventory | 열어 둔 상자 하나, 멀어지면(3m) 닫기. `Open`, `Close`, `CurrentContainer`, `OnContainerOpened`, `OnContainerClosed` | 4주차. 플레이어 프리팹 루트, Local Only |
| `Assets/_Scripts/Player/PlayerInputGate.cs` | ProjectD.Player | 막는 이유(인벤토리, 채팅)별로 인스펙터 목록의 입력 컴포넌트를 끄고 켬. `SetBlocked`, `IsBlocked`, `IsAimBlocked`, `OnChanged` | 4주차. 플레이어 프리팹 루트, Local Only |
| `Assets/_Scripts/Player/PlayerInteractor.cs` | ProjectD.Player | 0.1초마다 반경 2m Interactable 레이어에서 우선순위 → 거리로 대상 선택·강조, F 즉시/길게 실행, 회피 시 취소. `SetInteractHeld`, `CurrentTarget`, `HoldProgress`, `OnTargetChanged` | 4주차. 플레이어 프리팹 루트, Local Only |
| `Assets/_Scripts/Player/InteractionInputReader.cs` | ProjectD.Player | F(Interact) 입력을 PlayerInteractor에 전달. 실행 순서 -10 | 4주차. 플레이어 프리팹 루트, Local Only |
| `Assets/_Scripts/Network/PlayerItemLedger.cs` | ProjectD.Network | 마스터의 플레이어별 (ID, 개수) 사본. `Add`, `TryRemove`, `GetItems`, 나가면 삭제, 디버그 출력 메뉴 | 4주차. Raid 씬에 하나 |
| `Assets/_Scripts/Network/LootContainerSync.cs` | ProjectD.Network | 마스터가 내용물 결정(시작 시 루팅 테이블 또는 `GenerateContents`) → RPC All, 꺼내기 요청 검증(항목, 주인, 거리 4m) → 빠짐 RPC All + 요청자 결과, 사본 갱신, 중도 참가자에게 현재 내용물. `RequestTake` | 4주차. 상자·적 루트(PhotonView), 모든 화면 |
| `Assets/_Scripts/Network/EnemyCorpseLoot.cs` | ProjectD.Network | 적 사망 시 시체 상호작용 콜라이더 켜기, 마스터가 EnemyData 루팅 테이블로 내용물 결정 | 4주차. 적 루트, 모든 화면 |
| `Assets/_Scripts/Network/WorldItem.cs` | ProjectD.Network | 땅의 아이템 방 오브젝트 (생성 데이터 ID·개수, 모양은 각 화면에서 붙임), IInteractable(F 즉시), 줍기 요청 검증 후 결과 → Destroy. `RequestPickup`, `Stack`, `Item` | 4주차. Resources/WorldItem 프리팹 |
| `Assets/_Scripts/Network/PlayerItemTransfer.cs` | ProjectD.Network | 상자 꺼내기·땅 줍기(자리 예약 → 요청 → 확정/취소), 버리기(인벤토리에서 빼고 마스터가 사본 검증 후 WorldItem 생성), 디버그 지급. `RequestTake`, `RequestPickup`, `RequestDrop`, `OnTransferFailed` | 4주차. 플레이어 프리팹 루트, 모든 캐릭터 |
| `Assets/_Scripts/Network/RaidChat.cs` | ProjectD.Network | 방 안 채팅 RPC All, 보낸 사람 이름은 받는 쪽에서 붙임, 60자, 입장·퇴장 알림. `Send`, `IsAvailable`, `OnMessageReceived`, `OnSystemMessage` | 4주차. Raid 씬 PhotonView 오브젝트 |
| `Assets/_Scripts/UI/ItemIconPalette.cs` | ProjectD.UI | 분류별 아이콘 색 SO. `GetColor(item)` | 4주차 |
| `Assets/_Scripts/UI/InventoryWindow.cs` | ProjectD.UI | Tab 인벤토리 창, 칸·아이템 그리기, 드래그 옮기기·R 회전·합치기·창 밖 버리기, 놓일 자리 미리보기, 조준점 숨김, 실패 문구. `SetTarget`, `Open`, `Close`, `IsOverGrid` | 4주차. Raid·Sandbox Canvas |
| `Assets/_Scripts/UI/InventoryItemView.cs` | ProjectD.UI | 인벤토리 아이템 그림, 드래그 입력을 창에 전달 | 4주차. 프리팹 |
| `Assets/_Scripts/UI/LootWindow.cs` | ProjectD.UI | 상자 내용물 창(슬롯 목록), 우클릭·드래그로 꺼내기 요청, 열면 인벤토리 창도 열림. `SetTarget`, `TakeEntry` | 4주차. Raid·Sandbox Canvas |
| `Assets/_Scripts/UI/LootSlotView.cs` | ProjectD.UI | 상자 항목 하나, 우클릭·드래그 | 4주차. 프리팹 |
| `Assets/_Scripts/UI/InteractionPromptView.cs` | ProjectD.UI | "F 열기: 상자" 안내와 길게 누르기 진행 막대 | 4주차. Canvas |
| `Assets/_Scripts/UI/ChatView.cs` | ProjectD.UI | Enter 입력창 열기·보내기, Esc 취소, 입력 중 게임 입력 막기, 최근 6줄, 흐려짐, 오프라인 숨김 | 4주차. Raid Canvas |
| `Assets/_Scripts/UI/LocalPlayerUiBinder.cs` | ProjectD.UI | 내 캐릭터 생성 시(또는 Sandbox에서 씬 검색) 인벤토리·상자·안내·채팅 화면에 캐릭터 컴포넌트 연결 | 4주차. Raid·Sandbox |
 
현재 Sandbox 씬의 `Player` 구성: CharacterController, PlayerStamina, PlayerMover, PlayerAimer, PlayerDodger, PlayerDodgeDebugView, PlayerInputReader, PlayerWeaponHolder, PlayerWeaponSpread, PlayerReloader, ProjectileLauncher, PlayerShooter, WeaponInputReader, CinemachineImpulseSource, ShotCameraShake, 자식 `CameraTarget`(PlayerCameraTargetOffset), 자식 `Muzzle`
 
씬의 기타 오브젝트: `ProjectilePool`, `DummyTarget`(Health, DamageFlash, DummyTarget, DamageNumberSpawner), Canvas 아래 `Crosshair`(CrosshairView)
 
---
 
## 9. 커밋 대기 내용
 
커밋은 사용자가 요청할 때만 한다. 요청을 받으면 아래 목록으로 Summary와 Description을 작성하고, 커밋 후 목록을 비운다.
 
마지막 커밋: `chore: complete week 3 - enemy ai` (3주차 1~43번, 46번 포함, 태그 `week3`, 2026-11-19)
 
마지막 커밋 이후 변경
- (4주차) 아이템 데이터: ItemData, AmmoData, ConsumableData, ItemDatabase, ItemStack, LootTable, LootTableEntry 신규, WeaponData가 ItemData 상속, WeaponDatabase 삭제(ItemDatabase로 대체), EnemyData에 루팅 테이블, Enum.cs에 ItemCategory·ItemRarity·ConsumableEffect·InputBlockReason
- (4주차) 그리드 인벤토리: InventoryEntry, InventoryReservation, InventoryGrid, PlayerInventory, PlayerInputGate 신규, PlayerInputReader(인벤토리 중 조준 고정) 수정
- (4주차) 상호작용: IInteractable, InteractionHighlight, PlayerInteractor, InteractionInputReader 신규
- (4주차) 루팅과 월드 아이템: LootContainer, LootContainerInteractable, PlayerLootSession, LootContainerSync, EnemyCorpseLoot, WorldItem, PlayerItemTransfer, PlayerItemLedger 신규, PrefabPaths(WorldItem), PlayerFireSync·PlayerHitReporter(ItemDatabase) 수정
- (4주차) UI와 채팅: ItemIconPalette, InventoryWindow, InventoryItemView, LootWindow, LootSlotView, InteractionPromptView, ChatView, LocalPlayerUiBinder, RaidChat 신규, Input Actions에 Chat(Enter)·RotateItem(R) 추가
---
 
## 변경 이력
 
| 날짜 | 내용 |
|---|---|
| 2026-10-09 | 최초 작성 |
| 2026-10-10 | Unity 버전(6000.3.10f1) 기록, Photon 폴더를 Assets/Photon으로 유지, Git 세팅을 GitHub Desktop 기준으로 변경, 폴더 구조를 Assets/_Scripts 기준으로 변경, 에셋 검증을 조작 구현 뒤(애니메이션 전)로 이동하고 전체 번호 재정렬, 임시 에셋 선택(Toon Shooter Game Kit, Modular Sci-Fi MegaKit) 기록, 1~11번 완료 표시 |
| 2026-10-10 | 12~15번 완료 처리. 입력 방식(Generate C# Class, 길게 누르기는 코드 처리), 카메라 설정값과 카메라 타깃 자식 방식, 정면 시점 0도, 스태미나 고갈 후 재입력 규칙 기록. 커밋 형식을 Conventional Commits로 변경. 현재까지 만든 스크립트 목록 추가 |
| 2026-10-10 | 16~19번 스크립트 작성에 맞춰 "현재까지 만든 스크립트" 목록 갱신 |
| 2026-10-22 | 16~19번 완료 처리 |
| 2026-10-22 | 커밋 대기 내용 섹션 추가 |
| 2026-10-22 | 20~27번 사격 스크립트 작성에 맞춰 스크립트 목록, 씬 구성, 커밋 대기 내용 갱신 |
| 2026-10-22 | 20~27번 완료 처리 |
| 2026-10-22 | 스크립트 목록에 Enum.cs, ShaderPropertyIds.cs 반영(AmmoType.cs 삭제), 커밋 대기 내용 추가 |
| 2026-10-22 | 폴더 구조에 Core/Define 추가 |
| 2026-10-22 | 애니메이션 방식 변경: Generic과 팩 클립만 사용, 단일 레이어·블렌드 트리 없음·State 정수 파라미터. 29~34번 재작성, 기술 결정 표 추가 |
| 2026-10-23 | 31~34번 수정: Loop Time 대상, 발밑 기준점과 조정 값, 모델 방향, 재장전 클립 반복, 회피 시작 시 Animator.Play 재시작, AnimatorStates 추가, 발 미끄러짐 조정, 대쉬 클립 이동 확인 |
| 2026-10-23 | 28~30번 완료 처리 (추가 설정 없이 팩 클립 그대로 사용) |
| 2026-10-23 | 33번 스크립트 작성에 맞춰 스크립트 목록 갱신 (AnimatorParams, AnimatorStates, PlayerAnimationController, WeaponAnimationBridge, PlayerAnimState, PlayerMover.MoveInputDirection) |
| 2026-10-23 | 31~33번 완료 처리, 커밋 대기 내용 추가 |
| 2026-10-30 | 2주차 5~9번 입장 흐름 스크립트 14개를 스크립트 목록과 커밋 대기 내용에 추가 |
| 2026-10-31 | 2주차 10~13번 스크립트를 스크립트 목록과 커밋 대기 내용에 반영 |
| 2026-10-31 | BootConnector, RaidSceneLoader 설명 갱신(씬 자동 동기화 미사용), 커밋 대기 내용에 중도 참가 수정 추가 |
| 2026-10-31 | 2주차 15번 스크립트(PlayerAnimationSync, PlayerAnimationController 수정)를 스크립트 목록과 커밋 대기 내용에 반영 |
| 2026-10-31 | 2주차 17~19번 스크립트를 스크립트 목록과 커밋 대기 내용에 반영 |
| 2026-10-31 | 2주차 20~23번 스크립트를 스크립트 목록과 커밋 대기 내용에 반영 |
| 2026-10-31 | 커밋 대기 내용에 혼자 플레이 끊김 오판 수정 추가 |
| 2026-11-06 | 커밋 대기 내용 비움(2주차 마무리 커밋), 현재 위치와 45번 갱신(1주차 일지 작성, week1 태그 생략) |
| 2026-11-06 | 스크립트 목록에서 PhotonConnectionTester 제거(2주차 26번 삭제) |
| 2026-11-06 | 에셋 변경에 따라 35~37번 삭제, 38~41번 3주차로 이동, 42번 테마 확정(공사장) |
| 2026-11-06 | 3주차 Occluder, OccluderProbe를 스크립트 목록과 커밋 대기 내용에 추가 |
| 2026-11-07 | 3주차 15~18번 스크립트를 스크립트 목록과 커밋 대기 내용에 추가 |
| 2026-11-07 | 3주차 20~24번 감지 스크립트 8개 추가, PlayerFireSync·PlayerHitReporter 설명 갱신, 커밋 대기 내용 추가 |
| 2026-11-07 | 3주차 25~29번 상태 머신 스크립트 10개 추가, 수정 스크립트 6개 설명 갱신, 커밋 대기 내용 추가 |
| 2026-11-07 | 3주차 30~38번 적 공격·플레이어 피격 스크립트 8개 추가, 수정 스크립트 설명 갱신, EnemyAttackState 삭제, 커밋 대기 내용 추가 |
| 2026-11-07 | 3주차 39~42번 스크립트 3개 추가, Health·NetworkHealth·EnemyTarget 설명 갱신, 커밋 대기 내용 추가 |
| 2026-11-19 | 3주차 마무리 커밋으로 커밋 대기 내용 비움 |
| 2026-11-19 | 4주차 스크립트 36개 추가(아이템 데이터, 인벤토리, 상호작용, 루팅, 월드 아이템, 채팅), WeaponDatabase 삭제, 수정 스크립트 설명 갱신, 커밋 대기 내용 추가 |
 