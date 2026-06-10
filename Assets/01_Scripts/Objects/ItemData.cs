using UnityEngine;

public class ItemData : MonoBehaviour
{
     public enum ItemSize
    {
        Small,
        Medium,
        Large
    }
    public enum ItemType
    {
        Food,
        Litter,
        Toy,
        Scoop,
        Trash
    }
    [Header("Información")]
    public ItemType itemType;

    [Header("Tamaño")]
    public ItemSize size;

    [Header("Calidad")]
    [Min(1)]
    public int quality = 1;
}