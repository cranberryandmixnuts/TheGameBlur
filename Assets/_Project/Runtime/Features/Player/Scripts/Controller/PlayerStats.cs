using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PlayerStats : MonoBehaviour, IDamageable
{
    public event Action<int, int> DiceRolled;
    public event Action<int, int> DiceSettled;
    public event Action<float, float> DiceGaugeChanged;
    public event Action<int, int> HpChanged;
    public event Action<int, int> MpChanged;
    public event Action<bool> BattleChanged;

    [SerializeField] private bool isActive = true;
    private bool isBattle = false;

    [Header("VFX")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem criticalHitEffect;

    public ParticleSystem HitEffect => hitEffect;
    public ParticleSystem CriticalHitEffect => criticalHitEffect;

    public bool IsActive => isActive;

    public bool IsBattle => isBattle;
    public bool IsInvincible => isInvincible;

    public int DiceA => diceA;
    public int DiceB => diceB;

    public int SettledDiceA => settledDiceA;
    public int SettledDiceB => settledDiceB;

    public int DiceValue => settledDiceA + settledDiceB;

    public float DiceGauge => diceGauge;
    public float DiceGaugeMax => combat.EquippedUltimate != null && combat.EquippedUltimate.DiceEnabled ? combat.UltimateGaugeMax : 0f;

    public int Hp => hp;
    public int MaxHp => maxHp;

    public int Mp => mp;
    public int MaxMp => maxMp;

    private Player player;
    private PlayerSettings settings;
    private PlayerCombat combat;

    private bool isInvincible;
    private bool isInitialized;

    private int hp;
    private int maxHp;

    private int mp;
    private int maxMp;

    private int diceA = 1;
    private int diceB = 1;

    private int settledDiceA = 1;
    private int settledDiceB = 1;

    private int pendingSettledDiceA = 1;
    private int pendingSettledDiceB = 1;

    private float diceGauge;

    private float diceRollRemaining;
    private float diceSettleRemaining;
    private bool lastBattle;

    private bool diceIsFromRandomRoll;

    private PlayerUltimate lastUltimate;
    private bool lastDiceAbilityUnlocked;
    private bool lastSkillAbilityUnlocked;

    private void Awake()
    {
        player = GetComponent<Player>();

        settings = player.Settings;
        combat = player.Combat;
    }

    private void Start()
    {
        maxHp = settings.maxHp;
        hp = maxHp;

        maxMp = settings.maxMp;
        mp = maxMp;

        diceGauge = 0f;

        diceIsFromRandomRoll = false;

        lastBattle = isBattle;

        lastUltimate = combat.EquippedUltimate;
        lastDiceAbilityUnlocked = player.IsDiceAbilityUnlocked;
        lastSkillAbilityUnlocked = player.IsSkillAbilityUnlocked;

        ApplyUltimateDiceMode(lastUltimate);
        ApplySkillAbilityState(lastSkillAbilityUnlocked);
        ApplyDiceAbilityState(lastDiceAbilityUnlocked);

        isInitialized = true;
    }

    private void Update()
    {
        bool diceAbilityUnlocked = player.IsDiceAbilityUnlocked;
        if (diceAbilityUnlocked != lastDiceAbilityUnlocked)
        {
            lastDiceAbilityUnlocked = diceAbilityUnlocked;
            ApplyDiceAbilityState(diceAbilityUnlocked);
        }

        bool skillAbilityUnlocked = player.IsSkillAbilityUnlocked;
        if (skillAbilityUnlocked != lastSkillAbilityUnlocked)
        {
            lastSkillAbilityUnlocked = skillAbilityUnlocked;
            ApplySkillAbilityState(skillAbilityUnlocked);
        }

        if (!lastSkillAbilityUnlocked) RestoreMpToFull();
        if (!lastDiceAbilityUnlocked) ApplyNeutralDiceIfNeeded();

        PlayerUltimate nowUltimate = combat.EquippedUltimate;
        if (nowUltimate != lastUltimate)
        {
            lastUltimate = nowUltimate;
            ApplyUltimateDiceMode(nowUltimate);
        }

        if (isBattle != lastBattle)
        {
            lastBattle = isBattle;
            BattleChanged?.Invoke(isBattle);

            if (isBattle) EnterBattle();
        }

        if (!isBattle) return;
        if (!lastDiceAbilityUnlocked) return;

        PlayerUltimate ultimate = combat.EquippedUltimate;
        if (ultimate == null) return;

        if (!ultimate.DiceEnabled)
        {
            ApplyFixedDiceIfNeeded(ultimate);
            return;
        }

        float dt = Time.deltaTime;

        if (diceSettleRemaining > 0f)
        {
            diceSettleRemaining -= dt;
            if (diceSettleRemaining <= 0f) ApplySettledDice();
            return;
        }

        diceRollRemaining -= dt;
        if (diceRollRemaining > 0f) return;

        RollDiceInternal();
        AddDiceGauge(settings.diceGaugeGainPerRoll);

        diceRollRemaining = UnityEngine.Random.Range(settings.diceRollIntervalMin, settings.diceRollIntervalMax);
    }

    public void RestoreHpMpToFull()
    {
        bool hpChanged = hp != maxHp;
        hp = maxHp;
        if (hpChanged) HpChanged?.Invoke(hp, maxHp);

        bool mpChanged = mp != maxMp;
        mp = maxMp;
        if (mpChanged) MpChanged?.Invoke(mp, maxMp);
    }

    public void RefreshAbilityStates()
    {
        if (!isInitialized) return;

        lastDiceAbilityUnlocked = player.IsDiceAbilityUnlocked;
        lastSkillAbilityUnlocked = player.IsSkillAbilityUnlocked;

        ApplySkillAbilityState(lastSkillAbilityUnlocked);
        ApplyDiceAbilityState(lastDiceAbilityUnlocked);
    }

    public void SetBattle(bool value)
    {
        if (isBattle != value) isBattle = value;
    }

    public void PlayerSetActive(bool value)
    {
        if (isActive != value) isActive = value;
    }

    public void NotifyCombatActivity()
    {
    }

    public void SetInvincible(bool value) => isInvincible = value;

    public void ApplyDamage(DamagePayload payload)
    {
        if (isInvincible) return;

        float dodgeChance = DiceChanceTable.GetPlayerChance(DiceValue);
        if (dodgeChance > 0f && UnityEngine.Random.value < dodgeChance)
        {
            AudioManager.Instance.PlaySFX("Dodge");
            return;
        }

        int before = hp;
        hp -= payload.Amount;
        if (hp <= 0)
        {
            hp = 0;

            Debug.Log("Player died.");
            SceneType currentScene = System.Enum.Parse<SceneType>(SceneManager.GetActiveScene().name);
            SceneController.Instance.LoadScene(currentScene);
        }

        AudioManager.Instance.PlaySFX("TakeDamagePlayerAndMonster");
        hitEffect.Play();

        if (hp != before) HpChanged?.Invoke(hp, maxHp);
    }

    public void Heal(int amount)
    {
        int before = hp;
        hp += amount;
        if (hp > maxHp) hp = maxHp;

        if (hp != before) HpChanged?.Invoke(hp, maxHp);
    }

    public bool TrySpendMana(int amount)
    {
        if (amount <= 0) return true;
        if (mp < amount) return false;

        mp -= amount;
        MpChanged?.Invoke(mp, maxMp);
        return true;
    }

    public void GainMana(int amount)
    {
        int before = mp;
        mp += amount;
        if (mp > maxMp) mp = maxMp;

        if (mp != before) MpChanged?.Invoke(mp, maxMp);
    }

    public void SetDice(int a, int b)
    {
        diceA = Mathf.Clamp(a, 1, 6);
        diceB = Mathf.Clamp(b, 1, 6);

        pendingSettledDiceA = diceA;
        pendingSettledDiceB = diceB;

        settledDiceA = diceA;
        settledDiceB = diceB;

        diceIsFromRandomRoll = true;

        diceSettleRemaining = 0f;

        DiceRolled?.Invoke(diceA, diceB);
        DiceSettled?.Invoke(settledDiceA, settledDiceB);
    }

    public void RollDiceForUltimate()
    {
        if (!player.IsDiceAbilityUnlocked)
        {
            ApplyNeutralDiceIfNeeded();
            return;
        }

        PlayerUltimate ultimate = combat.EquippedUltimate;
        if (ultimate == null) return;
        if (!ultimate.DiceEnabled) return;

        AudioManager.Instance.PlaySFX("DiceRoll");

        if (ultimate.UseFixedUltimateDice)
        {
            SetDice(ultimate.FixedUltimateDiceA, ultimate.FixedUltimateDiceB);
            diceRollRemaining = UnityEngine.Random.Range(settings.diceRollIntervalMin, settings.diceRollIntervalMax);
            return;
        }

        RollDiceInternal();
        diceRollRemaining = UnityEngine.Random.Range(settings.diceRollIntervalMin, settings.diceRollIntervalMax);
    }

    public void AddDiceGauge(float amount)
    {
        float max = DiceGaugeMax;

        float before = diceGauge;
        diceGauge += amount;

        if (diceGauge > max) diceGauge = max;
        if (diceGauge < 0f) diceGauge = 0f;

        if (!Mathf.Approximately(before, diceGauge))
            DiceGaugeChanged?.Invoke(diceGauge, max);
    }

    public bool IsDiceGaugeFull()
    {
        float max = DiceGaugeMax;
        if (max <= 0f) return false;

        return diceGauge >= max - 0.0001f;
    }

    public void ConsumeAllDiceGauge()
    {
        float before = diceGauge;
        diceGauge = 0f;

        if (!Mathf.Approximately(before, diceGauge))
            DiceGaugeChanged?.Invoke(diceGauge, DiceGaugeMax);
    }

    private void EnterBattle()
    {
        diceSettleRemaining = 0f;

        if (!player.IsDiceAbilityUnlocked)
        {
            diceRollRemaining = float.PositiveInfinity;
            ApplyNeutralDiceIfNeeded();
            return;
        }

        PlayerUltimate ultimate = combat.EquippedUltimate;
        if (ultimate == null)
        {
            diceRollRemaining = float.PositiveInfinity;
            return;
        }

        if (!ultimate.DiceEnabled)
        {
            diceRollRemaining = float.PositiveInfinity;
            ApplyFixedDiceIfNeeded(ultimate);
            return;
        }

        if (!diceIsFromRandomRoll)
        {
            RollDiceInternal();
            AddDiceGauge(settings.diceGaugeGainPerRoll);
        }

        diceRollRemaining = UnityEngine.Random.Range(settings.diceRollIntervalMin, settings.diceRollIntervalMax);
    }

    private void ApplyUltimateDiceMode(PlayerUltimate ultimate)
    {
        diceSettleRemaining = 0f;

        if (!player.IsDiceAbilityUnlocked)
        {
            diceRollRemaining = float.PositiveInfinity;
            ApplyNeutralDiceIfNeeded();
            return;
        }

        if (ultimate == null)
        {
            diceRollRemaining = float.PositiveInfinity;
            return;
        }

        if (!ultimate.DiceEnabled)
        {
            if (!Mathf.Approximately(diceGauge, 0f))
            {
                diceGauge = 0f;
                DiceGaugeChanged?.Invoke(diceGauge, DiceGaugeMax);
            }

            diceRollRemaining = float.PositiveInfinity;
            ApplyFixedDiceIfNeeded(ultimate);
            return;
        }

        if (isBattle) EnterBattle();
    }

    private void ApplyFixedDiceIfNeeded(PlayerUltimate ultimate)
    {
        if (!ultimate.UseFixedDice) return;

        int a = Mathf.Clamp(ultimate.FixedDiceA, 1, 6);
        int b = Mathf.Clamp(ultimate.FixedDiceB, 1, 6);

        if (settledDiceA == a && settledDiceB == b && diceA == a && diceB == b) return;

        diceA = a;
        diceB = b;

        pendingSettledDiceA = a;
        pendingSettledDiceB = b;

        settledDiceA = a;
        settledDiceB = b;

        diceIsFromRandomRoll = false;

        DiceSettled?.Invoke(settledDiceA, settledDiceB);
    }

    private void RollDiceInternal()
    {
        diceA = UnityEngine.Random.Range(1, 7);
        diceB = UnityEngine.Random.Range(1, 7);

        diceIsFromRandomRoll = true;

        pendingSettledDiceA = diceA;
        pendingSettledDiceB = diceB;

        DiceRolled?.Invoke(diceA, diceB);

        diceSettleRemaining = GetDiceSettleDelay();
    }

    private float GetDiceSettleDelay()
    {
        float d = settings.uiDiceLowerStopDelay + settings.uiDiceUpperStopExtraDelay + settings.uiDiceStopTweenTime;
        if (d < 0f) d = 0f;
        return d;
    }

    private void ApplySettledDice()
    {
        diceSettleRemaining = 0f;

        settledDiceA = pendingSettledDiceA;
        settledDiceB = pendingSettledDiceB;

        DiceSettled?.Invoke(settledDiceA, settledDiceB);
    }

    private void ApplySkillAbilityState(bool unlocked)
    {
        if (unlocked) return;

        RestoreMpToFull();
    }

    private void ApplyDiceAbilityState(bool unlocked)
    {
        if (!unlocked)
        {
            diceSettleRemaining = 0f;
            diceRollRemaining = float.PositiveInfinity;
            ApplyNeutralDiceIfNeeded();
            return;
        }

        ApplyUltimateDiceMode(lastUltimate);
    }

    private void RestoreMpToFull()
    {
        bool mpChanged = mp != maxMp;
        mp = maxMp;
        if (mpChanged) MpChanged?.Invoke(mp, maxMp);
    }

    private void ApplyNeutralDiceIfNeeded()
    {
        if (settledDiceA == 3 && settledDiceB == 3 && diceA == 3 && diceB == 3 && pendingSettledDiceA == 3 && pendingSettledDiceB == 3) return;

        diceA = 3;
        diceB = 3;
        pendingSettledDiceA = 3;
        pendingSettledDiceB = 3;
        settledDiceA = 3;
        settledDiceB = 3;
        diceIsFromRandomRoll = false;

        DiceSettled?.Invoke(settledDiceA, settledDiceB);
    }
}