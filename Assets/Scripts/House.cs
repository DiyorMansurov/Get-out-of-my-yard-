using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    [SerializeField] private int _health = 100;
    [SerializeField] private GameObject _explosionVFX;
    [SerializeField] private AudioSource _explosionSFX;
    
    public void TakeDamage(int damage)
    {
        _health -= damage;
        UIManager.Instance.RefreshSlider(_health);
        UIManager.Instance.HouseAnim();
        if (_health <= 0)
        {
            DestroyHouse();
        }
    }

    private void DestroyHouse()
    {
        Instantiate(_explosionVFX, transform.position, Quaternion.identity);
        _explosionSFX.Play();
        UIManager.Instance.LoseSequence();
        Destroy(transform.parent.gameObject);
    }
}
