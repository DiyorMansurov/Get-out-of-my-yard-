using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private float _secondsToDestroy;
    [SerializeField] private GameObject _hitVFX;
    [SerializeField] private AudioSource _hitSFX;
    private bool _hasHit;


    private void OnCollisionEnter(Collision other) {
        if (_hasHit) return;
        if (other.gameObject.CompareTag("Player")) return;

        if (other.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            enemy.TakeDamage(_damage);
            Instantiate(_hitVFX, transform.position, Quaternion.identity);
            _hitSFX.Play();
        }

        _hasHit = true;

        StartCoroutine(DestroyInTime());
    }

    private IEnumerator DestroyInTime()
    {
        yield return new WaitForSeconds(_secondsToDestroy);

        Destroy(gameObject);
    }

    
}
