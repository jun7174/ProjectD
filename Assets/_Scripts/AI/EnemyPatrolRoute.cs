// 파일 경로: Assets/_Scripts/AI/EnemyPatrolRoute.cs
using System.Collections.Generic;
using UnityEngine;

namespace ProjectD.AI
{
    /// <summary>
    /// 적의 순찰 지점 목록과 지금 향하는 지점 번호를 보관한다.
    /// 지점은 생성한 쪽(EnemySpawner)이 스폰 지점 기준으로 넣어 준다. 넣어 주지 않으면 처음 위치 하나만 쓴다(제자리 경계).
    /// 순찰 상태와 복귀 상태가 같은 "지금 향하는 지점"을 쓰므로, 놓친 뒤 하던 순찰을 이어서 할 수 있다.
    /// 적 프리팹 루트에 붙인다.
    /// </summary>
    public class EnemyPatrolRoute : MonoBehaviour
    {
        private readonly List<Vector3> _points = new List<Vector3>();

        /// <summary>
        /// 순찰 지점 수
        /// </summary>
        public int Count => _points.Count;

        /// <summary>
        /// 지금 향하는 순찰 지점 번호
        /// </summary>
        public int CurrentIndex { get; private set; }

        /// <summary>
        /// 지금 향하는 순찰 지점 위치
        /// </summary>
        public Vector3 CurrentPoint => _points[CurrentIndex];

        /// <summary>
        /// 지점을 받기 전에도 동작하도록 처음 위치를 지점 하나로 넣어 둔다.
        /// </summary>
        private void Awake()
        {
            _points.Add(transform.position);
        }

        /// <summary>
        /// 순찰 지점 목록을 바꾸고 첫 지점부터 시작한다. 받은 목록은 복사해서 보관한다.
        /// </summary>
        /// <param name="points">순서대로 돌 순찰 지점</param>
        public void SetPoints(IReadOnlyList<Vector3> points)
        {
            if (points == null || points.Count == 0)
            {
                Debug.LogWarning($"[{nameof(EnemyPatrolRoute)}] 순찰 지점이 비어 있어 처음 위치에서만 경계합니다.", this);
                return;
            }

            _points.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                _points.Add(points[i]);
            }

            CurrentIndex = 0;
        }

        /// <summary>
        /// 다음 순찰 지점으로 넘어간다. 마지막 지점 다음은 첫 지점이다.
        /// </summary>
        public void AdvanceToNext()
        {
            if (_points.Count == 0)
            {
                return;
            }

            CurrentIndex = (CurrentIndex + 1) % _points.Count;
        }

        /// <summary>
        /// 선택했을 때 Scene 뷰에 순찰 경로를 그린다. 지점은 실행 중에 정해지므로 플레이 중에만 보인다.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_points.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < _points.Count; i++)
            {
                Vector3 from = _points[i];
                Vector3 to = _points[(i + 1) % _points.Count];

                Gizmos.DrawWireSphere(from, 0.3f);
                Gizmos.DrawLine(from, to);
            }
        }
    }
}
