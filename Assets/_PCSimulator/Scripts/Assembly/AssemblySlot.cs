using UnityEngine;
using PCSimulator.Data;

namespace PCSimulator.Assembly
{
    /// <summary>
    /// Componente que define um Slot de encaixe físico no simulador (ex: Soquete CPU, Slot RAM 1, PCIe x16, Slot M.2, etc.).
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

        [Header("Snapping Vector & Tolerance")]
        [SerializeField] private float snapDistanceThreshold = 0.25f;

        public string SlotId => slotId;
        public ComponentType AllowedType => allowedType;
        public SocketType AllowedSocket => allowedSocket;
        public bool IsOccupied => isOccupied;
        public ComputerComponent InstalledComponent => installedComponent;
        public float SnapDistanceThreshold => snapDistanceThreshold;

        public bool IsComponentCompatible(ComputerComponent component)
        {
            if (component == null || component.ComponentData == null) return false;

            var data = component.ComponentData;
            bool matchesType = data.ComponentType == allowedType;
            bool matchesSocket = allowedSocket == SocketType.None || data.RequiredSlotType == allowedSocket || data.SocketType == allowedSocket;

            return matchesType && matchesSocket;
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

            // Realiza o snap físico no transform do slot
            component.transform.SetParent(transform);
            component.transform.localPosition = Vector3.zero;
            component.transform.localRotation = Quaternion.identity;

            installedComponent = component;
            isOccupied = true;

            component.SetState(InstallationState.Installed);
            component.SetSlot(this);

            Debug.Log($"[AssemblySlot] Componente '{component.ComponentData.DisplayName}' instalado com sucesso no slot '{slotId}'.");
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

            Debug.Log($"[AssemblySlot] Componente '{removed.ComponentData.DisplayName}' removido do slot '{slotId}'.");
            return removed;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isOccupied ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.1f);
        }
    }
}
