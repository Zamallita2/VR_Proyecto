using UnityEngine;
public class CatLitterBox : MonoBehaviour
{
    [SerializeField] private int remainingUses = 5;
    [SerializeField] private int dirtiness = 0;

    public int RemainingUses => remainingUses;
    public int Dirtiness => dirtiness;

    public bool HasLitter => remainingUses > 0;

    public void UseLitter()
    {
        remainingUses--;
        dirtiness++;
    }

    public void Clean()
    {
        dirtiness = 0;
    }
    public bool CanBeCleaned()
    {
        return dirtiness > 0;
    }

    public bool AddLitter(ItemData litter)
    {
        if (litter.itemType != ItemData.ItemType.Litter)
            return false;

        remainingUses += litter.quality;

        return true;
    }
}