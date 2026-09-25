using System.Collections.Generic;
using UnityEngine;
using PCSimulator.Core;
using PCSimulator.Data;
using PCSimulator.Catalog;

namespace PCSimulator.Assembly
{
    public class BuildComponentSpawner : MonoBehaviour
    {
        [Header("Pontos de Spawn na Bancada")]
        [SerializeField] private Vector3 defaultSpawnStart = new Vector3(-0.8f, 0.1f, 0.3f);
        [SerializeField] private Vector3 spawnOffsetIncrement = new Vector3(0.0f, 0.0f, -0.2f);

        [Header("Componentes Gerados na Cena")]
        [SerializeField] private List<ComputerComponent> spawnedComponents = new List<ComputerComponent>();

        private void Start()
        {
            if (Networking.BuildReceiver.Instance != null)
            {
                Networking.BuildReceiver.Instance.OnBuildParsed += HandleBuildParsed;
            }
        }

        private void OnDestroy()
        {
            if (Networking.BuildReceiver.Instance != null)
            {
                Networking.BuildReceiver.Instance.OnBuildParsed -= HandleBuildParsed;
            }
        }

        private void HandleBuildParsed(BuildConfiguration config)
        {
            Debug.Log($"[BuildComponentsSpawner] Spawando pecas para a Build ID: {config.BuildId}");

            ClearPreviousSpawnedComponents();

            var ids = config.GetAllComponentIds();
            
            // Forçamos o spawn point a ficar no canto esquerdo da mesa, longe da placa mae
            Vector3 currentSpawnPos = defaultSpawnStart;

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
                    SpawnGenericPlaceholder(id, currentSpawnPos);
                }

                currentSpawnPos += spawnOffsetIncrement;
                count++;
                
                if (count % 4 == 0)
                {
                    currentSpawnPos.z = defaultSpawnStart.z;
                    currentSpawnPos.x -= 0.3f;
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
            
            var rb = obj.GetComponent<Rigidbody>();
            if (rb == null) rb = obj.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            var col = obj.GetComponent<Collider>();
            if (col != null) col.isTrigger = false;

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
            
            var rb = obj.GetComponent<Rigidbody>();
            if (rb == null) rb = obj.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            spawnedComponents.Add(comp);
        }

        public void ClearPreviousSpawnedComponents()
        {
            foreach (var c in spawnedComponents)
            {
                if (c != null)
                {
                    Destroy(c.gameObject);
                }
            }
            spawnedComponents.Clear();
        }

        private Color GetColorForType(ComponentType type)
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

