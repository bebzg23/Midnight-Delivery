using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    private void OnEnable()
    {
        RunSession.OnCoinsChanged += UpdateText;
        UpdateText(RunSession.Coins);
    }

    private void OnDisable() => RunSession.OnCoinsChanged -= UpdateText;

    private void UpdateText(int coins) => coinText.text = $"Coins: {coins}";
}