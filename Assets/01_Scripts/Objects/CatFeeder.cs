using UnityEngine;

public class CatFeeder : MonoBehaviour
{
    [SerializeField] private bool hasFood;
    [SerializeField] private int foodQuality;

    [Header("Visual")]
    [SerializeField] private GameObject foodVisual;

    public bool HasFood => hasFood;
    public int FoodQuality => foodQuality;

    private void Start()
    {
        UpdateVisual();
    }

    public bool AddFood(ItemData food)
    {
        if (hasFood)
            return false;

        if (food.itemType != ItemData.ItemType.Food)
            return false;

        hasFood = true;
        foodQuality = food.quality;

        UpdateVisual();

        return true;
    }

    public void ConsumeFood()
    {
        hasFood = false;
        foodQuality = 0;

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (foodVisual != null)
        {
            foodVisual.SetActive(hasFood);
        }
    }
}