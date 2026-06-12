using UnityEngine;
using System.Collections.Generic;

public class HappinessManager : MonoBehaviour
{
    public static HappinessManager Instance { get; private set; }

    private List<CatAI> activeCats = new List<CatAI>();

    [Header("Información")]
    [SerializeField] private float averageHappiness;

    public float AverageHappiness => averageHappiness;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
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
