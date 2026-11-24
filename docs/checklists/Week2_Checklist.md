# 2주차 체크리스트 — PUN 2 도입
 
> 이 문서는 새 대화방에서도 이 파일과 `GameDesign.md`, `ProjectGuidelines.md`, `Week1_Checklist.md`만 보고 바로 작업을 이어갈 수 있도록 작성했다.
> 기획 결정과 이유는 `GameDesign.md`, 대화·코드 규칙은 `ProjectGuidelines.md`, 지금까지 만든 스크립트와 커밋 대기 내용은 `Week1_Checklist.md` 8번, 9번에 있다.
>
> 작성일: 2026-10-24 / 최종 수정: 2026-10-30
 
---
 
## 0. 새 대화방을 시작할 때 읽을 것
 
### 0-1. 프로젝트 한눈에 보기
 
| 항목 | 내용 |
|---|---|
| 프로젝트 | 덕르코프 (코드명 ProjectD) |
| 목적 | 게임 클라이언트 프로그래머 취업용 포트폴리오 |
| 장르 | 3D 쿼터뷰 협동 익스트랙션 슈터 (Escape from Duckov 참고) |
| 플레이 | 2~4인 협동 PvE, 싱글 가능 |
| 엔진 | Unity 6000.3.10f1 (6.3), URP, New Input System, Cinemachine 3 |
| 네트워크 | Photon PUN 2, 마스터 클라이언트 권한 구조, 공개 방 목록, 중도 참가 허용, 싱글은 오프라인 모드, 리전 kr 고정 |
| 테스트 | ParrelSync(Clones Manager)로 에디터 2개 + 주 1회 실제 빌드 PC 2대 |
| 버전 관리 | GitHub Desktop, 공개 저장소 `ProjectD`, Git LFS |
| 현재 위치 | 1주차 1~33번 완료, 34~45번 보류. 2주차 2~19번 완료(필수 목표 달성, ParrelSync 확인). 1번 결정 사항 모두 확정. 20~24번 완료(Lobby 혼자 플레이 끊김 오판 수정 포함). 25번 보류(8주차 마무리로 미룸). 26~28번 완료(커밋, 태그 week2). **2주차 완료**. 다음은 3주차(`Week3_Checklist.md`) |
 
### 0-2. 1주차 상태
 
- 완료: 환경 세팅, 입력, 카메라, 이동, 스태미나, 조준, 정밀 조준, 회피와 무적, 사격 전체(투사체 풀, 탄퍼짐, 조준점, 재장전, 더미 적, 화면 흔들림), 캐릭터 모델 적용, 애니메이션(단일 레이어, State 번호 방식)
- 보류: 34번(회피 클립 길이, 발 미끄러짐 조정), 35~40번(실내 표현 검증), 41~45번(에셋 점검, 테마 확정, 1주차 마무리 커밋)
- **35~40번 실내 표현 검증은 3주차 그레이박스 맵 제작 전까지 반드시 끝낸다** (맵 제작 방식 전체가 여기에 달려 있음)
- 1주차 마무리 커밋과 `week1` 태그는 보류 항목을 끝낸 뒤에 한다
- 마지막 커밋 이후 변경 목록은 `Week1_Checklist.md` 9번 "커밋 대기 내용"에 있다. 2주차 작업도 커밋하기 전까지 같은 곳에 이어서 기록한다
### 0-3. 작업 규칙 요약
 
자세한 규칙은 `ProjectGuidelines.md`를 따른다.
 
- 한국어, 존댓말, 간결하게. 질문은 채팅 문장으로
- 결정이 필요하면 선택지별 장단점과 추천을 먼저 설명하고 최종 결정은 사용자가 한다
- 확실하지 않은 내용(특히 PUN 2 API)은 추측하지 않고 불확실하다고 밝힌다
- 문서는 수정안을 먼저 보여주고 허락을 받은 뒤 갱신하고, 갱신하면 claude.ai 프로젝트에도 저장한다
- **스크립트는 사용자가 요청할 때만 작성한다**. 두 개 이상이면 `Assets/_Scripts` 폴더 구조대로 압축 파일 하나로 제공
- 네임스페이스 `ProjectD.하위모듈`, 한글 주석, 이모지 금지, 파일 첫 줄 경로 주석, 모든 함수에 XML 주석
- 열거형은 `Core/Define/Enum.cs`에, 문자열 이름(파라미터, 셰이더 속성, 씬 이름, 프리팹 경로, 방 속성 키 등)은 `Core/Define`의 용도별 정적 클래스에 모은다
- 커밋은 사용자가 요청할 때만, 마지막 커밋 이후 변경을 모아 Summary(영어, Conventional Commits)와 Description(한글)으로 작성
### 0-4. 2주차에 하지 않는 것
 
- 적 AI, 적 투사체, 피격자 최종 확인, 인원 변경 시 적 체력 배율 재계산, 적 상태 중도 참가 동기화 → 3주차
- 아이템, 인벤토리, 루팅, 줍기, 상자·아이템 중도 참가 동기화 → 4주차
- 플레이어 체력 0 이후(다운, 부활, 사망, 시체), 연결 끊김 처리, 재참가 제한 → 5주차
- 탈출, 제한 시간, 저장, 상점, 창고, 중도 참가 마감(3분 경고, 탈출 카운트다운) → 6주차
- 2주차 Boot·Lobby UI는 테스트용 최소 구성. 정식 로비 UI는 6주차
- 마스터 이전, 재접속 → 범위 밖
---
 
## 1. 2주차 목표
 
1주차에 혼자 움직이던 캐릭터를 **2~4명이 같은 방에서 함께 움직이고 쏠 수 있게** 만든다. 싱글은 같은 구조를 오프라인 모드로 돌린다.
 
### 1-1. 필수 목표 (반드시 이번 주에 끝낸다)
 
1. **입장 흐름**: Boot에서 이름 입력 → Lobby에서 방 목록 확인 → 방 만들기면 바로 Raid, 목록에서 참가하면 진행 중인 Raid에 합류. 다른 플레이어가 나중에 Lobby에 들어와도 진행 중인 방이 목록에 보인다
2. **이동 동기화 확인**: Raid에서 서로의 이동이 동기화되는 것을 **ParrelSync(Clones Manager)로 에디터 2개를 띄워 확인**한다
필수 목표에 해당하는 항목은 3번 할 일에 **[필수]** 로 표시했다. 필수 항목을 먼저 끝내고 나머지를 진행한다.
 
### 1-2. 전체 목표 (완료되면 할 수 있어야 하는 것)
 
