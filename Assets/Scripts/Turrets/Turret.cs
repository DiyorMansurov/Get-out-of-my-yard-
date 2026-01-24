using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Turret : MonoBehaviour, IUpgradable, IHighlightable, ICrosshairTarget
{
    [SerializeField] private TurretDefinition _definition;
    [SerializeField] private Transform[] _bulletPoints;
    [SerializeField] private GameObject[] _tierPrefabs;
    [SerializeField] private int _currentTier = 0;
    private Outline outline;
    private float _fireCooldown = 0f;

    private Transform closestTarget = null;

    TurretTierData CurrentTierData => _definition.tiers[_currentTier];
    private void Update() {
        Shoot();

        if (closestTarget != null)
        {
            LookAtTarget(closestTarget);
        }
        
    }

    private void Awake()
    {
        outline = GetComponentInParent<Outline>();
        outline.enabled = false;
    }

 

    private void Shoot() {
        if (_fireCooldown > 0f) {
            _fireCooldown -= Time.deltaTime;
            return;
        }

        if (closestTarget != null)
        {
            float rangeSqr = CurrentTierData.range * CurrentTierData.range;

            if ((closestTarget.position - transform.position).sqrMagnitude > rangeSqr)
                closestTarget = null;
        }

        if (closestTarget == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, CurrentTierData.range);
            float closestDistance = Mathf.Infinity;

            foreach (var hit in hits) {
                if (hit.CompareTag("Enemy")) {

                    float distance = (hit.transform.position - transform.position).sqrMagnitude;
                    
                    if(distance < closestDistance) {
                        closestDistance = distance;
                        closestTarget = hit.transform;
                    }
                }
            }
        }


        if (closestTarget != null) { 
                foreach (var point in _bulletPoints)
                {
                    GameObject bullet = Instantiate(CurrentTierData.bulletPrefab, point.position, Quaternion.identity);
                    Bullet bulletComponent = bullet.GetComponent<Bullet>();
                    Enemy enemyScript = closestTarget.GetComponent<Enemy>();
                    Transform AimPoint = enemyScript.GetAimPoint();
                    bulletComponent.Initialize(AimPoint, CurrentTierData.damage, enemyScript);
                    _fireCooldown = 1f / CurrentTierData.fireRate;
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
        UpdateBulletPoints();  
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
