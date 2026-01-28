using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.Playables;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [System.Serializable]
    public struct Crosshair
    {
        public Sprite sprite;
        public CrosshairType type;        
    }

    [SerializeField] private Image _crosshairImage;
    [SerializeField] private Crosshair[] _crosshairs;

    private Dictionary<CrosshairType, Sprite> map;

    [SerializeField] private TMP_Text _moneyText;
    private int _moneyTarget = 0;
    private int _moneyCurrent = 0;
    [SerializeField] private Slider _houseHealthSlider;
    [SerializeField] private Image _houseHealthIcon;
    private Animator _houseHealthIcon_anim;
    private float _healthSliderTarget = 1;

    [SerializeField] private Slider _progressSlider;
    private float _progressTarget = 0;

    [SerializeField] private Image _minigameKey;
    [SerializeField] private TMP_Text _minigameSuccesessText;
    [SerializeField] private GameObject _minigameUI;
    [SerializeField] private Sprite wSprite;
    [SerializeField] private Sprite aSprite;
    [SerializeField] private Sprite sSprite;
    [SerializeField] private Sprite dSprite;

    [SerializeField] private Sprite NormalSprite;
    [SerializeField] private Sprite BuildSprite;
    [SerializeField] private Sprite DestroySprite;
    [SerializeField] private Image _modeIndicatorImage;
    [SerializeField] private TMP_Text _notificationText;
    private Animator _notificationAnim;

    private Dictionary<string, Sprite> keySprites;

    [SerializeField] private TMP_Text _cogsOnMap;
    [SerializeField] private TMP_Text _pearlsOnMap;
    [SerializeField] private TMP_Text _enemiesOnMap;
    private int _cogsOnMapAmount = 0;
    private int _pearlsOnMapAmount = 0;
    private int _enemiesOnMapAmount = 0;

    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private TMP_Text _mainMenuStartText;
    private bool _menuActive = true;
    private bool _textChanged = false;

    [SerializeField] private GameObject _tutorialMenu;
    [SerializeField] private GameObject[] _tutorialSlides;

    [SerializeField] private TMP_Text _pearlsText;
    [SerializeField] private TMP_Text _pearlsAmountGeneratorText;

    [SerializeField] private TMP_Text _restartText;
    [SerializeField] private PlayableDirector _winCutscene;
    [SerializeField] private PlayableDirector _loseCutscene;
    [SerializeField] private Image _winScreen;
    [SerializeField] private Image _loseScreen;


    
    private void Awake() {
        Instance = this;
        map = _crosshairs.ToDictionary(c => c.type, c => c.sprite);
        _houseHealthIcon_anim = _houseHealthIcon.GetComponent<Animator>();
        _notificationAnim = _notificationText.GetComponent<Animator>();
        keySprites = new Dictionary<string, Sprite>
        {
            { "W", wSprite },
            { "A", aSprite },
            { "S", sSprite },
            { "D", dSprite }
        };
    }
    private void Update() {
        HealthSliderAnimation();
        ProgressSliderAnimation();
        MoneyAnimation();

        if (_pearlsAmountGeneratorText != null)
        {
            RotateTowardsPlayer(_pearlsAmountGeneratorText.transform, Camera.main.transform);
        }
        
        
    }

    public void ToggleMenu()
    {
        _menuActive = !_menuActive;
        _mainMenu.SetActive(_menuActive);
    }

    public void ChangeMenuTextAfterStart()
    {
        if (_textChanged) return;
        _mainMenuStartText.text = "Continue";
    }

    public void ToggleTutorial(bool state)
    {
        _tutorialMenu.SetActive(state);
    }

    public void ChangeTutorialSlide(int ID)
    {
        foreach (var slide in _tutorialSlides)
        {
            slide.SetActive(false);
        }
        
        _tutorialSlides[ID].SetActive(true);
    }

    public void RestartTextAnimation()
    {
        StartCoroutine(RestartTextCoroutine());
    }

    private IEnumerator RestartTextCoroutine()
    {
        while (true)
        {
            _restartText.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
            _restartText.gameObject.SetActive(false);
            yield return new WaitForSeconds(1f);
        }
        

    }
    public void WinSequence()
    {
        GameManager.Instance.GameFinished();
        _winScreen.gameObject.SetActive(true);
        _loseScreen.gameObject.SetActive(false);
        GameManager.Instance.PlayCutscene(_winCutscene);
    }

    public void LoseSequence()
    {
        StartCoroutine(LoseAnimCoroutine());
        _winScreen.gameObject.SetActive(false);
        _loseScreen.gameObject.SetActive(true);
    }

    private IEnumerator LoseAnimCoroutine()
    {
        yield return new WaitForSeconds(2f);
        GameManager.Instance.GameFinished();
        GameManager.Instance.PlayCutscene(_loseCutscene);
    }

    public void UpdateModeIndicator(Player.Modes mode)
    {
        switch (mode)
        {
            case Player.Modes.Normal:
                _modeIndicatorImage.sprite = NormalSprite;
                break;
            case Player.Modes.Build:
                _modeIndicatorImage.sprite = BuildSprite;
                break;
            case Player.Modes.Destroy:
                _modeIndicatorImage.sprite = DestroySprite;
                break;
            default:
                break;
        }
    }

    public void UpdateOnMapIndicator(string type, int amount)
    {
        switch (type)
        {
            case "cog": _cogsOnMapAmount += amount;
                _cogsOnMap.text = $"X {_cogsOnMapAmount.ToString()}";
                break;
            case "pearl": _pearlsOnMapAmount += amount;
                _pearlsOnMap.text = $"X {_pearlsOnMapAmount.ToString()}";
                break;
            case "enemy": _enemiesOnMapAmount += amount;
                _enemiesOnMap.text = $"X {_enemiesOnMapAmount.ToString()}";
                break;
            default:break;
        }
    }

    public void CrosshairSet(CrosshairType type)
    {
        _crosshairImage.sprite = map[type];

        if (type != CrosshairType.Default)
        {
            float scale = 150f / _crosshairImage.sprite.rect.width;
            _crosshairImage.rectTransform.sizeDelta = new Vector2(
            _crosshairImage.sprite.rect.width * scale,
            _crosshairImage.sprite.rect.height * scale
            );
        } else
        {
            _crosshairImage.SetNativeSize();
        }
        
    }

    public void NotificationPopUp(string message, Color color)
    {
        _notificationText.text = message;
        _notificationText.color = color;
        _notificationAnim.Play("Notification", 0, 0f);
    }

    public void ToggleMinigameUI(bool state)
    {
        _minigameUI.SetActive(state);
    }

    public void ToggleCrosshair(bool state)
    {
        _crosshairImage.gameObject.SetActive(!state);
    }

    public void SetMinigameKey(string keyType)
    {
        _minigameKey.sprite = keySprites[keyType];
    }
    
    public void RefreshSuccesess(int Current, int Max)
    {
        _minigameSuccesessText.text = $"{(float)Current}/{(float)Max}";
    }

    public void RefreshGeneratorPearls(int amount, int toWin)
    {
        _pearlsAmountGeneratorText.text = $"{amount}/{toWin}";
    }

    private void RotateTowardsPlayer(Transform uiElement, Transform playerTransform)
    {
        if (uiElement == null) return;
  
        Vector3 direction = uiElement.position - playerTransform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            uiElement.rotation = Quaternion.Slerp(uiElement.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    public void RefreshMoneyAmount(int amount)
    {
        _moneyTarget = amount;
    }

    public void RefreshPearlText(int amount)
    {
        _pearlsText.text = $"{amount.ToString()}/5";
        Animator anim = _pearlsText.gameObject.GetComponent<Animator>();
        anim.SetInteger("Amount", amount);
    }

    private void MoneyAnimation()
    {
        if (_moneyCurrent < _moneyTarget)
        {
            _moneyCurrent += Mathf.CeilToInt(100f * Time.deltaTime);

            if (_moneyCurrent > _moneyTarget)
            {
                _moneyCurrent = _moneyTarget;
            }
    
        } else if (_moneyCurrent > _moneyTarget)
        {
            _moneyCurrent -= Mathf.CeilToInt(100f * Time.deltaTime);

            if (_moneyCurrent < _moneyTarget)
            {
                _moneyCurrent = _moneyTarget;
            }
        }

        _moneyText.text = _moneyCurrent.ToString();
    }

    public void RefreshSlider(int health)
    {
        _healthSliderTarget = (float)health/100f;
    }

    private void HealthSliderAnimation()
    {
        _houseHealthSlider.value = Mathf.Lerp(_houseHealthSlider.value,_healthSliderTarget , 5f * Time.deltaTime);
    }

    public void RefreshProgressSlider(int successes, int maxSuccesses)
    {
        _progressTarget = successes;
        _progressSlider.maxValue = (float)maxSuccesses;
    }

    private void ProgressSliderAnimation()
    {
        _progressSlider.value = Mathf.Lerp(_progressSlider.value,_progressTarget , 5f * Time.deltaTime);
    }

    public void HouseAnim()
    {
        _houseHealthIcon_anim.SetTrigger("Damage");
    }
}
