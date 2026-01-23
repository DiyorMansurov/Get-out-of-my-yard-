using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cog : MonoBehaviour, IInteractable, IHighlightable, ICrosshairTarget
{
    private Player _player;
    private Outline outline;

    private void Awake()
    {
        _player = GameObject.Find("Player/PlayerCapsule").GetComponent<Player>();
        outline = GetComponentInParent<Outline>();
        outline.enabled = false;
    }
    public void Interact()
    {
        _player.CollectCogs(20);
        Destroy(gameObject);
    }

    public void Highlight(bool state)
    {
        outline.enabled = state;
    }

    public CrosshairType GetCrosshairType()
    {
        return CrosshairType.Pickup;
    }
}
