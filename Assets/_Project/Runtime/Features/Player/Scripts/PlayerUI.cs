using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerUI : MonoBehaviour
{
    private const float UltimateGaugeMinY = -9f;
    private const float UltimateGaugeMaxY = 0f;

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

    [SerializeField] private Player player;

    [Header("Roots")]
    [SerializeField] private GameObject hpMpRoot;
    [SerializeField] private GameObject consumableSkillRoot;

    [Header("HP / MP")]
    [SerializeField] private Image hpFillImage;
    [SerializeField] private Image mpFillImage;

    [Header("Ultimate Gauge (Move Y -9 ~ 0)")]
    [SerializeField] private Transform ultimateGauge;

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

    [Header("Dice Chance Texts")]
    [SerializeField] private TMP_Text dodgeChanceText;
    [SerializeField] private TMP_Text criticalChanceText;
    [SerializeField] private TMP_Text skillSizeText;

    [Header("Dice Chance Text Colors")]
    [SerializeField] private Color lowDiceChanceTextColor = Color.blue;
    [SerializeField] private Color neutralDiceChanceTextColor = Color.white;
    [SerializeField] private Color highDiceChanceTextColor = Color.red;

    [Header("Dice Chance Text Shake")]
    [SerializeField] private float diceChanceShakeDuration = 0.25f;
    [SerializeField] private float diceChanceShakeStrength = 12f;
    [SerializeField] private int diceChanceShakeVibrato = 20;

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

    private Tween dodgeChanceShakeTween;
    private Tween criticalChanceShakeTween;
    private Tween skillSizeShakeTween;

    private Vector2 dodgeChanceBaseAnchoredPos;
    private Vector2 criticalChanceBaseAnchoredPos;
    private Vector2 skillSizeBaseAnchoredPos;

    private string lastDodgeChanceLabel;
    private string lastCriticalChanceLabel;
    private string lastSkillSizeLabel;

    private int pendingDiceA;
    private int pendingDiceB;

    private PlayerUltimate lastUltimate;

    private bool diceUiVisible;
    private bool battleUiVisible;

    private bool lastPotionAvailable;
    private bool lastDiceAbilityUnlocked;
    private bool lastSkillAbilityUnlocked;

    private void Start()
    {
        player = Player.Instance;

        settings = player.Settings;
        stats = player.Stats;
        combat = player.Combat;
        input = player.Input;

        potionType = PotionType.Hp;
        potionUses = Mathf.Clamp(settings.uiPotionStartUses, 0, settings.uiPotionMaxUses);

        dodgeChanceBaseAnchoredPos = dodgeChanceText.rectTransform.anchoredPosition;
        criticalChanceBaseAnchoredPos = criticalChanceText.rectTransform.anchoredPosition;
        skillSizeBaseAnchoredPos = skillSizeText.rectTransform.anchoredPosition;

        lastPotionAvailable = IsPotionAvailable();
        lastDiceAbilityUnlocked = player.IsDiceAbilityUnlocked;
        lastSkillAbilityUnlocked = player.IsSkillAbilityUnlocked;
        ApplyPotionUnlocked(lastPotionAvailable);

        ConfigureSkillImages();

        stats.BattleChanged += OnBattleChanged;
        stats.DiceRolled += OnDiceRolled;
        stats.DiceSettled += OnDiceSettled;
        stats.DiceGaugeChanged += OnDiceGaugeChanged;

        ApplyBattleUi(stats.IsBattle, true);
        StartDicePanelLoop(stats.IsBattle);

        lastUltimate = combat.EquippedUltimate;
        diceUiVisible = lastUltimate != null && lastUltimate.DiceEnabled;
        ApplyDiceUiVisible(diceUiVisible);
        RebuildDiceModels(lastUltimate);

        ApplySkillVisibility();
        RefreshPotionUi();
        RefreshHpMpUi();
        ApplyDiceValueUi(stats.DiceValue, false);
        ApplyUltimateGauge(stats.DiceGauge, stats.DiceGaugeMax);
    }

    private void OnDestroy()
    {
        stats.BattleChanged -= OnBattleChanged;
        stats.DiceRolled -= OnDiceRolled;
        stats.DiceSettled -= OnDiceSettled;
        stats.DiceGaugeChanged -= OnDiceGaugeChanged;

        KillDiceTweens();
        KillChanceTextTweens();
    }

    private void Update()
    {
        bool potionAvailable = IsPotionAvailable();
        if (potionAvailable != lastPotionAvailable)
        {
            lastPotionAvailable = potionAvailable;
            ApplyPotionUnlocked(lastPotionAvailable);
        }

        bool diceAbilityUnlocked = player.IsDiceAbilityUnlocked;
        if (diceAbilityUnlocked != lastDiceAbilityUnlocked)
        {
            lastDiceAbilityUnlocked = diceAbilityUnlocked;
            ApplyDiceUiVisible(diceUiVisible);
            ApplyDiceValueUi(stats.DiceValue, false);
        }

        bool skillAbilityUnlocked = player.IsSkillAbilityUnlocked;
        if (skillAbilityUnlocked != lastSkillAbilityUnlocked)
        {
            lastSkillAbilityUnlocked = skillAbilityUnlocked;
            ApplySkillVisibility();
        }

        float dt = Time.deltaTime;

        RefreshHpMpUi();

        if (player.IsSitting)
        {
            potionType = PotionType.Hp;

            wasHealHeld = false;
            healHoldElapsed = 0f;
            potionConsumedThisHold = false;
        }
        else
        {
            if (lastPotionAvailable) TickPotionInput(dt);
            else
            {
                wasHealHeld = false;
                healHoldElapsed = 0f;
                potionConsumedThisHold = false;
            }
        }

        RefreshPotionUi();
        RefreshSkillUi();
        TickDicePanelAnimation(dt);
        TickUltimatePotionRestore();

        PlayerUltimate nowUltimate = combat.EquippedUltimate;
        if (nowUltimate != lastUltimate)
        {
            lastUltimate = nowUltimate;

            diceUiVisible = lastUltimate != null && lastUltimate.DiceEnabled;
            ApplyDiceUiVisible(diceUiVisible);

            RebuildDiceModels(nowUltimate);
            ApplyUltimateGauge(stats.DiceGauge, stats.DiceGaugeMax);
        }
    }

    public void RestorePotionToFull()
    {
        potionUses = settings.uiPotionMaxUses;
        RefreshPotionUi();
    }

    public void RefreshAbilityStates()
    {
        if (settings == null || stats == null || combat == null) return;

        lastPotionAvailable = IsPotionAvailable();
        lastDiceAbilityUnlocked = player.IsDiceAbilityUnlocked;
        lastSkillAbilityUnlocked = player.IsSkillAbilityUnlocked;

        ApplyPotionUnlocked(lastPotionAvailable);
        ApplySkillVisibility();
        ApplyDiceUiVisible(diceUiVisible);
        RefreshPotionUi();
        ApplyDiceValueUi(stats.DiceValue, false);
        ApplyUltimateGauge(stats.DiceGauge, stats.DiceGaugeMax);
    }

    private void ApplyPotionUnlocked(bool unlocked)
    {
        potionIconImage.gameObject.SetActive(unlocked);
        potionCountText.gameObject.SetActive(unlocked);

        if (!unlocked)
        {
            potionType = PotionType.Hp;
            potionUses = settings.uiPotionMaxUses;

            wasHealHeld = false;
            healHoldElapsed = 0f;
            potionConsumedThisHold = false;
        }
        else
        {
            if (potionUses <= 0) potionUses = Mathf.Clamp(settings.uiPotionStartUses, 0, settings.uiPotionMaxUses);
            if (potionUses > settings.uiPotionMaxUses) potionUses = settings.uiPotionMaxUses;
        }
    }

    private void OnBattleChanged(bool battle)
    {
        AudioManager.Instance.PlaySFX("LeechOnAndOff");
        ApplyBattleUi(battle, false);
        if (IsDiceVisualVisible()) StartDicePanelTransition(battle);
        else diceInnerImage.gameObject.SetActive(false);
    }

    private void ApplyBattleUi(bool battle, bool immediate)
    {
        battleUiVisible = battle;

        hpMpRoot.SetActive(battle);
        consumableSkillRoot.SetActive(battle);

        ApplySkillVisibility();
        ApplyChanceTextVisibility();

        if (!immediate) return;

        dicePanelState = battle ? DicePanelState.BattleLoop : DicePanelState.IdleLoop;
        frameIndex = 0;
        frameTimer = 0f;
        ApplyPanelFrame(0);
        ApplyInnerVisibility();
    }

    private void ApplyDiceUiVisible(bool visible)
    {
        bool diceVisualVisible = visible && lastDiceAbilityUnlocked;

        diceOuterImage.gameObject.SetActive(visible);

        if (!diceVisualVisible) diceInnerImage.gameObject.SetActive(false);

        lowerDiceRoot.gameObject.SetActive(diceVisualVisible);
        upperDiceRoot.gameObject.SetActive(diceVisualVisible);
        diceSumValueText.gameObject.SetActive(diceVisualVisible);
        ultimateGauge.gameObject.SetActive(visible);

        ApplyChanceTextVisibility();

        if (!diceVisualVisible)
        {
            KillDiceTweens();
            KillChanceTextTweens();
        }
        else
        {
            StartDicePanelLoop(stats.IsBattle);
        }
    }

    private void ApplyChanceTextVisibility()
    {
        bool visible = IsDiceVisualVisible() && battleUiVisible;

        dodgeChanceText.gameObject.SetActive(visible);
        criticalChanceText.gameObject.SetActive(visible);
        skillSizeText.gameObject.SetActive(visible);

        if (!visible) KillChanceTextTweens();
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
        SetMpUiVisible(battleUiVisible && lastSkillAbilityUnlocked);
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
        if (!lastPotionAvailable) return;

        if (potionType == PotionType.Hp) potionType = PotionType.Mp;
        else potionType = PotionType.Hp;
    }

    private void TryConsumePotion()
    {
        if (!lastPotionAvailable) return;

        if (potionUses <= 0) return;
        if (player.IsSitting) return;

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
        if (!lastPotionAvailable) return;

        potionUses += amount;
        if (potionUses > settings.uiPotionMaxUses) potionUses = settings.uiPotionMaxUses;
        if (potionUses < 0) potionUses = 0;
    }

    private void TickUltimatePotionRestore()
    {
        bool ultimateActive = combat.IsUltimateActive;

        if (lastPotionAvailable && ultimateActive && !wasUltimateActive) RestorePotionUse(1);

        wasUltimateActive = ultimateActive;
    }

    private void RefreshPotionUi()
    {
        if (!lastPotionAvailable) return;

        potionIconImage.sprite = potionType == PotionType.Hp ? hpPotionSprite : mpPotionSprite;
        potionCountText.text = potionUses.ToString();
    }

    private void RefreshSkillSprites()
    {
        PlayerSkill skill = combat.EquippedSkill;

        if (!lastSkillAbilityUnlocked || skill == null || !skill.IsEquipped || skill.Icon == null)
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

        if (!lastSkillAbilityUnlocked || skill == null || !skill.IsEquipped || skill.Icon == null)
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
        if (!diceUiVisible) return;

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
        if (!IsDiceVisualVisible())
        {
            diceInnerImage.gameObject.SetActive(false);
            return;
        }

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
        if (!IsDiceVisualVisible()) return;

        pendingDiceA = a;
        pendingDiceB = b;

        if (lowerDiceModel == null || upperDiceModel == null) return;

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

    private void OnDiceSettled(int a, int b) => ApplyDiceValueUi(a + b, true);

    private void StartSpinDecel(Transform t, float duration, ref Tween tween)
    {
        Vector3 delta = new(Random.Range(1080f, 1800f), Random.Range(1080f, 1800f), Random.Range(1080f, 1800f));
        Vector3 end = t.localEulerAngles + delta;

        tween = t.DOLocalRotate(end, duration, RotateMode.FastBeyond360).SetEase(Ease.OutQuad);
    }

    private void StopLowerDiceToValue()
    {
        lowerSpinTween?.Kill();

        Quaternion target = GetDiceFaceRotation(pendingDiceA);
        lowerDiceModel.DOLocalRotateQuaternion(target, settings.uiDiceStopTweenTime).SetEase(Ease.OutQuad);
    }

    private void StopUpperDiceToValue()
    {
        upperSpinTween?.Kill();

        Quaternion target = GetDiceFaceRotation(pendingDiceB);
        upperDiceModel.DOLocalRotateQuaternion(target, settings.uiDiceStopTweenTime).SetEase(Ease.OutQuad);
    }

    private Quaternion GetDiceFaceRotation(int value)
    {
        int idx = Mathf.Clamp(value, 1, 6) - 1;
        return Quaternion.Euler(settings.uiDiceFaceForwardEuler[idx]);
    }

    private void ApplyDiceValueUi(int sum, bool animateChanceTexts)
    {
        ApplyDiceSumText(sum);
        ApplyDiceChanceTexts(sum, animateChanceTexts);
    }

    private void ApplyDiceSumText(int sum) => diceSumValueText.text = sum.ToString();

    private void ApplyDiceChanceTexts(int diceValue, bool animate)
    {
        string dodgeLabel = BuildChanceLabel("회피 확률", "적 회피 확률", diceValue);
        string criticalLabel = BuildChanceLabel("치명타 확률", "적 치명타 확률", diceValue);
        string sizeLabel = BuildSkillSizeLabel(diceValue);
        Color textColor = GetChanceTextColor(diceValue);

        dodgeChanceText.text = dodgeLabel;
        criticalChanceText.text = criticalLabel;
        skillSizeText.text = sizeLabel;

        dodgeChanceText.color = textColor;
        criticalChanceText.color = textColor;
        skillSizeText.color = textColor;

        if (animate && dodgeLabel != lastDodgeChanceLabel && dodgeChanceText.gameObject.activeInHierarchy)
            PlayChanceTextShake(dodgeChanceText.rectTransform, dodgeChanceBaseAnchoredPos, ref dodgeChanceShakeTween);

        if (animate && criticalLabel != lastCriticalChanceLabel && criticalChanceText.gameObject.activeInHierarchy)
            PlayChanceTextShake(criticalChanceText.rectTransform, criticalChanceBaseAnchoredPos, ref criticalChanceShakeTween);

        if (animate && sizeLabel != lastSkillSizeLabel && skillSizeText.gameObject.activeInHierarchy)
            PlayChanceTextShake(skillSizeText.rectTransform, skillSizeBaseAnchoredPos, ref skillSizeShakeTween);

        lastDodgeChanceLabel = dodgeLabel;
        lastCriticalChanceLabel = criticalLabel;
        lastSkillSizeLabel = sizeLabel;
    }

    private string BuildChanceLabel(string playerPrefix, string enemyPrefix, int diceValue)
    {
        float enemyChance = DiceChanceTable.GetEnemyChance(diceValue);
        if (enemyChance > 0f) return $"{enemyPrefix}: {Mathf.RoundToInt(enemyChance * 100f)}%";

        float playerChance = DiceChanceTable.GetPlayerChance(diceValue);
        return $"{playerPrefix}: {Mathf.RoundToInt(playerChance * 100f)}%";
    }

    private string BuildSkillSizeLabel(int diceValue)
    {
        int percent = Mathf.RoundToInt(DiceChanceTable.GetPlayerSkillSize(diceValue) * 100f);
        return $"스킬 크기: {percent}%";
    }

    private Color GetChanceTextColor(int diceValue)
    {
        if (diceValue >= 8 && diceValue <= 12) return highDiceChanceTextColor;
        if (diceValue == 7) return neutralDiceChanceTextColor;
        if (diceValue >= 2 && diceValue <= 6) return lowDiceChanceTextColor;
        return neutralDiceChanceTextColor;
    }

    private void PlayChanceTextShake(RectTransform rectTransform, Vector2 baseAnchoredPos, ref Tween tween)
    {
        tween?.Kill();

        rectTransform.anchoredPosition = baseAnchoredPos;
        tween = rectTransform.DOShakeAnchorPos(diceChanceShakeDuration, diceChanceShakeStrength, diceChanceShakeVibrato, 90f, false, true)
            .SetUpdate(true)
            .OnKill(() => rectTransform.anchoredPosition = baseAnchoredPos);
    }

    private void OnDiceGaugeChanged(float current, float max) => ApplyUltimateGauge(current, max);

    private void ApplyUltimateGauge(float current, float max)
    {
        if (!diceUiVisible) return;

        float ratio = 0f;
        if (max > 0f) ratio = Mathf.Clamp01(current / max);

        Vector3 p = ultimateGauge.localPosition;
        p.y = Mathf.Lerp(UltimateGaugeMinY, UltimateGaugeMaxY, ratio);
        ultimateGauge.localPosition = p;
    }

    private void RebuildDiceModels(PlayerUltimate ultimate)
    {
        KillDiceTweens();

        if (lowerDiceInstance != null) Destroy(lowerDiceInstance);
        if (upperDiceInstance != null) Destroy(upperDiceInstance);

        lowerDiceInstance = null;
        upperDiceInstance = null;

        if (!diceUiVisible)
        {
            lowerDiceModel = null;
            upperDiceModel = null;
            return;
        }

        GameObject prefab = ultimate != null ? ultimate.DicePrefab : null;

        if (prefab != null)
        {
            lowerDiceInstance = Instantiate(prefab, lowerDiceRoot);
            lowerDiceInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            lowerDiceInstance.transform.localScale = Vector3.one;
            SetLayerRecursively(lowerDiceInstance, settings.uiDiceModelLayer);

            upperDiceInstance = Instantiate(prefab, upperDiceRoot);
            upperDiceInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
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

        ApplyDiceValueUi(a + b, false);
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
        diceRollSequence?.Kill();
        lowerSpinTween?.Kill();
        upperSpinTween?.Kill();

        diceRollSequence = null;
        lowerSpinTween = null;
        upperSpinTween = null;
    }

    private void KillChanceTextTweens()
    {
        dodgeChanceShakeTween?.Kill();
        criticalChanceShakeTween?.Kill();
        skillSizeShakeTween?.Kill();

        dodgeChanceShakeTween = null;
        criticalChanceShakeTween = null;
        skillSizeShakeTween = null;

        dodgeChanceText.rectTransform.anchoredPosition = dodgeChanceBaseAnchoredPos;
        criticalChanceText.rectTransform.anchoredPosition = criticalChanceBaseAnchoredPos;
        skillSizeText.rectTransform.anchoredPosition = skillSizeBaseAnchoredPos;
    }

    private bool IsPotionAvailable() => settings.potionUnlocked && player.IsPotionAbilityUnlocked;

    private bool IsDiceVisualVisible() => diceUiVisible && lastDiceAbilityUnlocked;

    private void ApplySkillVisibility()
    {
        SetMpUiVisible(battleUiVisible && lastSkillAbilityUnlocked);

        if (!battleUiVisible || !lastSkillAbilityUnlocked)
        {
            skillBaseGreyImage.enabled = false;
            skillFillImage.enabled = false;
            return;
        }

        RefreshSkillSprites();
    }

    private void SetMpUiVisible(bool visible)
    {
        Transform parent = mpFillImage.transform.parent;
        if (parent != null)
            parent.gameObject.SetActive(visible);
        else
            mpFillImage.gameObject.SetActive(visible);
    }
}