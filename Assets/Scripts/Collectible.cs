using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour, IInteractable, IHighlightable, ICrosshairTarget
{
    [SerializeField] private CollectiblesType _type;
    [SerializeField] private int _amount;
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
        if (CanCollect())
        {
            _player.CollectItem(_type, _amount);
            Destroy(gameObject);
        }else
        {
            Debug.Log("Not enough Space");
        }
        
    }

    private bool CanCollect()
    {
        switch (_type)
        {
            case CollectiblesType.Cog: return true; 
            case CollectiblesType.Pearl: return _player.CanCollectPearls(_amount);
            default:return true;
        }
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
