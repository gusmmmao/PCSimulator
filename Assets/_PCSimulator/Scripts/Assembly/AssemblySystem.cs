using System;
using System.Collections.Generic;
using UnityEngine;
using PCSimulator.Data;

namespace PCSimulator.Assembly
{
    /// <summary>
    /// Sistema principal responsável por coordenar as operações de seleção, atração,
    /// encaixe, validação de snaps e contagem do progresso de montagem.
    /// </summary>
    public class AssemblySystem : MonoBehaviour
    {
        public static AssemblySystem Instance { get; private set; }

        [Header("Slots Registrados")]
        [SerializeField] private List<AssemblySlot> registeredSlots = new List<AssemblySlot>();

        public event Action<ComputerComponent, AssemblySlot> OnComponentInstalled;
        public event Action<ComputerComponent, AssemblySlot> OnComponentUninstalled;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            AutoDiscoverSlots();
        }

        public void AutoDiscoverSlots()
        {
            registeredSlots.Clear();
            registeredSlots.AddRange(FindObjectsByType<AssemblySlot>(FindObjectsSortMode.None));
            Debug.Log($"[AssemblySystem] {registeredSlots.Count} slots de montagem localizados na cena.");
        }

        public AssemblySlot FindNearestCompatibleSlot(ComputerComponent component, float maxDistance = 0.3f)
        {
            if (component == null) return null;

            AssemblySlot nearestSlot = null;
            float minDistance = float.MaxValue;

            foreach (var slot in registeredSlots)
            {
                if (slot == null || slot.IsOccupied) continue;

                if (slot.IsComponentCompatible(component))
                {
                    float dist = Vector3.Distance(component.transform.position, slot.transform.position);
                    if (dist <= maxDistance && dist < minDistance)
                    {
                        minDistance = dist;
                        nearestSlot = slot;
                    }
                }
            }

            return nearestSlot;
        }

        public bool TrySnapComponent(ComputerComponent component, AssemblySlot targetSlot)
        {
            if (component == null || targetSlot == null) return false;

            if (targetSlot.TryInstallComponent(component))
            {
                OnComponentInstalled?.Invoke(component, targetSlot);
                return true;
            }

            return false;
        }

        public void NotifyComponentUninstalled(ComputerComponent component, AssemblySlot slot)
        {
            OnComponentUninstalled?.Invoke(component, slot);
        }
    }
}
