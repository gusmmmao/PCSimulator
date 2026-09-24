using System.Collections.Generic;
using UnityEngine;
using PCSimulator.Core;
using PCSimulator.Catalog;
using PCSimulator.Data;
using PCSimulator.Networking;

namespace PCSimulator.Assembly
{
    /// <summary>
    /// Componente responsável por escutar o recebimento de uma Build (JSON)
    /// e instanciar dinamicamente na bancada do simulador os componentes correspondentes.
    /// </summary>
    public class BuildComponentSpawner : MonoBehaviour
    {
        public static BuildComponentSpawner Instance { get; private set; }

        [Header("Pontos de Spawn na Bancada")]
        [SerializeField] private Transform workbenchSpawnPoint;
        [SerializeField] private Vector3 spawnOffsetIncrement = new Vector3(0.35f, 0, 0);

        [Header("Componentes Gerados na Cena")]
        [SerializeField] private List<ComputerComponent> spawnedComponents = new List<ComputerComponent>();

        public IReadOnlyList<ComputerComponent> SpawnedComponents => spawnedComponents;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (BuildReceiver.Instance != null)
            {
                BuildReceiver.Instance.OnBuildParsed += HandleBuildParsed;
            }
        }

        private void OnDestroy()
        {
            if (BuildReceiver.Instance != null)
            {
                BuildReceiver.Instance.OnBuildParsed -= HandleBuildParsed;
            }
        }

        public void HandleBuildParsed(BuildConfiguration config)
        {
            if (config == null) return;

            Debug.Log($"[BuildComponentSpawner] Spawando peças para a Build ID: {config.BuildId}");
            ClearPreviousSpawnedComponents();

            var ids = config.GetAllComponentIds();
            Vector3 currentSpawnPos = (workbenchSpawnPoint != null) ? workbenchSpawnPoint.position : new Vector3(-0.8f, 0.05f, 0.3f);

            int count = 0;
            foreach (var id in ids)
            {
                ComponentDataSO data = ComponentCatalog.Instance != null ? ComponentCatalog.Instance.GetComponentData(id) : null;
                if (data != null)
                {
                    SpawnComponentData(data, currentSpawnPos);
                }
                else
                {
                    // Se o componente não estiver cadastrado com asset específico, cria um placeholder Genérico
                    SpawnGenericPlaceholder(id, currentSpawnPos);
                }

                currentSpawnPos += spawnOffsetIncrement;
                count++;
                if (count % 4 == 0)
                {
                    currentSpawnPos.x = (workbenchSpawnPoint != null) ? workbenchSpawnPoint.position.x : -0.8f;
                    currentSpawnPos.z -= 0.35f;
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.AssemblyInProgress);
            }
        }

        private void SpawnComponentData(ComponentDataSO data, Vector3 position)
        {
            GameObject obj;
            if (data.Prefab != null)
            {
                obj = Instantiate(data.Prefab, position, Quaternion.identity);
            }
            else
            {
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = position;
                obj.transform.localScale = data.Dimensions != Vector3.zero ? data.Dimensions : new Vector3(0.15f, 0.15f, 0.15f);

                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = GetColorForType(data.ComponentType);
                obj.GetComponent<Renderer>().material = mat;
            }

            obj.name = $"Comp_{data.ComponentId}";
            var comp = obj.GetComponent<ComputerComponent>();
            if (comp == null)
            {
                comp = obj.AddComponent<ComputerComponent>();
            }

            comp.Initialize(data);
            spawnedComponents.Add(comp);
        }

        private void SpawnGenericPlaceholder(string componentId, Vector3 position)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = $"Generic_{componentId}";
            obj.transform.position = position;
            obj.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Color.yellow;
            obj.GetComponent<Renderer>().material = mat;

            var comp = obj.AddComponent<ComputerComponent>();
            spawnedComponents.Add(comp);

            Debug.LogWarning($"[BuildComponentSpawner] Criado placeholder genérico para ID não catalogado: {componentId}");
        }

        public void ClearPreviousSpawnedComponents()
        {
            foreach (var comp in spawnedComponents)
            {
                if (comp != null && comp.CurrentSlot == null)
                {
                    Destroy(comp.gameObject);
                }
            }
            spawnedComponents.Clear();
        }

        private static Color GetColorForType(ComponentType type)
        {
            return type switch
            {
                ComponentType.CPU => Color.red,
                ComponentType.GPU => Color.black,
                ComponentType.RAM => Color.blue,
                ComponentType.Storage => Color.cyan,
                ComponentType.PSU => Color.gray,
                ComponentType.Motherboard => Color.green,
                ComponentType.Case => Color.white,
                _ => Color.magenta
            };
        }
    }
}
