using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IDamageable
{
    public static PlayerStats instance;
    //public static UIExperienceManager Instance;
    public static event System.Action<int, int> OnExperienceChanged; // Gửi (currentExp, expToNextLevel)
    public static event System.Action<int> OnLevelChanged;
    public static event System.Action<int, int> OnHealthChanged;

    [Header("Base/Progression")]
    [SerializeField] public int level = 1;
    [SerializeField] public int currentExp = 0;
    [SerializeField] public int expToNextLevel = 100;

    [Header("Combat Stats")]
    [SerializeField] public int maxHealth = 40;
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
        OnLevelChanged?.Invoke(level);
        OnExperienceChanged?.Invoke(currentExp, expToNextLevel);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ==== GỌI KHI GIẾT QUÁI / HOÀN THÀNH QUEST ====
    public void GainExp(int amount)
    {
        currentExp += Mathf.Max(0, amount);
        while (currentExp >= expToNextLevel) LevelUp(); // hỗ trợ lên nhiều cấp một lúc
        OnExperienceChanged?.Invoke(currentExp, expToNextLevel);
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
        OnLevelChanged?.Invoke(level);
        OnExperienceChanged?.Invoke(currentExp, expToNextLevel);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

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

    public void TakeDamage(int amount, Vector2 attackPosition)
    {
        // Tái sử dụng logic của hàm TakeDamage(int)
        TakeDamage(amount);

        // (Sau này bạn có thể dùng attackPosition để Player bị văng lùi)
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    void Die()
    {
        GameOverManager gm = FindObjectOfType<GameOverManager>();
        if (gm != null)
        {
            gm.ShowGameOver();
        }
        else
        {
            Debug.LogError("GameOverManager NOT FOUND!");
        }
    }
        public void ResetHealth()
    {
        currentHealth = maxHealth;
    }


}
