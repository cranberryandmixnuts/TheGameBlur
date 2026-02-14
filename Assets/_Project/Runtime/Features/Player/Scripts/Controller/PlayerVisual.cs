using UnityEngine;

public sealed class PlayerVisual : MonoBehaviour
{
    [Header("Visible Objects")]
    [SerializeField] private GameObject swordObject;
    [SerializeField] private GameObject bandObject;

    [Header("Mouse Look At IK (Humanoid)")]
    [SerializeField] private Animator animator;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform zReference;

    [Header("Head Pitch Clamp (Degrees)")]
    [SerializeField] private float headPitchMinDeg = -30f;
    [SerializeField] private float headPitchMaxDeg = 45f;

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

    private Player player;
    private PlayerSettings settings;
    private PlayerStats stats;
    private PlayerMovement movement;
    private PlayerCombat combat;

    private bool lastBattle;
    private Vector3 lookPoint;
    private bool lookActive;

    private Transform headBone;

    private void Start()
    {
        player = Player.Instance;

        settings = player.Settings;
        stats = player.Stats;
        movement = player.Movement;
        combat = player.Combat;

        if (targetCamera == null) targetCamera = Camera.main;
        if (zReference == null) zReference = transform;

        headBone = animator.GetBoneTransform(HumanBodyBones.Head);

        lastBattle = stats.IsBattle;
        ApplyBattleVisibility(lastBattle);
    }

    private void Update()
    {
        bool battle = stats.IsBattle;
        if (battle != lastBattle)
        {
            lastBattle = battle;
            ApplyBattleVisibility(battle);
        }

        lookActive = battle && !movement.IsDashing && !combat.IsUltimateActive && !player.IsSitting;

        if (!lookActive) return;

        float z = zReference.position.z;

        Ray ray = targetCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
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