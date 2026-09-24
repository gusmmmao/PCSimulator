using UnityEngine;

namespace PCSimulator.Data
{
    /// <summary>
    /// ScriptableObject que armazena todas as informações imutáveis orientadas a dados
    /// sobre uma peça de computador no catálogo.
    /// </summary>
    [CreateAssetMenu(fileName = "ComponentData_", menuName = "PC Simulator/Component Data Asset")]
    public class ComponentDataSO : ScriptableObject
    {
        [Header("Identificação")]
        [Tooltip("ID único correspondente ao retornado no JSON da Build (ex: ryzen_5_5600)")]
        [SerializeField] private string componentId;
        [SerializeField] private string displayName;
        [SerializeField] private ComponentType componentType;
        [TextArea(2, 4)]
        [SerializeField] private string description;

        [Header("Especificações Físicas & Compatibilidade")]
        [SerializeField] private SocketType socketType;
        [SerializeField] private SocketType requiredSlotType;
        [SerializeField] private Vector3 dimensions = Vector3.one;

        [Header("Visual & Assets")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private Sprite icon;

        // Propriedades públicas imutáveis
        public string ComponentId => componentId;
        public string DisplayName => displayName;
        public ComponentType ComponentType => componentType;
        public string Description => description;
        public SocketType SocketType => socketType;
        public SocketType RequiredSlotType => requiredSlotType;
        public Vector3 Dimensions => dimensions;
        public GameObject Prefab => prefab;
        public Sprite Icon => icon;
    }
}
