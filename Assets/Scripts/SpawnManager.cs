using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _minSpawnInterval = 1f;
    [SerializeField] private float _maxSpawnInterval = 10f;
    [SerializeField] private int _maxAliveEnemies = 10;
    [SerializeField] private House _house;
    private float _currentDifficulty;
    private float _interval;

    [SerializeField] private Transform[] targets;

    private float _timer;
    private float _aliveEnemies;
    private void Update() {
        _currentDifficulty = GameManager.Instance.GetDifficulty();
        _interval = Mathf.Lerp(_maxSpawnInterval, _minSpawnInterval, _currentDifficulty);
        _maxAliveEnemies = Mathf.RoundToInt(Mathf.Lerp(5, 25, _currentDifficulty));

        if (_aliveEnemies >= _maxAliveEnemies)
            return;

        _timer += Time.deltaTime;

        if (_timer >= _interval)
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

        UIManager.Instance.UpdateOnMapIndicator("enemy", 1);
        
        enemy.SetTarget(targets);
        enemy.SetHouse(_house);
        enemy.SetHealth(Mathf.RoundToInt(Mathf.Lerp(100, 300, _currentDifficulty)));
        enemy.SetDamage(Mathf.RoundToInt(Mathf.Lerp(10, 25, _currentDifficulty)));
        enemy.OnDeath += HandleEnemyDeath;

        _aliveEnemies++;
    }

    private void HandleEnemyDeath()
    {
        UIManager.Instance.UpdateOnMapIndicator("enemy", -1);
        _aliveEnemies--;
    }

    
}
