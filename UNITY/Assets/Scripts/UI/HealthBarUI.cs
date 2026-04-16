using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _healthFillImage;

    private HealthSystem _healthSystem;
    private Canvas _canvas;
    private Tank _tank;
    private void Awake()
    {
        _tank = GetComponentInParent<Tank>();
        _healthSystem = GetComponentInParent<HealthSystem>();
        _canvas = GetComponentInParent<Canvas>();
    }

    private void OnDestroy()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnHealthChanged -= UpdateHealthFillImage;
        }
       if (_tank != null)
       {
            _tank.OnTankDied -= () => _canvas.enabled = false;
            _tank.OnTankRespawned -= () => _canvas.enabled = true;
        }
    }
    void Start()
    {
        _healthSystem.OnHealthChanged += UpdateHealthFillImage;
        _healthFillImage.fillAmount = _healthSystem.HealthPercentage;

        _tank.OnTankDied += () => _canvas.enabled = false;
        _tank.OnTankRespawned += () => _canvas.enabled = true;
    }

    private void UpdateHealthFillImage()
    {
        _healthFillImage.fillAmount = _healthSystem.HealthPercentage;
    }
}
