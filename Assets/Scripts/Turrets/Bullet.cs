using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    private int _damage;
    private Transform _target;
    private Enemy _enemy;

    public void Initialize(Transform target, int damage, Enemy enemy)
    {
        _target = target;
        _damage = damage;
        _enemy = enemy;
    }

    void Update()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = _target.position - transform.position;
        float distanceThisFrame = _speed * Time.deltaTime;

        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
    }

    private void HitTarget()
    {
        if (_enemy != null)
        {
            _enemy.TakeDamage(_damage);
        }
        Destroy(gameObject);
    }
}
