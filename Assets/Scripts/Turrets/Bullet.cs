using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private GameObject _hitVFX;
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

        MoveToTarget();
        LookAtTarget();
    }

    private void LookAtTarget()
    {
        Vector3 direction = transform.position - _target.position;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 90f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void MoveToTarget()
    {
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
            Instantiate(_hitVFX, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Enemy") == false)
        {
            Destroy(gameObject);
        }
    }
}
