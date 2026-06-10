using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class InteractableObject : MonoBehaviour
{
    private Rigidbody rb;
    private PlacementPoint currentPlacement;

    public bool IsPlaced { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void PickUp(Transform holdPoint)
    {
        if (currentPlacement != null)
        {
            currentPlacement.RemoveObject(this);
            currentPlacement = null;
        }

        IsPlaced = false;

        rb.isKinematic = true;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void Place(Transform target, PlacementPoint placement)
    {
        currentPlacement = placement;

        transform.SetParent(null);

        transform.position = target.position;
        transform.rotation = target.rotation;

        rb.isKinematic = true;

        IsPlaced = true;
    }

    public void Drop()
    {
        IsPlaced = false;

        transform.SetParent(null);

        rb.isKinematic = false;
    }

    public PlacementPoint GetCurrentPlacement()
    {
        return currentPlacement;
    }
}