using System;
using UnityEngine;

public sealed class PlayerStats : MonoBehaviour, IDamageable
{
    public event Action<int, int> DiceRolled;
    public event Action<float, float> DiceGaugeChanged;
    public event Action<int, int> HpChanged;
    public event Action<int, int> MpChanged;
    public event Action<bool> BattleChanged;

    [SerializeField] private bool isBattle;

    public bool IsBattle => isBattle;
    public bool IsInvincible => isInvincible;

    public int DiceA => diceA;
    public int DiceB => diceB;
    public int DiceValue => diceA + diceB;

    public float DiceGauge => diceGauge;
    public float DiceGaugeMax => combat.UltimateGaugeMax > 0f ? combat.UltimateGaugeMax : settings.defaultUltimateGaugeMax;

    public int Hp => hp;
    public int MaxHp => maxHp;

    public int Mp => mp;
    public int MaxMp => maxMp;

    private PlayerSettings settings;
    private PlayerCombat combat;

    private bool isInvincible;

    private int hp;
    private int maxHp;

    private int mp;
    private int maxMp;

    private int diceA = 1;
    private int diceB = 1;

    private float diceGauge;

    private float diceRollRemaining;
    private bool lastBattle;

    private void Start()
    {
        settings = Player.Instance.Settings;
        combat = Player.Instance.Combat;

        maxHp = settings.maxHp;
        hp = maxHp;

        maxMp = settings.maxMp;
        mp = maxMp;

        diceGauge = 0f;

        lastBattle = isBattle;
        if (isBattle) EnterBattle();
    }

    private void Update()
    {
        if (isBattle != lastBattle)
        {
            lastBattle = isBattle;
            BattleChanged?.Invoke(isBattle);

            if (isBattle) EnterBattle();
        }

        if (!isBattle) return;

        float dt = Time.deltaTime;

        diceRollRemaining -= dt;
        if (diceRollRemaining > 0f) return;

        RollDice();
        AddDiceGauge(settings.diceGaugeGainPerRoll);

        diceRollRemaining = UnityEngine.Random.Range(settings.diceRollIntervalMin, settings.diceRollIntervalMax);
    }

    public void SetBattle(bool value)
    {
        if (isBattle == value) return;

        isBattle = value;
    }

    public void NotifyCombatActivity()
    {
    }

    public void SetInvincible(bool value) => isInvincible = value;

    public void ApplyDamage(DamagePayload payload)
    {
        if (isInvincible) return;

        int before = hp;
        hp -= payload.Amount;
        if (hp < 0) hp = 0;

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

        DiceRolled?.Invoke(diceA, diceB);
    }

    public void AddDiceGauge(float amount)
    {
        float before = diceGauge;
        diceGauge += amount;

        float max = DiceGaugeMax;
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
        diceRollRemaining = UnityEngine.Random.Range(settings.diceRollIntervalMin, settings.diceRollIntervalMax);
    }

    private void RollDice()
    {
        diceA = UnityEngine.Random.Range(1, 7);
        diceB = UnityEngine.Random.Range(1, 7);
        DiceRolled?.Invoke(diceA, diceB);
    }
}