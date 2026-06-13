using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Dinero")]
    [SerializeField] private int currentMoney = 100;
    [SerializeField] private TMP_Text moneyText;

    // Multiplicador de ganancias (permite decimales con acumulación)
    private float incomeMultiplier = 1.0f;
    private float fractionalMoney = 0f;

    [Header("Entrega")]
    [SerializeField] private float deliveryTime = 60f;
    [SerializeField] private Transform deliveryPoint;
    [SerializeField] private LootBox lootBoxPrefab;

    private readonly List<GameObject> pendingItems = new();
    private Coroutine deliveryCoroutine;
    private bool deliveryInProgress;

    public int CurrentMoney => currentMoney;
    public event Action OnShopUpdated;

    // ─── Comederos ───────────────────────────────────────────────
    [Header("Comederos")]
    [SerializeField] private List<GameObject> feederObjects;
    [SerializeField] private int feederPrice = 80;
    [SerializeField] private Button feederButton;
    [SerializeField] private TMP_Text feederPriceText;

    // ─── Areneros ─────────────────────────────────────────────────
    [Header("Areneros")]
    [SerializeField] private List<GameObject> litterBoxObjects;
    [SerializeField] private int litterBoxPrice = 100;
    [SerializeField] private Button litterBoxButton;
    [SerializeField] private TMP_Text litterBoxPriceText;

    // ─── Mejora de Ganancias ──────────────────────────────────────
    [Header("Mejora de Ganancias")]
    [SerializeField] private int incomeUpgradeBasePrice = 100;
    [SerializeField] private int incomeUpgradePriceStep = 50;
    private int incomeUpgradeCurrentPrice;
    [SerializeField] private Button incomeUpgradeButton;
    [SerializeField] private TMP_Text incomeUpgradePriceText;

    // ─── Comprar Gato ─────────────────────────────────────────────
    [Header("Adoptar Gato")]
    [SerializeField] private List<GameObject> catPrefabs;
    [SerializeField] private int catPrice = 150;
    [SerializeField] private Button catButton;
    [SerializeField] private TMP_Text catPriceText;

    // ─────────────────────────────────────────────────────────────

    private void Start()
    {
        incomeUpgradeCurrentPrice = incomeUpgradeBasePrice;

        // Asegurarse de que todos los objetos de la lista comiencen desactivados
        foreach (var obj in feederObjects)
            if (obj != null) obj.SetActive(false);
        foreach (var obj in litterBoxObjects)
            if (obj != null) obj.SetActive(false);

        UpdateMoneyUI();
        RefreshAllButtons();
    }

    // ═══════════════════════════════════════════════════════════════
    //  DINERO
    // ═══════════════════════════════════════════════════════════════

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = "$" + currentMoney.ToString();
    }

    /// <summary>
    /// Añade dinero aplicando el multiplicador de ganancias.
    /// Acumula la parte decimal para no perder monedas.
    /// </summary>
    public void AddMoney(int baseAmount)
    {
        float earned = baseAmount * incomeMultiplier + fractionalMoney;
        int whole = Mathf.FloorToInt(earned);
        fractionalMoney = earned - whole;

        currentMoney += whole;
        NotifyShopUpdated();
    }

    /// <summary>
    /// Añade dinero directamente sin multiplicador (ej. recompensa de adopción).
    /// </summary>
    public void AddMoneyRaw(int amount)
    {
        currentMoney += amount;
        NotifyShopUpdated();
    }

    private void NotifyShopUpdated()
    {
        UpdateMoneyUI();
        RefreshAllButtons();
        OnShopUpdated?.Invoke();
    }

    public bool CanAfford(int price) => currentMoney >= price;

    // ═══════════════════════════════════════════════════════════════
    //  SISTEMA DE PEDIDOS (objetos de la tienda normal)
    // ═══════════════════════════════════════════════════════════════

    public bool BuyItem(GameObject prefab, int price)
    {
        if (currentMoney < price) return false;

        currentMoney -= price;
        pendingItems.Add(prefab);

        if (!deliveryInProgress)
            deliveryCoroutine = StartCoroutine(DeliveryTimer());

        SoundManager.Instance?.PlaySFX(SoundManager.Instance.shopBuy);
        NotifyShopUpdated();
        return true;
    }

    public void RemoveItem(GameObject prefab)
    {
        pendingItems.Remove(prefab);
        if (pendingItems.Count == 0) CancelDelivery();
        NotifyShopUpdated();
    }

    public bool HasPendingItem(GameObject prefab) => pendingItems.Contains(prefab);

    public bool RemovePendingItem(GameObject prefab, int price)
    {
        bool removed = pendingItems.Remove(prefab);
        if (!removed) return false;

        if (pendingItems.Count == 0) CancelDelivery();
        SoundManager.Instance?.PlaySFX(SoundManager.Instance.shopCancel);
        AddMoneyRaw(price);
        return true;
    }

    public int GetPendingCount(GameObject prefab)
    {
        int count = 0;
        foreach (GameObject item in pendingItems)
            if (item == prefab) count++;
        return count;
    }

    private IEnumerator DeliveryTimer()
    {
        deliveryInProgress = true;
        yield return new WaitForSeconds(deliveryTime);
        SpawnDelivery();
        pendingItems.Clear();
        deliveryInProgress = false;
        NotifyShopUpdated();
    }

    private void SpawnDelivery()
    {
        LootBox box = Instantiate(lootBoxPrefab, deliveryPoint.position, deliveryPoint.rotation);
        box.SetLoot(new List<GameObject>(pendingItems));
    }

    private void CancelDelivery()
    {
        if (deliveryCoroutine != null) StopCoroutine(deliveryCoroutine);
        deliveryCoroutine = null;
        deliveryInProgress = false;
    }

    // ═══════════════════════════════════════════════════════════════
    //  MEJORAS Y COMPRAS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Activa el siguiente comedero de la lista.</summary>
    public void BuyFeeder()
    {
        if (!TrySpendMoney(feederPrice)) return;
        if (feederObjects.Count == 0) return;

        feederObjects[0].SetActive(true);
        feederObjects.RemoveAt(0);
        SoundManager.Instance?.PlaySFX(SoundManager.Instance.shopBuy);
        NotifyShopUpdated();
    }

    /// <summary>Activa el siguiente arenero de la lista.</summary>
    public void BuyLitterBox()
    {
        if (!TrySpendMoney(litterBoxPrice)) return;
        if (litterBoxObjects.Count == 0) return;

        litterBoxObjects[0].SetActive(true);
        litterBoxObjects.RemoveAt(0);
        SoundManager.Instance?.PlaySFX(SoundManager.Instance.shopBuy);
        NotifyShopUpdated();
    }

    /// <summary>Aumenta el multiplicador de ganancias en 0.5. El precio sube con cada mejora.</summary>
    public void BuyIncomeUpgrade()
    {
        if (!TrySpendMoney(incomeUpgradeCurrentPrice)) return;

        incomeMultiplier += 0.5f;
        incomeUpgradeCurrentPrice += incomeUpgradePriceStep;
        SoundManager.Instance?.PlaySFX(SoundManager.Instance.shopBuy);
        NotifyShopUpdated();
    }

    /// <summary>Instancia un gato aleatorio en el punto de entrega.</summary>
    public void BuyCat()
    {
        if (!TrySpendMoney(catPrice)) return;
        if (catPrefabs.Count == 0) return;

        int index = UnityEngine.Random.Range(0, catPrefabs.Count);
        Instantiate(catPrefabs[index], deliveryPoint.position, deliveryPoint.rotation);
        SoundManager.Instance?.PlaySFX(SoundManager.Instance.shopBuy);
        NotifyShopUpdated();
    }

    private bool TrySpendMoney(int amount)
    {
        if (currentMoney < amount) return false;
        currentMoney -= amount;
        return true;
    }

    // ═══════════════════════════════════════════════════════════════
    //  ACTUALIZACIÓN DE BOTONES
    // ═══════════════════════════════════════════════════════════════

    private void RefreshAllButtons()
    {
        RefreshUnlockButton(feederButton, feederPriceText, feederPrice, feederObjects.Count);
        RefreshUnlockButton(litterBoxButton, litterBoxPriceText, litterBoxPrice, litterBoxObjects.Count);
        RefreshUpgradeButton(incomeUpgradeButton, incomeUpgradePriceText, incomeUpgradeCurrentPrice);
        RefreshUpgradeButton(catButton, catPriceText, catPrice);
    }

    /// <summary>Para botones de desbloqueo (comederos/areneros) que se pueden agotar.</summary>
    private void RefreshUnlockButton(Button btn, TMP_Text label, int price, int remaining)
    {
        if (btn == null) return;

        if (remaining <= 0)
        {
            btn.interactable = false;
            if (label != null) label.text = "Agotado";
        }
        else
        {
            bool canAfford = CanAfford(price);
            btn.interactable = canAfford;
            if (label != null) label.text = "$" + price;
            SetButtonColor(btn, canAfford);
        }
    }

    /// <summary>Para botones de mejora sin límite de stock.</summary>
    private void RefreshUpgradeButton(Button btn, TMP_Text label, int price)
    {
        if (btn == null) return;
        bool canAfford = CanAfford(price);
        btn.interactable = canAfford;
        if (label != null) label.text = "$" + price;
        SetButtonColor(btn, canAfford);
    }

    private void SetButtonColor(Button button, bool enabled)
    {
        Image image = button.GetComponent<Image>();
        if (image == null) return;
        image.color = enabled ? Color.white : Color.gray;
    }
}