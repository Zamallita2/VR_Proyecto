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
        NeedsLitter,
        Playing,
        EatingFloorFood
    }

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 1.5f;

    [SerializeField] private Transform[] randomWaypoints;
    [SerializeField] private Transform[] kitchenWaypoints;
    [SerializeField] private string randomPointsParentName = "RandomPoints";
    [SerializeField] private string kitchenPointsParentName = "KitchenPoints";

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

    [Header("Ingresos")]
    [SerializeField] private float incomeTimer = 20f;
    private float currentIncomeTimer;

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

    [Header("Animaciones")]
    [SerializeField] private Animator animator;

    [Header("Sonidos")]
    [SerializeField] private AudioSource walkAudioSource;
    private float meowTimer;
    private float nextMeowTime;
    private float complainTimer;
    private float nextComplainTime;
    private bool wasPlayingReached = false;

    private CatState state = CatState.Wandering;

    // Variables de Juego (Play)
    private float playTimer;
    private float playCooldownTimer;
    private Transform playTarget;

    public float Happiness => happiness;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        LoadWaypoints();
        if (HappinessManager.Instance != null)
        {
            HappinessManager.Instance.RegisterCat(this);
        }

        ScheduleNextBathroom();
        GoToRandomWaypoint();
        ScheduleNextMeow();
        ScheduleNextComplain();
    }
    private void LoadWaypoints()
    {
        GameObject randomParent =
            GameObject.Find(randomPointsParentName);

        if (randomParent != null)
        {
            randomWaypoints =
                new Transform[randomParent.transform.childCount];

            for (int i = 0;
                i < randomParent.transform.childCount;
                i++)
            {
                randomWaypoints[i] =
                    randomParent.transform.GetChild(i);
            }
        }

        GameObject kitchenParent =
            GameObject.Find(kitchenPointsParentName);

        if (kitchenParent != null)
        {
            kitchenWaypoints =
                new Transform[kitchenParent.transform.childCount];

            for (int i = 0;
                i < kitchenParent.transform.childCount;
                i++)
            {
                kitchenWaypoints[i] =
                    kitchenParent.transform.GetChild(i);
            }
        }
    }

    private void OnDestroy()
    {
        if (HappinessManager.Instance != null)
        {
            HappinessManager.Instance.UnregisterCat(this);
        }
    }

    private void Update()
    {
        hunger -= hungerDrainPerSecond * Time.deltaTime;
        bathroomTimer += Time.deltaTime;
        
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

        // Lógica de Ingresos
        currentIncomeTimer += Time.deltaTime;
        if (currentIncomeTimer >= incomeTimer)
        {
            currentIncomeTimer = 0f;
            GenerateIncome();
        }

        if (hunger <= 0f && state != CatState.Starving)
        {
            EnterStarvingState();
        }

        if (playCooldownTimer > 0f)
        {
            playCooldownTimer -= Time.deltaTime;
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
            case CatState.Playing:
                UpdatePlaying();
                break;
        }

        UpdateAnimations();
        HandleMeowTimer();
        HandleComplainTimer();
        HandleWalkSound();
    }

    private void GenerateIncome()
    {
        ShopManager shop = FindAnyObjectByType<ShopManager>();
        if (shop != null)
        {
            if (happiness >= 50f)
                shop.AddMoney(5);
            else if (happiness >= 25f)
                shop.AddMoney(3);
        }
    }

    private void HandleMeowTimer()
    {
        // Solo maúlla cuando está tranquilo (Wandering) y no ocupado
        if (state != CatState.Wandering) return;

        meowTimer += Time.deltaTime;
        if (meowTimer >= nextMeowTime)
        {
            SoundManager.Instance?.PlaySFXAt(SoundManager.Instance.catMeow, transform.position);
            ScheduleNextMeow();
        }
    }

    private void ScheduleNextMeow()
    {
        meowTimer = 0f;
        nextMeowTime = Random.Range(5f, 10f);
    }

    private void HandleComplainTimer()
    {
        if (state != CatState.Starving && state != CatState.NeedsLitter) return;

        complainTimer += Time.deltaTime;
        if (complainTimer >= nextComplainTime)
        {
            SoundManager.Instance?.PlaySFXAt(SoundManager.Instance.catComplain, transform.position);
            ScheduleNextComplain();
        }
    }

    private void ScheduleNextComplain()
    {
        complainTimer = 0f;
        nextComplainTime = Random.Range(4f, 7f);
    }

    private void HandleWalkSound()
    {
        if (SoundManager.Instance == null || walkAudioSource == null) return;
        bool isWalking = agent.velocity.magnitude > 0.1f;
        SoundManager.Instance.SetLoopingSFX(walkAudioSource, SoundManager.Instance.catWalk, isWalking);
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        bool isWalking = agent.velocity.magnitude > 0.1f;
        animator.SetBool("IsWalking", isWalking);

        bool isTalking = (state == CatState.Starving || state == CatState.NeedsLitter);
        animator.SetBool("IsTalking", isTalking);

        bool isPlaying = (state == CatState.Playing && agent.remainingDistance <= agent.stoppingDistance);
        animator.SetBool("IsPlaying", isPlaying);
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

        // Comprobar comida en el piso y juguete si no está a punto de morir de hambre/baño
        if (CheckForFloorFood()) return;

        if (playCooldownTimer <= 0f && CheckForToy()) return;

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

    // --- NUEVOS COMPORTAMIENTOS --- //

    private bool CheckForToy()
    {
        ItemData[] items = FindObjectsByType<ItemData>(FindObjectsSortMode.None);
        foreach (ItemData item in items)
        {
            if (item.itemType == ItemData.ItemType.Toy)
            {
                InteractableObject interactable = item.GetComponent<InteractableObject>();
                // Validar que el jugador lo tiene: no está "colocado" pero es cinemático
                if (interactable != null && !interactable.IsPlaced && interactable.GetComponent<Rigidbody>().isKinematic)
                {
                    float distance = Vector3.Distance(transform.position, interactable.transform.position);
                    if (distance <= 4f)
                    {
                        StartPlaying(interactable.transform);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void StartPlaying(Transform target)
    {
        state = CatState.Playing;
        playTimer = 0f;
        wasPlayingReached = false;
        playTarget = target;
        agent.SetDestination(target.position);
    }

    private void UpdatePlaying()
    {
        if (hunger <= hungryThreshold)
        {
            if (TryGoEat()) return;
        }

        if (playTarget != null)
        {
            agent.SetDestination(playTarget.position);
        }

        // Sonido de juego al llegar al target (solo una vez)
        bool hasArrived = agent.remainingDistance <= agent.stoppingDistance;
        if (hasArrived && !wasPlayingReached)
        {
            wasPlayingReached = true;
            SoundManager.Instance?.PlaySFXAt(SoundManager.Instance.catPlay, transform.position);
        }

        // Aumentar felicidad mientras juega
        happiness += 2f * Time.deltaTime;
        happiness = Mathf.Clamp(happiness, 0, maxHappiness);

        playTimer += Time.deltaTime;
        if (playTimer >= 10f)
        {
            StopPlaying();
        }
    }

    private void StopPlaying()
    {
        state = CatState.Wandering;
        playCooldownTimer = 60f; // 1 minuto de cooldown
        GoToRandomWaypoint();
    }

    private bool CheckForFloorFood()
    {
        ItemData[] items = FindObjectsByType<ItemData>(FindObjectsSortMode.None);
        foreach (ItemData item in items)
        {
            if (item.itemType == ItemData.ItemType.Food)
            {
                InteractableObject interactable = item.GetComponent<InteractableObject>();
                // Comida en el suelo: no colocada y con físicas activas
                if (interactable != null && !interactable.IsPlaced && !interactable.GetComponent<Rigidbody>().isKinematic)
                {
                    float distance = Vector3.Distance(transform.position, interactable.transform.position);
                    if (distance <= 5f)
                    {
                        state = CatState.EatingFloorFood;
                        StartCoroutine(MoveToFloorFoodAndEat(interactable));
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator MoveToFloorFoodAndEat(InteractableObject floorFood)
    {
        agent.SetDestination(floorFood.transform.position);

        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            // Validar que siga en el suelo
            if (floorFood == null || floorFood.IsPlaced || floorFood.GetComponent<Rigidbody>().isKinematic)
            {
                state = CatState.Wandering;
                GoToRandomWaypoint();
                yield break;
            }
            yield return null;
        }

        if (animator != null)
        {
            animator.SetBool("IsEating", true);
        }

        SoundManager.Instance?.PlaySFXAt(SoundManager.Instance.catEat, transform.position);

        float eatTimer = 0f;
        float floorEatingDuration = eatingDuration * 1.5f;

        while (eatTimer < floorEatingDuration)
        {
            // Validar si el jugador la recoge mientras come
            if (floorFood == null || floorFood.IsPlaced || floorFood.GetComponent<Rigidbody>().isKinematic)
            {
                if (animator != null) animator.SetBool("IsEating", false);
                state = CatState.Wandering;
                GoToRandomWaypoint();
                yield break;
            }

            eatTimer += Time.deltaTime;
            yield return null;
        }

        if (animator != null)
        {
            animator.SetBool("IsEating", false);
        }

        ItemData data = floorFood.GetComponent<ItemData>();
        int quality = data != null ? data.quality : 1;

        hunger += quality * 30f;
        if (quality >= 2) happiness += quality * 5f;

        hunger = Mathf.Clamp(hunger, 0, maxHunger);
        happiness = Mathf.Clamp(happiness, 0, maxHappiness);

        Destroy(floorFood.gameObject);

        state = CatState.Wandering;
        GoToRandomWaypoint();
    }

    // --- FIN NUEVOS COMPORTAMIENTOS --- //

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

        if (animator != null)
        {
            animator.SetBool("IsEating", true);
        }

        SoundManager.Instance?.PlaySFXAt(SoundManager.Instance.catEat, transform.position);

        yield return new WaitForSeconds(
            eatingDuration
        );

        if (animator != null)
        {
            animator.SetBool("IsEating", false);
        }

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

        // Maúlla al salir del arenero
        SoundManager.Instance?.PlaySFXAt(SoundManager.Instance.catMeow, transform.position);

        ScheduleNextBathroom();

        state = CatState.Wandering;

        GoToRandomWaypoint();
    }
}