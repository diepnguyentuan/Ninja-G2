using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats instance;
    public static UIExperienceManager Instance;
    public static event System.Action<int, int> OnHealthChanged;

    [Header("Base/Progression")]
    [SerializeField] public int level = 1;
    [SerializeField] public int currentExp = 0;
    [SerializeField] public int expToNextLevel = 100;

    [Header("Combat Stats")]
    [SerializeField] public int maxHealth = 100;
    [SerializeField] public int currentHealth;
    [SerializeField] public int attackDamage = 10;

    [Header("Growth (per level)")]
    public int healthPerLevel = 20;
    public int attackPerLevel = 5;
    public float expCurveMultiplier = 1.3f; // expToNextLevel *= 1.3 mỗi lần lên

    // ==== Properties an toàn để đọc ở script khác ====
    public int Level => level;
    public int CurrentExp => currentExp;
    public int ExpToNextLevel => expToNextLevel;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int AttackDamage => attackDamage;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        currentHealth = maxHealth;
        UIExperienceManager.Instance?.UpdateLevelUI(level);
        UIExperienceManager.Instance?.UpdateExpUI(currentExp, expToNextLevel);
        UIExperienceManager.Instance?.UpdateHPUI(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ==== GỌI KHI GIẾT QUÁI / HOÀN THÀNH QUEST ====
    public void GainExp(int amount)
    {
        currentExp += Mathf.Max(0, amount);
        while (currentExp >= expToNextLevel) LevelUp(); // hỗ trợ lên nhiều cấp một lúc
        //UIExperienceManager.Instance?.UpdateExpUI(currentExp, expToNextLevel);
    }

    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        level++;

        // tăng chỉ số
        maxHealth += healthPerLevel;
        attackDamage += attackPerLevel;

        // hồi full HP khi lên cấp
        currentHealth = maxHealth;

        // tăng exp yêu cầu cho level kế tiếp
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * expCurveMultiplier);

        // cập nhật UI
        UIExperienceManager.Instance?.UpdateLevelUI(level);
        UIExperienceManager.Instance?.UpdateExpUI(currentExp, expToNextLevel);
        UIExperienceManager.Instance?.UpdateHPUI(currentHealth, maxHealth);

        Debug.Log($"🎉 Level Up → Lv.{level} | MaxHP={maxHealth} | ATK={attackDamage} | NextEXP={expToNextLevel}");
    }

    // ==== Hỗ trợ hệ thống máu của Player ====
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Gọi event để cập nhật UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        // TODO: chết thì xử lý ở đây (respawn, v.v.)
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    void Die()
    {
        Debug.Log("Player đã chết!");
        // Có thể thêm animation, respawn, v.v...
    }
}
