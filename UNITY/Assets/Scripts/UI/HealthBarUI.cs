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
    void Start()
    {
        _healthSystem.OnHealthChanged += UpdateHealthFillImage;
        _healthFillImage.fillAmount = _healthSystem.HealthPercentage;

        _tank.OnTankDied += DisableCanvas;
        _tank.OnTankRespawned += EnableCanvas;
    }

    private void OnDestroy()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnHealthChanged -= UpdateHealthFillImage;
        }
       if (_tank != null)
       {
            _tank.OnTankDied -= DisableCanvas;
            _tank.OnTankRespawned -= EnableCanvas;
        }
    }

    private void UpdateHealthFillImage()
    {
        _healthFillImage.fillAmount = _healthSystem.HealthPercentage;
    }

    private void DisableCanvas()
    {
        _canvas.enabled = false;
    }

    private void EnableCanvas()
    {
        _canvas.enabled = true;
    }
}
