using System.Collections.Generic;
using UnityEngine;
using PCSimulator.Data;

namespace PCSimulator.Catalog
{
    /// <summary>
    /// Catálogo orientado a dados responsável por mapear IDs de peças (recebidos via JSON)
    /// para os ScriptableObjects e Prefabs cadastrados no jogo.
    /// </summary>
    public class ComponentCatalog : MonoBehaviour
    {
        public static ComponentCatalog Instance { get; private set; }

        [Header("Registro de Componentes")]
        [SerializeField] private List<ComponentDataSO> registeredComponents = new List<ComponentDataSO>();

        private readonly Dictionary<string, ComponentDataSO> catalogMap = new Dictionary<string, ComponentDataSO>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitializeCatalog();
        }

        private void InitializeCatalog()
        {
            catalogMap.Clear();
            foreach (var data in registeredComponents)
            {
                if (data != null && !string.IsNullOrEmpty(data.ComponentId))
                {
                    if (!catalogMap.ContainsKey(data.ComponentId))
                    {
                        catalogMap.Add(data.ComponentId, data);
                    }
                    else
                    {
                        Debug.LogWarning($"[ComponentCatalog] ID duplicado encontrado e ignorado: {data.ComponentId}");
                    }
                }
            }
            Debug.Log($"[ComponentCatalog] Catálogo inicializado com {catalogMap.Count} componentes.");
        }

        public ComponentDataSO GetComponentData(string componentId)
        {
            if (string.IsNullOrEmpty(componentId)) return null;

            if (catalogMap.TryGetValue(componentId, out var data))
            {
                return data;
            }

            Debug.LogError($"[ComponentCatalog Error] Componente ID não encontrado no catálogo: {componentId}");
            return null;
        }

        public void RegisterComponent(ComponentDataSO data)
        {
            if (data != null && !string.IsNullOrEmpty(data.ComponentId) && !catalogMap.ContainsKey(data.ComponentId))
            {
                registeredComponents.Add(data);
                catalogMap.Add(data.ComponentId, data);
            }
        }
    }
}
