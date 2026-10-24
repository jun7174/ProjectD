-접속 (Boot)
ConnectUsingSettings로 접속, AutomaticallySyncScene = true, 닉네임 임시 지정(Player + 랜덤숫자), OnConnectedToMaster → Lobby 이동, 실패 시 메시지 (OnDisconnected)

-로비 (Lobby, 테스트용 최소 UI)
방 만들기 : 짧은 랜덤 코드를 방 이름으로, 최대 4명, IsVisible = false
방 참가 : 코드 입력 → JoinRoom, 실패 이유 표시(없는 방, 가득 참, 이미 시작) OnJoinRoomFailed의 returnCode로 구분
방 안 : 방 코드, 참가자 목록(OnPlayerEnteredRoom/LeftRoom에서 갱신), 시작 버튼 마스터만 활성
시작 : CurrentRoom.IsOpen = false → PhotonNetwork.LoadLevel(Raid)
혼자 플레이 : OfflineMode = true → CreateRoom → Raid 

-플레이어 프리팹 / 스폰
Sandbox Player → Resources에 프리팹, PhotonView 추가
Raid에 스폰 지점 여러 개, 각자 PhotonNetwork.Instantiate
IsMine 분기는 한 컴포넌트에 모으기 (PlayerNetworkSetup) : 입력리더, 카메라 오프셋, 화면흔들림은 남의 캐릭터에서 끄기
내 캐릭터 생성 시 Cinemachine Tracking Target 연결, CrosshairView는 FindFirstObjectByType 대신 연결받도록 수정

-이동 / 애니메이션 동기화
위치·회전 : IPunObservable 직접 구현, OnPhotonSerializeView에서 IsWriting이면 SendNext, 아니면 ReceiveNext → 받는 쪽은 Update에서 Lerp/Slerp 보간
남의 캐릭터는 PlayerMover, PlayerDodger가 직접 움직이지 않게 (두 곳에서 움직이면 떨림)
애니메이션 : State 번호 + MoveDirection만 전송, PlayerAnimationController에 원격 적용 함수 추가
연속 회피 : 회피 카운터(byte) 같이 보내고 값 바뀌면 Dodge 처음부터 재생
회피 무적(보류)

-사격 동기화
무기 식별 : 무기 데이터베이스 SO + WeaponData에 고유 ID
발사 : 내 화면 즉시 탄 재생 + RPC로 발사 정보 전송(RpcTarget.Others), 한 프레임 발사를 모아 RPC 하나 (시작점, 방향 배열, 무기 ID)
받는 쪽 : 시각용 탄만 재생, 데미지 판정 없음, 다시 보내지 않음

-명중 판정 (마스터 검증)
Combat에 IHitHandler 인터페이스 → 오프라인용(바로 데미지), 네트워크용(마스터에 RPC 보고, Network 폴더)
마스터 검사 : 대상 존재·생존, 거리 ≤ 최대 사거리 + 여유값
더미 적 PhotonView 추가, 체력은 마스터만 변경 → 결과를 RpcTarget.All로 알려서 피격 깜빡임, 데미지 숫자, 사망·재등장 모든 화면 동일
동시 사격으로 체력 0 아래 → 사망 한 번만 처리되는지 확인

-적 데이터 / 기본 구조
EnemyData SO : 체력, 이동 속도, 시야 거리·각도, 경계 게이지 속도, 공격 사거리, 예고 시간, 탄속, 데미지 NavMeshAgent 이동 Health, IDamageable 재사용
-FSM
코드 FSM (상태 클래스 또는 enum + switch 중 결정) 순찰 → 경계(소리 난 곳 조사) → 추적 → 공격 → 복귀(일정 시간 놓치면 원위치), 경직(근접 공격 피격,근접 공격과 연결) 상태 번호는 Enum.cs에

-감지
시야 : 거리 → 각도(Vector3.Angle) → 벽 레이캐스트 순으로 검사 경계 게이지 : 시야 안이면 참, 가까울수록 빨리 참, 가득 차면 추적 소음 : 달리기·사격·회피 시 반경 안 적에게 전달 (걷기는 아주 작은 반경), 소음 발생은 이벤트로 피격 반응 : 맞으면 즉시 공격자 방향 추적 타겟 선정 : 서 있는 플레이어 우선, 가장 가까운 감지 대상, 피격 시 공격자로 전환 후 2~3초 유지

-적 종류
돌격형 : 예고 동작 후 돌진 공격 사수형 : 거리 유지, 느린 투사체 점사 (탄속은 플레이어의 1/3~1/2) 모든 공격 예고 0.3~0.6초, 즉발 없음

-네트워크 (마스터 전용)
AI는 마스터에서만 실행 (IsMasterClient 아니면 FSM, NavMeshAgent 끄기) 동기화는 위치 보간 + 상태 값 하나 (플레이어 동기화 재사용) 레이드 시작 시 마스터가 스폰 지점별 적 종류 랜덤으로 일괄 스폰 휴면 : 플레이어 근처 적만 AI 실행, 먼 적은 AI와 위치 동기화 정지

-적 투사체와 피격자 최종 확인 (핵심 기술 포인트)
적 탄은 마스터가 시뮬레이션, 다른 클라는 시각용 탄만 (발사 정보 RPC 재사용) 명중 시 마스터 → 피격자에게 피격 신호 RPC 피격자가 자기 화면 기준 PlayerDodger.IsInvincible 확인 → 무적 아니면 피격 확정 회신 마스터가 체력 감소 (체력은 0에서 멈추거나 로그만)

-인원별 체력 배율
적 수 고정, 체력에 배율 (1인 1.0, 2인 1.3, 3인 1.6, 4인 1.9) 수치는 밸런스 설정 SO에 스폰 시 PlayerCount로 적용



https://velog.io/@ohjh1744/%EC%9C%A0%EB%8B%88%ED%8B%B0-Pun-%EC%82%AC%EC%9A%A9%EB%B2%95
https://velog.io/@ttangu5510/Unity-PUN2-2
https://doc.photonengine.com/ko-kr/pun/current/gameplay/offlinemode
