using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

namespace PCSimulator.UI
{
    public enum BuildMode
    {
        TestJson = 0,
        WebSocketLive = 1,
        QueueItem = 2
    }

    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager Instance { get; private set; }

        [Header("Configuracoes do Servidor WebSocket")]
        [SerializeField] private string serverUrl = "ws://localhost:8080/build";
        
        private string queueApiUrl = "http://localhost:8080/api/queue";
        private List<string> pendingQueueIds = new List<string>();
        private Dictionary<string, string> pendingQueueJsons = new Dictionary<string, string>();
        private bool isFetchingQueue = false;
        private string queueErrorMessage = "";

        private bool isSettingsOpen = false;
        private bool isQueuePanelOpen = false;
        private bool isDebugMode = true;

        private string[] mockQueueIds = new string[] { "ESTANDE-01-GAMER", "ESTANDE-02-OFFICE", "ESTANDE-03-HIGHEND" };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            serverUrl = PlayerPrefs.GetString("WebSocket_URL", "ws://localhost:8080/build");
            isDebugMode = PlayerPrefs.GetInt("DebugMode", 1) == 1;
        }

        public void StartTestBuild()
        {
            PlayerPrefs.SetInt("BuildMode", (int)BuildMode.TestJson);
            PlayerPrefs.Save();
            LoadAssemblyScene();
        }

        public void StartWebSocketBuild()
        {
            isQueuePanelOpen = true;
            FetchQueueFromAPI();
        }

        public void StartQueueBuild(string buildId)
        {
            PlayerPrefs.SetInt("BuildMode", (int)BuildMode.QueueItem);
            string jsonToSave = "";
            if (pendingQueueJsons.ContainsKey(buildId)) {
                jsonToSave = pendingQueueJsons[buildId];
            } else {
                jsonToSave = $@"{{
                    ""build_id"": ""{buildId}"",
                    ""cpu"": ""ryzen_5_5600"",
                    ""motherboard"": ""b550m"",
                    ""ram"": [""16gb_ddr4""],
                    ""gpu"": ""rx_7600"",
                    ""storage"": ""ssd_1tb"",
                    ""psu"": ""650w"",
                    ""case"": ""mid_tower""
                }}";
            }
            PlayerPrefs.SetString("QueueItemJson", jsonToSave);
            PlayerPrefs.Save();
            LoadAssemblyScene();
        }

        public void ToggleSettings()
        {
            isSettingsOpen = !isSettingsOpen;
        }

        public void ToggleQueuePanel()
        {
            isQueuePanelOpen = !isQueuePanelOpen;
            if (isQueuePanelOpen) FetchQueueFromAPI();
        }

        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void LoadAssemblyScene()
        {
            if (Application.CanStreamedLevelBeLoaded("MainLab")) SceneManager.LoadScene("MainLab");
            else if (Application.CanStreamedLevelBeLoaded("SampleScene")) SceneManager.LoadScene("SampleScene");
            else if (SceneManager.sceneCountInBuildSettings > 1) SceneManager.LoadScene(1);
        }

        private void FetchQueueFromAPI()
        {
            StartCoroutine(FetchQueueRoutine());
        }

        private IEnumerator FetchQueueRoutine()
        {
            isFetchingQueue = true;
            queueErrorMessage = "Buscando fila do servidor...";
            pendingQueueIds.Clear();
            pendingQueueJsons.Clear();

            string endpoint = queueApiUrl;
            if (string.IsNullOrEmpty(endpoint)) endpoint = serverUrl.Replace("ws://", "http://").Replace("wss://", "https://") + "/queue";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(endpoint))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    queueErrorMessage = "Offline (Mocks)";
                    foreach(var m in mockQueueIds) { pendingQueueIds.Add(m); }
                }
                else
                {
                    try
                    {
                        var list = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(webRequest.downloadHandler.text);
                        foreach(var item in list) {
                            if (item.ContainsKey("build_id")) {
                                string bId = item["build_id"].ToString();
                                pendingQueueIds.Add(bId);
                                pendingQueueJsons[bId] = JsonConvert.SerializeObject(item);
                            }
                        }
                        queueErrorMessage = $"Recebidos {pendingQueueIds.Count} pedidos.";
                    }
                    catch (System.Exception) { queueErrorMessage = "Erro JSON da Fila"; }
                }
            }
            isFetchingQueue = false;
        }

        private void OnGUI()
        {
            if (SceneManager.GetActiveScene().name != "MainMenu") return;

            int sw = Screen.width;
            int sh = Screen.height;
            float pW = 480;
            float pH = 450;
            float posX = (sw - pW) / 2f;
            float posY = (sh - pH) / 2f;

            GUI.Box(new Rect(posX, posY, pW, pH), "");

            GUIStyle tStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            tStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(posX, posY + 20, pW, 35), "SIMULADOR 3D DE PC", tStyle);

            GUIStyle sStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, alignment = TextAnchor.MiddleCenter };
            sStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f);
            GUI.Label(new Rect(posX, posY + 55, pW, 25), "Montagem 3D & Integracao", sStyle);

            if (isQueuePanelOpen)
            {
                GUIStyle qStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                qStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(posX, posY + 75, pW, 30), "FILA DE SOLICITACOES (ESTANDE)", qStyle);
                
                GUI.Label(new Rect(posX, posY + 105, pW, 25), queueErrorMessage, sStyle);

                float bW = 340;
                float bX = (sw - bW) / 2f;
                float sY = posY + 130;

                for (int i = 0; i < pendingQueueIds.Count && i < 5; i++)
                {
                    if (GUI.Button(new Rect(bX, sY + (i * 45), bW, 40), $"[ MONTAR ] {pendingQueueIds[i]}"))
                    {
                        StartQueueBuild(pendingQueueIds[i]);
                    }
                }

                if (GUI.Button(new Rect(bX, sY + (5 * 45) + 20, bW, 40), "VOLTAR AO MENU"))
                {
                    ToggleQueuePanel();
                }
            }
            else if (!isSettingsOpen)
            {
                float bW = 340; float bH = 48; float bX = (sw - bW) / 2f;
                if (GUI.Button(new Rect(bX, posY + 95, bW, bH), "MONTAGEM DE TESTE")) StartTestBuild();
                if (GUI.Button(new Rect(bX, posY + 155, bW, bH), "FILA DE PEDIDOS (API)")) StartWebSocketBuild();
                if (GUI.Button(new Rect(bX, posY + 215, bW, bH), "CONFIGURACOES")) ToggleSettings();
                if (GUI.Button(new Rect(bX, posY + 275, bW, bH), "SAIR DO JOGO")) QuitGame();
            }
            else
            {
                GUIStyle shStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                shStyle.normal.textColor = Color.cyan;
                GUI.Label(new Rect(posX, posY + 95, pW, 30), "CONFIGURACOES", shStyle);

                GUI.Label(new Rect(posX + 40, posY + 140, 400, 25), "URL WebSocket:");
                serverUrl = GUI.TextField(new Rect(posX + 40, posY + 170, 400, 32), serverUrl);

                GUI.Label(new Rect(posX + 40, posY + 210, 400, 25), "URL API Fila:");
                queueApiUrl = GUI.TextField(new Rect(posX + 40, posY + 235, 400, 32), queueApiUrl);

                isDebugMode = GUI.Toggle(new Rect(posX + 40, posY + 275, 400, 25), isDebugMode, " Habilitar HUD de DEBUG");

                if (GUI.Button(new Rect(posX + 40, posY + 315, 180, 42), "SALVAR"))
                {
                    PlayerPrefs.SetString("WebSocket_URL", serverUrl);
                    PlayerPrefs.SetInt("DebugMode", isDebugMode ? 1 : 0);
                    PlayerPrefs.Save();
                    ToggleSettings();
                }
                if (GUI.Button(new Rect(posX + 260, posY + 315, 180, 42), "VOLTAR")) ToggleSettings();
            }
        }
    }
}
