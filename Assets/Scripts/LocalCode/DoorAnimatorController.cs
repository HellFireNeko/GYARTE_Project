using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator DoorAnimator;
    [SerializeField] private bool State;

    void Start()
    {
        DoorAnimator.SetBool("Open", State);
    }

    public void ToggleDoorState()
    {
        State = !State;
        DoorAnimator.SetBool("Open", State);
    }
}
