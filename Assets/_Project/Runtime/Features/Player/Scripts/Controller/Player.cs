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

    public PlayerSettings Settings => settings;
    public Rigidbody Body => body;
    public Collider BodyCollider => bodyCollider;
    public Animator Animator => animator;
    public PlayerMovement Movement => movement;
    public PlayerCombat Combat => combat;
    public PlayerStats Stats => stats;
    public InputManager Input => InputManager.Instance;

    public bool IsSitting => chairState != ChairState.None;
    public ChairState CurrentChairState => chairState;

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
    }
}