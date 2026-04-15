using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _healthFillImage;

    private HealthSystem _healthSystem;
    // Start is called before the first frame update
    public void Awake()
    {
        _healthSystem = GetComponentInParent<HealthSystem>();
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
