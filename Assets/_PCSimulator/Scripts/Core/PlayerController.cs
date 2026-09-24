using UnityEngine;
using PCSimulator.Assembly;
using PCSimulator.Data;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PCSimulator.Core
{
    /// <summary>
    /// Controlador de Interação 3D com trava de movimento estritamente no Plano Horizontal (XZ).
    /// As peças deslizam sobre a bancada a uma altura fixa sem subir ou descer até encaixarem nos slots.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Configurações de Interação")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float maxRaycastDistance = 10f;
        [SerializeField] private float fixedHeightY = 0.05f;
        [SerializeField] private float moveSpeed = 25f;
        [SerializeField] private float rotationSpeed = 120f;

        [Header("Estado de Interação")]
        [SerializeField] private ComputerComponent hoveredComponent;
        [SerializeField] private ComputerComponent heldComponent;

        private AssemblySlot hoveredSlot;
        private Plane horizontalPlane;

        private void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            horizontalPlane = new Plane(Vector3.up, new Vector3(0, fixedHeightY, 0));
        }

        private void Update()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            horizontalPlane = new Plane(Vector3.up, new Vector3(0, fixedHeightY, 0));

            HandleRaycast();
            HandleInput();

            if (heldComponent != null)
            {
                UpdateHeldComponentPosition();
            }
        }

        private Vector2 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
#endif
            try { return Input.mousePosition; } catch { return Vector2.zero; }
        }

        private bool IsLeftClickPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.leftButton.wasPressedThisFrame;
#endif
            try { return Input.GetMouseButtonDown(0); } catch { return false; }
        }

        private bool IsRotateKeyHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null) return Keyboard.current.rKey.isPressed;
#endif
            try { return Input.GetKey(KeyCode.R); } catch { return false; }
        }

        private void HandleRaycast()
        {
            Ray ray = mainCamera.ScreenPointToRay(GetMousePosition());

            if (heldComponent == null)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance))
                {
                    var comp = hit.collider.GetComponentInParent<ComputerComponent>();
                    if (comp != hoveredComponent)
                    {
                        if (hoveredComponent != null) hoveredComponent.SetHighlight(false);
                        hoveredComponent = comp;
                        if (hoveredComponent != null) hoveredComponent.SetHighlight(true);
                    }
                }
                else
                {
                    if (hoveredComponent != null)
                    {
                        hoveredComponent.SetHighlight(false);
                        hoveredComponent = null;
                    }
                }
            }
            else
            {
                if (AssemblySystem.Instance != null)
                {
                    hoveredSlot = AssemblySystem.Instance.FindNearestCompatibleSlot(heldComponent, 0.35f);
                }
            }
        }

        private void HandleInput()
        {
            if (IsLeftClickPressedThisFrame())
            {
                if (heldComponent == null && hoveredComponent != null)
                {
                    PickUpComponent(hoveredComponent);
                }
                else if (heldComponent != null)
                {
                    ReleaseComponent();
                }
            }

            if (heldComponent != null && IsRotateKeyHeld())
            {
                heldComponent.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }
        }

        private void PickUpComponent(ComputerComponent component)
        {
            if (component.CurrentSlot != null)
            {
                component.CurrentSlot.UninstallComponent();
            }

            heldComponent = component;
            heldComponent.SetState(InstallationState.HeldByPlayer);
            Debug.Log($"[PlayerController] Peça pegada: {component.name}");
        }

        private void ReleaseComponent()
        {
            if (heldComponent == null) return;

            if (hoveredSlot != null && AssemblySystem.Instance != null)
            {
                if (AssemblySystem.Instance.TrySnapComponent(heldComponent, hoveredSlot))
                {
                    Debug.Log($"[PlayerController] Peça encaixada no slot '{hoveredSlot.SlotId}'");
                    heldComponent = null;
                    return;
                }
            }

            heldComponent.SetState(InstallationState.Uninstalled);
            Debug.Log($"[PlayerController] Peça solta: {heldComponent.name}");
            heldComponent = null;
        }

        private void UpdateHeldComponentPosition()
        {
            Vector3 targetPos;

            // Se a peça estiver próxima de um slot compatível, atrai para a posição exata do slot
            if (hoveredSlot != null)
            {
                targetPos = hoveredSlot.transform.position;
            }
            else
            {
                // Trava o movimento ESTRITAMENTE no plano horizontal XZ sobre a mesa
                Ray ray = mainCamera.ScreenPointToRay(GetMousePosition());
                if (horizontalPlane.Raycast(ray, out float enter))
                {
                    targetPos = ray.GetPoint(enter);
                    targetPos.y = fixedHeightY; // Garante que a peça não sobe nem desce
                }
                else
                {
                    targetPos = heldComponent.transform.position;
                }
            }

            heldComponent.transform.position = Vector3.Lerp(heldComponent.transform.position, targetPos, Time.deltaTime * moveSpeed);
        }
    }
}
