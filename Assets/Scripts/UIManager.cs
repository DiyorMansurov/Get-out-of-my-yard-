using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

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

    private Dictionary<string, Sprite> keySprites;
    
    
    private void Awake() {
        Instance = this;
        map = _crosshairs.ToDictionary(c => c.type, c => c.sprite);
        _houseHealthIcon_anim = _houseHealthIcon.GetComponent<Animator>();

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

    public void RefreshMoneyAmount(int amount)
    {
        _moneyTarget = amount;
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
