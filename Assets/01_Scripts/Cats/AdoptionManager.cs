using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdoptionManager : MonoBehaviour
{
    [Header("Temporizador de Adopción")]
    [SerializeField] private float minAdoptionTime = 120f;
    [SerializeField] private float maxAdoptionTime = 300f;

    [Header("Mínimo de gatos para activar sistema")]
    [SerializeField] private int minCatsToStartAdoption = 3;

    private float adoptionTimer;
    private float nextAdoptionTime;

    private void Start()
    {
        ScheduleNextAdoption();
    }

    private void Update()
    {
        if (HappinessManager.Instance == null) return;
        if (HappinessManager.Instance.ActiveCats.Count < minCatsToStartAdoption) return;

        adoptionTimer += Time.deltaTime;
        if (adoptionTimer >= nextAdoptionTime)
        {
            AttemptAdoption();
            ScheduleNextAdoption();
        }
    }

    private void ScheduleNextAdoption()
    {
        nextAdoptionTime = Random.Range(minAdoptionTime, maxAdoptionTime);
        adoptionTimer = 0f;
    }

    private void AttemptAdoption()
    {
        HappinessManager manager = HappinessManager.Instance;
        if (manager == null) return;

        float happinessAvg = manager.AverageHappiness;

        // Probabilidad directamente proporcional a la felicidad media
        float probability = happinessAvg / 100f;
        if (Random.value > probability)
        {
            Debug.Log("[AdoptionManager] No hubo adopción esta vez. Felicidad: " + Mathf.RoundToInt(happinessAvg) + "%");
            return;
        }

        // Elegir un gato aleatorio de los disponibles
        IReadOnlyList<CatAI> cats = manager.ActiveCats;
        if (cats.Count == 0) return;

        int index = Random.Range(0, cats.Count);
        CatAI chosenCat = cats[index];
        if (chosenCat == null) return;

        // La recompensa es la felicidad actual del gato como monedas
        int reward = Mathf.RoundToInt(chosenCat.Happiness);

        Debug.Log("[AdoptionManager] ¡Han adoptado a " + chosenCat.name + "! Recompensa: $" + reward);

        // Dar dinero (sin multiplicador — es una recompensa directa)
        ShopManager shop = FindAnyObjectByType<ShopManager>();
        if (shop != null)
        {
            shop.AddMoneyRaw(reward);
        }

        // El gato desaparece — se lo han adoptado
        Destroy(chosenCat.gameObject);
    }
}
