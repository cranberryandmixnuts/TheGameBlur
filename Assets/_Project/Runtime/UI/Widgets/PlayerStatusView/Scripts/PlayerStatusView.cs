using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusView : MonoBehaviour
{
    public static PlayerStatusView Instance {  get; private set; }

    [SerializeField] private Image healthView;
    [SerializeField] private Image manaView;
    [SerializeField] private float smoothSpeed = 5f;

    private float healthFillAmount = 1f;
    private float manaFillAmount = 1f;

    private void Awake()
    {
        Instance = this;
    }

    public void SetHealthView(float amount)
    {
        healthFillAmount = amount;
    }

    public void SetManaView(float amount)
    {
        manaFillAmount = amount;
    }

    private void Update()
    {
        healthView.fillAmount = Mathf.Lerp(healthView.fillAmount, healthFillAmount, Time.deltaTime * smoothSpeed);
        manaView.fillAmount = Mathf.Lerp(manaView.fillAmount, manaFillAmount, Time.deltaTime * smoothSpeed);
    }
}
