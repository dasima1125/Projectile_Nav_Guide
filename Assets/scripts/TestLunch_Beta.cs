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
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }
    float TraceRead2D(Vector2 targetPos, Vector2 startPos, Vector2 targetVel, float ProjectileSpeed)
    {
        Vector2 displacement = targetPos - startPos;
        float a = Vector2.Dot(targetVel, targetVel) - (ProjectileSpeed * ProjectileSpeed);
        float b = Vector2.Dot(displacement, targetVel) * 2;
        float c = Vector2.Dot(displacement, displacement);

        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0 || Mathf.Abs(a) < 0.0001f)
        {
            // Currently under refinement: if the projectile speed meets the threshold condition,
            // a fallback position will be assigned based on the minimum viable value that satisfies the conditional check.

            // Warning: The fallback state is unstable when the projectile speed
            // is less than or equal to the target's speed. Please be cautious.
        
            float over = GetTNS2D().velocity.magnitude + 3;

            Vector2 fallbackLeadPos = targetPos + targetVel.normalized * over; 
            Vector2 fallbackDir = (fallbackLeadPos - startPos).normalized;

            return Mathf.Atan2(fallbackDir.y, fallbackDir.x) * Mathf.Rad2Deg;
        
        }
        float rootP = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
        float rootM = (-b - Mathf.Sqrt(discriminant)) / (2 * a);
        float solution = Mathf.Max(rootP, rootM);

        Vector2 leadPos = targetPos + targetVel * solution;
        Vector2 dir = (leadPos - startPos).normalized;
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
    Pure ,Lead ,Pn
}