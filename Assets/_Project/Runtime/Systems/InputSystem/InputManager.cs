using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputManager : Singleton<InputManager, GlobalScope>
{
    [SerializeField] private InputActionAsset actions;

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

    private InputActionRebindingExtensions.RebindingOperation currentRebind;

    private InputAction currentRebindAction;
    private int currentRebindBindingIndex;
    private bool currentRebindExcludeMouse;

    private const string RebindsKey = "InputService_Rebinds";

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

    public bool IsRebinding { get; private set; }

    public InputActionAsset Actions => actions;

    public event Action OnRebindStarted;
    public event Action OnRebindCompleted;
    public event Action OnRebindCanceled;

    protected override void SingletonAwake()
    {
        InitializeActions();
        LoadBindingOverrides();
    }

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
        if (IsRebinding)
        {
            MoveVector = Vector2.zero;

            JumpDown = false;
            JumpUp = false;
            JumpHeld = false;

            DashDown = false;
            RunHeld = false;

            AttackDown = false;
            SkillDown = false;
            DiceSkillDown = false;

            HealDown = false;
            HealHeld = false;

            InteractionDown = false;
            MapDown = false;

            EscapeDown = false;
            return;
        }

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

    private InputAction FindAction(string mapName, string actionName) => actions.FindAction(mapName + "/" + actionName);

    public void SaveBindingOverrides()
    {
        string json = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(RebindsKey, json);
        PlayerPrefs.Save();
    }

    public void LoadBindingOverrides()
    {
        if (!PlayerPrefs.HasKey(RebindsKey)) return;

        string json = PlayerPrefs.GetString(RebindsKey);
        actions.LoadBindingOverridesFromJson(json);
    }

    public void ClearBindingOverrides()
    {
        actions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(RebindsKey);
    }

    public void CancelCurrentRebind()
    {
        if (currentRebind == null) return;
        currentRebind.Cancel();
    }

    public void StartRebind(string mapName, string actionName, int bindingIndex)
    {
        InputAction action = FindAction(mapName, actionName);
        if (action == null) return;

        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return;

        currentRebind?.Cancel();

        IsRebinding = true;

        currentRebindAction = action;
        currentRebindBindingIndex = bindingIndex;
        currentRebindExcludeMouse = false;

        action.Disable();

        OnRebindStarted?.Invoke();

        currentRebind = BuildRebindOperation(action, bindingIndex, currentRebindExcludeMouse);
        currentRebind.Start();
    }

    public void SetCurrentRebindExcludeMouse(bool excludeMouse)
    {
        if (!IsRebinding) return;
        if (currentRebind == null) return;
        if (currentRebindAction == null) return;
        if (currentRebindExcludeMouse == excludeMouse) return;

        currentRebindExcludeMouse = excludeMouse;

        currentRebind.Dispose();
        currentRebind = null;

        currentRebind = BuildRebindOperation(currentRebindAction, currentRebindBindingIndex, currentRebindExcludeMouse);
        currentRebind.Start();
    }

    private InputActionRebindingExtensions.RebindingOperation BuildRebindOperation(InputAction action, int bindingIndex, bool excludeMouse)
    {
        InputActionRebindingExtensions.RebindingOperation op = action.PerformInteractiveRebinding(bindingIndex);

        if (excludeMouse)
            op.WithControlsExcluding("Mouse");

        op.OnMatchWaitForAnother(0.1f);

        return op
            .OnComplete(o => FinishRebind(action))
            .OnCancel(o => CancelRebind(action));
    }

    private void FinishRebind(InputAction action)
    {
        action.Enable();

        currentRebind.Dispose();
        currentRebind = null;

        currentRebindAction = null;

        IsRebinding = false;

        SaveBindingOverrides();
        OnRebindCompleted?.Invoke();
    }

    private void CancelRebind(InputAction action)
    {
        action.Enable();

        currentRebind.Dispose();
        currentRebind = null;

        currentRebindAction = null;

        IsRebinding = false;

        OnRebindCanceled?.Invoke();
    }
}