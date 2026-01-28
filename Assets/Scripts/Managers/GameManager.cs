using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool _isCutscenePlaying = false;
    public static bool _gameFinished = false;
    [SerializeField] private float _difficulty;
    [SerializeField] private float _secondsForMaxLevel = 480f;
    private float _timePlayed;
    private bool _isPaused = true;
    private bool _isStartCutscenePlayed = false;

    private bool _tutorialActivated = false;
    private int _tutorialID = 0;


    [SerializeField] private PlayableDirector _currentDirector;
    [SerializeField] private FirstPersonController _playerControllerScript;
    [SerializeField] private StarterAssetsInputs _starterAssetsInputs;
    

    private void Awake() {
        Instance = this;

        Time.timeScale = 0f;
        _starterAssetsInputs.SetCursorState(false);
    }

    private void Update() {
        if (_isCutscenePlaying) return;
        DifficultyScaler();
    }

    public void PlayCutscene(PlayableDirector director)
    {
        if (director == null) return;
        _currentDirector = director;
        _currentDirector.Play();
        _isCutscenePlaying = true;
    }
    public void EndCutscene()
    {
        _isCutscenePlaying = false;
        _currentDirector = null;
    }

    public void TogglePauseGame()
    {
        if (_tutorialActivated) return;
        if (_isPaused)
        {
            Time.timeScale = 1f;
            UIManager.Instance.ToggleMenu();
            UIManager.Instance.ChangeMenuTextAfterStart();
            _playerControllerScript.TogglePlayerinput(true);
            _starterAssetsInputs.SetCursorState(true);
            _isPaused = false;
            if (!_isStartCutscenePlayed) PlayCutscene(_currentDirector);
        }
        else
        {
            _starterAssetsInputs.SetCursorState(false);
            UIManager.Instance.ToggleMenu();
            Time.timeScale = 0f;
            _isPaused = true;
            _playerControllerScript.TogglePlayerinput(false);
        }
        
    }

    public void ToggleTutorial()
    {
        _tutorialActivated = !_tutorialActivated;
        UIManager.Instance.ToggleTutorial(_tutorialActivated);
    }

    public void TutorialNextSlide()
    {
        _tutorialID++;
        if (_tutorialID > 2) _tutorialID = 0;
        UIManager.Instance.ChangeTutorialSlide(_tutorialID);
    }

    public void TutorialPreviousSlide()
    {
        _tutorialID--;
        if (_tutorialID < 0) _tutorialID = 2;
        UIManager.Instance.ChangeTutorialSlide(_tutorialID);
    }

    public void GameFinished()
    {
        _gameFinished = true;
    }

    public void HideTutorial()
    {
        _tutorialActivated = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        _isCutscenePlaying = false;
        _gameFinished = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SkipCutscene()
    {
        if (_currentDirector == null || !_isCutscenePlaying) return;
        _currentDirector.time = _currentDirector.duration;
        _currentDirector.Evaluate();
        EndCutscene();
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
