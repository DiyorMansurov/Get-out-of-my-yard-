using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _health = 100f;
    [SerializeField] private GameObject _cogPrefab;
    [SerializeField] private GameObject _pearlPrefab;
    UnityEngine.AI.NavMeshAgent agent;
    private bool isAttacking = false;
    private House _house;
    public Action OnDeath;
    private Transform _target;
    private int _damage;
    private int _randomAmountOfCogs;
    private Animator _anim;
    private bool _dropsPearl;
    private bool _isDead = false;
    [SerializeField] private int _chanceToDropPearl;
    [SerializeField] private Transform _AimPoint;
    [SerializeField] private float _cogDropForce = 2f;


    [SerializeField] private Slider _hpSlider;

    [SerializeField] private AudioSource _hitSFX;
    [SerializeField] private AudioSource _deathSFX;


    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _randomAmountOfCogs = UnityEngine.Random.Range(1,3);
        CheckIfDropsPearl();
        _anim = GetComponent<Animator>();
        _anim.SetBool("IsRunning", true);
        _hpSlider.maxValue = _health;
    }

    void Update()
    {
        DistanceCheck();
        HealthSliderAnimation();
        RotateTowardsPlayer(_hpSlider.transform, Camera.main.transform);
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

    private void HealthSliderAnimation()
    {
        _hpSlider.value = Mathf.Lerp(_hpSlider.value,_health , 5f * Time.deltaTime);
    }

    private void CheckIfDropsPearl()
    {
        if (UnityEngine.Random.Range(1,_chanceToDropPearl + 1) == _chanceToDropPearl)
        {
            _dropsPearl = true;
        }
    }
    public void SetTarget(Transform[] targetsReceived)
    {
        float closestDistance = Mathf.Infinity;

        foreach (var target in targetsReceived)
        {
            float distance = (target.position - transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                _target = target;
            }
        }

        if (_target != null)
        {
            agent.SetDestination(_target.position);
        }
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    public void SetHealth(int health)
    {
        _health = health;
    }

    public Transform GetAimPoint()
    {
        return _AimPoint;
    }

    public void SetHouse(House house)
    {
        if (house != null)
        {
            _house = house;
        }
        
    }
    public void TakeDamage(float damage)
    {
        if (_isDead) return;

        _health -= damage;
        if (_health <= 0)
        {
            _isDead = true;
            Die();
        }
    }

    public void Die()
    {
        OnDeath?.Invoke();
        DropLoot();
        _deathSFX.Play();
        Destroy(gameObject);
    }

    private void DropLoot()
    {
        for (int i = 0; i < _randomAmountOfCogs; i++)
        {
            GameObject cog = Instantiate(_cogPrefab, _AimPoint.position, Quaternion.identity);
            UIManager.Instance.UpdateOnMapIndicator("cog", 1);

            Rigidbody _cogRB;
            _cogRB = cog.GetComponent<Rigidbody>();

            if (_cogRB != null)
            {
                Vector3 randomDir = UnityEngine.Random.onUnitSphere;
                randomDir.y = Mathf.Abs(randomDir.y);

                _cogRB.AddForce(randomDir * _cogDropForce, ForceMode.Impulse);
                _cogRB.AddTorque(UnityEngine.Random.onUnitSphere * _cogDropForce, ForceMode.Impulse);
            }
        }

        if (_dropsPearl)
        {
            GameObject pearl = Instantiate(_pearlPrefab, _AimPoint.position, Quaternion.identity);
            UIManager.Instance.UpdateOnMapIndicator("pearl", 1);

            Rigidbody _pearlRB;
            _pearlRB = pearl.GetComponent<Rigidbody>();

            if (_pearlRB != null)
            {
                Vector3 randomDir = UnityEngine.Random.onUnitSphere;
                randomDir.y = Mathf.Abs(randomDir.y);

                _pearlRB.AddForce(randomDir * _cogDropForce, ForceMode.Impulse);
            }
        }
    }

    private void DistanceCheck()
    {
        if (isAttacking) return;

        if (_target != null)
        {
            float distance = Vector3.Distance(transform.position, _target.position);
            if (distance <= 2f)
            {
                if (_house != null)
                {   
                    House house = _house.GetComponent<House>();
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;

                    _anim.SetBool("IsRunning", false);
                    StartCoroutine(AttackHouse(house));
                    isAttacking = true;
                }
            }
        }
    }

    private IEnumerator AttackHouse(House house)
    {
        while (house != null)
        {
            _anim.SetTrigger("Attack");
            _hitSFX.Play();
            house.TakeDamage(_damage);
            yield return new WaitForSeconds(3f);
        }
    }


}
