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
    private bool _dreamsMessageWorked = false;

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

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Invisible_Wall") && !_dreamsMessageWorked)
        {
            UIManager.Instance.NotificationPopUp("Why is there a void instead of forest, feels like a dream...", new Color(0.8362166f, 0.6556604f, 1f));

            _dreamsMessageWorked = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Invisible_Wall") && _dreamsMessageWorked)
        {
            _dreamsMessageWorked = false;
        }
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
            UIManager.Instance.NotificationPopUp($"{_currentUpgradeTier} turrets learned", new Color(0.6415326f, 0.9811321f, 0.4026344f));
        } else
        {
            Debug.Log("Stop");
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
            case CollectiblesType.Cog:_money += amount;
                 UIManager.Instance.UpdateOnMapIndicator("cog", -1);
                 break;
            case CollectiblesType.Pearl:_pearls += amount;
                 UIManager.Instance.UpdateOnMapIndicator("pearl", -1);
                 break;
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

    private bool CheckValidRotation()
    {
        Vector3 right = _ghostObject.transform.right;
        Vector3 forward = _ghostObject.transform.forward;

        float xTilt = Vector3.Angle(Vector3.ProjectOnPlane(right, Vector3.up), right);
        float zTilt = Vector3.Angle(Vector3.ProjectOnPlane(forward, Vector3.up), forward);

        return xTilt <= 30f && zTilt <= 30f;
    }

    private bool CheckValidPlacement(Vector3 position)
    {
        float radius = 1f;

        bool SpaceClear = !Physics.CheckSphere(position, radius, _placementLayerMask);
        bool roationValid = CheckValidRotation();
        return SpaceClear && roationValid;
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
            if (Physics.Raycast(ray, out hit, _interactionDistance, _buildSurface) && CheckValidPlacement(hit.point))
            {
                if (_money >= _definition.tiers[0].cost)
                {
                    Instantiate(_realTurretPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                    _money -= _definition.tiers[0].cost;
                }else
                {
                    UIManager.Instance.NotificationPopUp("Not Enough money", new Color(0.9900868f, 0.990566f, 0.8083392f));
                }
                
            }else
            {
                UIManager.Instance.NotificationPopUp("Placement is not valid", new Color(0.9900868f, 0.990566f, 0.8083392f));
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
                        UIManager.Instance.NotificationPopUp("Not Enough Money for Upgrade", new Color(0.7885814f, 0.9200487f, 0.9339623f));
                    }
                }else
                {
                    UIManager.Instance.NotificationPopUp("Next tier is not learned", new Color(0.7885814f, 0.9200487f, 0.9339623f));
                }
                
            }
            else
            {
                UIManager.Instance.NotificationPopUp("Turret is already fully leveled up", new Color(0.7885814f, 0.9200487f, 0.9339623f));
            }
        }                    
    }

    private void OnDrawGizmos() {
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * _interactionDistance);
    }

}
