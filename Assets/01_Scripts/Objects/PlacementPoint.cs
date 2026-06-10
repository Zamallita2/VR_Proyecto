using UnityEngine;

public class PlacementPoint : MonoBehaviour
{
    [Header("Punto donde se colocará el objeto")]
    [SerializeField] private Transform placePoint;

    private InteractableObject currentObject;
    private Collider cr;

    public bool IsOccupied => currentObject != null;

    private void Awake()
    {
        cr = GetComponent<Collider>();
    }

    public Transform PlacePoint
    {
        get
        {
            if (placePoint == null)
                return transform;

            return placePoint;
        }
    }

    public bool TryPlaceObject(InteractableObject obj)
    {
        if (IsOccupied)
            return false;

        currentObject = obj;

        if (cr != null)
            cr.enabled = false;

        obj.Place(PlacePoint, this);

        return true;
    }

    public void RemoveObject(InteractableObject obj)
    {
        if (currentObject == obj)
        {
            currentObject = null;

            if (cr != null)
                cr.enabled = true;
        }
    }
}