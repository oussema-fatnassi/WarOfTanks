using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _healthFillImage;

    private HealthSystem _healthSystem;
    private void Awake()
    {
        _healthSystem = GetComponentInParent<HealthSystem>();
    }

    private void OnDestroy()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnHealthChanged -= UpdateHealthFillImage;
        }
    }
    void Start()
    {
        _healthSystem.OnHealthChanged += UpdateHealthFillImage;
        _healthFillImage.fillAmount = _healthSystem.HealthPercentage;
    }

    private void UpdateHealthFillImage()
    {
        _healthFillImage.fillAmount = _healthSystem.HealthPercentage;
    }
}
