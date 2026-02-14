using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerUI : MonoBehaviour
{
    private enum DicePanelState
    {
        IdleLoop,
        BattleLoop,
        Entering,
        Exiting
    }

    private enum PotionType
    {
        Hp,
        Mp
    }

    [Header("Roots")]
    [SerializeField] private GameObject hpMpRoot;
    [SerializeField] private GameObject consumableSkillRoot;

    [Header("HP / MP")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Image mpFillImage;

    [Header("Consumable (Potion)")]
    [SerializeField] private Image potionIconImage;
    [SerializeField] private TMP_Text potionCountText;
    [SerializeField] private Sprite hpPotionSprite;
    [SerializeField] private Sprite mpPotionSprite;

    [Header("Skill")]
    [SerializeField] private Image skillBaseGreyImage;
    [SerializeField] private Image skillFillImage;

    [Header("Dice Panel Images")]
    [SerializeField] private Image diceOuterImage;
    [SerializeField] private Image diceInnerImage;

    [Header("Dice Panel Frames - Outer")]
    [SerializeField] private Sprite[] diceOuterEnterFrames = new Sprite[5];
    [SerializeField] private Sprite[] diceOuterBattleLoopFrames = new Sprite[5];
    [SerializeField] private Sprite[] diceOuterIdleLoopFrames = new Sprite[5];

    [Header("Dice Panel Frames - Inner")]
    [SerializeField] private Sprite[] diceInnerEnterFrames = new Sprite[5];
    [SerializeField] private Sprite[] diceInnerBattleLoopFrames = new Sprite[5];

    [Header("Dice 3D Roots (Parents Only)")]
    [SerializeField] private Transform lowerDiceRoot;
    [SerializeField] private Transform upperDiceRoot;

    [Header("Dice Value Text")]
    [SerializeField] private TMP_Text diceSumValueText;

    private PlayerSettings settings;
    private PlayerStats stats;
    private PlayerCombat combat;
    private InputManager input;

    private DicePanelState dicePanelState;
    private int frameIndex;
    private float frameTimer;

    private PotionType potionType;
    private int potionUses;
    private bool wasHealHeld;
    private float healHoldElapsed;
    private bool potionConsumedThisHold;

    private bool wasUltimateActive;

    private GameObject lowerDiceInstance;
    private GameObject upperDiceInstance;
    private Transform lowerDiceModel;
    private Transform upperDiceModel;

    private Tween lowerSpinTween;
    private Tween upperSpinTween;
    private Sequence diceRollSequence;

    private int pendingDiceA;
    private int pendingDiceB;

    private PlayerUltimate lastUltimate;

    private void Start()
    {
        Player player = Player.Instance;

        settings = player.Settings;
        stats = player.Stats;
        combat = player.Combat;
        input = player.Input;

        potionType = PotionType.Hp;
        potionUses = Mathf.Clamp(settings.uiPotionStartUses, 0, settings.uiPotionMaxUses);

        ConfigureSkillImages();

        stats.BattleChanged += OnBattleChanged;
        stats.DiceRolled += OnDiceRolled;
        stats.DiceSettled += OnDiceSettled;

        ApplyBattleUi(stats.IsBattle, true);
        StartDicePanelLoop(stats.IsBattle);

        lastUltimate = combat.EquippedUltimate;
        RebuildDiceModels(lastUltimate);

        RefreshSkillSprites();
        RefreshPotionUi();
        RefreshHpMpUi();
        ApplyDiceSumText(stats.DiceValue);
    }

    private void OnDestroy()
    {
        stats.BattleChanged -= OnBattleChanged;
        stats.DiceRolled -= OnDiceRolled;
        stats.DiceSettled -= OnDiceSettled;

        KillDiceTweens();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        RefreshHpMpUi();
        TickPotionInput(dt);
        RefreshPotionUi();
        RefreshSkillUi();
        TickDicePanelAnimation(dt);
        TickUltimatePotionRestore();

        PlayerUltimate nowUltimate = combat.EquippedUltimate;
        if (nowUltimate != lastUltimate)
        {
            lastUltimate = nowUltimate;
            RebuildDiceModels(nowUltimate);
        }
    }

    private void OnBattleChanged(bool battle)
    {
        ApplyBattleUi(battle, false);
        StartDicePanelTransition(battle);
    }

    private void ApplyBattleUi(bool battle, bool immediate)
    {
        hpMpRoot.SetActive(battle);
        consumableSkillRoot.SetActive(battle);

        if (!immediate) return;

        dicePanelState = battle ? DicePanelState.BattleLoop : DicePanelState.IdleLoop;
        frameIndex = 0;
        frameTimer = 0f;
        ApplyPanelFrame(0);
        ApplyInnerVisibility();
    }

    private void ConfigureSkillImages()
    {
        skillBaseGreyImage.type = Image.Type.Filled;
        skillBaseGreyImage.fillMethod = Image.FillMethod.Vertical;
        skillBaseGreyImage.fillOrigin = (int)Image.OriginVertical.Top;
        skillBaseGreyImage.fillClockwise = true;

        skillBaseGreyImage.color = settings.uiSkillGreyColor;
        skillBaseGreyImage.fillAmount = 0f;
    }

    private void RefreshHpMpUi()
    {
        hpFillImage.fillAmount = stats.MaxHp > 0 ? (float)stats.Hp / stats.MaxHp : 0f;
        mpFillImage.fillAmount = stats.MaxMp > 0 ? (float)stats.Mp / stats.MaxMp : 0f;
    }

    private void TickPotionInput(float dt)
    {
        bool held = input.HealHeld;

        if (input.HealDown)
        {
            healHoldElapsed = 0f;
            potionConsumedThisHold = false;
        }

        if (held)
        {
            healHoldElapsed += dt;

            if (!potionConsumedThisHold && healHoldElapsed >= settings.uiPotionHoldTime)
            {
                TryConsumePotion();
                potionConsumedThisHold = true;
            }
        }

        if (!held && wasHealHeld)
        {
            if (!potionConsumedThisHold) TogglePotionType();
        }

        wasHealHeld = held;
    }

    private void TogglePotionType()
    {
        if (potionType == PotionType.Hp) potionType = PotionType.Mp;
        else potionType = PotionType.Hp;
    }

    private void TryConsumePotion()
    {
        if (potionUses <= 0) return;

        if (potionType == PotionType.Hp)
        {
            if (stats.Hp >= stats.MaxHp) return;

            stats.Heal(settings.uiHpPotionHealAmount);
            potionUses--;
            return;
        }

        if (stats.Mp >= stats.MaxMp) return;

        stats.GainMana(settings.uiMpPotionHealAmount);
        potionUses--;
    }

    private void RestorePotionUse(int amount)
    {
        potionUses += amount;
        if (potionUses > settings.uiPotionMaxUses) potionUses = settings.uiPotionMaxUses;
        if (potionUses < 0) potionUses = 0;
    }

    private void TickUltimatePotionRestore()
    {
        bool ultimateActive = combat.IsUltimateActive;

        if (ultimateActive && !wasUltimateActive) RestorePotionUse(1);

        wasUltimateActive = ultimateActive;
    }

    private void RefreshPotionUi()
    {
        potionIconImage.sprite = potionType == PotionType.Hp ? hpPotionSprite : mpPotionSprite;
        potionCountText.text = potionUses.ToString();
    }

    private void RefreshSkillSprites()
    {
        PlayerSkill skill = combat.EquippedSkill;

        if (skill == null)
        {
            skillBaseGreyImage.enabled = false;
            skillFillImage.enabled = false;
            return;
        }

        skillBaseGreyImage.enabled = true;
        skillFillImage.enabled = true;

        skillBaseGreyImage.sprite = skill.Icon;
        skillFillImage.sprite = skill.Icon;
    }

    private void RefreshSkillUi()
    {
        PlayerSkill skill = combat.EquippedSkill;

        if (skill == null)
        {
            skillBaseGreyImage.enabled = false;
            skillFillImage.enabled = false;
            return;
        }

        if (!skillBaseGreyImage.enabled || skillBaseGreyImage.sprite != skill.Icon) RefreshSkillSprites();

        if (stats.Mp < skill.ManaCost)
        {
            skillBaseGreyImage.fillAmount = 1f;
            return;
        }

        float duration = combat.SkillCooldownDuration;
        float remaining = combat.SkillCooldownRemaining;

        if (duration <= 0f)
        {
            skillBaseGreyImage.fillAmount = 0f;
            return;
        }

        skillBaseGreyImage.fillAmount = Mathf.Clamp01(remaining / duration);
    }

    private void StartDicePanelTransition(bool battle)
    {
        frameTimer = 0f;

        if (battle)
        {
            dicePanelState = DicePanelState.Entering;
            frameIndex = 0;
            ApplyPanelFrame(frameIndex);
            ApplyInnerVisibility();
            return;
        }

        dicePanelState = DicePanelState.Exiting;
        frameIndex = diceOuterEnterFrames.Length - 1;
        ApplyPanelFrame(frameIndex);
        ApplyInnerVisibility();
    }

    private void StartDicePanelLoop(bool battle)
    {
        dicePanelState = battle ? DicePanelState.BattleLoop : DicePanelState.IdleLoop;
        frameIndex = 0;
        frameTimer = 0f;
        ApplyPanelFrame(frameIndex);
        ApplyInnerVisibility();
    }

    private void TickDicePanelAnimation(float dt)
    {
        frameTimer += dt;
        if (frameTimer < settings.uiDiceFrameTime) return;

        frameTimer -= settings.uiDiceFrameTime;

        if (dicePanelState == DicePanelState.Entering)
        {
            frameIndex++;
            if (frameIndex >= diceOuterEnterFrames.Length)
            {
                StartDicePanelLoop(true);
                return;
            }

            ApplyPanelFrame(frameIndex);
            return;
        }

        if (dicePanelState == DicePanelState.Exiting)
        {
            frameIndex--;
            if (frameIndex < 0)
            {
                StartDicePanelLoop(false);
                return;
            }

            ApplyPanelFrame(frameIndex);
            return;
        }

        int len = GetCurrentOuterFrames().Length;
        frameIndex++;
        if (frameIndex >= len) frameIndex = 0;

        ApplyPanelFrame(frameIndex);
    }

    private void ApplyPanelFrame(int index)
    {
        diceOuterImage.sprite = GetCurrentOuterFrames()[index];

        if (dicePanelState == DicePanelState.Entering || dicePanelState == DicePanelState.Exiting)
            diceInnerImage.sprite = diceInnerEnterFrames[index];
        else if (dicePanelState == DicePanelState.BattleLoop)
            diceInnerImage.sprite = diceInnerBattleLoopFrames[index];
    }

    private void ApplyInnerVisibility()
    {
        if (dicePanelState == DicePanelState.IdleLoop)
        {
            diceInnerImage.gameObject.SetActive(false);
            return;
        }

        diceInnerImage.gameObject.SetActive(true);
    }

    private Sprite[] GetCurrentOuterFrames()
    {
        if (dicePanelState == DicePanelState.Entering || dicePanelState == DicePanelState.Exiting) return diceOuterEnterFrames;
        if (dicePanelState == DicePanelState.BattleLoop) return diceOuterBattleLoopFrames;
        return diceOuterIdleLoopFrames;
    }

    private void OnDiceRolled(int a, int b)
    {
        pendingDiceA = a;
        pendingDiceB = b;

        if (lowerDiceModel == null || upperDiceModel == null)
            return;

        KillDiceTweens();

        float lowerDuration = settings.uiDiceLowerStopDelay;
        float upperDuration = settings.uiDiceLowerStopDelay + settings.uiDiceUpperStopExtraDelay;

        StartSpinDecel(lowerDiceModel, lowerDuration, ref lowerSpinTween);
        StartSpinDecel(upperDiceModel, upperDuration, ref upperSpinTween);

        diceRollSequence = DOTween.Sequence();
        diceRollSequence.AppendInterval(lowerDuration);
        diceRollSequence.AppendCallback(StopLowerDiceToValue);
        diceRollSequence.AppendInterval(settings.uiDiceUpperStopExtraDelay);
        diceRollSequence.AppendCallback(StopUpperDiceToValue);
    }

    private void OnDiceSettled(int a, int b) => ApplyDiceSumText(a + b);

    private void StartSpinDecel(Transform t, float duration, ref Tween tween)
    {
        Vector3 delta = new Vector3(Random.Range(1080f, 1800f), Random.Range(1080f, 1800f), Random.Range(1080f, 1800f));
        Vector3 end = t.localEulerAngles + delta;

        tween = t.DOLocalRotate(end, duration, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);
    }

    private void StopLowerDiceToValue()
    {
        if (lowerDiceModel == null) return;

        if (lowerSpinTween != null) lowerSpinTween.Kill();

        Quaternion target = GetDiceFaceRotation(pendingDiceA);
        lowerDiceModel.DOLocalRotateQuaternion(target, settings.uiDiceStopTweenTime).SetEase(Ease.OutQuad);
    }

    private void StopUpperDiceToValue()
    {
        if (upperDiceModel == null) return;

        if (upperSpinTween != null) upperSpinTween.Kill();

        Quaternion target = GetDiceFaceRotation(pendingDiceB);
        upperDiceModel.DOLocalRotateQuaternion(target, settings.uiDiceStopTweenTime).SetEase(Ease.OutQuad);
    }

    private Quaternion GetDiceFaceRotation(int value)
    {
        int idx = Mathf.Clamp(value, 1, 6) - 1;
        return Quaternion.Euler(settings.uiDiceFaceForwardEuler[idx]);
    }

    private void ApplyDiceSumText(int sum) => diceSumValueText.text = sum.ToString();

    private void RebuildDiceModels(PlayerUltimate ultimate)
    {
        KillDiceTweens();

        if (lowerDiceInstance != null) Destroy(lowerDiceInstance);
        if (upperDiceInstance != null) Destroy(upperDiceInstance);

        lowerDiceInstance = null;
        upperDiceInstance = null;

        GameObject prefab = ultimate != null ? ultimate.DicePrefab : null;

        if (prefab != null)
        {
            lowerDiceInstance = Instantiate(prefab, lowerDiceRoot);
            lowerDiceInstance.transform.localPosition = Vector3.zero;
            lowerDiceInstance.transform.localRotation = Quaternion.identity;
            lowerDiceInstance.transform.localScale = Vector3.one;
            SetLayerRecursively(lowerDiceInstance, settings.uiDiceModelLayer);

            upperDiceInstance = Instantiate(prefab, upperDiceRoot);
            upperDiceInstance.transform.localPosition = Vector3.zero;
            upperDiceInstance.transform.localRotation = Quaternion.identity;
            upperDiceInstance.transform.localScale = Vector3.one;
            SetLayerRecursively(upperDiceInstance, settings.uiDiceModelLayer);

            lowerDiceModel = lowerDiceInstance.transform;
            upperDiceModel = upperDiceInstance.transform;
        }
        else
        {
            lowerDiceModel = lowerDiceRoot.childCount > 0 ? lowerDiceRoot.GetChild(0) : null;
            upperDiceModel = upperDiceRoot.childCount > 0 ? upperDiceRoot.GetChild(0) : null;
        }

        int a = stats.SettledDiceA;
        int b = stats.SettledDiceB;

        if (lowerDiceModel != null) lowerDiceModel.localRotation = GetDiceFaceRotation(a);
        if (upperDiceModel != null) upperDiceModel.localRotation = GetDiceFaceRotation(b);

        ApplyDiceSumText(a + b);
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;

        Transform t = go.transform;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i).gameObject, layer);
    }

    private void KillDiceTweens()
    {
        if (diceRollSequence != null) diceRollSequence.Kill();
        if (lowerSpinTween != null) lowerSpinTween.Kill();
        if (upperSpinTween != null) upperSpinTween.Kill();

        diceRollSequence = null;
        lowerSpinTween = null;
        upperSpinTween = null;
    }
}