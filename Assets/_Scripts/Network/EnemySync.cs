// 파일 경로: Assets/_Scripts/Network/EnemySync.cs
using Photon.Pun;
using ProjectD.AI;
using ProjectD.Core.Define;
using UnityEngine;

namespace ProjectD.Network
{
    /// <summary>
    /// 다른 화면에도 보여 줘야 하는 적 상태를 정기 전송으로 동기화한다. 위치는 PhotonTransformView가 맡고, 이 컴포넌트는 상태와 경계 게이지를 맡는다.
    /// 마스터는 EnemyStatus의 값을 보내고, 다른 클라이언트는 받은 값을 EnemyStatus에 넣는다.
    /// 두 값 모두 1바이트로 줄여 보낸다.
    /// 적 프리팹 루트에 붙이고 PhotonView의 Observed Components에 등록한다.
    /// </summary>
    public class EnemySync : MonoBehaviourPun, IPunObservable
    {
        [Header("참조 (비워 두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private EnemyStatus _status;

        /// <summary>
        /// 참조를 찾고 비어 있으면 경고한다.
        /// </summary>
        private void Awake()
        {
            if (_status == null && !TryGetComponent(out _status))
            {
                Debug.LogWarning($"[{nameof(EnemySync)}] EnemyStatus를 찾지 못했습니다. 적 상태가 동기화되지 않습니다.", this);
            }
        }

        /// <summary>
        /// PhotonView가 정기적으로 호출한다. 마스터는 보내고 다른 클라이언트는 받는다.
        /// </summary>
        /// <param name="stream">보내거나 받을 데이터 스트림</param>
        /// <param name="info">메시지 정보</param>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (_status == null)
            {
                return;
            }

            if (stream.IsWriting)
            {
                // 0~1 게이지를 0~255로 바꿔 1바이트로 보낸다. 표시용이라 이 정도 정밀도면 충분하다
                stream.SendNext((byte)Mathf.RoundToInt(_status.AlertLevel * 255f));
                stream.SendNext((byte)_status.CurrentState);
                stream.SendNext(_status.IsTelegraphing);
            }
            else
            {
                byte alertByte = (byte)stream.ReceiveNext();
                _status.SetAlertLevel(alertByte / 255f);
                _status.SetState((EnemyState)(byte)stream.ReceiveNext());
                _status.SetTelegraphing((bool)stream.ReceiveNext());
            }
        }
    }
}
