using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private float _difficulty;
    [SerializeField] private float _secondsForMaxLevel = 480f;
    private float _timePlayed;

    private void Awake() {
        Instance = this;
    }
    private void Update() {
        DifficultyScaler();
    }

    private void DifficultyScaler()
    {
        _timePlayed += Time.deltaTime;

        _difficulty = Mathf.Clamp01(_timePlayed / _secondsForMaxLevel);
    }

    public float GetDifficulty()
    {
        return _difficulty; 
    }

}
