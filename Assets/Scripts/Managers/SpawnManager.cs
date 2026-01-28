using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private float _minSpawnInterval = 0.5f;
    [SerializeField] private float _maxSpawnInterval = 12f;
    [SerializeField] private int _maxAliveEnemies = 10;
    [SerializeField] private House _house;
    private readonly List<Enemy> _aliveEnemyList = new();
    private float _currentDifficulty;
    private float _interval;

    [SerializeField] private Transform[] targets;

    private float _timer;
    private void Update() {
        if (GameManager._isCutscenePlaying) return;
        
        _currentDifficulty = GameManager.Instance.GetDifficulty();
        _interval = Mathf.Lerp(_maxSpawnInterval, _minSpawnInterval, _currentDifficulty);
        _maxAliveEnemies = Mathf.RoundToInt(Mathf.Lerp(5, 40, _currentDifficulty));

        if (_aliveEnemyList.Count >= _maxAliveEnemies)
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
        
        enemy.OnDeath += () => HandleEnemyDeath(enemy);

        _aliveEnemyList.Add(enemy);
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        UIManager.Instance.UpdateOnMapIndicator("enemy", -1);

        enemy.OnDeath -= () => HandleEnemyDeath(enemy);
        _aliveEnemyList.Remove(enemy);
    }

    public void KillAllEnemies()
    {
        for (int i = _aliveEnemyList.Count - 1; i >= 0; i--)
        {
            if (_aliveEnemyList[i] != null)
            {
                _aliveEnemyList[i].TakeDamage(999);
            }
        }

        _aliveEnemyList.Clear();
    }
    
}
