using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycaster : MonoBehaviour
{
    [SerializeField, Range(1, 100)] private float RayRange = 5;
    [SerializeField] private Vector3 RayOffset;
    [SerializeField] private DirectionEnum RayDirection;
    [SerializeField] private bool ShowRayOrigin;
    [SerializeField] private bool ShowRayLine;
    [SerializeField] private bool ShowRayHit;

    public void RayCast()
    {
        var positionOrigin = transform.position + RayOffset;
        Ray ray = new Ray(positionOrigin, GetRayDirection());
        if (Physics.Raycast(ray, out RaycastHit hit, RayRange))
        {
            if (hit.collider.TryGetComponent(out Target c))
            {
                c.SendMessage("HitEventMethod", this);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (ShowRayOrigin)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + RayOffset, .3f);
        }
        if (ShowRayLine)
        {
            var positionOrigin = transform.position + RayOffset;
            var rayEndPoint = positionOrigin + (GetRayDirection() * RayRange);
            Gizmos.color = Color.black;
            Gizmos.DrawLine(positionOrigin, rayEndPoint);
        }
        if (ShowRayHit)
        {
            var positionOrigin = transform.position + RayOffset;
            Ray ray = new Ray(positionOrigin, GetRayDirection());
            if (Physics.Raycast(ray, out RaycastHit hit, RayRange))
            {
                if (hit.collider.TryGetComponent(out Target c))
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(hit.point, .3f);
                }
            }
        }
    }

    private Vector3 GetRayDirection()
    {
        Func<Vector3> SwitchCase = new Func<Vector3>(() =>
        {
            switch (RayDirection)
            {
                case DirectionEnum.Up:
                    return Vector3.up;

                case DirectionEnum.Down:
                    return Vector3.down;

                case DirectionEnum.Forward:
                    return Vector3.forward;

                case DirectionEnum.Backward:
                    return Vector3.back;

                case DirectionEnum.Left:
                    return Vector3.left;

                case DirectionEnum.Right:
                    return Vector3.right;

                default:
                    return Vector3.zero;
            }
        });
        return transform.TransformDirection(SwitchCase());
    }

    public enum DirectionEnum
    {
        Up,
        Down,
        Forward,
        Backward,
        Left,
        Right,
    }
}