- 이름을 입력하고 로비에 들어가 방 목록을 본다
- 방을 만들면 바로 레이드에 들어가고, 그 방이 다른 사람의 로비 목록에 보인다
- 다른 사람이 목록에서 그 방을 골라 진행 중인 레이드에 합류한다. 4명이면 참가할 수 없다
- 서로의 이동, 조준 방향, 회피, 애니메이션이 보인다
- 누가 쏘든 모든 화면에 탄이 보이고, 더미 적에게 맞으면 모든 화면에서 같은 체력이 줄어든다. 나중에 들어온 사람에게도 같은 체력이 보인다
- 혼자 플레이 버튼으로 인터넷 없이 같은 흐름을 플레이할 수 있다
### 1-3. 입장 흐름 그림
 
```
Boot (이름 입력, 접속)
  └─ 접속 성공 → Lobby
  └─ 접속 실패 → 다시 시도 또는 혼자 플레이(오프라인)
 
Lobby (방 목록: 방 이름, 인원 n/4, 참가 가능 여부)
  ├─ 방 만들기 → 방 생성(목록에 공개) → Raid (내가 마스터)
  ├─ 목록에서 참가 → 진행 중인 Raid에 합류 (시작 지점에서 등장)
  └─ 혼자 플레이 → 오프라인 모드 방 생성(목록에 안 보임) → Raid
 
Raid
  └─ 나가기(임시 버튼) → Lobby
     마스터가 나가면 남은 사람도 Lobby로 (세션 종료, 정식 처리는 5·6주차)
```
 
---
 
## 2. 시작 전에 정할 것
 
D1, D6, D7과 2-2의 N1~N7은 확정되었다. D2~D5는 추천안이 있지만 **확정되지 않았다**. 해당 항목을 시작하기 전에 사용자에게 선택지별 장단점과 추천을 설명하고 결정을 받는다. 결정되면 이 표의 상태를 확정으로 바꾸고 `GameDesign.md`에도 반영한다.
 
### D1. 다른 플레이어의 위치·회전 동기화 방식 [확정: A. PhotonTransformView]
 
| 선택지 | 장점 | 단점 |
|---|---|---|
| A. PUN 2 기본 컴포넌트 (PhotonTransformView 계열) | 코드 없이 바로 동작 | 보간 방식을 세밀하게 조정하기 어렵고, 포트폴리오에서 보여줄 것이 적음 |
| B. 직접 작성 (IPunObservable로 위치·회전을 보내고 받는 쪽에서 보간) **추천** | 보간 방식을 직접 설명할 수 있어 포트폴리오 가치가 큼. 필요한 값만 보냄 | 코드 작성과 튜닝이 필요 |
 
- 2026-10-30 사용자 결정: A. PhotonTransformView
- 설정: 위치와 회전만 동기화, 스케일은 끈다
- 남의 캐릭터에서는 PlayerMover, PlayerDodger, PlayerAimer를 끈다. 위치와 회전을 PhotonTransformView와 두 곳에서 바꾸면 떨린다. 특히 PlayerAimer는 매 프레임 조준점 쪽으로 회전하므로 반드시 끈다
### D2. 다른 플레이어의 애니메이션 동기화 방식 [확정: B. State 번호와 MoveDirection 전송]
 
| 선택지 | 장점 | 단점 |
|---|---|---|
| A. 이동·달리기·조준·회피 상태 값들을 보내고, 받는 쪽 PlayerAnimationController가 각자 State 계산 | 받는 쪽도 같은 규칙으로 계산 | 보낼 값이 많고, 사격·재장전 정보도 따로 보내야 함 |
| B. 계산된 State 번호와 MoveDirection만 보냄 **추천** | 보내는 값이 최소. "State를 정하는 곳은 한 곳"이라는 1주차 원칙과 맞음 | 받는 쪽에서 세부 상태를 알 수 없음 (애니메이션에는 불필요) |
 
- B 추천 세부: PlayerAnimationController에 원격용 함수(예: 받은 State와 MoveDirection을 그대로 적용)를 추가해서 Animator에 쓰는 곳은 여전히 이 컴포넌트 하나로 유지
- 연속 회피 재시작: 회피 횟수 카운터(byte)를 함께 보내고, 받은 값이 바뀌면 받는 쪽에서도 Dodge를 처음부터 재생
- 2026-10-31 사용자 결정: B. PlayerAnimationSync(IPunObservable)가 PhotonView 정기 전송으로 State(byte), 뒷걸음 여부(bool), 회피 횟수(byte)를 보냄. 받는 쪽은 PlayerAnimationController.ApplyExternalState, PlayDodgeFromStart로 적용
### D3. RPC로 보낼 무기 식별 방법 [확정: B. 무기 데이터베이스 + int ID]
 
WeaponData(ScriptableObject)는 RPC로 직접 보낼 수 없다.
 
| 선택지 | 장점 | 단점 |
|---|---|---|
| A. PlayerWeaponHolder 목록의 번호 | 추가 작업 없음 | 4주차 인벤토리로 무기 목록이 바뀌면 번호가 어긋남 |
| B. 무기 데이터베이스 ScriptableObject (모든 WeaponData 목록) + WeaponData에 고유 ID **추천** | 인벤토리, 상점, 저장에서도 같은 ID를 재사용 | 데이터베이스 에셋 하나와 ID 관리가 추가됨 |
 
- 2026-10-31 사용자 결정: B. ID는 int, 분류별 번호 대역(무기 1000번대). WeaponDatabase(ScriptableObject)가 ID로 무기를 찾고, 비어 있는 칸·ID 0·중복 ID를 에디터에서 경고한다. 4주차 ItemData ID도 같은 int 체계로 잇는다
### D4. 산탄총처럼 한 번에 여러 발이 나갈 때의 RPC [확정: B. 한 번 쏠 때 RPC 하나]
 
| 선택지 | 장점 | 단점 |
|---|---|---|
| A. 탄 한 발마다 RPC 하나 | 가장 단순 | 산탄총 한 번에 RPC 8개 |
| B. 한 프레임의 발사를 모아 RPC 하나 (시작점, 방향 배열, 무기 ID) **추천** | 메시지 수 최소 | 모으는 코드 필요. Vector3 배열을 RPC 인자로 보낼 수 있는지 확인 필요 (안 되면 float 배열로) |
| C. 시작점, 조준 방향, 무작위 시드만 보내고 받는 쪽에서 같은 탄퍼짐 재계산 | 메시지 크기 최소 | 받는 쪽 계산이 정확히 같아야 해서 구현이 까다로움 |
 
- 2026-10-31 사용자 결정: B. RPC 인자는 무기 ID와 방향 배열(Vector3[])만 보낸다. 시작점은 보내지 않고 받는 쪽 화면의 그 캐릭터 총구 위치를 쓴다 (남의 캐릭터는 약간 늦게 그려지므로 탄이 캐릭터에서 나가 보이게 하기 위함. 시각용이라 판정에는 영향 없음). RpcTarget.Others, 서버 보관 없음
### D5. 명중 처리 구조 (Combat 모듈이 네트워크를 모르게) [확정: B. IHitHandler]
 
