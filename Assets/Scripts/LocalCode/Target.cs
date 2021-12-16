using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Target : MonoBehaviour
{
    public UnityEvent OnHit;

    public void HitEventMethod(Raycaster sender)
    {
        OnHit.Invoke();
    }
}
