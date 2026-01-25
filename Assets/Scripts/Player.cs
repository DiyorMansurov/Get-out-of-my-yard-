using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class Player : MonoBehaviour
{
    private PlayerInputActions _input;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private TurretDefinition _definition;
    [SerializeField] private int _money = 100;
    [SerializeField] private int _pearls = 0;
    private GameObject _ghostObject;
    [SerializeField] private GameObject _turretPrefab;
    [SerializeField] private GameObject _realTurretPrefab;
    private int _placementLayerMask;
    private IHighlightable _currentHighlight;
    [SerializeField] private float _interactionDistance = 5f; 
    private RaycastHit _cachedHit;
    private bool _hasHit;
    private int _buildSurface;

    public enum UpgradeTiers
    {
        Tier0,
        Tier1,
        Tier2,
        Tier3,
    }

    private UpgradeTiers _currentUpgradeTier = UpgradeTiers.Tier0;

    public enum Modes
    {
        Normal,
        Build,
        Destroy
    }

    private Modes _currentMode = Modes.Normal;
  

    private void Awake() {
        _input = new PlayerInputActions();
        _placementLayerMask = ~LayerMask.GetMask("Ground", "Preview");
        _buildSurface = LayerMask.GetMask("Ground");
    }

    private void OnEnable() {
        _input.Player.Enable();
        _input.Player.Interact.performed += ctx => RaycastInteract();
        _input.Player.BuildMode.performed += ctx => ChangePlayerMode(Modes.Build);
        _input.Player.DestroyMode.performed += ctx => ChangePlayerMode(Modes.Destroy);
        _input.Player.RightButton.performed += ctx => RightClick();
    }
    private void OnDisable() {
        _input.Player.Disable();
    }

    public UpgradeTiers GetCurrentUpgradeTier()
    {
        return _currentUpgradeTier;
    }

    public void UpgradeCurrentUpgradeTier()
    {
        if (_currentUpgradeTier < UpgradeTiers.Tier3)
        {
            _currentUpgradeTier++;
        }
    }

    public void DepositPearls()
    {
        _pearls--;
    }

    public bool PearlsEmpty()
    {
        return _pearls == 0;
    }

    private void Update()
    {
        RaycastInfo();
        UpdateMoney();
        UpdatePearls();
    }

    public void CollectItem(CollectiblesType type, int amount)
    {
        switch (type)
        {
            case CollectiblesType.Cog:_money += amount;break;
            case CollectiblesType.Pearl:_pearls += amount;break;
            default:break;
        }
        
    }

    public bool CanCollectPearls(int amount)
    {
        return _pearls + amount <= 5;
    }
    private void UpdateMoney()
    {
        UIManager.Instance.RefreshMoneyAmount(_money);
    }
    private void UpdatePearls()
    {
        UIManager.Instance.RefreshPearlText(_pearls);
    }

    private void RightClick()
    {
        if (_currentMode != Modes.Normal)
        {
            ChangePlayerMode(Modes.Normal);
        }
    }


    private void ChangePlayerMode(Modes newMode)
    {
        if (_currentMode == newMode && newMode != Modes.Normal)
        {newMode = Modes.Normal;}
        _currentMode = newMode;
        UIManager.Instance.UpdateModeIndicator(_currentMode);
        switch (_currentMode)
        {
            case Modes.Normal:
                EnterNormalMode();
                break;
            case Modes.Build:
                EnterBuildMode();
                break;
            case Modes.Destroy:
                EnterDestroyMode();
                break;
            default:
                break;
        }
    }

    private void EnterNormalMode()
    {
        UIManager.Instance.ToggleCrosshair(false);
        UIManager.Instance.CrosshairSet(CrosshairType.Default);
        if (_ghostObject != null)
        {
            Destroy(_ghostObject);
        }
    }

    private void EnterBuildMode()
    {
        UIManager.Instance.ToggleCrosshair(true);
        _ghostObject = Instantiate(_turretPrefab, Vector3.zero, Quaternion.identity);
    }
    private void EnterDestroyMode()
    {
        UIManager.Instance.ToggleCrosshair(false);
        UIManager.Instance.CrosshairSet(CrosshairType.Destroy);
        if (_ghostObject != null)
        {
            Destroy(_ghostObject);
        }
    }

    private bool CheckValidPlacement(Vector3 position)
    {
        float radius = 1f;
        return !Physics.CheckSphere(position, radius, _placementLayerMask);
    }

    private void RaycastInfo()
    {
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        
        if (_currentMode == Modes.Build)
        {
            _hasHit = Physics.Raycast(ray, out _cachedHit, _interactionDistance, _buildSurface);
            HandleBuildMode(_cachedHit);

        }else if (_currentMode == Modes.Destroy)
        {
            _hasHit = Physics.Raycast(ray, out _cachedHit, _interactionDistance);
            if (_hasHit)
            {
                Turret turret = _cachedHit.collider.GetComponentInParent<Turret>();
                if (turret != null)
                {
                    HandleHighlight(_cachedHit);
                }
            }
            else
            {
                HighlightObject(null);
            }
        }
        else
        {
            _hasHit = Physics.Raycast(ray, out _cachedHit, _interactionDistance);
            if (_hasHit)
            {
                HandleHighlight(_cachedHit);
                HandleCrosshairChange(_cachedHit);
            }
            else
            {
                HighlightObject(null);
            }
        }
    }
    private void HandleBuildMode(RaycastHit hit)
    {
        if (_currentMode == Modes.Build)
        {
            _ghostObject.transform.position = hit.point;
            _ghostObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            bool isValidPlacement = CheckValidPlacement(hit.point);
            SetPreviewMaterial(isValidPlacement);
        }

    }

    private void HandleCrosshairChange(RaycastHit hit)
    {
        ICrosshairTarget target = hit.collider.GetComponentInParent<ICrosshairTarget>();
        if (target == null)
        {
            target = hit.collider.GetComponentInChildren<ICrosshairTarget>();
        }

        if (target != null)
        {
            UIManager.Instance.CrosshairSet(target.GetCrosshairType());
            return;
        }

        UIManager.Instance.CrosshairSet(CrosshairType.Default);
    }

    private void SetPreviewMaterial(bool isValid)
    {
        Renderer[] renderers = _ghostObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                if (isValid)
                {
                    mat.color = new Color(0f, 1f, 0f, 0.5f); 
                }
                else
                {
                    mat.color = new Color(1f, 0f, 0f, 0.5f); 
                }
            }
        }
    }
    
    private void HandleHighlight(RaycastHit hit)
    {
        IHighlightable newHighlight = null;
            
        newHighlight = hit.collider.GetComponentInParent<IHighlightable>();
        if (newHighlight == null)
        {
            newHighlight = hit.collider.GetComponentInChildren<IHighlightable>();
        }
        HighlightObject(newHighlight);
    }
    private void RaycastInteract()
    {
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        RaycastHit hit;
        
        if (_currentMode == Modes.Normal)
        {
            if (Physics.Raycast(ray, out hit, _interactionDistance))
            {   
                if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
                {
                    interactable.Interact();
                    
                }else
                {
                    Turret turretParent = hit.collider.GetComponentsInParent<Turret>().FirstOrDefault();
                    IUpgradable upgradable = hit.collider.GetComponentsInChildren<IUpgradable>(true).FirstOrDefault();
                    
                    if (turretParent != null)
                    {
                        UpgradeTurret(turretParent, null);                         
                    }else if (upgradable != null)
                    {
                        turretParent = hit.collider.GetComponentInChildren<Turret>(true);
                        UpgradeTurret(turretParent, upgradable);  
                    }
                }

                
            }
        } else if (_currentMode == Modes.Build)
        {
            if (Physics.Raycast(ray, out hit, _interactionDistance, _buildSurface) && CheckValidPlacement(hit.point) && _money >= _definition.tiers[0].cost)
            {
                Instantiate(_realTurretPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                _money -= _definition.tiers[0].cost;
            }
        }else
        {
            if (Physics.Raycast(ray, out hit, _interactionDistance))
            {
                    Turret turret = hit.collider.GetComponentsInParent<Turret>().FirstOrDefault();
                    if (turret == null)
                    {
                        turret = hit.collider.GetComponentInChildren<Turret>(true);
                    }

                    if (turret != null)
                    {
                        int refundAmount = turret.GetSellAmount();
                        _money += refundAmount;
                        Destroy(turret.gameObject);
                    }
            }
        }
    }

    private void HighlightObject(IHighlightable newHighlight)
    {
        MonoBehaviour mb = _currentHighlight as MonoBehaviour;

        if (_currentHighlight != null && _currentHighlight != newHighlight && mb != null)
        {
            _currentHighlight.Highlight(false);
        }

        _currentHighlight = null;

        if (newHighlight != null && newHighlight != _currentHighlight)
        {
            newHighlight.Highlight(true);
        }

        _currentHighlight = newHighlight;
    }

    private void UpgradeTurret(Turret parent, IUpgradable upgradableInterface)
    {
        IUpgradable upgradable;

        if (parent != null)
        {
            upgradable = parent.GetComponentsInChildren<IUpgradable>(true).FirstOrDefault();
        }
        else
        {
            upgradable = upgradableInterface;
        }
        

        if (upgradable != null)
        {
            if (upgradable.CanUpgrade())
            {
                if (parent.CurrentTier() < (int)_currentUpgradeTier)
                {
                    int cost = upgradable.UpgradeCost();
                    if (_money >= cost)
                    {
                        _money -= cost;
                        upgradable.Upgrade();
                    }
                    else
                    {
                        Debug.Log("Not Enough Money for Upgrade");
                    }
                }else
                {
                    Debug.Log("Next tier is not learned");
                }
                
            }
            else
            {
                Debug.Log("Turret is already fully leveled up");
            }
        }                    
    }

    private void OnDrawGizmos() {
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * _interactionDistance);
    }

}
