using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Dinero")]
    [SerializeField] private int currentMoney = 100;
    [SerializeField] private TMP_Text moneyText;

    [Header("Entrega")]
    [SerializeField] private float deliveryTime = 60f;

    [SerializeField] private Transform deliveryPoint;

    [SerializeField] private LootBox lootBoxPrefab;

    private readonly List<GameObject> pendingItems =
        new();

    private Coroutine deliveryCoroutine;

    private bool deliveryInProgress;

    public int CurrentMoney => currentMoney;
    public event Action OnShopUpdated;

    private void Start()
    {
        UpdateMoneyUI();
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "$" + currentMoney.ToString();
        }
    }

    public bool BuyItem(
        GameObject prefab,
        int price)
    {
        if (currentMoney < price)
            return false;

        currentMoney -= price;

        pendingItems.Add(prefab);

        if (!deliveryInProgress)
        {
            deliveryCoroutine =
                StartCoroutine(
                    DeliveryTimer()
                );
        }
        NotifyShopUpdated();
        return true;
    }

    public void RemoveItem(
        GameObject prefab)
    {
        pendingItems.Remove(prefab);

        if (pendingItems.Count == 0)
        {
            CancelDelivery();
        }
        NotifyShopUpdated();
    }

    private IEnumerator DeliveryTimer()
    {
        deliveryInProgress = true;

        yield return new WaitForSeconds(
            deliveryTime
        );

        SpawnDelivery();

        pendingItems.Clear();

        deliveryInProgress = false;

        NotifyShopUpdated();
    }

    private void SpawnDelivery()
    {
        LootBox box =
            Instantiate(
                lootBoxPrefab,
                deliveryPoint.position,
                deliveryPoint.rotation
            );

        box.SetLoot(
            new List<GameObject>(
                pendingItems
            )
        );
    }

    private void CancelDelivery()
    {
        if (deliveryCoroutine != null)
        {
            StopCoroutine(
                deliveryCoroutine
            );
        }

        deliveryCoroutine = null;

        deliveryInProgress = false;
    }

    public void AddMoney(
        int amount)
    {
        currentMoney += amount;
        NotifyShopUpdated();
    }
    private void NotifyShopUpdated()
    {
        UpdateMoneyUI();
        OnShopUpdated?.Invoke();
    }
    public bool CanAfford(int price)
    {
        return currentMoney >= price;
    }

    public bool HasPendingItem(GameObject prefab)
    {
        return pendingItems.Contains(prefab);
    }

    public bool RemovePendingItem(GameObject prefab, int price)
    {
        bool removed = pendingItems.Remove(prefab);

        if (!removed) return false;

        if (pendingItems.Count == 0)
        {
            CancelDelivery();
        }
        
        AddMoney(price); // This calls NotifyShopUpdated()
        
        return true;
    }
    public int GetPendingCount(
    GameObject prefab)
    {
        int count = 0;

        foreach (GameObject item in pendingItems)
        {
            if (item == prefab)
                count++;
        }

        return count;
    }
}