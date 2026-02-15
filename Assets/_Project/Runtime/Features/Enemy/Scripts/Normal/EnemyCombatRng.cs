using System.Collections;
using UnityEngine;

public sealed class EnemyCombatRng : MonoBehaviour
{
    [Header("Evasion Visual")]
    [Range(0f, 1f)] public float evadeAlpha = 0.75f;
    public float evadeAlphaDuration = 0.15f;

    [SerializeField] private SpriteRenderer[] renderers;

    Coroutine alphaRoutine;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    float CurrentChance01
    {
        get
        {
            int dv = 6;
            if (Player.Instance != null && Player.Instance.Stats != null)
                dv = Player.Instance.Stats.DiceValue;

            return DiceChanceTable.GetEnemyChance(dv);
        }
    }

    public int ApplyCritToOutgoingDamage(int baseDamage)
    {
        bool isCrit = Random.value < CurrentChance01;
        return isCrit ? baseDamage * 2 : baseDamage;
    }

    public bool TryEvadeIncomingDamage()
    {
        bool evaded = Random.value < CurrentChance01;
        if (evaded) PlayEvadeAlpha();
        return evaded;
    }

    void PlayEvadeAlpha()
    {
        if (alphaRoutine != null) StopCoroutine(alphaRoutine);
        alphaRoutine = StartCoroutine(EvadeAlphaRoutine());
    }

    IEnumerator EvadeAlphaRoutine()
    {
        SetAlpha(evadeAlpha);
        yield return new WaitForSeconds(evadeAlphaDuration);
        SetAlpha(1f);
        alphaRoutine = null;
    }

    void SetAlpha(float a)
    {
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            var c = renderers[i].color;
            c.a = a;
            renderers[i].color = c;
        }
    }
}
