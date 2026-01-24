using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workshop : MonoBehaviour, IInteractable, IHighlightable, ICrosshairTarget
{
    private Outline outline;
    [SerializeField] private Minigame _minigame;
    

    private void Awake()
    {
        outline = GetComponentInParent<Outline>();
        outline.enabled = false;
    }

    public void Interact()
    {

        _minigame.OnCompleted += GiveReward;
        _minigame.StartGame();
    }

    private void GiveReward()
    {
        Debug.Log("Completed");
        _minigame.OnCompleted -= GiveReward;
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
