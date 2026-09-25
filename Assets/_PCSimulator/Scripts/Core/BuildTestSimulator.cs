using UnityEngine;
using PCSimulator.Networking;
using PCSimulator.Power;
using PCSimulator.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PCSimulator.Core
{
    /// <summary>
    /// Utilitário que inicializa a bancada e dispara o teste da Build de acordo com a opção escolhida no Menu Inicial.
    /// </summary>
    public class BuildTestSimulator : MonoBehaviour
    {
        public static BuildTestSimulator Instance { get; private set; }

        [Header("JSON de Teste de Exemplo")]
        [TextArea(10, 15)]
        [SerializeField] private string sampleBuildJson = @"{
  ""build_id"": ""TEST-BUILD-001"",
  ""cpu"": ""ryzen_5_5600"",
  ""motherboard"": ""b550m"",
  ""ram"": [""16gb_ddr4""],
  ""gpu"": ""rx_7600"",
  ""storage"": ""ssd_1tb"",
  ""psu"": ""650w"",
  ""case"": ""mid_tower""
}";

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
            // Lê a opção selecionada pelo jogador na Tela Inicial
            int mode = PlayerPrefs.GetInt("BuildMode", (int)BuildMode.TestJson);
            if (mode == (int)BuildMode.TestJson)
            {
                Debug.Log("[BuildTestSimulator] Modo Montagem de Teste ativo. Disparando JSON de teste automaticamente...");
                Invoke(nameof(TriggerTestBuild), 0.2f);
            }
            else if (mode == (int)BuildMode.QueueItem)
            {
                Debug.Log("[BuildTestSimulator] Modo Item de Fila ativo. Disparando JSON salvo da fila...");
                Invoke(nameof(TriggerQueueBuild), 0.2f);
            }
            else if (mode == (int)BuildMode.WebSocketLive)
            {
                Debug.Log("[BuildTestSimulator] Modo WebSocket Ativo. Conectando ao servidor e aguardando payload...");
                if (NetworkManager.Instance != null)
                {
                    NetworkManager.Instance.Connect();
                }
            }
        }

        private void Update()
        {
            bool isTPressed = false;
            bool isPPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                isTPressed = Keyboard.current.tKey.wasPressedThisFrame;
                isPPressed = Keyboard.current.pKey.wasPressedThisFrame;
            }
            else
#endif
            {
                try
                {
                    isTPressed = Input.GetKeyDown(KeyCode.T);
                    isPPressed = Input.GetKeyDown(KeyCode.P);
                }
                catch { }
            }

            if (isTPressed)
            {
                TriggerTestBuild();
            }

            if (isPPressed)
            {
                TriggerPowerButton();
            }
        }

        public void TriggerQueueBuild()
        {
            Debug.Log("[BuildTestSimulator] Disparando payload JSON salvo da Fila...");
            string json = PlayerPrefs.GetString("QueueItemJson", "");
            if (!string.IsNullOrEmpty(json) && NetworkManager.Instance != null)
            {
                NetworkManager.Instance.SimulateReceiveJson(json);
            }
        }

        [ContextMenu("Disparar JSON de Teste")]
        public void TriggerTestBuild()
        {
            Debug.Log("[BuildTestSimulator] Disparando teste de payload JSON...");
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.SimulateReceiveJson(sampleBuildJson);
            }
            else
            {
                Debug.LogWarning("[BuildTestSimulator] NetworkManager.Instance não foi encontrado na cena.");
            }
        }

        [ContextMenu("Pressionar Botão Power")]
        public void TriggerPowerButton()
        {
            Debug.Log("[BuildTestSimulator] Disparando teste do botão Power...");
            if (PowerSystem.Instance != null)
            {
                PowerSystem.Instance.TryPressPowerButton();
            }
            else
            {
                Debug.LogWarning("[BuildTestSimulator] PowerSystem.Instance não foi encontrado na cena.");
            }
        }
    }
}
