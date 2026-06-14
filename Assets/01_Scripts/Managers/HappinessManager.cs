using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class DonationItem
{
    public GameObject prefab;
    [Range(1, 3)]
    public int quality = 1;
}

public class HappinessManager : MonoBehaviour
{
    public static HappinessManager Instance { get; private set; }

    private List<CatAI> activeCats = new List<CatAI>();

    public IReadOnlyList<CatAI> ActiveCats => activeCats;

    [Header("Información")]
    [SerializeField] private float averageHappiness;
    public float AverageHappiness => averageHappiness;

    [Header("UI")]
    [SerializeField] private Image happinessFillImage;
    [SerializeField] private TMP_Text happinessText;
    [SerializeField] private float uiUpdateInterval = 0.5f;
    private float uiUpdateTimer;

    [Header("Donaciones")]
    [SerializeField] private List<DonationItem> possibleDonations;
    [SerializeField] private float minDonationTime = 60f;
    [SerializeField] private float maxDonationTime = 120f;
    [SerializeField] private Transform donationSpawnPoint;
    [SerializeField] private LootBox lootBoxPrefab;
    [SerializeField] private int moneyPerCatDonation = 20;
    
    private float donationTimer;
    private float nextDonationTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ScheduleNextDonation();
    }

    private void Update()
    {
        CalculateHappiness();

        HandleUIUpdate();

        HandleDonations();
    }

    private void CalculateHappiness()
    {
        if (activeCats.Count == 0)
        {
            averageHappiness = 0f;
            return;
        }

        float totalHappiness = 0f;
        foreach (CatAI cat in activeCats)
        {
            totalHappiness += cat.Happiness;
        }
        averageHappiness = totalHappiness / activeCats.Count;
    }

    private void HandleUIUpdate()
    {
        uiUpdateTimer += Time.deltaTime;
        if (uiUpdateTimer >= uiUpdateInterval)
        {
            uiUpdateTimer = 0f;
            
            if (happinessFillImage != null)
            {
                happinessFillImage.fillAmount = averageHappiness / 100f;
            }

            if (happinessText != null)
            {
                happinessText.text = Mathf.RoundToInt(averageHappiness) + "%";
            }
        }
    }

    private void HandleDonations()
    {
        if (activeCats.Count == 0) return;

        donationTimer += Time.deltaTime;
        if (donationTimer >= nextDonationTime)
        {
            AttemptDonation();
            ScheduleNextDonation();
        }
    }

    private void ScheduleNextDonation()
    {
        nextDonationTime = Random.Range(minDonationTime, maxDonationTime);
        donationTimer = 0f;
    }

    private void AttemptDonation()
    {
        // La probabilidad es igual a la felicidad media en porcentaje (0.0 a 1.0)
        float probability = averageHappiness / 100f;
        
        if (Random.value > probability)
        {
            Debug.Log("La donación no llegó esta vez. Felicidad media: " + Mathf.RoundToInt(averageHappiness) + "%");
            return;
        }

        // ── 50/50: donación monetaria o caja física ─────────────────
        bool isMonetaryDonation = (Random.value < 0.5f);
        if (isMonetaryDonation)
        {
            int reward = activeCats.Count * moneyPerCatDonation;
            ShopManager shop = FindAnyObjectByType<ShopManager>();
            if (shop != null) shop.AddMoneyRaw(reward);
            NotificationManager.Instance?.ShowNotification("¡Has recibido $" + reward + " en donaciones!");
            return;
        }

        // ── Donación de objetos físicos ─────────────────────────────
        int minQuality = 1;
        int maxQuality = 1;

        if (averageHappiness >= 80f)
        {
            minQuality = 2;
            maxQuality = 3;
        }
        else if (averageHappiness >= 50f)
        {
            minQuality = 1;
            maxQuality = 2;
        }

        List<GameObject> filteredItems = new List<GameObject>();
        foreach (DonationItem item in possibleDonations)
        {
            if (item.quality >= minQuality && item.quality <= maxQuality)
                filteredItems.Add(item.prefab);
        }

        if (filteredItems.Count == 0)
        {
            foreach (DonationItem item in possibleDonations)
                filteredItems.Add(item.prefab);
        }

        if (filteredItems.Count == 0) return;

        int itemsToDonateCount = activeCats.Count;
        List<GameObject> chosenItems = new List<GameObject>();
        for (int i = 0; i < itemsToDonateCount; i++)
        {
            int randomIndex = Random.Range(0, filteredItems.Count);
            chosenItems.Add(filteredItems[randomIndex]);
        }

        if (lootBoxPrefab != null && donationSpawnPoint != null)
        {
            LootBox box = Instantiate(lootBoxPrefab, donationSpawnPoint.position, donationSpawnPoint.rotation);
            box.SetLoot(chosenItems);
            NotificationManager.Instance?.ShowNotification("¡Has recibido una caja de donación con " + itemsToDonateCount + " objetos!");
        }
    }

    public void RegisterCat(CatAI cat)
    {
        if (!activeCats.Contains(cat))
        {
            activeCats.Add(cat);
        }
    }

    public void UnregisterCat(CatAI cat)
    {
        if (activeCats.Contains(cat))
        {
            activeCats.Remove(cat);
        }
    }
}
