using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float health = 40f;
    [SerializeField] private GameObject _cogPrefab;
    UnityEngine.AI.NavMeshAgent agent;
    private bool isAttacking = false;
    private House _house;
    public Action OnDeath;
    private Transform _target;
    private int _randomAmountOfCogs;
    private Animator _anim;
    [SerializeField] private Transform _AimPoint;
    [SerializeField] private float _cogDropForce = 2f;


    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _randomAmountOfCogs = UnityEngine.Random.Range(1,3);
        _anim = GetComponent<Animator>();
        _anim.SetBool("IsRunning", true);
    }

    void Update()
    {
        DistanceCheck();
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

    public Transform GetAimPoint()
    {
        return _AimPoint;
    }

    public void SetHouse(House house)
    {
        _house = house;
    }
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        OnDeath?.Invoke();
        for (int i = 0; i < _randomAmountOfCogs; i++)
        {
            GameObject cog = Instantiate(_cogPrefab, _AimPoint.position, Quaternion.identity);

            Rigidbody _cogRB;
            _cogRB = _cogPrefab.GetComponent<Rigidbody>();

            if (_cogRB != null)
            {
                Vector3 randomDir = UnityEngine.Random.onUnitSphere;
                randomDir.y = Mathf.Abs(randomDir.y);

                _cogRB.AddForce(randomDir * _cogDropForce, ForceMode.Impulse);
            }
        }
        
        Destroy(gameObject);
    }

    private void DistanceCheck()
    {
        if (isAttacking) return;

        if (_target != null)
        {
            float distance = Vector3.Distance(transform.position, _target.position);
            if (distance <= 2f)
            {
                House house = _house.GetComponent<House>();
                if (house != null)
                {     
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
            house.TakeDamage(10);
            yield return new WaitForSeconds(3f);
        }
    }
}
