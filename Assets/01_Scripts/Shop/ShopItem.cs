using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField]
    private ShopManager shopManager;

    [SerializeField]
    private GameObject prefab;

    [Header("Datos")]
    [SerializeField]
    private int price;

    [Header("UI")]
    [SerializeField]
    private TMP_Text priceText;

    [SerializeField]
    private TMP_Text quantityText;

    [SerializeField]
    private Button buyButton;

    [SerializeField]
    private Button cancelButton;

    [Header("Colores")]
    [SerializeField]
    private Color enabledColor = Color.white;

    [SerializeField]
    private Color disabledColor = Color.gray;

    private void Start()
    {
        UpdatePriceText();

        if (shopManager != null)
        {
            shopManager.OnShopUpdated += RefreshUI;
        }

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (shopManager != null)
        {
            shopManager.OnShopUpdated -= RefreshUI;
        }
    }

    public void Buy()
    {
        if (shopManager == null)
            return;

        shopManager.BuyItem(prefab, price);
    }

    public void Cancel()
    {
        if (shopManager == null)
            return;

        shopManager.RemovePendingItem(prefab, price);
    }

    private void RefreshUI()
    {
        RefreshButtons();
        RefreshQuantity();
        UpdatePriceText();
    }

    private void UpdatePriceText()
    {
        if (priceText != null)
        {
            priceText.text = price.ToString();
        }
    }

    private void RefreshQuantity()
    {
        if (quantityText == null ||
            shopManager == null)
        {
            return;
        }

        int count =
            shopManager.GetPendingCount(prefab);

        quantityText.text = ("X"+count.ToString());
    }

    private void RefreshButtons()
    {
        if (shopManager == null)
            return;

        bool canBuy =
            shopManager.CanAfford(price);

        bool canCancel =
            shopManager.HasPendingItem(prefab);

        if (buyButton != null)
        {
            buyButton.interactable = canBuy;

            SetButtonColor(
                buyButton,
                canBuy
            );
        }

        if (cancelButton != null)
        {
            cancelButton.interactable = canCancel;

            SetButtonColor(
                cancelButton,
                canCancel
            );
        }
    }

    private void SetButtonColor(
        Button button,
        bool enabled)
    {
        Image image =
            button.GetComponent<Image>();

        if (image == null)
            return;

        image.color =
            enabled
                ? enabledColor
                : disabledColor;
    }
}