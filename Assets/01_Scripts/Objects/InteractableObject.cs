using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class InteractableObject : MonoBehaviour
{
    private Rigidbody rb;
    private PlacementPoint currentPlacement;

    private Collider[] colliders;

    public bool IsPlaced { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        colliders = GetComponentsInChildren<Collider>();
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

        SetColliders(false);

        transform.SetParent(holdPoint);
        if (holdPoint != null)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    public void Place(Transform target, PlacementPoint placement)
    {
        currentPlacement = placement;

        transform.SetParent(null);

        transform.position = target.position;
        transform.rotation = target.rotation;

        rb.isKinematic = true;

        SetColliders(true);

        IsPlaced = true;
    }

    public void Drop()
    {
        IsPlaced = false;

        transform.SetParent(null);

        rb.isKinematic = false;

        SetColliders(true);
    }

    private void SetColliders(bool enabled)
    {
        foreach (Collider col in colliders)
        {
            col.enabled = enabled;
        }
    }

    public PlacementPoint GetCurrentPlacement()
    {
        return currentPlacement;
    }
}