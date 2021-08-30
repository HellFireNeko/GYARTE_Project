using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTriggerShow : MonoBehaviour
{
    public bool OnGround = false;
    public bool OnPlatform = false;
    public GameObject PreviousPlatform;
    public bool ForgetPrevious;
    public int IllegalMoves;

    public int IllegalTime = 10;

    public UnityEngine.Events.UnityEvent OnIllegalMove;

    void UnblockPlatform()
    {
        ForgetPrevious = true;
        PreviousPlatform = null;
    }

    void BlockPlatform()
    {
        ForgetPrevious = false;
        IllegalMoves++;
        OnIllegalMove.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SafeZone"))
        {
            if (IsInvoking()) { CancelInvoke(); }
            OnPlatform = true;
            if (PreviousPlatform != null)
            {
                if (!ForgetPrevious && other.gameObject == PreviousPlatform)
                {
                    IllegalMoves++;
                    OnIllegalMove.Invoke();
                }
            }
            PreviousPlatform = other.gameObject;
            Invoke("BlockPlatform", IllegalTime);
        }
        else if (other.CompareTag("GroundZone"))
        {
            OnGround = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafeZone"))
        {
            if (IsInvoking()) { CancelInvoke(); }
            OnPlatform = false;
            ForgetPrevious = false;
            Invoke("UnblockPlatform", IllegalTime);
        }
        else if (other.CompareTag("GroundZone"))
        {
            OnGround = false;
        }
    }
}
