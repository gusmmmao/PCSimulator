#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using PCSimulator.Core;
using PCSimulator.Catalog;
using PCSimulator.Assembly;
using PCSimulator.Data;
using PCSimulator.Networking;
using PCSimulator.Power;
using PCSimulator.UI;

namespace PCSimulator.EditorTools
{
    /// <summary>
    /// Utilitário do Editor Unity que monta automaticamente a cena 3D do simulador,
    /// cria os placeholders 3D, configura os slots de encaixe e gera os ScriptableObjects de dados.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneSetupUtility
    {
        static SceneSetupUtility()
        {
            EditorApplication.delayCall += ExecuteSetupIfNeeded;
        }

        private static void ExecuteSetupIfNeeded()
        {
            if (Object.FindFirstObjectByType<GameManager>() == null)
            {
                SetupCompleteScene();
            }
        }

        [MenuItem("PC Simulator/Montar Cena do Simulador Auto", false, 1)]
        public static void SetupCompleteScene()
        {
            Debug.Log("[SceneSetupUtility] Iniciando montagem automatizada da cena do simulador...");

            // 1. Criar ou Obter GameObject _Systems
            GameObject systemsObj = GameObject.Find("_Systems");
            if (systemsObj == null)
            {
                systemsObj = new GameObject("_Systems");
            }

            EnsureComponent<GameManager>(systemsObj);
            var catalog = EnsureComponent<ComponentCatalog>(systemsObj);
            var assemblySystem = EnsureComponent<AssemblySystem>(systemsObj);
            EnsureComponent<BuildReceiver>(systemsObj);
            EnsureComponent<NetworkManager>(systemsObj);
            EnsureComponent<PowerSystem>(systemsObj);
            EnsureComponent<BuildTestSimulator>(systemsObj);
            EnsureComponent<BuildComponentSpawner>(systemsObj);
            EnsureComponent<UIMainHUD>(systemsObj);

            // 2. Criar ou Ajustar Câmera e Player Controller
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            mainCam.transform.position = new Vector3(0f, 0.85f, -0.25f);
            mainCam.transform.rotation = Quaternion.Euler(65f, 0f, 0f);

            EnsureComponent<CameraController>(mainCam.gameObject);
            EnsureComponent<PlayerController>(mainCam.gameObject);

            // 3. Criar a Bancada de Trabalho (Mesa)
            GameObject workbench = GameObject.Find("Workbench");
            if (workbench == null)
            {
                workbench = GameObject.CreatePrimitive(PrimitiveType.Cube);
                workbench.name = "Workbench";
                workbench.transform.position = new Vector3(0, -0.4f, 0);
                workbench.transform.localScale = new Vector3(2.5f, 0.8f, 1.5f);

                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.2f, 0.25f, 0.3f);
                workbench.GetComponent<Renderer>().material = mat;
            }

            // Aplicar alinhamento fixo de topo na Câmera
            var camCtrl = mainCam.GetComponent<CameraController>();
            if (camCtrl != null)
            {
                camCtrl.ApplyFixedTopDownView();
            }

            // 4. Criar Placa-Mãe Placeholder (Base com Slots)
            GameObject mbObj = GameObject.Find("Motherboard_Base");
            if (mbObj == null)
            {
                mbObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mbObj.name = "Motherboard_Base";
                mbObj.transform.position = new Vector3(0, 0.02f, 0);
                mbObj.transform.localScale = new Vector3(0.6f, 0.02f, 0.6f);

                var mbMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mbMat.color = new Color(0.05f, 0.4f, 0.15f); // Verde Placa-mãe
                mbObj.GetComponent<Renderer>().material = mbMat;

                // Criar Slots na Placa-mãe
                CreateSlot(mbObj.transform, "slot_cpu", ComponentType.CPU, SocketType.AM4, new Vector3(-0.1f, 0.02f, 0.1f));
                CreateSlot(mbObj.transform, "slot_ram_1", ComponentType.RAM, SocketType.DDR4, new Vector3(0.15f, 0.02f, 0.1f));
                CreateSlot(mbObj.transform, "slot_gpu", ComponentType.GPU, SocketType.PCIe_x16, new Vector3(0f, 0.02f, -0.15f));
                CreateSlot(mbObj.transform, "slot_ssd", ComponentType.Storage, SocketType.M2_NVMe, new Vector3(-0.15f, 0.02f, -0.1f));
                CreateSlot(mbObj.transform, "slot_psu", ComponentType.PSU, SocketType.ATX_24Pin, new Vector3(0.2f, 0.02f, -0.2f));
            }

            // 5. Criar Peças 3D Placeholders para Montagem na Bancada
            CreateComponentPlaceholder("CPU_Ryzen5_5600", "ryzen_5_5600", ComponentType.CPU, SocketType.AM4, new Vector3(-0.8f, 0.05f, 0.3f), new Vector3(0.12f, 0.02f, 0.12f), Color.red);
            CreateComponentPlaceholder("RAM_16GB_DDR4", "16gb_ddr4", ComponentType.RAM, SocketType.DDR4, new Vector3(-0.8f, 0.05f, 0.1f), new Vector3(0.03f, 0.08f, 0.25f), Color.blue);
            CreateComponentPlaceholder("GPU_RX7600", "rx_7600", ComponentType.GPU, SocketType.PCIe_x16, new Vector3(-0.8f, 0.08f, -0.2f), new Vector3(0.35f, 0.1f, 0.12f), Color.black);
            CreateComponentPlaceholder("SSD_1TB", "ssd_1tb", ComponentType.Storage, SocketType.M2_NVMe, new Vector3(0.8f, 0.03f, 0.3f), new Vector3(0.08f, 0.01f, 0.22f), Color.cyan);
            CreateComponentPlaceholder("PSU_650W", "650w", ComponentType.PSU, SocketType.ATX_24Pin, new Vector3(0.8f, 0.12f, -0.2f), new Vector3(0.3f, 0.2f, 0.25f), Color.grey);

            // Redescobrir slots no AssemblySystem
            if (assemblySystem != null)
            {
                assemblySystem.AutoDiscoverSlots();
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[SceneSetupUtility] Cena do Simulador montada e configurada com sucesso!");
        }

        private static T EnsureComponent<T>(GameObject obj) where T : Component
        {
            T comp = obj.GetComponent<T>();
            if (comp == null)
            {
                comp = obj.AddComponent<T>();
            }
            return comp;
        }

        private static void CreateSlot(Transform parent, string slotId, ComponentType type, SocketType socket, Vector3 localPos)
        {
            GameObject slotObj = new GameObject($"Slot_{slotId}");
            slotObj.transform.SetParent(parent);
            slotObj.transform.localPosition = localPos;
            slotObj.transform.localRotation = Quaternion.identity;

            var slot = slotObj.AddComponent<AssemblySlot>();
            // Configurar via SerializedObject para garantir persistência
            SerializedObject so = new SerializedObject(slot);
            so.FindProperty("slotId").stringValue = slotId;
            so.FindProperty("allowedType").enumValueIndex = (int)type;
            so.FindProperty("allowedSocket").enumValueIndex = (int)socket;
            so.ApplyModifiedProperties();
        }

        private static void CreateComponentPlaceholder(string objName, string id, ComponentType type, SocketType socket, Vector3 worldPos, Vector3 scale, Color color)
        {
            if (GameObject.Find(objName) != null) return;

            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = objName;
            obj.transform.position = worldPos;
            obj.transform.localScale = scale;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            obj.GetComponent<Renderer>().material = mat;

            var comp = obj.AddComponent<ComputerComponent>();

            // Criar Data Asset SO se não existir
            string path = $"Assets/_PCSimulator/ScriptableObjects/CatalogData/DA_{id}.asset";
            ComponentDataSO dataAsset = AssetDatabase.LoadAssetAtPath<ComponentDataSO>(path);

            if (dataAsset == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/_PCSimulator/ScriptableObjects"))
                {
                    AssetDatabase.CreateFolder("Assets/_PCSimulator", "ScriptableObjects");
                }
                if (!AssetDatabase.IsValidFolder("Assets/_PCSimulator/ScriptableObjects/CatalogData"))
                {
                    AssetDatabase.CreateFolder("Assets/_PCSimulator/ScriptableObjects", "CatalogData");
                }

                dataAsset = ScriptableObject.CreateInstance<ComponentDataSO>();
                SerializedObject dataSO = new SerializedObject(dataAsset);
                dataSO.FindProperty("componentId").stringValue = id;
                dataSO.FindProperty("displayName").stringValue = objName;
                dataSO.FindProperty("componentType").enumValueIndex = (int)type;
                dataSO.FindProperty("requiredSlotType").enumValueIndex = (int)socket;
                dataSO.ApplyModifiedProperties();

                AssetDatabase.CreateAsset(dataAsset, path);
                AssetDatabase.SaveAssets();
            }

            comp.Initialize(dataAsset);

            // Registrar no catálogo
            if (ComponentCatalog.Instance != null)
            {
                ComponentCatalog.Instance.RegisterComponent(dataAsset);
            }
        }
    }
}
#endif
