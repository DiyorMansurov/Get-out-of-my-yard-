using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    private int _damage;
    private Transform _target;

    public void Initialize(Transform target, int damage)
    {
        _target = target;
        _damage = damage;
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
        Enemy enemy = _target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(_damage);
        }
        Destroy(gameObject);
    }
}
