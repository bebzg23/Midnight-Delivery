using System.Text;
using TMPro;
using UnityEngine;

/// Simple HUD: order list + timer + end-of-night panels.
/// Hook the buttons' OnClick to DeliveryManager.BeginCurrentNight (Next Night / Retry).
public class DeliveryUI : MonoBehaviour
{
    [SerializeField] TMP_Text nightTitleText;
    [SerializeField] TMP_Text ordersText;
    [SerializeField] TMP_Text timerText;
    [SerializeField] GameObject completePanel;
    [SerializeField] GameObject failPanel;

    DeliveryManager dm;

    void Start()
    {
        dm = DeliveryManager.Instance;
        dm.onOrdersChanged.AddListener(Refresh);
        dm.onNightStarted.AddListener(OnNightStarted);
        dm.onNightCompleted.AddListener(() => completePanel.SetActive(true));
        dm.onNightFailed.AddListener(() => failPanel.SetActive(true));
        completePanel.SetActive(false);
        failPanel.SetActive(false);
    }

    void OnNightStarted(NightConfig cfg)
    {
        completePanel.SetActive(false);
        failPanel.SetActive(false);
        nightTitleText.text = $"Night {cfg.nightNumber} - {cfg.difficultyLabel}";
    }

    void Update()
    {
        if (dm == null || !dm.NightActive) return;
        int t = Mathf.CeilToInt(dm.TimeRemaining);
        timerText.text = $"{t / 60:00}:{t % 60:00}";
    }

    void Refresh()
    {
        var sb = new StringBuilder();
        foreach (var o in dm.Orders)
        {
            string mark = o.Delivered ? "[x]" : "[ ]";
            sb.AppendLine($"{mark} {o.Item.displayName}  ({o.Shop.shopName} > {o.Customer.npcName})  +{o.Reward}");
        }
        ordersText.text = sb.ToString();
    }
}