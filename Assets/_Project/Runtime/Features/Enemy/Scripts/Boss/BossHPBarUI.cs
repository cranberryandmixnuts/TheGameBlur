using UnityEngine;
using UnityEngine.UI;

public sealed class BossHPBarUI : MonoBehaviour
{
    [Header("HP Fill Image")]
    [SerializeField] private Image hpFillImage;

    [Header("HP바 전체 루트")]
    [SerializeField] private GameObject hpBarRoot;

    public void Show()
    {
        if (hpBarRoot != null)
            hpBarRoot.SetActive(true);
    }

    public void Hide()
    {
        if (hpBarRoot != null)
            hpBarRoot.SetActive(false);
    }

    public void Refresh(int currentHP, int maxHP)
    {
        if (hpFillImage == null) return;

        float ratio = 0f;
        if (maxHP > 0)
            ratio = (float)currentHP / maxHP;

        hpFillImage.fillAmount = Mathf.Clamp01(ratio);
    }
}