현재 ProjectileLauncher는 명중하면 IDamageable에 바로 데미지를 준다. 멀티에서는 쏜 사람이 판정하고 마스터가 검증 후 적용해야 한다(GameDesign 9-1).
 
| 선택지 | 장점 | 단점 |
|---|---|---|
| A. ProjectileLauncher 안에서 네트워크 여부로 분기 | 파일이 늘지 않음 | Combat이 Network를 알게 되어 모듈 방향 규칙 위반 |
| B. Combat에 명중 처리 인터페이스(IHitHandler)를 두고, 오프라인용(바로 데미지)과 네트워크용(마스터에 보고, Network 모듈)을 각각 구현 **추천** | Combat은 네트워크를 모름. 구현이 두 개라 인터페이스 사용 기준(지침 4번)에 맞음 | 인터페이스와 클래스 2개 추가 |
 
- 오프라인 모드도 마스터가 자기 자신이므로 네트워크용 처리로 그대로 동작한다. 오프라인용은 Sandbox 테스트용으로 남는다
- 2026-10-31 사용자 결정: B. Combat에 IHitHandler와 DirectDamageHitHandler(기본, 바로 데미지), ProjectileLauncher.SetHitHandler. Network의 PlayerHitReporter가 방 안에서만 자신을 명중 처리로 넣고, 쏜 사람은 마스터에 보고, 마스터는 검증 후 적용
### D6. 2클라이언트 테스트 도구 [확정: ParrelSync]
 
- **ParrelSync(Clones Manager)** 로 에디터 2개를 띄워 테스트한다 (2026-10-30 사용자 결정)
- 주 1회 실제 빌드로 PC 2대 테스트 (GameDesign 9번)
### D8. 받는 쪽 탄을 시각용으로 쏘는 방법 [확정: A. LaunchVisualOnly]
 
같은 프리팹이라 Deals Damage 설정 하나로는 내 캐릭터와 남의 캐릭터를 구분할 수 없어, 그대로 두면 모든 화면에서 같은 탄이 데미지를 준다.
 
| 선택지 | 장점 | 단점 |
|---|---|---|
| A. ProjectileLauncher에 LaunchVisualOnly(FireInfo) 추가 **추천** | 의도가 이름으로 보임, 프리팹 하나로 처리 | ProjectileLauncher 수정 |
| B. 시각용 ProjectileLauncher를 하나 더 붙임 | 코드 수정 없음 | 같은 컴포넌트가 둘이라 연결 실수 위험 |
| C. PlayerNetworkSetup이 남의 캐릭터의 Deals Damage를 끔 | 호출 코드 그대로 | Network가 Combat 설정 값을 직접 건드림 |
 
- 2026-10-31 사용자 결정: A. Deals Damage 설정은 제거하고 Launch(판정용), LaunchVisualOnly(시각용) 두 함수로 구분
### D9. 쏜 사실을 Network에 알리는 방법 [확정: A. 발사 단위 FireInfo 목록 이벤트]
 
| 선택지 | 장점 | 단점 |
|---|---|---|
| A. PlayerShooter가 한 번 쏠 때마다 FireInfo 목록을 이벤트로 알림 **추천** | 발사 하나에 이벤트 하나라 D4 B와 맞음, Combat은 네트워크를 모름 | 이벤트 하나 추가 |
| B. ProjectileLauncher가 탄마다 이벤트, Network가 프레임 단위로 모음 | PlayerShooter 수정 없음 | 모으는 처리 필요, 흐름이 덜 직관적 |
 
- 2026-10-31 사용자 결정: A. PlayerShooter.OnShotFired(IReadOnlyList<FireInfo>)와 MuzzlePosition 추가. PlayerFireSync(Network)가 구독해 RPC 전송, 받은 쪽은 MuzzlePosition에서 LaunchVisualOnly
### D10. 더미 적 체력 동기화 [확정: A. 마스터가 데미지 적용 RPC]
 
| 선택지 | 장점 | 단점 |
|---|---|---|
| A. 마스터가 데미지 적용(양, 위치)을 RpcTarget.All로 보내고 각자 Health.TakeDamage 실행 **추천** | 기존 Health 이벤트(깜빡임, 데미지 숫자, 사망)가 모든 화면에서 그대로 동작, RPC 순서 보장으로 체력 일치 | 중도 참가자에게 현재 체력을 따로 보내야 함 |
| B. 현재 체력은 정기 전송, 연출 이벤트만 RPC | 체력 값이 항상 최신 | 값과 연출 순서가 꼬일 수 있음, 변화가 없어도 계속 전송 |
 
- 2026-10-31 사용자 결정: A. NetworkHealth(Network)가 담당. 마스터는 OnPlayerEnteredRoom에서 새 플레이어에게만 현재 체력 전송(Health.SetCurrentHealth 추가). 사망 이후 데미지는 마스터가 받지 않아 사망은 한 번만 처리. 더미 적 재등장 타이머는 각 화면에서 따로 돈다(3주차에 실제 적으로 대체)
### D11. 쏜 사람 화면의 명중 연출 시점 [확정: A. 마스터 결과 후]
 
| 선택지 | 장점 | 단점 |
|---|---|---|
| A. 마스터 결과가 돌아온 뒤 표시 **추천** | 단순, 모든 화면 동일, 거절된 명중은 연출 없음 | 왕복 지연만큼 늦음 |
| B. 내 화면은 즉시 연출, 체력은 마스터 결과로 반영 | 즉각적인 손맛 | 중복 연출 방지 필요, 거절된 명중도 연출 |
 
- 2026-10-31 사용자 결정: A. 즉시 보여 줄 히트마커는 7주차 피격 피드백에서 따로 붙인다
### D7. Network 모듈 의존 방향 [확정]
 
- 확정 (2026-10-30): **Network → Player, Combat, Core 참조 가능. Player, Combat은 Network를 참조하지 않음**
- 이유: Player와 Combat은 오프라인 Sandbox에서도 그대로 동작해야 하고, 네트워크 부품은 그 위에 얹는 구조
- UI는 Network의 이벤트를 구독할 수 있다. Network는 UI를 모른다
### 2-2. 입장 흐름 결정 (N1~N7) [확정]
 
2026-10-30 사용자 결정. 참고용으로 받은 PUN Basics 튜토리얼 기반 코드(Launcher, LobbyManager, GameManager, PlayerNameInputField)의 흐름을 따르되, 프로젝트 구조에 맞게 새로 작성한다.
 
