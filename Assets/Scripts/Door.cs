using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable, IHighlightable, ICrosshairTarget
{
    private Outline outline;
    private bool IsOpened = false;
    private bool isRotating = false;

    private void Awake()
    {
        outline = GetComponentInParent<Outline>();
        outline.enabled = false;
    }
    public void Interact()
    {
        if (isRotating) return;

        float targetZ = IsOpened ? 0f : 90f;
        StartCoroutine(RotateDoor(targetZ));

        IsOpened = !IsOpened;
        
    }

    private IEnumerator RotateDoor(float targetZ)
    {
        isRotating = true;

        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(-90f, 0f, targetZ);

        float time = 0f;
        float duration = 0.5f;

        while (time < duration)
        {
            time += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, targetRot, time / duration);
            yield return null;
        }

        transform.rotation = targetRot;
        isRotating = false;
    }

    public void Highlight(bool state)
    {
        outline.enabled = state;
    }

    public CrosshairType GetCrosshairType()
    {
        return CrosshairType.Interact;
    }
}
