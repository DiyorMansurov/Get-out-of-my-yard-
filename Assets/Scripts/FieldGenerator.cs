using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FieldGenerator : MonoBehaviour, IInteractable, IHighlightable, ICrosshairTarget{
    private Outline[] outlines;
    [SerializeField] private Player _playerScript;
    [SerializeField] private int _pearlsToWin = 10;
    private int _pearlAmount = 0;

    private void Awake()
    {
        outlines = GetComponentsInChildren<Outline>();
        foreach (var outline in outlines)
        {
            outline.enabled = false;
        }
    }

    public void Interact()
    {
        if (!_playerScript.PearlsEmpty())
        {
            _playerScript.DepositPearls();
            _pearlAmount++;
            UIManager.Instance.RefreshGeneratorPearls(_pearlAmount, _pearlsToWin);
            if (_pearlAmount >= _pearlsToWin)
            {
                UIManager.Instance.WinSequence();
            }
        }
    }

    public void Highlight(bool state)
    {
        foreach (var outline in outlines)
        {
            outline.enabled = state;
        }
    }

    public CrosshairType GetCrosshairType()
    {
        return CrosshairType.Interact;
    }
}