| 번호 | 결정 |
|---|---|
| N1 | Photon 기본 로비 사용. 방 목록은 바뀐 방만 오므로 딕셔너리에 반영해 전체 목록을 직접 보관 (참고 코드의 SQL 로비 + 요청 방식은 자동 갱신이 없어 사용하지 않음) |
| N2 | 씬별 작은 클래스 + 네트워크와 UI 분리 (싱글톤 NetworkSession 사용 안 함) |
| N3 | 이름은 앞뒤 공백 제거 후 1~12자, 마지막 이름을 PlayerPrefs에 저장해 다음 실행 때 미리 채움, 중복 허용 |
| N4 | 레이드에서 나가면 Lobby로 이동 (혼자 플레이는 Boot로) |
| N5 | 시작 지점 안의 자리는 ActorNumber를 4로 나눈 나머지로 배정 |
| N6 | 마스터가 이탈하면 새 마스터가 방을 닫고(목록에서도 숨김) 전원이 나가 Lobby로 이동 |
| N7 | 방 목록이 바뀌면 항목을 전부 지우고 다시 그림 (풀링 없음) |
 
씬 이동 방식 (2026-10-31 변경): AutomaticallySyncScene을 쓰지 않고, 방에 들어간 사람이 각자 RaidSceneLoader의 OnJoinedRoom에서 PhotonNetwork.LoadLevel로 Raid 씬을 불러온다. 자동 동기화를 쓰면 중도 참가자가 기존 캐릭터 생성 정보를 Lobby 씬에서 처리해 버려, 씬이 바뀔 때 기존 캐릭터가 함께 사라졌다 (아래 개발 일지 후보 참고).
 
---
 
## 3. 할 일 (순서대로)
 
각 항목의 `확인`이 그 항목의 완료 기준이다. 스크립트 이름은 예시이며 실제 이름은 작성할 때 정한다. **[필수]** 는 1-1 필수 목표에 해당하는 항목이다.
 
### A. 준비 (1~4)
 
- [ ] **1. 2주차 결정 사항 확정**
  - D1, D7, N1~N7 결정 완료 (2026-10-30). D2~D5는 해당 항목 시작 전에 정한다
- [x] **2. [필수] ParrelSync 설치와 복제 프로젝트 만들기**
  - Package Manager에서 Git URL로 설치 (주소는 ParrelSync GitHub README에서 확인. 확인 필요)
  - 메뉴 ParrelSync > Clones Manager에서 복제 프로젝트 생성 후 두 번째 에디터로 열기
  - 주의: 복제 프로젝트는 원본의 Assets를 공유한다. **수정은 원본 에디터에서만** 하고 복제 쪽은 실행만 한다
  - 주의: `.gitignore`에 복제 폴더(`*_clone_*`)가 커밋되지 않도록 확인
  - 확인: 두 에디터가 동시에 열리고, 원본에서 저장한 변경이 복제 쪽에 반영됨
- [x] **3. 폴더와 상수 준비**
  - `Assets/Resources/` 폴더 생성 (PhotonNetwork.Instantiate는 기본적으로 Resources 폴더의 프리팹을 이름으로 찾는다)
  - `Core/Define`에 씬 이름 상수(Boot, Lobby, Raid), 프리팹 경로 상수, 방 속성 키 상수(표시 이름, 시작 지점) 추가
  - RPC 함수 이름은 문자열 대신 `nameof`로 쓴다
- [x] **4. 씬 구성 확인**
  - Sandbox 씬은 오프라인 테스트용으로 그대로 유지
  - Boot, Lobby, Raid 씬을 Build Profiles 씬 목록에 Boot가 맨 위로 등록 (1주차 10번에서 완료했는지 확인)
### B. 입장 흐름 (5~9) [필수]
 
완료 (2026-10-31, ParrelSync로 확인). 스크립트 `Week2_EntryFlow_Scripts.zip`. 씬 구성은 부록 A를 따른다. 사용 클래스: BootConnector, SoloPlayStarter, RaidSceneLoader, RoomListService, RoomListEntry, RaidRoomProperties, RaidSessionWatcher (Network), BootView, RoomListView, RoomEntryView, RaidSessionView (UI), SceneNames, RoomPropertyKeys, PlayerPrefsKeys (Core/Define)
 
- [x] **5. [필수] Boot 씬: 이름 입력과 접속**
  - 이름 입력칸과 접속 버튼. 빈 이름은 접속 버튼 비활성
  - 입력한 이름을 PUN 닉네임으로 설정하고 접속 (리전 kr)
  - 씬 자동 동기화(AutomaticallySyncScene)는 끈다. Raid 진입은 RaidSceneLoader가 각자 처리
  - 접속 성공 시 Lobby 씬으로 이동. 실패 시 메시지와 함께 다시 시도, 혼자 플레이 버튼 표시
  - 확인: 원본·복제 에디터에서 서로 다른 이름으로 접속해 각각 Lobby로 넘어감
- [x] **6. [필수] Lobby 씬: 방 목록**
  - Photon 로비에 들어가 방 목록을 받는다 (JoinLobby, OnRoomListUpdate로 알고 있음. 확인 필요)
  - 방 목록은 바뀐 부분만 오는 것으로 알고 있으므로, 받은 목록을 직접 보관하고 삭제된 방은 지운다 (확인 필요)
  - 표시: 방 이름, 인원 n/4, 참가 가능 여부. 버튼: 방 만들기, 참가, 혼자 플레이
  - 테스트용 최소 UI. 정식 로비(창고, 상점)는 6주차
  - 확인: 방이 없을 때 빈 목록이 보이고 에러가 없음
- [x] **7. [필수] 방 만들기**
  - 실제 방 이름은 중복 없는 고유 문자열, 표시 이름 "닉네임의 방"은 방 속성에 넣고 로비 목록에서도 보이도록 지정
  - 시작 지점 후보 중 하나를 무작위로 골라 방 속성에 저장 (Raid 씬의 시작 지점 번호)
  - 최대 4명, 목록에 공개, 참가 가능 상태로 생성
  - 방 생성 후 바로 Raid 씬 불러오기
  - 확인: 원본에서 방을 만들면 바로 Raid로 들어가고, 그 뒤 복제 에디터의 Lobby 목록에 그 방이 나타남. 복제 에디터를 나중에 실행해 Lobby에 들어가도 보임
- [x] **8. [필수] 방 참가 (중도 참가)**
  - 목록에서 방을 골라 참가 → 마스터가 있는 Raid 씬으로 이동
  - 참가에 성공하면 RaidSceneLoader가 Raid 씬을 불러온다 (방장과 같은 방식)
  - 실패 이유 표시: 없는 방, 가득 참, 닫힌 방
  - 확인: 원본이 Raid에서 돌아다니는 중에 복제가 참가해 같은 Raid에 들어감. 목록의 인원이 2/4로 갱신됨
- [x] **9. [필수] 레이드 나가기 (임시)**
  - Raid 씬에 임시 나가기 버튼(또는 디버그 키) → 방 나가기 → Lobby
  - 마스터가 나가면 남은 사람도 Lobby로 돌아감 (세션 종료 임시 처리. 레이드 무효 처리와 저장은 6주차)
  - 확인: 참가자가 나가면 목록 인원이 줄어듦. 마스터가 나가면 남은 사람이 Lobby로 돌아가고, 마지막 사람이 나간 방은 목록에서 사라짐
