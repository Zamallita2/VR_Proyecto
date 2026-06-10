using UnityEngine;

public class CatFeeder : MonoBehaviour
{
    [SerializeField] private bool hasFood;
    [SerializeField] private int foodQuality;

    public bool HasFood => hasFood;
    public int FoodQuality => foodQuality;

    public bool AddFood(ItemData food)
    {
        if (hasFood)
            return false;

        if (food.itemType != ItemData.ItemType.Food)
            return false;

        hasFood = true;
        foodQuality = food.quality;

        return true;
    }

    public void ConsumeFood()
    {
        hasFood = false;
        foodQuality = 0;
    }
}