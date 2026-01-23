using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    [SerializeField] private int health = 100;

    public void TakeDamage(int damage)
    {
        health -= damage;
        UIManager.Instance.RefreshSlider(health);
        if (health <= 0)
        {
            DestroyHouse();
        }
    }

    private void DestroyHouse()
    {
        Destroy(gameObject);
    }
}