### C. 플레이어 프리팹과 스폰 (10~13) [필수]
 
완료 (2026-10-31, ParrelSync로 확인. 중도 참가 시 기존 플레이어 미표시 문제 수정 포함). 스크립트 `Week2_Spawn_Scripts.zip`. 신규: PrefabPaths, PlayerNetworkSetup, RaidPlayerSpawner, LocalPlayerCameraBinder, LocalPlayerCrosshairBinder. 수정: RaidRoomProperties(GetStartPointSeed 추가), CrosshairView(SetTarget 추가, 씬 검색 선택). 3번의 Resources 폴더는 10번에서 만든다
 
- [x] **10. [필수] 플레이어 프리팹 만들기**
  - Sandbox의 Player를 프리팹으로 만들어 `Assets/Resources/`에 저장
  - PhotonView 추가, 동기화할 컴포넌트 연결 (D1, D2 결정에 따라). Synchronization은 **Unreliable** (Unreliable On Change는 멈춰 있으면 보내지 않아 중도 참가자에게 시작 지점 위치로 보임)
- [x] **11. [필수] 내 캐릭터 / 남의 캐릭터 구분 (한 곳에서 처리)**
  - 스폰 시 내 캐릭터인지(IsMine) 확인해서 켜고 끄는 처리를 한 컴포넌트에 모은다 (예: PlayerNetworkSetup)
| 컴포넌트 | 내 캐릭터 | 남의 캐릭터 |
|---|---|---|
| PlayerInputReader, WeaponInputReader | 켬 | 끔 |
| PlayerMover, PlayerDodger | 켬 | 끔 (위치는 PhotonTransformView) |
| PlayerAimer | 켬 | 끔 (회전은 PhotonTransformView) |
| PlayerStamina | 켬 | 끔 (로컬 전용) |
| CharacterController | 켬 | 켬 (탄이 맞는 충돌체로 사용, 이동은 하지 않음) |
| PlayerCameraTargetOffset, ShotCameraShake | 켬 | 끔 |
| PlayerAnimationController | 직접 계산 | 받은 State 적용 (D2) |
| PlayerDodgeDebugView | 켬 | 선택 |
 
  - 확인: 내 캐릭터만 입력에 반응하고, 남의 캐릭터는 내 키 입력에 반응하지 않음
- [x] **12. [필수] Raid 씬 스폰**
  - 시작 지점 2~3곳 배치, 각 시작 지점 안에 겹치지 않는 자리 4개
  - 시작 지점 번호는 방 속성 StartPointSeed를 시작 지점 수로 나눈 나머지, 자리 번호는 ActorNumber를 4로 나눈 나머지 (N5)
  - 각 클라이언트가 Raid 씬에 들어오면 방 속성의 시작 지점에서 자기 캐릭터를 PhotonNetwork.Instantiate로 생성 (중도 참가자도 같은 방식)
  - 확인: 먼저 들어온 사람이 멀리 이동한 뒤 나중에 들어온 사람이 시작 지점에 생기고, 두 화면 모두에 캐릭터 2개가 보임
- [x] **13. [필수] 로컬 플레이어 연결**
  - 내 캐릭터가 생기면 Cinemachine Camera의 Tracking Target을 그 캐릭터의 CameraTarget으로 연결
  - **CrosshairView 수정 필요**: 지금은 씬에서 첫 번째 PlayerAimer를 찾기 때문에 여러 명이면 남의 캐릭터를 잡을 수 있다. 내 캐릭터가 생길 때 연결받도록 바꾼다
  - ProjectilePool은 씬에 하나, 모든 캐릭터가 공유 (변경 없음)
  - 확인: 각 화면의 카메라와 조준점이 자기 캐릭터를 따라감
### D. 이동·상태 동기화 (14~16)
 
- [x] **14. [필수] 위치·회전 동기화** (D1)
  - 플레이어 프리팹에 PhotonTransformView 추가 (위치, 회전 동기화, 스케일 끔), PhotonView의 Observed Components에 연결 (D1)
  - 남의 캐릭터에서 PlayerMover, PlayerDodger, PlayerAimer가 꺼져 있는지 확인 (11번)
  - 전송 빈도(PhotonNetwork.SendRate, SerializationRate)는 기본값으로 시작해서 필요하면 조정
  - 확인 (ParrelSync): 원본에서 걷기, 달리기, 회피로 움직이면 복제 화면에서 순간이동 없이 부드럽게 따라오고 회전도 맞음. 반대 방향도 같은지 확인. 중도 참가 직후에도 먼저 온 사람의 위치가 바로 맞음
- [x] **15. 애니메이션 동기화** (D2)
  - 완료 (2026-10-31, ParrelSync로 확인). 스크립트 `Week2_AnimSync_Scripts.zip`. 신규 PlayerAnimationSync(Network), 수정 PlayerAnimationController(ApplyExternalState, PlayDodgeFromStart, CurrentMoveDirection 추가)
  - 플레이어 프리팹 루트에 PlayerAnimationSync를 붙이고 PhotonView Observed Components에 등록
  - 확인: 상대 캐릭터의 걷기, 달리기, 뒷걸음 역재생, 사격 자세, 회피, 재장전 끄덕임이 보임. 연속 회피 시 대쉬가 처음부터 재생
- [x] **16. 회피 무적 관련 확인**
  - 무적 판정은 3주차 피격자 최종 확인에서 각자 자기 화면 기준으로 하므로 2주차에는 동기화하지 않는다
  - 확인할 것은 회피 동작이 상대 화면에 보이는지뿐
### E. 사격 동기화 (17~19)
 
- [x] **17. 무기 식별 준비** (D3)
  - 17~19번 완료 (2026-10-31, ParrelSync로 확인. Vector3[] RPC 인자 정상 동작 확인). 스크립트 `Week2_FireSync_Scripts.zip`. 신규 WeaponDatabase(Data), PlayerFireSync(Network). 수정 WeaponData(WeaponId), PlayerShooter(OnShotFired, MuzzlePosition), ProjectileLauncher(LaunchVisualOnly, Deals Damage 제거)
  - WD_Pistol 1001, WD_Rifle 1002, WD_Shotgun 1003으로 ID 지정, WeaponDatabase 에셋 생성 후 플레이어 프리팹의 PlayerFireSync에 연결
- [x] **18. 발사 정보 전송** (D4)
  - 내가 쏘면 내 화면에서는 즉시 탄 재생, 같은 발사 정보를 다른 클라이언트에 보냄
  - 받은 쪽은 ProjectileLauncher로 **시각용 탄**만 재생 (데미지 판정 없음)
  - 받은 쪽은 다시 보내지 않는다 (내 캐릭터만 보냄)
  - 확인: 상대가 쏜 탄이 내 화면에서도 같은 방향으로 날아가고 벽에 막힘
