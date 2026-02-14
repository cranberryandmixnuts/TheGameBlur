using UnityEngine;

[DefaultExecutionOrder(-29900)]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerStats))]
public sealed class Player : Singleton<Player, SceneScope>
{
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

    private void Reset()
    {
        body = GetComponent<Rigidbody>();
        bodyCollider = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        stats = GetComponent<PlayerStats>();
    }
}