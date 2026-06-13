using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class LootBox : MonoBehaviour
{
    [Header("Visuales")]
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private GameObject objectToDeactivate;

    [Header("Loot")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private List<GameObject> prefabsToSpawn;
    
    [Header("Fuerzas físicas")]
    [SerializeField] private float minForce = 3f;
    [SerializeField] private float maxForce = 6f;
    [SerializeField] private float upwardForce = 4f;
    [SerializeField] private float timePerItem = 0.2f;
    

    private bool isOpened = false;
    public void SetLoot(List<GameObject> loot)
    {
        prefabsToSpawn = new List<GameObject>(loot);
    }
    public void Interact()
    {
        if (isOpened) return;
        isOpened = true;

        SoundManager.Instance?.PlaySFX(SoundManager.Instance.lootBoxOpen);

        if (objectToActivate != null) objectToActivate.SetActive(true);
        if (objectToDeactivate != null) objectToDeactivate.SetActive(false);

        StartCoroutine(SpawnLoot());
    }

    private IEnumerator SpawnLoot()
    {
        if (prefabsToSpawn == null ||
            prefabsToSpawn.Count == 0 ||
            spawnPoint == null)
        {
            yield break;
        }

        foreach (GameObject prefab in prefabsToSpawn)
        {
            if (prefab == null)
                continue;

            GameObject loot = Instantiate(
                prefab,
                spawnPoint.position,
                Quaternion.identity
            );

            Rigidbody rb = loot.GetComponent<Rigidbody>();

            if (rb != null)
            {
                float randomAngle =
                    Random.Range(0f, 360f);

                Vector3 direction =
                    Quaternion.Euler(0, randomAngle, 0) *
                    Vector3.forward;

                float force =
                    Random.Range(minForce, maxForce);

                Vector3 finalForce =
                    (direction * force) +
                    (Vector3.up * upwardForce);

                rb.AddForce(
                    finalForce,
                    ForceMode.Impulse
                );

                SoundManager.Instance?.PlaySFXAt(SoundManager.Instance.lootBoxItemPop, loot.transform.position);
            }

            yield return new WaitForSeconds(timePerItem);
        }
    }
}
