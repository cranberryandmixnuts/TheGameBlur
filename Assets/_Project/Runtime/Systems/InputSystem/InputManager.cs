using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputManager : Singleton<InputManager, GlobalScope>
{
    [SerializeField] private InputActionAsset actions;
    public InputActionAsset Actions => actions;

    [Header("Action Map")]
    [SerializeField] private string playerMapName = "Player";
    [SerializeField] private string uiMapName = "UI";

    [Header("Action Names")]
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string runActionName = "Run";
    [SerializeField] private string dashActionName = "Dash";
    [SerializeField] private string jumpActionName = "Jump";
    [SerializeField] private string attackActionName = "Attack";
    [SerializeField] private string skillActionName = "Skill";
    [SerializeField] private string diceSkillActionName = "DiceSkill";
    [SerializeField] private string healActionName = "Heal";
    [SerializeField] private string interactionActionName = "Interaction";
    [SerializeField] private string mapActionName = "Map";
    [SerializeField] private string escapeActionName = "Escape";

    private InputAction moveAction;
    private InputAction runAction;
    private InputAction dashAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    private InputAction skillAction;
    private InputAction diceSkillAction;
    private InputAction healAction;
    private InputAction interactionAction;
    private InputAction mapAction;
    private InputAction escapeAction;

    #region 입력 프로퍼티
    public Vector2 MoveVector { get; private set; }
    public float MoveAxis => MoveVector.x;

    public bool JumpDown { get; private set; }
    public bool JumpUp { get; private set; }
    public bool JumpHeld { get; private set; }

    public bool DashDown { get; private set; }
    public bool RunHeld { get; private set; }

    public bool AttackDown { get; private set; }
    public bool SkillDown { get; private set; }
    public bool DiceSkillDown { get; private set; }

    public bool HealDown { get; private set; }
    public bool HealHeld { get; private set; }

    public bool InteractionDown { get; private set; }
    public bool MapDown { get; private set; }

    public bool EscapeDown { get; private set; }
    #endregion

    private InputAction FindAction(string mapName, string actionName) =>
        actions.FindAction(mapName + "/" + actionName);

    protected override void SingletonAwake() => InitializeActions();

    private void OnEnable()
    {
        if (Instance != this) return;
        actions.Enable();
    }

    private void OnDisable()
    {
        if (Instance != this) return;
        actions.Disable();
    }

    private void Update()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();
        MoveVector = new Vector2(Mathf.Clamp(move.x, -1f, 1f), Mathf.Clamp(move.y, -1f, 1f));

        JumpDown = jumpAction.WasPressedThisFrame();
        JumpUp = jumpAction.WasReleasedThisFrame();
        JumpHeld = jumpAction.IsPressed();

        DashDown = dashAction.WasPressedThisFrame();
        RunHeld = runAction.IsPressed();

        AttackDown = attackAction.WasPressedThisFrame();
        SkillDown = skillAction.WasPressedThisFrame();
        DiceSkillDown = diceSkillAction.WasPressedThisFrame();

        HealDown = healAction.WasPressedThisFrame();
        HealHeld = healAction.IsPressed();

        InteractionDown = interactionAction.WasPressedThisFrame();
        MapDown = mapAction.WasPressedThisFrame();

        EscapeDown = escapeAction.WasPressedThisFrame();
    }

    private void InitializeActions()
    {
        moveAction = FindAction(playerMapName, moveActionName);
        runAction = FindAction(playerMapName, runActionName);
        dashAction = FindAction(playerMapName, dashActionName);
        jumpAction = FindAction(playerMapName, jumpActionName);
        attackAction = FindAction(playerMapName, attackActionName);
        skillAction = FindAction(playerMapName, skillActionName);
        diceSkillAction = FindAction(playerMapName, diceSkillActionName);
        healAction = FindAction(playerMapName, healActionName);
        interactionAction = FindAction(playerMapName, interactionActionName);
        mapAction = FindAction(playerMapName, mapActionName);

        escapeAction = FindAction(uiMapName, escapeActionName);
    }
}