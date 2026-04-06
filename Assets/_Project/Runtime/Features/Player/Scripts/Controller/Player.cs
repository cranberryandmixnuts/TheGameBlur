using System;
using UnityEngine;

[DefaultExecutionOrder(-29900)]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerStats))]
public sealed class Player : Singleton<Player, SceneScope>
{
    public enum ChairState
    {
        None,
        SittingDown,
        Idle,
        StandingUp
    }

    [Header("Settings")]
    [SerializeField] private PlayerSettings settings;

    [Header("Core Components")]
    [SerializeField] private Rigidbody body;
    [SerializeField] private Collider bodyCollider;
    [SerializeField] private Animator animator;

    [Header("Modules")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlayerUI ui;

    [Header("Ability Unlock")]
    [SerializeField] private bool diceAbilityUnlocked = true;
    [SerializeField] private bool skillAbilityUnlocked = true;
    [SerializeField] private bool potionAbilityUnlocked = true;

    [Header("BettleModeCheck")]
    [SerializeField] private float battleHoldDuration = 1f;
    [SerializeField] private BoxCollider CheckCollider;
    [SerializeField] private LayerMask EnemyLayer;
    public bool UseForceBattle = false;
    public bool ForceBattleMode = false;

    private readonly Collider[] results = new Collider[16];

    public PlayerSettings Settings => settings;
    public Rigidbody Body => body;
    public Collider BodyCollider => bodyCollider;
    public Animator Animator => animator;
    public PlayerMovement Movement => movement;
    public PlayerCombat Combat => combat;
    public PlayerStats Stats => stats;
    public PlayerUI UI => ui;
    public InputManager Input => InputManager.Instance;

    public bool IsDiceAbilityUnlocked => diceAbilityUnlocked;
    public bool IsSkillAbilityUnlocked => skillAbilityUnlocked;
    public bool IsPotionAbilityUnlocked => potionAbilityUnlocked;

    public bool IsSitting => chairState != ChairState.None;
    public ChairState CurrentChairState => chairState;

    public Action OnPlayerStandUp;

    private float battleHoldTimer;
    private ElectricChair currentChair;
    private ChairState chairState = ChairState.None;

    private void Reset()
    {
        body = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        stats = GetComponent<PlayerStats>();

        RefreshAbilityStates();
    }

    private void Update()
    {
        bool setBattleMode = false;

        setBattleMode |= combat.IsAnyActionActive;

        Vector3 center = CheckCollider.transform.TransformPoint(CheckCollider.center);
        Vector3 halfExtents = Vector3.Scale(CheckCollider.size * 0.5f, CheckCollider.transform.lossyScale);

        int count = Physics.OverlapBoxNonAlloc(
            center,
            halfExtents,
            results,
            CheckCollider.transform.rotation,
            EnemyLayer,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            Collider hit = results[i];

            if (hit == gameObject) continue;

            if (hit.GetComponent(typeof(IDamageable)) != null || hit.GetComponentInParent(typeof(IDamageable)) != null)
            {
                setBattleMode = true;
                break;
            }
        }

        if (IsSitting)
        {
            battleHoldTimer = 0f;
            setBattleMode = false;
        }
        else if (setBattleMode)
        {
            battleHoldTimer = battleHoldDuration;
        }
        else if (battleHoldTimer > 0f)
        {
            battleHoldTimer -= Time.deltaTime;
            setBattleMode = true;
        }

        if (UseForceBattle) setBattleMode = ForceBattleMode;

        stats.SetBattle(setBattleMode);
    }

    public void SetDiceAbilityUnlocked(bool unlocked)
    {
        if (diceAbilityUnlocked == unlocked) return;

        diceAbilityUnlocked = unlocked;
        RefreshAbilityStates();
    }

    public void SetSkillAbilityUnlocked(bool unlocked)
    {
        if (skillAbilityUnlocked == unlocked) return;

        skillAbilityUnlocked = unlocked;
        RefreshAbilityStates();
    }

    public void SetPotionAbilityUnlocked(bool unlocked)
    {
        if (potionAbilityUnlocked == unlocked) return;

        potionAbilityUnlocked = unlocked;
        RefreshAbilityStates();
    }

    public void Sit(ElectricChair chair, Vector3 seatWorldPosition)
    {
        if (IsSitting) return;

        currentChair = chair;
        chairState = ChairState.SittingDown;

        Vector3 p = seatWorldPosition;
        p.z = settings.planeZ;

        body.position = p;
        body.linearVelocity = Vector3.zero;

        movement.EnterSitting();
        combat.ResetSkillCooldown();
        stats.RestoreHpMpToFull();
        ui.RestorePotionToFull();
    }

    public void RequestStandUpFromChair()
    {
        if (!IsSitting) return;
        if (chairState != ChairState.Idle) return;

        chairState = ChairState.StandingUp;
        movement.BeginStandingUpFromChair();
    }

    public void NotifyChairSittingAnimationCompleted()
    {
        if (chairState == ChairState.SittingDown) chairState = ChairState.Idle;
    }

    public void NotifyChairStandingAnimationCompleted()
    {
        if (chairState != ChairState.StandingUp) return;

        chairState = ChairState.None;
        currentChair = null;
        movement.ExitSitting();
        OnPlayerStandUp?.Invoke();
    }

    private void RefreshAbilityStates()
    {
        stats.RefreshAbilityStates();
        ui.RefreshAbilityStates();
    }

    public void FreezePlayer()
    {

    }

    public void UnFreezePlayer()
    {

    }
}