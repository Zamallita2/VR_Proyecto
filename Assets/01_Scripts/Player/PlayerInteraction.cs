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

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            PlacementPoint point =
                hit.collider.GetComponentInParent<PlacementPoint>();

            if (point != null)
            {
                if (point.TryPlaceObject(heldObject))
                {
                    heldObject.Place(point.PlacePoint, point);

                    heldObject = null;

                    return true;
                }
            }
        }

        return false;
    }

    private void DropObject()
    {
        heldObject.Drop();
        heldObject = null;
    }
}