- [x] **19. 무기 교체 표시**
  - 2주차에는 무기 모델이 없으므로 탄 모양 차이로만 확인 (무기 모델 교체는 5주차 장비와 함께)
### F. 명중 판정과 마스터 검증 (20~23)
 
- [x] **20. 명중 처리 구조 변경** (D5)
  - 20~23번 완료 (2026-10-31, ParrelSync로 확인). 스크립트 `Week2_HitSync_Scripts.zip`. 신규 IHitHandler, DirectDamageHitHandler(Combat), PlayerHitReporter, NetworkHealth(Network). 수정 ProjectileLauncher(SetHitHandler), Health(SetCurrentHealth)
  - 오프라인 Sandbox에서는 지금처럼 바로 데미지, 네트워크에서는 마스터에 보고
- [x] **21. 더미 적 네트워크화**
  - Raid 씬의 더미 적에 PhotonView 추가 (씬에 미리 놓인 네트워크 오브젝트)
  - 체력은 마스터만 바꾸고, 바뀐 결과를 모두에게 알림
  - 피격 깜빡임, 데미지 숫자, 사망·재등장이 모든 화면에서 같게 보이도록 Health 이벤트를 모든 클라이언트에서 발생시킴
  - **중도 참가자에게 현재 체력과 사망 상태 전달** (GameDesign 9번 "중도 참가자 상태 동기화"의 첫 적용. 3주차 이후 적, 상자에도 같은 방식을 쓴다)
- [x] **22. 마스터 검증 규칙**
  - 쏜 사람이 보낸 명중 보고를 마스터가 검사한 뒤 데미지 적용
  - 최소 검사: 대상이 존재하고 살아 있음, 거리가 무기 최대 사거리 + 여유값 이내
  - 협동 PvE라 치트 방지는 최소한으로만 (GameDesign 9-1)
- [x] **23. 확인**
  - 마스터가 아닌 클라이언트가 쏴도 더미 적 체력이 모든 화면에서 같이 줄어듦
  - 두 명이 동시에 쏴서 체력이 0 아래로 내려가도 사망이 한 번만 처리됨
  - 사거리 밖에서 맞았다는 보고는 무시됨 (디버그로 일부러 보내서 확인)
  - 더미 적 체력을 깎은 뒤 들어온 사람에게도 같은 체력이 보임
### G. 오프라인 모드 (24)
 
- [x] **24. 혼자 플레이**
  - 혼자 플레이 버튼: 오프라인 모드로 전환 → 방 만들기 → Raid
  - 접속, 방, 스폰, 명중 판정이 온라인과 같은 코드로 동작
  - 오프라인 방은 다른 사람의 목록에 보이지 않음
  - 확인: 인터넷을 끊고도 혼자 플레이로 레이드에 들어가 이동, 사격, 더미 적 처치가 됨
### H. 테스트 (25)
 
- [ ] **25. 지연 시뮬레이션 테스트** [보류: 8주차 마무리로 미룸, 2026-10-31 사용자 결정]
  - 도구: Raid 씬 RaidSystem에 PhotonLagSimulationGui 추가 (Simulate, Lag, Jit, Loss, RTT 표시). PhotonStatsGui는 구 Input 클래스를 써서 Input System 전용 설정에서 예외가 나므로 사용하지 않는다 (Active Input Handling을 Both로 바꾸거나 Photon 코드를 고치지 않는다)
  - 단계: A 끔(기준 핑), B Lag 50/Jit 10/Loss 0, C Lag 100/Jit 20/Loss 2(기준), D Lag 150/Jit 30/Loss 5. 기본은 복제(일반 참가자)에만 켜고, 마지막에 원본도 C로 확인
  - PUN 2에 포함된 지연 시뮬레이션 도구(PhotonLagSimulationGui로 알려져 있음, 이름과 위치는 확인 필요)로 지연과 손실을 주고 ParrelSync 환경에서 테스트
  - 확인: 지연 100~200ms에서도 상대 움직임이 크게 튀지 않고, 사격과 명중이 정상 처리됨
  - 주 1회 실제 빌드 2개(또는 PC 2대)로 같은 흐름 확인
### I. 마무리 (26~28)
 
- [x] **26. 임시 스크립트 정리**
  - `PhotonConnectionTester` 삭제 (Boot 씬 접속으로 대체)
- [x] **27. 완료 기준 점검** (4번)
  - 2026-11-05 점검: 4명 제한은 미확인(빌드로 확인 예정), 지연 시뮬레이션은 8주차 보류. 나머지 완료
- [x] **28. 개발 일지와 주차 마무리**
  - 2주차 개발 일지 작성 (`Week1_Checklist.md` 6번 템플릿)
  - 마무리 커밋: Summary 예 `chore: complete week 2 - pun 2 networking`, Description은 `ProjectGuidelines.md` 8번 형식
  - 태그 `week2` (1주차 보류 항목이 끝나지 않았으면 `week1` 태그와의 순서를 사용자와 정한다)
  - 2026-11-06: 개발 일지 `DevLog_Week2.md` 작성, 마무리 커밋 완료 (1주차 16~33번 포함). 태그는 `week2`만 붙이고 `week1`은 생략 (사용자 결정). 1주차 일지는 `DevLog_Week1.md`로 따로 작성
---
 
## 4. 완료 기준
 
### 필수 (ParrelSync로 확인)
 
- [x] Boot에서 이름을 입력하고 접속하면 Lobby로 넘어간다
- [x] Lobby에 방 목록이 보이고, 방을 만들면 바로 Raid로 들어간다
- [x] 방을 만든 사람이 Raid에 있는 동안, 나중에 Lobby에 들어온 사람의 목록에 그 방이 보인다
- [ ] 목록에서 참가하면 진행 중인 Raid에 합류하고, 4명이면 참가할 수 없다 (중도 참가 확인 완료. 4명 제한은 복제 프로젝트 3개 또는 빌드로 아직 확인 필요)
- [x] 각자 자기 캐릭터만 조작하고, 카메라와 조준점이 자기 캐릭터를 따라간다
- [x] 상대의 이동과 회전이 부드럽게 동기화된다 (중도 참가 직후 포함)
### 나머지
 
- [x] 상대의 애니메이션(회피 포함)이 보인다
- [x] 누가 쏘든 모든 화면에 탄이 보인다
- [x] 더미 적 체력과 사망이 모든 화면에서 같고, 명중은 마스터가 검증 후 적용한다. 중도 참가자에게도 같은 체력이 보인다
- [x] 혼자 플레이가 인터넷 없이 같은 흐름으로 동작한다
- [ ] 지연 시뮬레이션에서도 크게 문제없다 (25번 보류로 8주차 마무리에서 확인)
- [x] 모든 작업이 커밋되어 있다
---
 
## 5. 2주차 코드 원칙
 
