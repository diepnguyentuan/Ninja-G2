using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIExperienceManager : MonoBehaviour
{
    public static UIExperienceManager Instance;

    [Header("UI References")]
    public TMP_Text levelText;
    public Slider expSlider;
    public Slider hpSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        if (PlayerStats.instance != null)
        {
            PlayerStats.OnHealthChanged += OnHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (PlayerStats.instance != null)
        {
            PlayerStats.OnHealthChanged -= OnHealthChanged;
        }
    }

    private void Start()
    {
        if (PlayerStats.instance != null)
        {
            UpdateLevelUI(PlayerStats.instance.Level);
            UpdateExpUI(PlayerStats.instance.CurrentExp, PlayerStats.instance.ExpToNextLevel);
            UpdateHPUI(PlayerStats.instance.CurrentHealth, PlayerStats.instance.MaxHealth);
        }
    }

    public void UpdateLevelUI(int level)
    {
        if (levelText != null)
            levelText.text = $"Lv. {level}";
    }

    public void UpdateExpUI(int currentExp, int expToNext)
    {
        if (expSlider == null) return;
        if (expToNext <= 0) { expSlider.value = 0; return; }

        expSlider.value = Mathf.Clamp01((float)currentExp / expToNext);
    }

    private void OnHealthChanged(int currentHP, int maxHP)
    {
        UpdateHPUI(currentHP, maxHP);
    }

    public void UpdateHPUI(int currentHP, int maxHP)
    {
        if (hpSlider == null) return;
        if (maxHP <= 0) { hpSlider.value = 0; return; }

        hpSlider.value = Mathf.Clamp01((float)currentHP / maxHP);
    }
}
