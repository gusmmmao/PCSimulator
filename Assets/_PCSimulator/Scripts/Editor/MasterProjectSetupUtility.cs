using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using PCSimulator.Core;
using PCSimulator.Assembly;
using PCSimulator.Networking;
using PCSimulator.Data;
using PCSimulator.Catalog;
using PCSimulator.Power;
using PCSimulator.UI;

#if UNITY_EDITOR
namespace PCSimulator.Editor
{
    public static class MasterProjectSetupUtility
    {
        [MenuItem("PC Simulator/Setup Project")]
        public static void SetupProject()
        {
            string baseFolder = "Assets/_PCSimulator";
            CreateFolderStructure(baseFolder);

            string scenePath = $"{baseFolder}/Scenes/MainLab.unity";
            BuildMainScene(scenePath);

            string menuScenePath = $"{baseFolder}/Scenes/MainMenu.unity";
            BuildMainMenuScene(menuScenePath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateFolderStructure(string root)
        {
            string[] folders = {
                "Scenes",
                "Prefabs",
                "Scripts/Core",
                "Scripts/Assembly",
                "Scripts/Networking",
                "Scripts/Data",
                "Scripts/UI",
                "Scripts/Editor",
                "Scripts/Power",
                "ScriptableObjects/CatalogData"
            };

            foreach (var f in folders)
            {
                string path = $"{root}/{f}";
                if (!AssetDatabase.IsValidFolder(path))
                {
                    string parent = path.Substring(0, path.LastIndexOf('/'));
                    string folderName = path.Substring(path.LastIndexOf('/') + 1);
                    AssetDatabase.CreateFolder(parent, folderName);
                }
            }
        }

        private static void BuildMainScene(string scenePath)
        {
            Scene labScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Luz
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Cmera
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0, 1.5f, -1.2f);
            camObj.transform.rotation = Quaternion.Euler(45, 0, 0);

            // Sistemas Core
            GameObject systemsObj = new GameObject("GameSystems");
            var gameManager = EnsureComponent<GameManager>(systemsObj);
            var buildReceiver = EnsureComponent<BuildReceiver>(systemsObj);
            var assemblySystem = EnsureComponent<AssemblySystem>(systemsObj);
            var networkManager = EnsureComponent<NetworkManager>(systemsObj);
            var componentSpawner = EnsureComponent<BuildComponentSpawner>(systemsObj);
            var buildSimulator = EnsureComponent<BuildTestSimulator>(systemsObj);
            var powerSystem = EnsureComponent<PowerSystem>(systemsObj);

            // HUD
            GameObject hudObj = new GameObject("UIMainHUD");
            hudObj.AddComponent<UI.UIMainHUD>();

            // Setup Catlogo
            GameObject catalogObj = new GameObject("ComponentCatalog");
            var catalog = EnsureComponent<ComponentCatalog>(catalogObj);
            
            // Cenrio Bsico
            GameObject envObj = new GameObject("Environment");
            
            GameObject tableObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tableObj.name = "Table";
            tableObj.transform.SetParent(envObj.transform);
            tableObj.transform.position = new Vector3(0, -0.05f, 0);
            tableObj.transform.localScale = new Vector3(2f, 0.1f, 1f);
            var tableMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            tableMat.color = new Color(0.4f, 0.4f, 0.4f);
            tableObj.GetComponent<Renderer>().material = tableMat;

            GameObject spawnPointObj = new GameObject("SpawnPoint");
            spawnPointObj.transform.SetParent(envObj.transform);
            spawnPointObj.transform.position = new Vector3(-0.8f, 0.05f, 0.3f);
            
// Removed workbenchSpawnPoint setup

            // Placa Me Root
            GameObject mbRoot = new GameObject("MotherboardBase");
            mbRoot.transform.position = new Vector3(0, 0, 0);
            
            GameObject mbMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mbMesh.name = "MB_Mesh";
            mbMesh.transform.SetParent(mbRoot.transform);
            mbMesh.transform.localPosition = new Vector3(0, 0.01f, 0);
            mbMesh.transform.localScale = new Vector3(0.6f, 0.02f, 0.6f);
            
            var mbMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mbMat.color = new Color(0.1f, 0.5f, 0.1f);
            mbMesh.GetComponent<Renderer>().material = mbMat;

            // Slots ATX Reais
            CreateVisualSlot(mbRoot.transform, "slot_cpu", ComponentType.CPU, SocketType.AM4, new Vector3(0f, 0.02f, 0.15f));
            CreateVisualSlot(mbRoot.transform, "slot_ram_1", ComponentType.RAM, SocketType.DDR4, new Vector3(0.18f, 0.02f, 0.15f));
            CreateVisualSlot(mbRoot.transform, "slot_gpu", ComponentType.GPU, SocketType.PCIe_x16, new Vector3(0f, 0.02f, -0.05f));
            CreateVisualSlot(mbRoot.transform, "slot_ssd", ComponentType.Storage, SocketType.M2_NVMe, new Vector3(0.15f, 0.02f, -0.15f));
            
            // PSU FORA DA PLACA ME
            CreateVisualSlot(envObj.transform, "slot_psu", ComponentType.PSU, SocketType.ATX_24Pin, new Vector3(0.6f, 0.1f, -0.2f));

            // Componentes
            CreateComponentDataAndRegister(catalog, "CPU_Ryzen5_5600", "ryzen_5_5600", ComponentType.CPU, SocketType.AM4, new Vector3(0.1f, 0.02f, 0.1f), Color.red);
            CreateComponentDataAndRegister(catalog, "RAM_16GB_DDR4", "16gb_ddr4", ComponentType.RAM, SocketType.DDR4, new Vector3(0.02f, 0.08f, 0.25f), Color.blue);
            CreateComponentDataAndRegister(catalog, "GPU_RX7600", "rx_7600", ComponentType.GPU, SocketType.PCIe_x16, new Vector3(0.3f, 0.12f, 0.08f), Color.black);
            CreateComponentDataAndRegister(catalog, "SSD_1TB", "ssd_1tb", ComponentType.Storage, SocketType.M2_NVMe, new Vector3(0.04f, 0.01f, 0.12f), Color.cyan);
            CreateComponentDataAndRegister(catalog, "PSU_650W", "650w", ComponentType.PSU, SocketType.ATX_24Pin, new Vector3(0.25f, 0.2f, 0.25f), Color.gray);
            CreateComponentDataAndRegister(catalog, "Motherboard_B550M", "b550m", ComponentType.Motherboard, SocketType.AM4, new Vector3(0.6f, 0.02f, 0.6f), Color.green);
            CreateComponentDataAndRegister(catalog, "Case_MidTower", "mid_tower", ComponentType.Case, SocketType.None, new Vector3(0.7f, 0.7f, 0.7f), Color.white);

            if (assemblySystem != null)
            {
                assemblySystem.AutoDiscoverSlots();
            }

            EditorSceneManager.SaveScene(labScene, scenePath);
            Debug.Log($"[MasterProjectSetupUtility] Cena MainLab criada em: {scenePath}");
        }

        private static void BuildMainMenuScene(string scenePath)
        {
            Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cmera de Fundo Limpa do Menu
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.1f, 0.14f);

            // Gerenciador da UI do Menu
            GameObject menuManagerObj = new GameObject("MainMenuManager");
            menuManagerObj.AddComponent<MainMenuManager>();

            EditorSceneManager.SaveScene(menuScene, scenePath);
            Debug.Log($"[MasterProjectSetupUtility] Cena MainMenu limpa criada em: {scenePath}");
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

        private static void CreateVisualSlot(Transform parent, string slotId, ComponentType type, SocketType socket, Vector3 localPos)
        {
            GameObject slotObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slotObj.name = $"Slot_{slotId}";
            slotObj.transform.SetParent(parent);
            slotObj.transform.localPosition = localPos;
            slotObj.transform.localRotation = Quaternion.identity;

            Vector3 slotVisualScale = type switch
            {
                ComponentType.CPU => new Vector3(0.11f, 0.015f, 0.11f),
                ComponentType.RAM => new Vector3(0.03f, 0.025f, 0.26f),
                ComponentType.GPU => new Vector3(0.31f, 0.025f, 0.09f),
                ComponentType.Storage => new Vector3(0.05f, 0.015f, 0.13f),
                ComponentType.PSU => new Vector3(0.26f, 0.03f, 0.26f),
                _ => new Vector3(0.1f, 0.02f, 0.1f)
            };

            slotObj.transform.localScale = slotVisualScale;

            Color slotColor = type switch
            {
                ComponentType.CPU => new Color(0.25f, 0.25f, 0.25f),
                ComponentType.RAM => new Color(0.1f, 0.1f, 0.35f),
                ComponentType.GPU => new Color(0.05f, 0.05f, 0.05f),
                ComponentType.Storage => new Color(0.35f, 0.35f, 0.4f),
                ComponentType.PSU => new Color(0.15f, 0.15f, 0.15f),
                _ => Color.gray
            };

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Configurar como material Transparente (Holograma)
            mat.SetFloat("_Surface", 1.0f);
            mat.SetFloat("_Blend", 0.0f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            
            mat.color = new Color(slotColor.r, slotColor.g, slotColor.b, 0.4f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            
            slotObj.GetComponent<Renderer>().material = mat;

            var col = slotObj.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            var slotComponent = slotObj.AddComponent<AssemblySlot>();
            SerializedObject so = new SerializedObject(slotComponent);
            so.FindProperty("slotId").stringValue = slotId;
            so.FindProperty("allowedType").enumValueIndex = (int)type;
            so.FindProperty("allowedSocket").enumValueIndex = (int)socket;
            so.ApplyModifiedProperties();
        }

        private static void CreateComponentDataAndRegister(ComponentCatalog catalog, string objName, string id, ComponentType type, SocketType socket, Vector3 scale, Color color)
        {
            string path = $"Assets/_PCSimulator/ScriptableObjects/CatalogData/DA_{id}.asset";
            ComponentDataSO dataAsset = AssetDatabase.LoadAssetAtPath<ComponentDataSO>(path);

            if (dataAsset == null)
            {
                dataAsset = ScriptableObject.CreateInstance<ComponentDataSO>();
                SerializedObject dataSO = new SerializedObject(dataAsset);
                dataSO.FindProperty("componentId").stringValue = id;
                dataSO.FindProperty("displayName").stringValue = objName;
                dataSO.FindProperty("componentType").enumValueIndex = (int)type;
                dataSO.FindProperty("requiredSlotType").enumValueIndex = (int)socket;
                dataSO.FindProperty("dimensions").vector3Value = scale;
                dataSO.ApplyModifiedProperties();

                AssetDatabase.CreateAsset(dataAsset, path);
                AssetDatabase.SaveAssets();
            }
            else
            {
                // Atualiza dimenses se j existir
                SerializedObject dataSO = new SerializedObject(dataAsset);
                dataSO.FindProperty("dimensions").vector3Value = scale;
                dataSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(dataAsset);
            }

            if (catalog != null)
            {
                SerializedObject catalogSO = new SerializedObject(catalog);
                SerializedProperty listProp = catalogSO.FindProperty("registeredComponents");
                
                if (listProp != null)
                {
                    bool alreadyExists = false;
                    for (int i = 0; i < listProp.arraySize; i++)
                    {
                        if (listProp.GetArrayElementAtIndex(i).objectReferenceValue == dataAsset)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }
                    if (!alreadyExists)
                    {
                        listProp.arraySize++;
                        listProp.GetArrayElementAtIndex(listProp.arraySize - 1).objectReferenceValue = dataAsset;
                        catalogSO.ApplyModifiedProperties();
                    }
                }
            }
        }
    }
}
#endif



