using UnityEngine;
using PCSimulator.Data;

namespace PCSimulator.Assembly
{
    /// <summary>
    /// Classe base anexada a todo GameObject de peça no ambiente 3D.
    /// Gerencia o estado físico, atração por slots, destaques e dados do componente.
    /// </summary>

    [RequireComponent(typeof(Collider))]
    public class ComputerComponent : MonoBehaviour
    {
        [Header("Dados do Componente")]
        [SerializeField] private ComponentDataSO componentData;
        [SerializeField] private InstallationState state = InstallationState.Uninstalled;

        [Header("Referências")]
        [SerializeField] private MeshRenderer meshRenderer;

        public ComponentDataSO ComponentData => componentData;
        public InstallationState State => state;
        public AssemblySlot CurrentSlot { get; private set; }

        private Material originalMaterial;
        [SerializeField] private Material highlightMaterial;

        private void Awake()
        {
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            if (meshRenderer != null)
            {
                originalMaterial = meshRenderer.sharedMaterial;
            }
        }

        public void Initialize(ComponentDataSO data)
        {
            componentData = data;
            state = InstallationState.Uninstalled;
        }

        public void SetState(InstallationState newState)
        {
            state = newState;
        }

        public void SetSlot(AssemblySlot slot)
        {
            CurrentSlot = slot;
        }

        public void SetHighlight(bool enable)
        {
            if (meshRenderer == null) return;

            if (enable && highlightMaterial != null)
            {
                meshRenderer.material = highlightMaterial;
            }
            else if (originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
        }
    }
}