1. **네트워크 코드는 Network 모듈에만 둔다.** Player, Combat은 네트워크를 모른 채 오프라인 Sandbox에서도 그대로 동작해야 한다 (D7)
2. **내 캐릭터 / 남의 캐릭터 분기는 한 컴포넌트에 모은다.** 여러 스크립트에 IsMine 확인이 흩어지지 않게 한다
3. **RPC 함수는 얇게.** 받은 값을 확인하고 일반 함수를 호출하는 정도만 하고, 실제 처리는 일반 함수에 둔다 (GameDesign 10번)
4. **RPC 이름은 nameof로, 씬 이름·프리팹 경로·방 속성 키는 Core/Define 상수로** 문자열 직접 사용을 피한다
5. **권한 표(GameDesign 9-1)를 따른다.** 이동·사격은 소유자, 명중 적용·더미 적 체력·방 열기와 닫기는 마스터
6. **보내는 값은 최소로.** 받는 쪽에서 계산할 수 있는 값은 보내지 않는다
7. **중도 참가를 기본으로 생각한다.** 한 번 일어난 일은 늦게 온 사람도 알 수 있게 만든다 (계속 동기화되는 값, 또는 참가 시점에 마스터가 현재 상태 전달)
---
 
## 6. 3주차 미리보기
 
다음 주차 체크리스트는 `Week3_Checklist.md`로 작성한다. 3주차 목표는 적 AI다.
 
- 시작 전: 1주차 35~40번 실내 표현 검증 완료 (그레이박스 맵 규칙 필요)
- 그레이박스 맵 1차, 돌격형·사수형 적, FSM, 경계 게이지, 시야·소음 감지
- 적 AI는 마스터에서만 실행하고 결과만 동기화, 먼 적은 휴면
- 적 투사체와 피격자 최종 확인 (1주차 PlayerDodger.IsInvincible 사용)
- 인원별 체력 배율, 인원이 바뀔 때 살아 있는 적의 체력 재계산
- 적 상태 중도 참가 동기화 (2주차 더미 적 방식 재사용)
---
 
## 7. 2주차에 만든 스크립트
 
2주차에 새로 만들거나 공개 함수가 바뀐 스크립트는 `Week1_Checklist.md` 8번 "현재까지 만든 스크립트" 표에 이어서 기록한다. 한 곳에서 전체 목록을 볼 수 있게 하기 위해서다.
 
2주차에 새로 만든 스크립트
- 5~9번 입장 흐름: 14개 (B 섹션 머리말 참고)
2주차에 수정이 예상되는 기존 스크립트
- `CrosshairView`: 내 캐릭터를 연결받도록 변경 (13번)
- `ProjectileLauncher`: 명중 처리 구조 변경 (20번, D5)
- `PlayerAnimationController`: 원격용 상태 적용 함수 추가 (15번, D2)
- `Health`, `DummyTarget`: 마스터 권한 체력, 모든 클라이언트 이벤트, 중도 참가자 상태 전달 (21번)
- `PlayerMover`, `PlayerDodger`: 남의 캐릭터에서 직접 이동하지 않게 하는 방법 (14번, D1)
---
 
## 8. 개발 일지 후보 (해결한 기술 문제)
 
### Lobby에서 혼자 플레이를 누르면 Boot로 돌아감 (2026-10-31)
 
- 증상: Lobby의 혼자 플레이 버튼을 누르면 "서버 접속이 끊겼습니다 (DisconnectByClientLogic)" 경고 후 Boot 씬으로 이동
- 원인: 접속을 끊으면 OnDisconnected가 여러 컴포넌트에 차례로 호출된다. SoloPlayStarter가 먼저 받으면 그 콜백 안에서 오프라인 모드 전환 → 방 생성 → 방 입장(IsStarting 해제)까지 한 번에 끝난다. 그 뒤 RoomListService가 OnDisconnected를 받을 때는 IsStarting이 이미 false라서 진짜 끊김으로 판단하고 Boot를 불러옴
- 해결: RoomListService, BootConnector의 OnDisconnected에서 IsStarting과 함께 PhotonNetwork.OfflineMode도 확인
- 배운 점: Photon 콜백은 같은 이벤트를 여러 대상에게 순서 보장 없이 차례로 호출하고, 콜백 안에서 다른 콜백이 연쇄로 일어날 수 있다. 상태 플래그만 믿지 말고 바뀌지 않는 근거(오프라인 모드)로 판단한다
### 중도 참가자에게 기존 플레이어가 보이지 않음 (2026-10-31)
 
- 증상: 먼저 들어온 사람은 나중에 들어온 사람이 보이지만, 나중에 들어온 사람에게는 기존 플레이어가 전혀 보이지 않음
- 원인: Raid 진입을 PUN 씬 자동 동기화(AutomaticallySyncScene)에 맡겼더니, 중도 참가자가 방 입장 직후 받은 기존 캐릭터 생성 정보(서버에 보관된 Instantiate 이벤트)를 아직 Lobby 씬에서 처리함. 이어서 Raid 씬으로 교체되며 Lobby 씬과 함께 삭제됨
- 해결: 자동 동기화를 끄고, OnJoinedRoom 콜백 안에서 각자 PhotonNetwork.LoadLevel 호출. LoadLevel이 즉시 메시지 처리를 멈추고 씬 로드 후 재개하므로 생성 정보가 Raid 씬에서 처리됨
- 함께 수정: PhotonView Synchronization을 Unreliable On Change에서 Unreliable로 변경. 멈춰 있는 플레이어는 위치를 보내지 않아 중도 참가자에게 처음 생성 위치로 보이던 문제 예방
- 배운 점: 네트워크 메시지 처리 시점과 씬 전환 시점을 맞춰야 한다. 중도 참가는 "보관된 이벤트를 어느 씬에서 받는가"까지 설계해야 한다
---
 
## 부록 A. 입장 흐름 씬 구성 (5~9번)
 
### A-0. 공통 준비
 
| 항목 | 설정 |
|---|---|
| 한글 폰트 | TMP 기본 폰트에는 한글이 없어 네모로 보인다. Pretendard(무료 상업용, GameDesign 11번)로 TMP Font Asset을 만들고(Atlas Population Mode: Dynamic), Project Settings > TextMesh Pro > Settings의 Default Font Asset에 지정 |
| EventSystem | 씬마다 하나. Input System 전용 프로젝트이므로 InputSystemUIInputModule이어야 한다. StandaloneInputModule이면 인스펙터의 Replace 버튼으로 교체 |
| PhotonServerSettings | Fixed Region `kr`, App Version 두 에디터 동일, Run In Background 켬 |
| Player Settings | Resolution and Presentation > Run In Background 켬 (ParrelSync에서 포커스 없는 에디터가 멈추지 않게) |
| Canvas Scaler | Scale With Screen Size, 1920×1080, Match 0.5 |
| Build Profiles | Boot(0번), Lobby, Raid 순서 |
 
### A-1. Boot 씬
 
