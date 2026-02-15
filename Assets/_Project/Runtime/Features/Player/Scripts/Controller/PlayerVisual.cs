using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerVisual : MonoBehaviour
{
    [Header("Visible Objects")]
    [SerializeField] private GameObject swordObject;
    [SerializeField] private GameObject bandObject;

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private float crossFadeTime = 0.08f;

    [Header("Model Original Root (Reset Local Position on Stand Up)")]
    [SerializeField] private Transform modelOriginalRoot;

    [Header("Mouse Look At IK (Humanoid)")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform zReference;

    [Header("Head Pitch Clamp (Degrees)")]
    [SerializeField] private float headPitchMinDeg = -30f;
    [SerializeField] private float headPitchMaxDeg = 45f;

    [Header("Jump 판정 (vy 기준)")]
    [SerializeField] private float jumpVyThreshold = 0.05f;

    [Range(0f, 1f)]
    [SerializeField] private float weight = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float bodyWeight = 0f;
    [Range(0f, 1f)]
    [SerializeField] private float headWeight = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float eyesWeight = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float clampWeight = 0.6f;

    private static readonly int Idle_NoWeapon = Animator.StringToHash("Idle_NoWeapon");
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Walk_NoWeapon = Animator.StringToHash("Walk_NoWeapon");
    private static readonly int Walk = Animator.StringToHash("Walk");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int BackStep = Animator.StringToHash("BackStep");
    private static readonly int Fall = Animator.StringToHash("Fall");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int AirAttack = Animator.StringToHash("AirAttack");
    private static readonly int Technology = Animator.StringToHash("Technology");
    private static readonly int SittingChair = Animator.StringToHash("SittingChair");
    private static readonly int ChairIdle = Animator.StringToHash("ChairIdle");
    private static readonly int EscapeFromChair = Animator.StringToHash("EscapeFromChair");

    private Player player;
    private PlayerSettings settings;
    private PlayerStats stats;
    private PlayerMovement movement;
    private PlayerCombat combat;

    private bool lastBattle;

    private Vector3 lookPoint;
    private bool lookActive;

    private Transform headBone;

    private bool actionOverrideActive;
    private int actionOverrideHash;

    private int lastPlayedHash;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        if (animator != null) modelOriginalRoot = animator.transform;
    }

    private void Start()
    {
        player = Player.Instance;

        settings = player.Settings;
        stats = player.Stats;
        movement = player.Movement;
        combat = player.Combat;

        if (animator == null) animator = player.Animator;
        if (targetCamera == null) targetCamera = Camera.main;
        if (zReference == null) zReference = transform;

        headBone = animator.GetBoneTransform(HumanBodyBones.Head);

        if (modelOriginalRoot == null) modelOriginalRoot = animator.transform;

        lastBattle = stats.IsBattle;
        ApplyBattleVisibility(lastBattle);

        combat.AnimationRequested += OnCombatAnimationRequested;
    }

    private void OnDestroy()
    {
        if (combat != null) combat.AnimationRequested -= OnCombatAnimationRequested;
    }

    private void Update()
    {
        bool battle = stats.IsBattle;
        if (battle != lastBattle)
        {
            lastBattle = battle;
            ApplyBattleVisibility(battle);
        }

        UpdateLookIk();
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (player.IsSitting)
        {
            actionOverrideActive = false;
            UpdateChairAnimation();
            return;
        }

        if (movement.IsDashing || combat.IsUltimateActive) return;

        if (actionOverrideActive && actionOverrideHash == AirAttack && movement.IsGrounded)
        {
            actionOverrideActive = false;
            lastPlayedHash = 0;
        }

        if (actionOverrideActive)
        {
            if (!IsCurrentState(actionOverrideHash)) return;
            if (!IsCurrentStateFinished()) return;

            actionOverrideActive = false;
        }

        int desired = DetermineLocomotionState();
        PlayIfNeeded(desired);
    }

    private void UpdateChairAnimation()
    {
        Player.ChairState cs = player.CurrentChairState;

        if (cs == Player.ChairState.SittingDown)
        {
            PlayIfNeeded(SittingChair);

            if (IsCurrentState(SittingChair) && IsCurrentStateFinished())
            {
                player.NotifyChairSittingAnimationCompleted();
                PlayIfNeeded(ChairIdle);
            }

            return;
        }

        if (cs == Player.ChairState.Idle)
        {
            PlayIfNeeded(ChairIdle);
            return;
        }

        if (cs == Player.ChairState.StandingUp)
        {
            PlayIfNeeded(EscapeFromChair);

            if (IsCurrentState(EscapeFromChair) && IsCurrentStateFinished())
            {
                ResetModelOriginalLocalPosition();
                player.NotifyChairStandingAnimationCompleted();
            }

            return;
        }
    }

    private void ResetModelOriginalLocalPosition()
    {
        modelOriginalRoot.localPosition = Vector3.zero;
    }

    private int DetermineLocomotionState()
    {
        bool grounded = movement.IsGrounded;

        if (!grounded)
        {
            float vy = player.Body.linearVelocity.y;

            if (vy > jumpVyThreshold) return Jump;

            return Fall;
        }

        int m = movement.MoveSign;

        if (m == 0)
            return stats.IsBattle ? Idle : Idle_NoWeapon;

        bool running = movement.RunHeld;

        if (running) return Run;

        if (stats.IsBattle && m != movement.FacingSign) return BackStep;

        return stats.IsBattle ? Walk : Walk_NoWeapon;
    }

    private void OnCombatAnimationRequested(PlayerCombat.AnimRequest req)
    {
        if (player.IsSitting) return;

        if (req == PlayerCombat.AnimRequest.Attack) StartActionOverride(Attack);
        else if (req == PlayerCombat.AnimRequest.AirAttack) StartActionOverride(AirAttack);
        else StartActionOverride(Technology);
    }

    private void StartActionOverride(int stateHash)
    {
        actionOverrideActive = true;
        actionOverrideHash = stateHash;

        lastPlayedHash = 0;
        PlayIfNeeded(stateHash);
    }

    private void PlayIfNeeded(int stateHash)
    {
        if (lastPlayedHash == stateHash) return;

        animator.CrossFade(stateHash, crossFadeTime);
        lastPlayedHash = stateHash;
    }

    private bool IsCurrentState(int stateHash)
    {
        AnimatorStateInfo s = animator.GetCurrentAnimatorStateInfo(0);
        return s.shortNameHash == stateHash;
    }

    private bool IsCurrentStateFinished()
    {
        AnimatorStateInfo s = animator.GetCurrentAnimatorStateInfo(0);
        return s.normalizedTime >= 1f;
    }

    private void UpdateLookIk()
    {
        lookActive = stats.IsBattle && !movement.IsDashing && !combat.IsUltimateActive && !player.IsSitting && stats.IsActive && !movement.RunHeld;

        if (!lookActive) return;

        float z = zReference.position.z;

        Ray ray = targetCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, z));

        if (plane.Raycast(ray, out float enter))
            lookPoint = ray.GetPoint(enter);
        else
            lookPoint = new Vector3(transform.position.x, transform.position.y, z);

        ClampLookPointPitch();
    }

    private void ClampLookPointPitch()
    {
        Vector3 headPos = headBone.position;
        headPos.z = lookPoint.z;

        float dx = lookPoint.x - headPos.x;
        float dy = lookPoint.y - headPos.y;

        float forward = dx * movement.FacingSign;
        if (forward < 0.0001f) forward = 0.0001f;

        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        if (dist < 0.0001f) dist = 0.0001f;

        float pitchDeg = Mathf.Atan2(dy, forward) * Mathf.Rad2Deg;
        pitchDeg = Mathf.Clamp(pitchDeg, headPitchMinDeg, headPitchMaxDeg);

        float pitchRad = pitchDeg * Mathf.Deg2Rad;

        float newForward = Mathf.Cos(pitchRad) * dist;
        float newDy = Mathf.Sin(pitchRad) * dist;

        float newDx = newForward * movement.FacingSign;

        lookPoint = new Vector3(headPos.x + newDx, headPos.y + newDy, lookPoint.z);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (lookActive)
        {
            animator.SetLookAtWeight(weight, bodyWeight, headWeight, eyesWeight, clampWeight);
            animator.SetLookAtPosition(lookPoint);
        }
        else
        {
            animator.SetLookAtWeight(0f, 0f, 0f, 0f, 0f);
        }
    }

    private void ApplyBattleVisibility(bool battle)
    {
        if (swordObject != null) swordObject.SetActive(battle);
        if (bandObject != null) bandObject.SetActive(!battle);
    }
}