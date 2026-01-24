using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _spawnInterval = 5f;
    [SerializeField] private int _maxAliveEnemies = 10;
    [SerializeField] private House _house;

    [SerializeField] private Transform[] targets;

    private float _timer;
    private float _aliveEnemies;
    private void Update() {
        if (_aliveEnemies >= _maxAliveEnemies)
            return;

        _timer += Time.deltaTime;

        if (_timer >= _spawnInterval)
        {
            SpawnEnemy();
            _timer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        Transform spawn = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
        GameObject enemySpawned = Instantiate(_enemyPrefab, spawn.position, Quaternion.identity);
        Enemy enemy = enemySpawned.GetComponent<Enemy>();
        
        enemy.SetTarget(targets);
        enemy.SetHouse(_house);
        enemy.OnDeath += HandleEnemyDeath;

        _aliveEnemies++;
    }

    private void HandleEnemyDeath()
    {
        _aliveEnemies--;
    }

    
}
