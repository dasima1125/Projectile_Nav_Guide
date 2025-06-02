using System;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class TestLunch_Beta : MonoBehaviour
{
    Rigidbody2D rb;
    public GameObject Projectile;
    public Transform Target;
    [Header("Firing Control")]
    public bool LockMode;
    public AimingMode TrackMode;
    [Header("Guidance Configuration")]
    //public 
    public GuidanceAuthority GuidanceType;
    public TrackLogicType InjectLogic;
    public bool PathDebugMode = false;
    public bool DebugMode = false;
    Vector2 aimPos;

    [Header("Projectile Parameters")]
    public float projectileSpeed = 0;
    public float projectileLife = 0;
    public float maxAngularVelocity = 0;
    [Range(3f, 50f)]
    public float navigationConstant = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void LaunchTest()
    {
        if (Target == null)
        {
            Debug.Log("Non Target");
            return;
        }

        TrackLogicInjector();

        //TestMode
        //AllFire();
    }

    void FixedUpdate()
    {
        aim(LockMode);
    }
    void aim(bool trace)
    {
        if (!trace) return;
        switch (TrackMode)
        {
            case AimingMode.Intuitive:
                rb.rotation = TraceStright2D(GetTNS2D().position, transform.position);
                break;

            case AimingMode.Lead:
                rb.rotation = TraceRead2D(GetTNS2D().position, transform.position, GetTNS2D().velocity, projectileSpeed);
                break;
        }
    }
    void TrackLogicInjector()
    {
        switch (GuidanceType)
        {
            case GuidanceAuthority.None:
                Projectile Projectile_None = Instantiate(Projectile, transform.position, transform.rotation).AddComponent<Projectile>();
                Projectile_None.Init(projectileSpeed, projectileLife);
                break;

            case GuidanceAuthority.Command:
                Guided Projectile_Command = Instantiate(Projectile, transform.position, transform.rotation).AddComponent<Guided>();
                Projectile_Command.Init(this, InjectLogic, projectileSpeed, maxAngularVelocity, projectileLife, navigationConstant);
                break;
        }

    }

    void AllFire()
    {
        foreach (TrackLogicType Logic in Enum.GetValues(typeof(TrackLogicType)))
        {
            Guided Projectile_Command2 = Instantiate(Projectile, transform.position, transform.rotation).AddComponent<Guided>();
            Projectile_Command2.Init(this, Logic, projectileSpeed, maxAngularVelocity, projectileLife, navigationConstant);
        }
    }

    float TraceStright2D(Vector2 targetPos, Vector2 startPos)
    {
        Vector2 direction = (targetPos - startPos).normalized;
        aimPos = targetPos;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
    float TraceRead2D(Vector2 targetPos, Vector2 startPos, Vector2 targetVel, float ProjectileSpeed)
    {
        Vector2 displacement = targetPos - startPos;
        float a = Vector2.Dot(targetVel, targetVel) - (ProjectileSpeed * ProjectileSpeed);
        float b = Vector2.Dot(displacement, targetVel) * 2;
        float c = Vector2.Dot(displacement, displacement);

        float discriminant = b * b - 4 * a * c;

        // The last condition is not ideal, but necessary until a better solution is found.
        if (discriminant < 0 || Mathf.Abs(a) < 0.0001f || GetTNS2D().velocity.magnitude > ProjectileSpeed) //<-- Probably without the last condition,when the target speed and projectile speed get close, 
        {                                                                                                  //    the filter is bypassed, causing issues.
            // Currently under refinement: if the projectile speed meets the threshold condition,
            // a fallback position will be assigned based on the minimum viable value that satisfies the conditional check.

            // Warning: The fallback state is unstable when the projectile speed
            // is less than or equal to the target's speed. Please be cautious.
            float over = GetTNS2D().velocity.magnitude + 3;
            Vector2 fallbackLeadPos = targetPos + targetVel.normalized * over;
            Vector2 fallbackDir = (fallbackLeadPos - startPos).normalized;
            aimPos = fallbackLeadPos;

            return Mathf.Atan2(fallbackDir.y, fallbackDir.x) * Mathf.Rad2Deg;
        }

        float rootP = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
        float rootM = (-b - Mathf.Sqrt(discriminant)) / (2 * a);
        float solution = Mathf.Max(rootP, rootM);
        

        Vector2 leadPos = targetPos + targetVel * solution;
        Vector2 dir = (leadPos - startPos).normalized;
        aimPos = leadPos;

        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }


    public TNS2DData GetTNS2D()
    {
        return new TNS2DData
        {
            position = Target.position,
            velocity = Target.GetComponent<Rigidbody2D>().velocity,
        };
    }
    void OnDrawGizmos()
    {
        if (aimPos == Vector2.zero || !DebugMode) return;
        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(transform.position, aimPos);
        Gizmos.DrawSphere(aimPos, 0.5f);
    }

}

public enum AimingMode
{
    Intuitive ,Lead

}
public enum GuidanceAuthority
{
    None,
    Command          
}
public enum TrackLogicType
{
    Pure, Lead, Pn
}


/// I think the fallback handling itself is done properly.
/// The problem is that there are frequent cases where the conditions to trigger the fallback are skipped.
/// When the target moves away, then turns and comes back toward me, it momentarily bypasses the fallback condition and ends up making a huge prediction error — it turns sharply in the opposite direction of the target, then turns back again.
/// I feel like this needs to be fixed, but I don’t really understand why it’s happening.
/// Is the quadratic formula’s terms behaving erratically?
/// What am I missing here?
/*
float TraceRead2D(Vector2 targetPos, Vector2 startPos, Vector2 targetVel, float ProjectileSpeed)
    {
        Vector2 displacement = targetPos - startPos;
        float a = Vector2.Dot(targetVel, targetVel) - (ProjectileSpeed * ProjectileSpeed);
        float b = Vector2.Dot(displacement, targetVel) * 2;
        float c = Vector2.Dot(displacement, displacement);

        float discriminant = b * b - 4 * a * c;
        
        List<float> validRoots = new List<float>();

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float rootP = (-b + sqrtDiscriminant) / (2 * a);
        float rootM = (-b - sqrtDiscriminant) / (2 * a);

        if (rootP > 0) validRoots.Add(rootP);
        if (rootM > 0) validRoots.Add(rootM);
        if (discriminant < 0 || Mathf.Abs(a) < 0.0001f  || validRoots.Count == 0)
        {
            // Currently under refinement: if the projectile speed meets the threshold condition,
            // a fallback position will be assigned based on the minimum viable value that satisfies the conditional check.

            // Warning: The fallback state is unstable when the projectile speed
            // is less than or equal to the target's speed. Please be cautious.

            float over = GetTNS2D().velocity.magnitude + 3;

            Vector2 fallbackLeadPos = targetPos + targetVel.normalized * over;
            Vector2 fallbackDir = (fallbackLeadPos - startPos).normalized;
            aimPos = fallbackLeadPos;

            return Mathf.Atan2(fallbackDir.y, fallbackDir.x) * Mathf.Rad2Deg;

        }


        float solution = validRoots.Min();
        

        Vector2 leadPos = targetPos + targetVel * solution;
        Vector2 dir = (leadPos - startPos).normalized;
        aimPos = leadPos;


        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
*/

