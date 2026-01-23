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
    public Action OnDeath;
    private Transform target;
    private int _randomAmountOfCogs;
    [SerializeField] private float _cogDropForce = 2f;


    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _randomAmountOfCogs = UnityEngine.Random.Range(1,3);
        
    }

    void Update()
    {
        DistanceCheck();
    }

    public void SetTarget(Transform targetReceived)
    {
        target = targetReceived;

        if (target != null)
        {
            agent.SetDestination(target.position);
        }
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
            GameObject cog = Instantiate(_cogPrefab, transform.position, Quaternion.identity);

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

        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= 2f)
            {
                House house = target.GetComponent<House>();
                if (house != null)
                {
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
            house.TakeDamage(10);
            yield return new WaitForSeconds(3f);
        }
    }
}
