using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    #region Singleton
    private static WaypointManager instance;

    public static WaypointManager Instance
    {
        get
        {
            if (instance == null) instance = new WaypointManager();
            return instance;
        }
    }

    #endregion

    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    public LayerMask driftingWaypointLayer;
    public float waypointSwitchDistance = 10f; // 다음 웨이포인트로 넘어갈 거리

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }


        //SetWaypoints();
    }

    private void SetWaypoints()
    {
        // TODO 자식오브젝트들을 모두 waypoints에 추가하기
        Transform[] findWaypoints = GetComponentsInChildren<Transform>();

        waypoints.Clear();

        for (int i = 0; i < findWaypoints.Length; i++)
        {
            if (findWaypoints[i] == this.transform) continue;

            waypoints.Add(findWaypoints[i]);
        }

        // 웨이포인트가 없는 경우 경고 메시지 출력
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("웨이포인트가 설정되지 않았습니다. WaypointManager 오브젝트에 자식 오브젝트를 추가해주세요.");
        }
    }

    public List<Transform> GetWaypoints()
    {
        return waypoints;
    }

    #region DrawGizmos
    #if UNITY_EDITOR
    // 디버깅을 위한 시각화
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null) continue;

            // 웨이포인트의 레이아웃에 따라 색상 결정
            if (((1 << waypoints[i].gameObject.layer) & driftingWaypointLayer) != 0)
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.yellow;
            }

            // 웨이포인트 지점 시각화
            Gizmos.DrawSphere(waypoints[i].position, 5);
            Gizmos.DrawWireSphere(waypoints[i].position, waypointSwitchDistance);

            // 다음 웨이포인트까지의 경로 (청록색으로 통일)
            int nextIndex = (i + 1) % waypoints.Count;
            if (waypoints[nextIndex] != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }
        }


    }
    #endif
    #endregion
}
