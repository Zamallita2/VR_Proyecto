using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CatAI : MonoBehaviour
{
    private enum CatState
    {
        Wandering,
        Eating,
        UsingLitter,
        Starving,
        NeedsLitter
    }

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 1.5f;

    [SerializeField] private Transform[] randomWaypoints;
    [SerializeField] private Transform[] kitchenWaypoints;

    [Header("Necesidades")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hunger = 100f;

    [SerializeField] private float hungerDrainPerSecond = 1f;

    [Header("Felicidad")]
    [SerializeField] private float maxHappiness = 100f;
    [SerializeField] private float happiness = 100f;

    [SerializeField] private float happinessDrainPerSecond = 0.2f;
    [Header("Castigo por hambre extrema")]
    [SerializeField] private float starvingHappinessDrain = 10f;

    [Header("Tiempos")]
    [SerializeField] private float wanderWaitTime = 3f;
    [SerializeField] private float eatingDuration = 4f;

    [Header("Umbrales")]
    [SerializeField] private float hungryThreshold = 25f;
    [Header("Baño")]
    [SerializeField] private float litterDuration = 10f;

    [SerializeField] private float needsLitterHappinessDrain = 5f;
    [SerializeField]
    float minBathroomTime = 60f;

    [SerializeField]
    float maxBathroomTime = 120f;
    private float bathroomTimer;
    private float nextBathroomTime;

    private NavMeshAgent agent;

    private float waitTimer;
    private bool waiting;
    private float litterSearchTimer;

    private CatState state = CatState.Wandering;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    private void Update()
    {
        hunger -= hungerDrainPerSecond * Time.deltaTime;
        bathroomTimer += Time.deltaTime;
        Debug.Log(state);
        if (state == CatState.Starving)
        {
            happiness -= starvingHappinessDrain * Time.deltaTime;
        }
        else if (state == CatState.NeedsLitter)
        {
            happiness -= needsLitterHappinessDrain * Time.deltaTime;
        }
        else
        {
            happiness -= happinessDrainPerSecond * Time.deltaTime;
        }

        hunger = Mathf.Clamp(hunger, 0, maxHunger);
        happiness = Mathf.Clamp(happiness, 0, maxHappiness);
        if (hunger <= 0f && state != CatState.Starving)
        {
            EnterStarvingState();
        }

        switch (state)
        {
            case CatState.Wandering:
                UpdateWandering();
                break;

            case CatState.Starving:
                TryGoEat();
                break;
            case CatState.NeedsLitter:
                UpdateNeedsLitter();
                break;
        }
    }
    private void UpdateNeedsLitter()
    {
        if (hunger <= hungryThreshold)
        {
            if (TryGoEat())
                return;
        }

        litterSearchTimer += Time.deltaTime;

        if (litterSearchTimer >= 2f)
        {
            litterSearchTimer = 0f;

            TryUseLitter();
        }
    }
    private void EnterStarvingState()
    {
        state = CatState.Starving;

        hunger = 0;

        agent.ResetPath();
    }

    private void UpdateWandering()
    {
        if (hunger <= hungryThreshold)
        {
            if (TryGoEat())
                return;
        }
        if (bathroomTimer >= nextBathroomTime)
        {
            if (TryUseLitter())
                return;
        }

        if (waiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= wanderWaitTime)
            {
                waitTimer = 0;
                waiting = false;

                GoToRandomWaypoint();
            }

            return;
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            waiting = true;
        }
    }

    private void Start()
    {
        ScheduleNextBathroom();
        GoToRandomWaypoint();
    }
    private void ScheduleNextBathroom()
    {
        nextBathroomTime = Random.Range(
            minBathroomTime,
            maxBathroomTime
        );

        bathroomTimer = 0;
    }

    private void GoToRandomWaypoint()
    {
        int index = Random.Range(0, randomWaypoints.Length);

        agent.SetDestination(
            randomWaypoints[index].position
        );
    }

    private bool TryGoEat()
    {
        CatFeeder[] feeders = FindObjectsByType<CatFeeder>(
            FindObjectsSortMode.None
        );

        CatFeeder bestFeeder = null;

        float bestDistance = Mathf.Infinity;

        foreach (CatFeeder feeder in feeders)
        {
            if (!feeder.HasFood)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                feeder.transform.position
            );

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestFeeder = feeder;
            }
        }

        if (bestFeeder == null)
            return false;

        state = CatState.Eating;

        StartCoroutine(
            MoveToFoodAndEat(bestFeeder)
        );

        return true;
    }

    private IEnumerator MoveToFoodAndEat(
        CatFeeder feeder)
    {
        agent.SetDestination(
            feeder.transform.position
        );

        while (agent.pathPending ||
               agent.remainingDistance >
               agent.stoppingDistance)
        {
            yield return null;
        }

        yield return new WaitForSeconds(
            eatingDuration
        );

        int quality = feeder.FoodQuality;

        hunger += quality * 30f;

        if (quality >= 2)
        {
            happiness += quality * 5f;
        }

        hunger = Mathf.Clamp(
            hunger,
            0,
            maxHunger
        );

        happiness = Mathf.Clamp(
            happiness,
            0,
            maxHappiness
        );

        feeder.ConsumeFood();

        state = CatState.Wandering;

        GoToRandomWaypoint();
    }
    private bool TryUseLitter()
    {
        CatLitterBox[] boxes =
            FindObjectsByType<CatLitterBox>(
                FindObjectsSortMode.None
            );

        CatLitterBox bestBox = null;

        int lowestDirtiness = int.MaxValue;

        foreach (CatLitterBox box in boxes)
        {
            if (!box.HasLitter)
                continue;

            if (box.Dirtiness < lowestDirtiness)
            {
                lowestDirtiness = box.Dirtiness;
                bestBox = box;
            }
        }

        if (bestBox == null)
        {
            state = CatState.NeedsLitter;
            return true;
        }

        state = CatState.UsingLitter;

        StartCoroutine(
            MoveToLitterAndUse(bestBox)
        );

        return true;
    }
    private IEnumerator MoveToLitterAndUse(
    CatLitterBox box)
    {
        agent.SetDestination(
            box.transform.position
        );

        while (
            agent.pathPending ||
            agent.remainingDistance >
            agent.stoppingDistance
        )
        {
            yield return null;
        }

        Renderer[] renderers =
            GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        yield return new WaitForSeconds(
            litterDuration
        );

        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }

        box.UseLitter();

        if (box.Dirtiness >= 3)
        {
            happiness -= 10;
        }

        ScheduleNextBathroom();

        state = CatState.Wandering;

        GoToRandomWaypoint();
    }
}