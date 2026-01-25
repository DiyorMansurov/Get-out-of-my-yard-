using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Minigame : MonoBehaviour
{
    private PlayerInputActions _input;
    [SerializeField] private int _requiredSuccesses;
    [SerializeField] private StarterAssets.FirstPersonController _playerControllerScript;
    [SerializeField] private Player _playerScript;


    private int _currentSuccesses = 0;
    private Key _expectedKey;
    private bool _IsActive;

    private Key[] _possibleKeys =
    {
        Key.W,
        Key.A,
        Key.S,
        Key.D,
    };

    public System.Action OnCompleted;

    private void Awake() {
        _input = new PlayerInputActions();
    }
    
    private void Start() {
        UIManager.Instance.RefreshProgressSlider(_currentSuccesses, _requiredSuccesses);
    }
    private void OnEnable() {
        _input.Minigame.Press.performed += OnKeyPressed;
        _input.Minigame.Exit.performed += CancelGame;
    }

    private void OnDisable() {
        _input.Minigame.Press.performed -= OnKeyPressed;
        _input.Minigame.Exit.performed -= CancelGame;
    }

    public void StartGame()
    {
        if (_IsActive) return;
        if (_playerScript.GetCurrentUpgradeTier() >= Player.UpgradeTiers.Tier3) return;

        switch (_playerScript.GetCurrentUpgradeTier())
        {
            case Player.UpgradeTiers.Tier0:
                _requiredSuccesses = 5;
                break;
            case Player.UpgradeTiers.Tier1:
                _requiredSuccesses = 10;
                break;
            case Player.UpgradeTiers.Tier2:
                _requiredSuccesses = 15;
                break;
            default: break;
        }

        _playerControllerScript.TogglePlayerinput();
        _currentSuccesses = 0;
        _IsActive = true;
        UIManager.Instance.ToggleMinigameUI(_IsActive);
        UIManager.Instance.ToggleCrosshair(_IsActive);
        UIManager.Instance.RefreshSuccesess(_currentSuccesses, _requiredSuccesses);
        PickRandomKey();
        _input.Minigame.Enable();
    }

    private void PickRandomKey()
    {
        _expectedKey = _possibleKeys[Random.Range(0,_possibleKeys.Length)];
        UIManager.Instance.SetMinigameKey(_expectedKey.ToString());
    }

    private void OnKeyPressed(InputAction.CallbackContext ctx)
    {
        if (!_IsActive) return;

        Key pressedKey = ctx.control.name switch
        {
            "w" => Key.W,
            "a" => Key.A,
            "s" => Key.S,
            "d" => Key.D,
            _ => Key.None
        };

        if (pressedKey == _expectedKey)
        {
            _currentSuccesses++;
            UIManager.Instance.RefreshSuccesess(_currentSuccesses, _requiredSuccesses);
            UIManager.Instance.RefreshProgressSlider(_currentSuccesses, _requiredSuccesses);

            if (_currentSuccesses == _requiredSuccesses)
            {
                CompleteGame();
            }else
            {
                PickRandomKey();
            }
        }
        else
        {
            Fail();
        }
    }

    private void Fail()
    {
        _currentSuccesses = 0; 
        UIManager.Instance.RefreshProgressSlider(_currentSuccesses, _requiredSuccesses);  
        UIManager.Instance.RefreshSuccesess(_currentSuccesses, _requiredSuccesses);
        PickRandomKey();
        Debug.Log("Failed");
    }

    private void CompleteGame()
    {
        _currentSuccesses = 0; 
        _playerScript.UpgradeCurrentUpgradeTier();
        UIManager.Instance.RefreshProgressSlider(_currentSuccesses, _requiredSuccesses);  
        _playerControllerScript.TogglePlayerinput();
        _IsActive = false;
        _input.Minigame.Disable();
        UIManager.Instance.ToggleMinigameUI(_IsActive);
        UIManager.Instance.ToggleCrosshair(_IsActive);
        OnCompleted?.Invoke();
    }

    
    private void CancelGame(InputAction.CallbackContext ctx)
    {   
        _currentSuccesses = 0; 
        UIManager.Instance.RefreshProgressSlider(_currentSuccesses, _requiredSuccesses);
        _playerControllerScript.TogglePlayerinput();
        _IsActive = false;
        _input.Minigame.Disable();
        UIManager.Instance.ToggleMinigameUI(_IsActive);
    }
}
