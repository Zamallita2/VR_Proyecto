using TMPro;
using UnityEngine;

public class CatLitterBox : MonoBehaviour
{
    [Header("Estado")]
    [SerializeField] private int remainingUses = 3;
    [SerializeField] private int dirtiness = 0;

    [Header("UI")]
    [SerializeField] private TMP_Text dirtinessText;
    [SerializeField] private TMP_Text usesText;

    [Header("Configuración")]
    [SerializeField] private int maxUses = 3;

    public int RemainingUses => remainingUses;
    public int Dirtiness => dirtiness;

    public bool HasLitter => remainingUses > 0;

    private void Start()
    {
        UpdateUI();
    }

    public void UseLitter()
    {
        if (remainingUses <= 0)
            return;

        remainingUses--;
        dirtiness++;

        UpdateUI();
    }

    public void Clean()
    {
        dirtiness = 0;

        UpdateUI();
    }

    public bool CanBeCleaned()
    {
        return dirtiness > 0;
    }

    public bool AddLitter(ItemData litter)
    {
        if (litter.itemType != ItemData.ItemType.Litter)
            return false;

        if (remainingUses >= maxUses)
        {
            Debug.Log("El arenero ya está lleno.");
            return false;
        }

        remainingUses += litter.quality;

        if (remainingUses > maxUses)
        {
            int wasted = remainingUses - maxUses;

            Debug.Log(
                $"El arenero se llenó. Se desperdiciaron {wasted} usos."
            );

            remainingUses = maxUses;
        }

        UpdateUI();

        return true;
    }

    private void UpdateUI()
    {
        if (usesText != null)
        {
            usesText.text = $"{remainingUses}/{maxUses}";
        }

        if (dirtinessText != null)
        {
            if (dirtiness < 1)
            {
                dirtinessText.text = "Limpio";
            }
            else if (dirtiness <= 2)
            {
                dirtinessText.text = "Usado";
            }
            else
            {
                dirtinessText.text = "Sucio";
            }
        }
    }
}