using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPoint;

    [Header("Configuración")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private Transform raycastOrigin;

    private InteractableObject heldObject;
    [SerializeField]
    private GameObject trashBagPrefab;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (heldObject == null)
            {
                TryPickUp();
            }
            else
            {
                if (!TryPlaceObject())
                {
                    DropObject();
                }
            }
        }
    }

    private void TryPickUp()
    {
        
        Ray ray = new Ray(
            raycastOrigin.position,
            raycastOrigin.forward
        );
        Debug.DrawRay(
            ray.origin,
            ray.direction * interactDistance,
            Color.red,
            2f);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            InteractableObject interactable =
                hit.collider.GetComponentInParent<InteractableObject>();

            if (interactable != null)
            {
                heldObject = interactable;
                heldObject.PickUp(holdPoint);
            }
        }
    }
    private bool TryPlaceObject()
    {
        Ray ray = new Ray(
            raycastOrigin.position,
            raycastOrigin.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactDistance))
        {
            return false;
        }

        ItemData itemData =
            heldObject.GetComponent<ItemData>();

        if (itemData == null)
            return false;

        // BASURERO

        if (TryTrashBin(hit, itemData))
            return true;

        // LIMPIAR ARENERO

        if (TryCleanLitterBox(hit, itemData))
            return true;

        // COMEDERO

        if (TryFeedCat(hit, itemData))
            return true;

        // SLOT NORMAL

        if (TryPlaceInSlot(hit))
            return true;
        
        if (TryAddLitter(hit, itemData))
            return true;

        return false;
    }

    private void DropObject()
    {
        heldObject.Drop();
        heldObject = null;
    }
    private bool TryAddLitter(
    RaycastHit hit,
    ItemData itemData)
    {
        CatLitterBox litterBox =
            hit.collider.GetComponentInParent<CatLitterBox>();

        if (litterBox == null)
            return false;

        if (itemData.itemType != ItemData.ItemType.Litter)
            return false;

        if (!litterBox.AddLitter(itemData))
            return false;

        Destroy(heldObject.gameObject);

        heldObject = null;

        return true;
    }
    private bool TryPlaceInSlot(
    RaycastHit hit)
    {
        PlacementPoint point =
            hit.collider.GetComponentInParent<PlacementPoint>();

        if (point == null)
            return false;

        if (!point.TryPlaceObject(heldObject))
            return false;

        heldObject = null;

        return true;
    }
    private bool TryCleanLitterBox(
    RaycastHit hit,
    ItemData itemData)
    {
        CatLitterBox litterBox =
            hit.collider.GetComponentInParent<CatLitterBox>();

        if (litterBox == null)
            return false;

        if (itemData.itemType != ItemData.ItemType.Scoop)
            return false;

        if (!litterBox.CanBeCleaned())
            return false;

        litterBox.Clean();

        SpawnTrashBag(
            holdPoint.position
        );

        return true;
    }
    private bool TryTrashBin(
    RaycastHit hit,
    ItemData itemData)
    {
        TrashBin bin =
            hit.collider.GetComponentInParent<TrashBin>();

        if (bin == null)
            return false;

        if (itemData.itemType != ItemData.ItemType.Trash)
            return false;

        Destroy(heldObject.gameObject);

        heldObject = null;

        return true;
    }
    private bool TryFeedCat(
    RaycastHit hit,
    ItemData itemData)
    {
        CatFeeder feeder =
            hit.collider.GetComponentInParent<CatFeeder>();

        if (feeder == null)
            return false;

        if (itemData.itemType != ItemData.ItemType.Food)
            return false;

        if (!feeder.AddFood(itemData))
            return false;

        Destroy(heldObject.gameObject);

        heldObject = null;

        return true;
    }
    private void SpawnTrashBag(
    Vector3 position)
    {
        Instantiate(
            trashBagPrefab,
            position,
            Quaternion.identity
        );
    }
}