using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using PCSimulator.Assembly;
using PCSimulator.Data;

namespace PCSimulator.Core
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Configuracoes de Interacao")]
        [SerializeField] private float maxRaycastDistance = 20f;
        [SerializeField] private float moveSpeed = 15f;
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float fixedHeightY = 0.1f;

        [Header("Estado Interno")]
        [SerializeField] private ComputerComponent hoveredComponent;
        [SerializeField] private ComputerComponent heldComponent;
        [SerializeField] private AssemblySlot hoveredSlot;

        private Camera mainCamera;
        private Plane horizontalPlane;

        private void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            horizontalPlane = new Plane(Vector3.up, new Vector3(0, fixedHeightY, 0));
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.AssemblyInProgress)
                return;

            HandleInput();
            UpdateInteraction();

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

        private bool IsLeftClickPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.leftButton.wasPressedThisFrame;
#endif
            return Input.GetMouseButtonDown(0);
        }

        private bool IsLeftClickReleased()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.leftButton.wasReleasedThisFrame;
#endif
            return Input.GetMouseButtonUp(0);
        }

        private bool IsRotateKeyHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null) return Keyboard.current.rKey.isPressed;
#endif
            return Input.GetKey(KeyCode.R);
        }

        private void UpdateInteraction()
        {
            Ray ray = mainCamera.ScreenPointToRay(GetMousePosition());
            Debug.DrawRay(ray.origin, ray.direction * maxRaycastDistance, Color.yellow);

            if (heldComponent == null)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance))
                {
                    var comp = hit.collider.GetComponentInParent<ComputerComponent>();
                    if (comp != hoveredComponent)
                    {
                        if (hoveredComponent != null) hoveredComponent.SetHighlight(false);
                        hoveredComponent = comp;
                        if (hoveredComponent != null && hoveredComponent.State == InstallationState.Uninstalled)
                        {
                            hoveredComponent.SetHighlight(true);
                        }
                    }
                }
                else
                {
                    if (hoveredComponent != null) hoveredComponent.SetHighlight(false);
                    hoveredComponent = null;
                }
            }
            else
            {
                if (AssemblySystem.Instance != null)
                {
                    AssemblySlot newSlot = AssemblySystem.Instance.FindNearestCompatibleSlot(heldComponent, 0.4f);
                    if (newSlot != hoveredSlot)
                    {
                        if (hoveredSlot != null) hoveredSlot.SetHighlight(false);
                        hoveredSlot = newSlot;
                        if (hoveredSlot != null) hoveredSlot.SetHighlight(true);
                    }
                }
            }
        }

        private void HandleInput()
        {
            if (IsLeftClickPressed())
            {
                if (heldComponent == null && hoveredComponent != null && hoveredComponent.State == InstallationState.Uninstalled)
                {
                    PickUpComponent(hoveredComponent);
                }
                else if (heldComponent == null)
                {
                    Debug.Log($"[PlayerController] Clique falhou. hoveredComponent {(hoveredComponent == null ? "nulo" : "invalido")}");
                }
            }
            else if (IsLeftClickReleased())
            {
                if (heldComponent != null)
                {
                    ReleaseComponent();
                }
            }

            if (heldComponent != null && IsRotateKeyHeld())
            {
                heldComponent.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }

#if ENABLE_INPUT_SYSTEM
            bool isPowerPressed = Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame;
#else
            bool isPowerPressed = Input.GetKeyDown(KeyCode.P);
#endif
            if (isPowerPressed && PCSimulator.Power.PowerSystem.Instance != null)
            {
                PCSimulator.Power.PowerSystem.Instance.TryPressPowerButton();
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
            Debug.Log($"[PlayerController] Peca pegada: {component.name}");
        }

        private void ReleaseComponent()
        {
            if (heldComponent == null) return;

            if (hoveredSlot != null && AssemblySystem.Instance != null)
            {
                if (AssemblySystem.Instance.TrySnapComponent(heldComponent, hoveredSlot))
                {
                    Debug.Log($"[PlayerController] Peca encaixada no slot '{hoveredSlot.SlotId}'");
                    heldComponent = null;
                    return;
                }
            }

            heldComponent.SetState(InstallationState.Uninstalled);
            Debug.Log($"[PlayerController] Peca solta: {heldComponent.name}");
            heldComponent = null;
        }

        private void UpdateHeldComponentPosition()
        {
            Vector3 targetPos;

            if (hoveredSlot != null)
            {
                targetPos = hoveredSlot.transform.position;
            }
            else
            {
                Ray ray = mainCamera.ScreenPointToRay(GetMousePosition());
                if (horizontalPlane.Raycast(ray, out float enter))
                {
                    targetPos = ray.GetPoint(enter);
                    targetPos.y = fixedHeightY;
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