```
BootSystem (빈 오브젝트): BootConnector, SoloPlayStarter, RaidSceneLoader
Canvas
  BootPanel (Image, 중앙 600×420, Vertical Layout Group): BootView
    TitleText (TMP)
    NameInput (Input Field - TextMeshPro)
    ConnectButton (Button - TextMeshPro) "접속"
    SoloPlayButton (Button - TextMeshPro) "혼자 플레이"
    StatusText (TMP)
EventSystem
Main Camera
```
 
- BootView 연결: Connector, Solo Play Starter = BootSystem / Name Input, Connect Button, Solo Play Button, Status Text = 같은 이름의 오브젝트
### A-2. Lobby 씬
 
```
LobbySystem (빈 오브젝트): RoomListService, SoloPlayStarter, RaidSceneLoader
Canvas
  LobbyPanel (Image, 화면 가득, 여백 80): RoomListView
    PlayerNameText (TMP, 왼쪽 위)
    TitleText (TMP) "방 목록"
    RoomListScroll (Scroll View, 가로 스크롤 끔)
      Viewport > Content (Vertical Layout Group + Content Size Fitter)
    EmptyListMessage (TMP) "열린 방이 없습니다"  ※ Content 밖, RoomListScroll의 형제
    ButtonRow (Horizontal Layout Group)
      CreateRoomButton "방 만들기"
      SoloPlayButton "혼자 플레이"
    StatusText (TMP)
EventSystem
Main Camera
```
 
- RoomListView 연결: Room List Service, Solo Play Starter = LobbySystem / Room List Content = Content / Room Entry Prefab = RoomEntry 프리팹 / 나머지는 같은 이름의 오브젝트
### A-3. 방 목록 항목 프리팹 (Assets/Prefabs/UI/RoomEntry.prefab)
 
```
RoomEntry (Image, Horizontal Layout Group, Layout Element 높이 70): RoomEntryView
  RoomNameText (TMP, Layout Element Flexible Width 1)
  PlayerCountText (TMP, Layout Element Preferred Width 100)
  JoinButton (Button - TextMeshPro, Layout Element Preferred Width 160)
    Text (TMP) "참가"
```
 
- RoomEntryView 연결: Room Name Text, Player Count Text, Join Button, Join Button Text(= JoinButton의 자식 Text)
### A-4. Raid 씬 (5~9번 시점)
 
```
RaidSystem (빈 오브젝트): RaidSessionWatcher
Canvas
  RaidSessionPanel (Image 반투명, 왼쪽 위 420×320, Vertical Layout Group): RaidSessionView
    RoomNameText (TMP)
    PlayerListText (TMP)
    StatusText (TMP)
    LeaveButton (Button - TextMeshPro) "나가기"
EventSystem
Main Camera
```
 
- 플레이어 스폰(10~13번)은 이후 이 씬에 추가한다
### A-5. ParrelSync 확인 순서
 
1. 원본: Boot 접속 → Lobby → 방 만들기 → Raid (참가자 1/4, 내 이름 옆 방장)
2. 복제: 그 뒤에 실행 → Boot 접속 → Lobby 목록에 원본의 방(1/4)이 보임
3. 복제: 참가 → Raid, 두 화면 모두 참가자 2/4
4. 복제: 나가기 → Lobby 목록 1/4, 원본 화면 1/4
5. 복제 다시 참가 후 원본(방장) 나가기 → 복제에 "방장이 나가 레이드가 종료됩니다" 후 Lobby, 목록에서 방이 사라짐
6. 혼자 플레이: Lobby에서 혼자 플레이 → Raid "혼자 플레이 1/1", 다른 쪽 목록에는 안 보임 → 나가기 → Boot (이름 미리 채워짐)
7. 접속 실패: 인터넷을 끊고 Boot 접속 → 실패 문구와 혼자 플레이 버튼 표시
8. Lobby 또는 Raid 씬을 바로 실행 → 경고 후 Boot로 이동
---
 
## 변경 이력
 
| 날짜 | 내용 |
|---|---|
| 2026-10-24 | 최초 작성 |
| 2026-10-30 | 입장 흐름 변경(Boot 이름 입력 → Lobby 공개 방 목록 → 방 만들기 즉시 Raid / 중도 참가), 필수 목표 지정(입장 흐름, ParrelSync 이동 동기화 확인), 테스트 도구 ParrelSync 확정, 레이드 나가기 항목 추가, 더미 적 중도 참가 상태 전달 추가, 번호 재정리(전체 28개) |
| 2026-10-30 | D1(PhotonTransformView), D7 확정, 2-2 입장 흐름 결정 N1~N7 추가, 11번 표에 PlayerAimer·PlayerStamina·CharacterController 추가, 12번 자리 배정 규칙, 14번 PhotonTransformView로 변경, 5~9번 스크립트 작성 완료 표시, 부록 A 입장 흐름 씬 구성 추가 |
| 2026-10-31 | 2, 4~9번 완료 처리. 10~13번 스크립트 작성 표시 |
| 2026-10-31 | 씬 이동 방식 변경(AutomaticallySyncScene 미사용, 각자 LoadLevel), 10번에 PhotonView Synchronization Unreliable 추가, 8번 개발 일지 후보 섹션 추가 |
| 2026-10-31 | 3, 10~14번 완료 처리, 필수 완료 기준 체크(4명 제한 확인은 남음), 현재 위치 갱신 |
| 2026-10-31 | D2 확정(B), 15번 스크립트 작성 표시, 현재 위치 갱신 |
| 2026-10-31 | 15~16번 완료 처리, 애니메이션 완료 기준 체크, 현재 위치 갱신 |
| 2026-10-31 | D3(B, int ID), D4(B, 시작점은 받는 쪽 총구), D8(LaunchVisualOnly), D9(발사 단위 이벤트) 확정, 17~19번 스크립트 작성 표시 |
| 2026-10-31 | 17~19번 완료 처리(Vector3[] RPC 확인), 사격 완료 기준 체크, 현재 위치 갱신 |
| 2026-10-31 | D5(B), D10(A), D11(A) 확정, 20~23번 스크립트 작성 표시 |
| 2026-10-31 | 20~23번 완료 처리, 혼자 플레이 끊김 오판 버그 기록(개발 일지 후보) |
| 2026-10-31 | 24번 완료 처리, 혼자 플레이 완료 기준 체크 |
| 2026-10-31 | 25번에 테스트 도구와 단계 기록, PhotonStatsGui 사용 안 함(구 Input 사용) |
| 2026-10-31 | 25번 지연 시뮬레이션 테스트 보류(마무리 단계로 미룸) |
| 2026-11-05 | 25번 보류 시점을 8주차 마무리로 명시 |
| 2026-11-06 | 27번 완료, 28번 일지·커밋 완료 기록, 태그 결정(week2만, week1 생략) |
| 2026-11-06 | 26, 28번 완료(PhotonConnectionTester 삭제, 태그 week2). 2주차 완료 |
 