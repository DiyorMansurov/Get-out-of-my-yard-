using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;


public class Turret : MonoBehaviour, IUpgradable, IHighlightable, ICrosshairTarget
{
    [SerializeField] private TurretDefinition _definition;
    [SerializeField] private Transform[] _bulletPoints;
    [SerializeField] private GameObject[] _tierPrefabs;
    [SerializeField] private GameObject _shootVFX;
    [SerializeField] private int _currentTier = 0;
    [SerializeField] private TMP_Text _onTurretCost;
    [SerializeField] private Canvas _onTurretCanvas;
    [SerializeField] private GameObject _onTurretCogIcon;
    private Outline outline;
    private float _fireCooldown = 0f;

    private Transform _closestTarget = null;
    private bool _isMaxed = false;

    TurretTierData CurrentTierData => _definition.tiers[_currentTier];
    private void Update() {
        Shoot();

        if (_closestTarget != null)
        {
            LookAtTarget(_closestTarget);
        }

        RotateTowardsPlayer(_onTurretCanvas.transform, Camera.main.transform);
        
    }

    private void Awake()
    {
        outline = GetComponentInParent<Outline>();
        outline.enabled = false;
    }

    private void Start() {
        RefreshOnTurretCost(_definition.tiers[_currentTier + 1].cost);
    }
    private void RotateTowardsPlayer(Transform uiElement, Transform playerTransform)
    {
        Vector3 direction = uiElement.position - playerTransform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            uiElement.rotation = Quaternion.Slerp(uiElement.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
    private void RefreshOnTurretCost(int Cost)
    {
        if (_isMaxed) return;
        
        if (Cost > 0)
        {
            _onTurretCost.text = Cost.ToString();
        } else
        {
            _onTurretCost.text = "Max LVL";
            _onTurretCost.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 160f);
            _onTurretCogIcon.SetActive(false);
            _isMaxed = true;
        }
        
    }
    private void Shoot() {
        if (_fireCooldown > 0f) {
            _fireCooldown -= Time.deltaTime;
            return;
        }

        if (_closestTarget != null)
        {
            float rangeSqr = CurrentTierData.range * CurrentTierData.range;

            if ((_closestTarget.position - transform.position).sqrMagnitude > rangeSqr)
                _closestTarget = null;
        }

        if (_closestTarget == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, CurrentTierData.range);
            float closestDistance = Mathf.Infinity;

            foreach (var hit in hits) {
                if (hit.CompareTag("Enemy")) {

                    float distance = (hit.transform.position - transform.position).sqrMagnitude;
                    
                    if(distance < closestDistance) {
                        closestDistance = distance;
                        _closestTarget = hit.transform;
                    }
                }
            }
        }


        if (_closestTarget != null) { 
                foreach (var point in _bulletPoints)
                {
                    GameObject bullet = Instantiate(CurrentTierData.bulletPrefab, point.position, Quaternion.identity);
                    Bullet bulletComponent = bullet.GetComponent<Bullet>();
                    Enemy enemyScript = _closestTarget.GetComponent<Enemy>();
                    Transform AimPoint = enemyScript.GetAimPoint();
                    bulletComponent.Initialize(AimPoint, CurrentTierData.damage, enemyScript);
                    _fireCooldown = 1f / CurrentTierData.fireRate;
                    Vector3 direction = _closestTarget.position - transform.position;
                    Instantiate(_shootVFX, point.position, Quaternion.LookRotation(direction));
                }
            }
        
    }

    private void LookAtTarget(Transform target)
    {
            Vector3 direction = target.position - transform.position;

    if (direction == Vector3.zero)
        return;

    Quaternion targetRotation = Quaternion.LookRotation(direction);

    transform.rotation = Quaternion.RotateTowards(
        transform.rotation,
        targetRotation,
        270f * Time.deltaTime);
    }

    public bool CanUpgrade()
    {
        return _currentTier + 1 < _definition.tiers.Length;
    }

    public int CurrentTier()
    {
        return _currentTier;
    }
    public int UpgradeCost()
    {
        if (!CanUpgrade())
            return 0;

        return _definition.tiers[_currentTier + 1].cost;
    }

    private void UpdateBulletPoints()
    {
        Debug.Log("Length" + _tierPrefabs.Length);
        Debug.Log(_currentTier);
        GameObject activeTier = _tierPrefabs[_currentTier];
        _bulletPoints = activeTier.GetComponentsInChildren<Transform>()
                        .Where(t => t.CompareTag("BulletPoint"))
                        .ToArray();
    }
    public void Upgrade()
    {
        _tierPrefabs[_currentTier].SetActive(false);
        _currentTier++;
        _tierPrefabs[_currentTier].SetActive(true);

        if (CanUpgrade())
        {
            RefreshOnTurretCost(_definition.tiers[_currentTier + 1].cost);
        }else
        {
            RefreshOnTurretCost(-1);
        }
        
        UpdateBulletPoints();  
    }

    public int GetSellAmount()
    {
        int sellAmount = 0;
        for (int i = 0; i <= _currentTier; i++)
        {
            sellAmount += _definition.tiers[i].cost;
        }
        sellAmount = Mathf.FloorToInt(sellAmount * 0.5f);
        Destroy(this.transform.parent.gameObject);
        return sellAmount;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, CurrentTierData.range);
    }

    public void Highlight(bool state)
    {
        outline.enabled = state;
    }

    public CrosshairType GetCrosshairType()
    {
        return CrosshairType.Hammer;
    }
}
