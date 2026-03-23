using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform player;
    public Transform parentObject;

    [SerializeField] private GameObject PointA;
    [SerializeField] private GameObject PointB;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float delay = 1f;
    [SerializeField] private GameObject platform;

    private Vector3 targetPosition;
    private bool goingToB = true; // 🔥 thêm biến này

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            player.SetParent(parentObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            player.SetParent(null);
        }
    }

    void Start()
    {
        platform.transform.position = PointA.transform.position;
        targetPosition = PointB.transform.position;
        StartCoroutine(MovePlatform());
    }

    IEnumerator MovePlatform()
    {
        while (true)
        {
            // Move tới target
            while ((targetPosition - platform.transform.position).sqrMagnitude > 0.01f)
            {
                platform.transform.position = Vector3.MoveTowards(
                    platform.transform.position,
                    targetPosition,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            // Đợi
            yield return new WaitForSeconds(delay);

            // 🔥 Đổi hướng
            goingToB = !goingToB;
            targetPosition = goingToB 
                ? PointB.transform.position 
                : PointA.transform.position;
        }
    }
}