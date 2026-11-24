using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    [Header("Content")]
    [SerializeField] NightConfig[] nights;            // index 0 = Night 1
    [SerializeField] ParcelDefinition[] parcelCatalog;
    [SerializeField] ShopPoint[] shops;
    [SerializeField] NpcCustomer[] npcs;
    [SerializeField] JumpscareDirector jumpscareDirector;

    [Header("Flow")]
    [SerializeField] bool autoStartOnPlay = true;

    [Header("Events")]
    public UnityEvent onOrdersChanged;
    public UnityEvent<NightConfig> onNightStarted;
    public UnityEvent<DeliveryOrder> onOrderDelivered;
    public UnityEvent onNightCompleted;
    public UnityEvent onNightFailed;

    readonly List<DeliveryOrder> orders = new List<DeliveryOrder>();
    public IReadOnlyList<DeliveryOrder> Orders => orders;
    public NightConfig CurrentConfig { get; private set; }
    public float TimeRemaining { get; private set; }
    public bool NightActive { get; private set; }
    int coinsAtNightStart;

    void Awake()
    {
        Instance = this;
        RunSession.EnsureInitialized();
    }

    void Start()
    {
        if (autoStartOnPlay) BeginCurrentNight();
    }

    void Update()
    {
        if (!NightActive) return;
        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f) FailNight();
    }

    /// Starts RunSession.CurrentNight. Use for "Next Night" and "Retry" buttons too.
    public void BeginCurrentNight()
    {
        if (nights == null || nights.Length == 0 || shops.Length == 0 ||
            npcs.Length == 0 || parcelCatalog.Length == 0)
        {
            Debug.LogError("DeliveryManager: assign nights, catalog, shops and npcs.");
            return;
        }

        ClearNight();
        coinsAtNightStart = RunSession.Coins;

        int night = RunSession.CurrentNight;
        CurrentConfig = nights[Mathf.Clamp(night - 1, 0, nights.Length - 1)]; // past last night = repeat last
        var rng = RunSession.RngForNight(night);

        int count = rng.Next(CurrentConfig.minParcels, CurrentConfig.maxParcels + 1);
        count = Mathf.Min(count, npcs.Length);

        // random NPC order: shuffle, then take the first 'count'
        var pool = new List<NpcCustomer>(npcs);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        for (int i = 0; i < count; i++)
        {
            var item = parcelCatalog[rng.Next(parcelCatalog.Length)];
            var order = new DeliveryOrder
            {
                Id = i,
                Customer = pool[i],
                Shop = shops[rng.Next(shops.Length)],
                Item = item,
                Reward = rng.Next(item.minReward, item.maxReward + 1)
            };

            var go = Instantiate(order.Item.prefab, order.Shop.GetSpawnPosition(rng), Quaternion.identity);
            if (!go.TryGetComponent(out Parcel parcel)) parcel = go.AddComponent<Parcel>();
            parcel.Init(order);
            order.Instance = parcel;

            order.Customer.AssignOrder(order);
            orders.Add(order);
        }

        TimeRemaining = CurrentConfig.nightDurationSeconds;
        NightActive = true;

        if (jumpscareDirector) jumpscareDirector.Schedule(CurrentConfig.jumpscares, CurrentConfig.nightDurationSeconds, rng);

        onNightStarted.Invoke(CurrentConfig);
        onOrdersChanged.Invoke();
    }

    public void Deliver(DeliveryOrder order)
    {
        if (!NightActive || order == null || order.Delivered) return;

        order.Delivered = true;
        RunSession.AddCoins(order.Reward);
        order.Customer.ClearOrder();
        if (order.Instance) Destroy(order.Instance.gameObject);

        onOrderDelivered.Invoke(order);
        onOrdersChanged.Invoke();

        if (orders.TrueForAll(o => o.Delivered)) CompleteNight();
    }

    void CompleteNight()
    {
        NightActive = false;
        if (jumpscareDirector) jumpscareDirector.StopAll();
        RunSession.AddCoins(CurrentConfig.completionBonus);
        RunSession.CurrentNight++;
        onNightCompleted.Invoke();
    }

    void FailNight()
    {
        NightActive = false;
        RunSession.SetCoins(coinsAtNightStart); // failed night = no coins (stops retry farming)
        if (jumpscareDirector) jumpscareDirector.StopAll();
        onNightFailed.Invoke();
    }

    void ClearNight()
    {
        foreach (var o in orders)
        {
            o.Customer.ClearOrder();
            if (o.Instance) Destroy(o.Instance.gameObject);
        }
        orders.Clear();
    }
}