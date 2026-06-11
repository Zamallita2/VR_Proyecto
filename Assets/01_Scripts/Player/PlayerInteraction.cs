using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform rightHoldPoint;
    [SerializeField] private Transform leftHoldPoint;

    [Header("Configuración")]
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private float maxHandDistance = 1.5f;

    private InteractableObject rightHeldObject;
    private InteractableObject leftHeldObject;

    [SerializeField]
    private GameObject trashBagPrefab;

    private void Update()
    {
        // Actualizar posición de objeto de dos manos y comprobar distancia
        if (rightHeldObject != null && rightHeldObject == leftHeldObject)
        {
            if (leftHoldPoint != null && rightHoldPoint != null)
            {
                float dist = Vector3.Distance(leftHoldPoint.position, rightHoldPoint.position);
                if (dist > maxHandDistance)
                {
                    DropTwoHandedObject();
                }
                else
                {
                    Vector3 midPoint = (leftHoldPoint.position + rightHoldPoint.position) / 2f;
                    rightHeldObject.transform.position = midPoint;
                    rightHeldObject.transform.rotation = Quaternion.Slerp(leftHoldPoint.rotation, rightHoldPoint.rotation, 0.5f);
                }
            }
        }

        // Clic izquierdo -> interactúa con la mano DERECHA
        if (Input.GetMouseButtonDown(0))
        {
            HandleInteraction(ref rightHeldObject, rightHoldPoint, isRightHand: true);
        }

        // Clic derecho -> interactúa con la mano IZQUIERDA
        if (Input.GetMouseButtonDown(1))
        {
            HandleInteraction(ref leftHeldObject, leftHoldPoint, isRightHand: false);
        }
    }

    private void HandleInteraction(ref InteractableObject handObj, Transform handPoint, bool isRightHand)
    {
        // Si el objeto actual es de dos manos, se suelta de ambas manos
        if (handObj != null && leftHeldObject == rightHeldObject)
        {
            if (!TryPlaceObject(ref handObj))
            {
                DropTwoHandedObject();
            }
            return;
        }

        if (handObj == null)
        {
            TryPickUp(ref handObj, handPoint);
        }
        else
        {
            if (!TryPlaceObject(ref handObj))
            {
                DropObject(ref handObj);
            }
        }
    }

    private void DropTwoHandedObject()
    {
        if (rightHeldObject != null)
        {
            rightHeldObject.Drop();
            rightHeldObject = null;
            leftHeldObject = null;
        }
    }

    private void TryPickUp(ref InteractableObject handObj, Transform handPoint)
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
                ItemData itemData = interactable.GetComponent<ItemData>();
                if (itemData != null && itemData.size == ItemData.ItemSize.Large)
                {
                    // Solo recoger si AMBAS manos están vacías
                    if (leftHeldObject == null && rightHeldObject == null)
                    {
                        leftHeldObject = interactable;
                        rightHeldObject = interactable;
                        interactable.PickUp(null); // holdPoint null para objeto grande
                    }
                    else
                    {
                        Debug.Log("Necesitas ambas manos vacías para recoger este objeto.");
                    }
                }
                else
                {
                    handObj = interactable;
                    handObj.PickUp(handPoint);
                }
            }
        }
    }

    private bool TryPlaceObject(ref InteractableObject handObj)
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
            handObj.GetComponent<ItemData>();

        if (itemData == null)
            return false;

        bool isTwoHanded = (leftHeldObject != null && leftHeldObject == rightHeldObject);

        bool success = false;

        // BASURERO
        if (TryTrashBin(hit, itemData, ref handObj))
            success = true;
        // LIMPIAR ARENERO
        else if (TryCleanLitterBox(hit, itemData, handObj.transform.position))
            success = true;
        // COMEDERO
        else if (TryFeedCat(hit, itemData, ref handObj))
            success = true;
        // SLOT NORMAL
        else if (TryPlaceInSlot(hit, ref handObj))
            success = true;
        // LITTER
        else if (TryAddLitter(hit, itemData, ref handObj))
            success = true;

        if (success && isTwoHanded)
        {
            // Limpiar la otra mano si era de dos manos
            leftHeldObject = null;
            rightHeldObject = null;
        }

        return success;
    }

    private void DropObject(ref InteractableObject handObj)
    {
        handObj.Drop();
        handObj = null;
    }

    private bool TryAddLitter(RaycastHit hit, ItemData itemData, ref InteractableObject handObj)
    {
        CatLitterBox litterBox =
            hit.collider.GetComponentInParent<CatLitterBox>();

        if (litterBox == null)
            return false;

        if (itemData.itemType != ItemData.ItemType.Litter)
            return false;

        if (!litterBox.AddLitter(itemData))
            return false;

        Destroy(handObj.gameObject);

        handObj = null;

        return true;
    }

    private bool TryPlaceInSlot(RaycastHit hit, ref InteractableObject handObj)
    {
        PlacementPoint point =
            hit.collider.GetComponentInParent<PlacementPoint>();

        if (point == null)
            return false;

        if (!point.TryPlaceObject(handObj))
            return false;

        handObj = null;

        return true;
    }

    private bool TryCleanLitterBox(RaycastHit hit, ItemData itemData, Vector3 dropPosition)
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
            dropPosition
        );

        return true;
    }

    private bool TryTrashBin(RaycastHit hit, ItemData itemData, ref InteractableObject handObj)
    {
        TrashBin bin =
            hit.collider.GetComponentInParent<TrashBin>();

        if (bin == null)
            return false;

        if (itemData.itemType != ItemData.ItemType.Trash)
            return false;

        Destroy(handObj.gameObject);

        handObj = null;

        return true;
    }

    private bool TryFeedCat(RaycastHit hit, ItemData itemData, ref InteractableObject handObj)
    {
        CatFeeder feeder =
            hit.collider.GetComponentInParent<CatFeeder>();

        if (feeder == null)
            return false;

        if (itemData.itemType != ItemData.ItemType.Food)
            return false;

        if (!feeder.AddFood(itemData))
            return false;

        Destroy(handObj.gameObject);

        handObj = null;

        return true;
    }

    private void SpawnTrashBag(Vector3 position)
    {
        Instantiate(
            trashBagPrefab,
            position,
            Quaternion.identity
        );
    }
}