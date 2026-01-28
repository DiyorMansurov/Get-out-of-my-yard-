using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour, IInteractable, IHighlightable, ICrosshairTarget
{
    [SerializeField] private CollectiblesType _type;
    [SerializeField] private int _amount;
    [SerializeField] private AudioClip _collectSFX;
    private Player _player;
    private Outline outline;

    private void Awake()
    {
        _player = GameObject.Find("Player/PlayerCapsule").GetComponent<Player>();
        outline = GetComponentInChildren<Outline>();
        outline.enabled = false;
    }
    public void Interact()
    {
        if (CanCollect())
        {
            _player.CollectItem(_type, _amount);
            AudioManager.Instance.PlaySFX(_collectSFX);
            Destroy(gameObject);
        }else
        {
            UIManager.Instance.NotificationPopUp($"Not enough space", new Color(0.9245283f, 0.2049662f, 0.2049662f));
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
