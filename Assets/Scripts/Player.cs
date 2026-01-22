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
    public bool _buildMode = false;
    private GameObject _ghostObject;
    [SerializeField] private GameObject _turretPrefab;
    [SerializeField] private GameObject _realTurretPrefab;
    private int _placementLayerMask;
    private IHighlightable _currentHighlight;
    [SerializeField] private float _interactionDistance = 20f; 

    private void Awake() {
        _input = new PlayerInputActions();
        _placementLayerMask = ~LayerMask.GetMask("Ground", "Preview");
    }

    private void OnEnable() {
        _input.Player.Enable();
        _input.Player.Interact.performed += ctx => RaycastInteract();
        _input.Player.BuildMode.performed += ctx => ToggleBuildMode();
        _input.Player.RightButton.performed += ctx => RightClick();
    }
    private void OnDisable() {
        _input.Player.Disable();

        _input.Player.Interact.performed -= ctx => RaycastInteract();
        _input.Player.BuildMode.performed -= ctx => ToggleBuildMode();
        _input.Player.RightButton.performed -= ctx => RightClick();
    }

    private void Update()
    {
        if (_buildMode)
        {
            RaycastBuildMode();
        }
        else
        {
            RaycastHighlight();
        }
    }

    private void RightClick()
    {
        if (_buildMode == true)
        {
            ToggleBuildMode();
        }
    }

  

    private void ToggleBuildMode()
    {
        _buildMode = !_buildMode;
        if (_buildMode)
        {
            _ghostObject = Instantiate(_turretPrefab, Vector3.zero, Quaternion.identity);
        }
        else
        {
            if (_ghostObject != null)
            {
                Destroy(_ghostObject);
            }
        }
    }

    private bool CheckValidPlacement(Vector3 position)
    {
        float radius = 1f;
        return !Physics.CheckSphere(position, radius, _placementLayerMask);
    }

    private void RaycastBuildMode()
    {
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _interactionDistance, LayerMask.GetMask("Ground")))
        {
            if (_buildMode)
            {
                _ghostObject.transform.position = hit.point;
                _ghostObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                bool isValidPlacement = CheckValidPlacement(hit.point);
                SetPreviewMaterial(isValidPlacement);
            }
        }
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
    
    private void RaycastHighlight()
    {
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        RaycastHit hit;
        IHighlightable newHighlight = null;

        if (Physics.Raycast(ray, out hit, _interactionDistance))
        {
            newHighlight = hit.collider.GetComponentInParent<IHighlightable>();
            if (newHighlight == null)
            {
                newHighlight = hit.collider.GetComponentInChildren<IHighlightable>();
            }
            HighlightObject(newHighlight);
        }
    }
    private void RaycastInteract()
    {
        Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
        RaycastHit hit;
        
        if (!_buildMode)
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
                        UpgradeTurret(null, upgradable);  
                    }
                }

                
            }
        } else
        {
            if (Physics.Raycast(ray, out hit, _interactionDistance, LayerMask.GetMask("Ground")) && CheckValidPlacement(hit.point) && _money >= _definition.tiers[0].cost)
            {
                Instantiate(_realTurretPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                _money -= _definition.tiers[0].cost;
            }
        }
    }

    private void HighlightObject(IHighlightable newHighlight)
    {
        if (_currentHighlight != null && _currentHighlight != newHighlight)
        {
            _currentHighlight.Highlight(false);
        }


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
