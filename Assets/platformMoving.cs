using System;
using System.Collections;
using UnityEngine;

public class platformMoving : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Transform player;
    public Transform parentObject;
    [SerializeField]  GameObject PointA;
    [SerializeField]  GameObject PointB;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float delay = 1f;
    [SerializeField] GameObject platform;

    private Vector3 targetPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (player == null) player = other.transform;
            player.parent = parentObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (player == null) player = other.transform;
            player.parent = null;
        }
    }   

     void Start()
    {
        platform.transform.position = PointA.transform.position;
        targetPosition = PointB.transform.position;
        StartCoroutine(MovePlatForm());
    }

    IEnumerator MovePlatForm()
    {
        while (true)
        {
            while((targetPosition - platform.transform.position).sqrMagnitude > 0.01f)
            {
                platform.transform.position = Vector3.MoveTowards(platform.transform.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }

            targetPosition = targetPosition == PointA.transform.position ? PointB.transform.position : PointA.transform.position;
            yield return new WaitForSeconds(delay);
        }
    }


}
