using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Target_Test : MonoBehaviour
{
    public TestTargetMode mode;

    private Vector2 A, B;
    private Vector2 targetPos;
    private Rigidbody2D rb;

    public float turnRate, speed, minWaypointDist;
    public List<Transform> waypoints = new();
    private int currentWaypointIndex;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        A = new Vector2(transform.position.x, transform.position.y - 100f);
        B = new Vector2(transform.position.x, transform.position.y + 100f);
        targetPos = A;
        currentWaypointIndex = 0;
    }

    void FixedUpdate()
    {
        switch (mode)
        {
            case TestTargetMode.Normal:
                NormalMove();
                break;
            case TestTargetMode.Dynamic:
                DynamicMove();
                break;
        }
    }

    private void NormalMove()
    {
        if (Vector2.Distance(rb.position, targetPos) < 1f)
        {
            targetPos = targetPos == A ? B : A;
        }

        Vector2 direction = (targetPos - rb.position).normalized;
        rb.velocity = direction * speed;
        rb.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    private void DynamicMove()
    {
        if (waypoints.Count == 0) return;
        Steer();
        CheckIfCloseToWaypoint();
    }
    private void Steer()
    {
        Vector2 direction = ((Vector2)waypoints[currentWaypointIndex].position - rb.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, turnRate * Time.fixedDeltaTime);
        rb.rotation = newAngle;

        rb.velocity = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)) * speed;

        //Vector2 way = (targetPos - rb.position).normalized;
     
        //float angle = Mathf.Atan2(way.y, way.x) * Mathf.Rad2Deg;
        //rb.rotation = angle - 90f;
    }
    


    private void CheckIfCloseToWaypoint()
    {
        if (waypoints.Count == 0) return;

        if (Vector2.Distance(rb.position, waypoints[currentWaypointIndex].position) < minWaypointDist)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        }
    }


    public void Hit()
    {
        Debug.Log("Hit");
    }
    void OnDrawGizmos()
    {
        if (waypoints.Count == 0) return;

        Gizmos.color = Color.white;

        Vector3[] points = new Vector3[waypoints.Count];
        for (int i = 0; i < waypoints.Count; i++)
        {
            points[i] = waypoints[i].position;
            Gizmos.DrawWireSphere(points[i], 0.5f);
        }

        Gizmos.DrawLineStrip(points, true);
    }

}

public enum TestTargetMode
{
    Normal,
    Dynamic
}
