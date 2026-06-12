using UnityEngine;

public class PlayerEnvironmentInteraction : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (raycastOrigin == null)
            return;

        Ray ray = new Ray(
            raycastOrigin.position,
            raycastOrigin.forward
        );

        Debug.DrawRay(
            ray.origin,
            ray.direction * interactDistance,
            Color.green,
            1f
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactDistance))
        {
            LootBox box =
                hit.collider.GetComponentInParent<LootBox>();

            if (box != null)
            {
                box.Interact();
                return;
            }

            UnityEngine.UI.Button button =
                hit.collider.GetComponentInParent<UnityEngine.UI.Button>();

            if (button != null)
            {
                button.onClick.Invoke();
                return;
            }
        }
    }
}
