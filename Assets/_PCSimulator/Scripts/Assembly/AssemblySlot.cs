using UnityEngine;
using PCSimulator.Data;

namespace PCSimulator.Assembly
{
    /// <summary>
    /// Componente que define um Slot de encaixe físico no simulador (ex: Soquete CPU, Slot RAM 1, PCIe x16, Slot M.2, etc.).
    /// Gerencia a atração física, escala correta de encaixe e feedback visual (Highlights de encaixe).
    /// </summary>
    public class AssemblySlot : MonoBehaviour
    {
        [Header("Configurações do Slot")]
        [SerializeField] private string slotId;
        [SerializeField] private ComponentType allowedType;
        [SerializeField] private SocketType allowedSocket;

        [Header("Estado")]
        [SerializeField] private bool isOccupied;
        [SerializeField] private ComputerComponent installedComponent;

        [Header("Snapping & Tolerância")]
        [SerializeField] private float snapDistanceThreshold = 0.35f;

        [Header("Feedback Visual 3D")]
        [SerializeField] private MeshRenderer slotVisualRenderer;

        public string SlotId => slotId;
        public ComponentType AllowedType => allowedType;
        public SocketType AllowedSocket => allowedSocket;
        public bool IsOccupied => isOccupied;
        public ComputerComponent InstalledComponent => installedComponent;
        public float SnapDistanceThreshold => snapDistanceThreshold;

        private Material originalSlotMaterial;
        private static Material highlightCompatibleMaterial;

        private void Awake()
        {
            if (slotVisualRenderer == null)
            {
                slotVisualRenderer = GetComponentInChildren<MeshRenderer>();
            }

            if (slotVisualRenderer != null)
            {
                originalSlotMaterial = slotVisualRenderer.material;
            }

            CreateHighlightMaterialsIfNeeded();
        }

        private static void CreateHighlightMaterialsIfNeeded()
        {
            if (highlightCompatibleMaterial == null)
            {
                highlightCompatibleMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                highlightCompatibleMaterial.color = new Color(0.1f, 1f, 0.3f, 0.8f); // Verde neon transparente
            }
        }

        public bool IsComponentCompatible(ComputerComponent component)
        {
            if (component == null || component.ComponentData == null) return false;

            var data = component.ComponentData;
            bool matchesType = data.ComponentType == allowedType;
            bool matchesSocket = allowedSocket == SocketType.None || data.RequiredSlotType == allowedSocket || data.SocketType == allowedSocket;

            return matchesType && matchesSocket;
        }

        public void SetHighlight(bool enable)
        {
            if (slotVisualRenderer == null) return;

            if (enable && !isOccupied)
            {
                slotVisualRenderer.material = highlightCompatibleMaterial;
            }
            else if (originalSlotMaterial != null)
            {
                slotVisualRenderer.material = originalSlotMaterial;
            }
        }

        public bool TryInstallComponent(ComputerComponent component)
        {
            if (isOccupied)
            {
                Debug.LogWarning($"[AssemblySlot] Slot '{slotId}' já está ocupado!");
                return false;
            }

            if (!IsComponentCompatible(component))
            {
                Debug.LogWarning($"[AssemblySlot] Componente '{component.ComponentData?.DisplayName}' é incompatível com o slot '{slotId}'.");
                return false;
            }

            // Realiza o encaixe mantendo a escala de mundo (evita achatamento de parentes)
            component.transform.SetParent(transform, true);
            component.transform.position = transform.position;
            component.transform.rotation = transform.rotation;

            installedComponent = component;
            isOccupied = true;

            component.SetState(InstallationState.Installed);
            component.SetSlot(this);
            SetHighlight(false);

            Debug.Log($"[AssemblySlot] Componente '{component.ComponentData?.DisplayName ?? component.name}' instalado com sucesso no slot '{slotId}'.");
            return true;
        }

        public ComputerComponent UninstallComponent()
        {
            if (!isOccupied || installedComponent == null) return null;

            var removed = installedComponent;
            removed.transform.SetParent(null);
            removed.SetState(InstallationState.Uninstalled);
            removed.SetSlot(null);

            installedComponent = null;
            isOccupied = false;

            AssemblySystem.Instance?.NotifyComponentUninstalled(removed, this);

            Debug.Log($"[AssemblySlot] Componente '{removed.ComponentData?.DisplayName ?? removed.name}' removido do slot '{slotId}'.");
            return removed;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isOccupied ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.08f);
        }
    }
}
