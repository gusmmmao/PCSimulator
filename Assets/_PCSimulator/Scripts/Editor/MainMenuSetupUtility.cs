#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using PCSimulator.UI;

namespace PCSimulator.EditorTools
{
    /// <summary>
    /// Utilitário de Editor que gera automaticamente a cena limpa da Tela Inicial (MainMenu.unity)
    /// e registra a ordem das cenas no Build Settings do Unity (0=MainMenu, 1=MainLab).
    /// </summary>
    public static class MainMenuSetupUtility
    {
        [MenuItem("PC Simulator/Criar Tela Inicial (Main Menu)", false, 2)]
        public static void CreateMainMenuScene()
        {
            Debug.Log("[MainMenuSetupUtility] Gerando a cena limpa da Tela Inicial...");

            if (!AssetDatabase.IsValidFolder("Assets/_PCSimulator/Scenes"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_PCSimulator"))
                {
                    AssetDatabase.CreateFolder("Assets", "_PCSimulator");
                }
                AssetDatabase.CreateFolder("Assets/_PCSimulator", "Scenes");
            }

            string mainMenuScenePath = "Assets/_PCSimulator/Scenes/MainMenu.unity";
            string mainLabScenePath = "Assets/_PCSimulator/Scenes/MainLab.unity";

            // Salva alterações atuais antes de criar a cena
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // Criar Nova Cena 100% Limpa apenas com Camera e UI do Menu
            var menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Câmera de Fundo da Tela Inicial
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.1f, 0.14f); // Azul escuro corporativo

            // GameObject do Gerenciador do Menu
            GameObject menuManagerObj = new GameObject("MainMenuManager");
            menuManagerObj.AddComponent<MainMenuManager>();

            // Salvar a Cena do Menu
            EditorSceneManager.SaveScene(menuScene, mainMenuScenePath);
            Debug.Log($"[MainMenuSetupUtility] Cena da Tela Inicial criada com sucesso em: {mainMenuScenePath}");

            // Registrar Build Settings (0: MainMenu, 1: MainLab / SampleScene)
            List<EditorBuildSettingsScene> scenesList = new List<EditorBuildSettingsScene>();
            scenesList.Add(new EditorBuildSettingsScene(mainMenuScenePath, true));

            if (System.IO.File.Exists(mainLabScenePath))
            {
                scenesList.Add(new EditorBuildSettingsScene(mainLabScenePath, true));
            }
            else
            {
                // Adiciona qualquer outra cena como Cena 1
                string activePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                if (!string.IsNullOrEmpty(activePath) && activePath != mainMenuScenePath)
                {
                    scenesList.Add(new EditorBuildSettingsScene(activePath, true));
                }
            }

            EditorBuildSettings.scenes = scenesList.ToArray();
            Debug.Log($"[MainMenuSetupUtility] Build Settings configurado: 0={mainMenuScenePath}, 1={(scenesList.Count > 1 ? scenesList[1].path : "N/A")}");
        }
    }
}
#endif
