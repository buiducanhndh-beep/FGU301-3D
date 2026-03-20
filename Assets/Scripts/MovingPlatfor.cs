using UnityEngine;

public class MovingPlatfor : MonoBehaviour
{
    public enum MovementMode
    {
        Loop,
        PingPong
    }

    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private bool useLocalSpace;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float stopDistance = 0.01f;
    [SerializeField] private float waitTimeAtPoint = 0.25f;
    [SerializeField] private MovementMode movementMode = MovementMode.Loop;

    private int currentTargetIndex;
    private int direction = 1;
    private float waitTimer;
    private bool isPaused;

    private void Start()
    {
        InitializeMovement();
    }

    private void Update()
    {
        if (isPaused || !CanMove())
        {
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            return;
        }

        MoveToCurrentTarget();
    }

    public void InitializeMovement()
    {
        currentTargetIndex = 0;
        direction = 1;
        waitTimer = 0f;

        if (!CanMove())
        {
            return;
        }

        transform.position = GetWaypointPosition(0);
        SetNextTargetIndex();
    }

    public void PausePlatform(bool pause)
    {
        isPaused = pause;
    }

    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        InitializeMovement();
    }

    private bool CanMove()
    {
        return waypoints != null && waypoints.Length > 1;
    }

    private void MoveToCurrentTarget()
    {
        Vector3 targetPosition = GetWaypointPosition(currentTargetIndex);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) <= stopDistance)
        {
            transform.position = targetPosition;
            waitTimer = waitTimeAtPoint;
            SetNextTargetIndex();
        }
    }

    private void SetNextTargetIndex()
    {
        if (movementMode == MovementMode.Loop)
        {
            currentTargetIndex = (currentTargetIndex + 1) % waypoints.Length;
            return;
        }

        if (currentTargetIndex == waypoints.Length - 1)
        {
            direction = -1;
        }
        else if (currentTargetIndex == 0)
        {
            direction = 1;
        }

        currentTargetIndex += direction;
    }

    private Vector3 GetWaypointPosition(int index)
    {
        if (!useLocalSpace)
        {
            return waypoints[index].position;
        }

        return transform.parent != null
            ? transform.parent.TransformPoint(waypoints[index].localPosition)
            : waypoints[index].localPosition;
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                continue;
            }

            Vector3 point = Application.isPlaying ? GetWaypointPosition(i) : waypoints[i].position;
            Gizmos.DrawSphere(point, 0.15f);

            int nextIndex = i + 1;
            if (nextIndex >= waypoints.Length)
            {
                nextIndex = movementMode == MovementMode.Loop ? 0 : i;
            }

            if (nextIndex != i && waypoints[nextIndex] != null)
            {
                Vector3 nextPoint = Application.isPlaying ? GetWaypointPosition(nextIndex) : waypoints[nextIndex].position;
                Gizmos.DrawLine(point, nextPoint);
            }
        }
    }
}
