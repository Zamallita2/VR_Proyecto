using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEnvironmentInteraction : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float interactDistance = 3f;

    [Header("Configuración VR")]
    [SerializeField] private Transform rightRaycastOrigin;
    [SerializeField] private Transform leftRaycastOrigin;
    [SerializeField] private InputActionReference rightInteractAction;
    [SerializeField] private InputActionReference leftInteractAction;

    private void Update()
    {
        if (rightInteractAction != null && rightInteractAction.action.WasPressedThisFrame())
        {
            TryInteract(rightRaycastOrigin);
        }

        if (leftInteractAction != null && leftInteractAction.action.WasPressedThisFrame())
        {
            TryInteract(leftRaycastOrigin);
        }
    }

    private void TryInteract(Transform currentOrigin)
    {
        if (currentOrigin == null)
            return;

        Ray ray = new Ray(
            currentOrigin.position,
            currentOrigin.forward
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
