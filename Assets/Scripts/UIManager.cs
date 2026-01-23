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
    [SerializeField] private Slider _houseHealthSlider;

    private void Awake() {
        Instance = this;
        map = _crosshairs.ToDictionary(c => c.type, c => c.sprite);
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

    public void RefreshMoneyAmount(int amount)
    {
        _moneyText.text = amount.ToString();
    }

    public void RefreshSlider(int health)
    {
        _houseHealthSlider.value = (float)health/100f;
    }
}
