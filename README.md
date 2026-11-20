-인벤 탄약 재장전(R 입력 -> 인벤 내부 탄약확인후 인벤에서 가져오고 채우기)
RPC사용해서 뿌려줘야함
WeaponData:	AmmoType(이 무기가 쓰는 탄 종류), MagazineSize
AmmoData: AmmoType. 인벤토리 아이템 중 이것이 무기의 AmmoType과 같은 묶음을 찾음
PlayerInventory:	Grid.Entries를 돌며 entry.Item is AmmoData ammo 확인, entry.Count 합산
PlayerReloader:	TryStartReload(시작 조건에 "탄 있음" 추가), 완료 이벤트(여기서 차감)
PlayerWeaponHolder:	RefillMagazine(지금은 무한으로 채움 → 채울 수를 받게 바꿔야 함), 현재 탄창 탄 수
-퀵슬롯 회복 ( 숫자키 -> 소모품 확인 -> UseSeconds동안 사용 -> 효과 적용후 차감)
ConsumableData:	Effect(Health/Stamina), Amount, UseSeconds
PlayerInventory:	슬롯에 지정한 아이템 ID로 묶음을 찾고 1개 차감 (재장전과 같은 차감 함수 재사용)
PlayerStamina:	스태미나 효과. 지금은 Drain, TryConsume만 있어서 회복 함수를 추가해야 함. 로컬 전용이라 바로 적용해도 됨
NetworkHealth / Health:	체력 효과. 체력은 마스터 권한이라 ApplyDamageAsMaster처럼 회복도 마스터가 정하고 RpcTarget.All로 적용해야 함 (Health에 회복 함수, NetworkHealth에 회복 RPC)
PlayerDodger:	OnDodgeStarted로 사용 취소 (PlayerReloader가 회피로 취소되는 방식 참고)
Input Actions:	퀵슬롯 1~3 키. 지금 1, 2가 무기 교체라 겹치므로 키 배치를 정해야 함
