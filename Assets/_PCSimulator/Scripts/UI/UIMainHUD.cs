using UnityEngine;
using UnityEngine.SceneManagement;
using PCSimulator.Core;
using PCSimulator.Assembly;
using PCSimulator.Networking;
using PCSimulator.Data;

namespace PCSimulator.UI
{
    public class UIMainHUD : MonoBehaviour
    {
        public static UIMainHUD Instance { get; private set; }

        private string currentBuildId = "Nenhuma";
        private int totalComponents = 0;
        private int installedComponentsCount = 0;
        private string statusNotification = "Aguardando payload JSON da Build...";
        private bool showReturnButton = false;

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
                BuildReceiver.Instance.OnParseError += HandleParseError;
            }
            if (AssemblySystem.Instance != null)
            {
                AssemblySystem.Instance.OnComponentInstalled += HandleComponentInstalled;
                AssemblySystem.Instance.OnComponentUninstalled += HandleComponentUninstalled;
            }
            if (PCSimulator.Power.PowerSystem.Instance != null)
            {
                PCSimulator.Power.PowerSystem.Instance.OnPowerSequenceStarted += HandlePowerStarted;
                PCSimulator.Power.PowerSystem.Instance.OnComputerBootSuccess += HandlePowerSuccess;
                PCSimulator.Power.PowerSystem.Instance.OnPowerFailed += HandleParseError;
            }
        }

        private void OnDestroy()
        {
            if (BuildReceiver.Instance != null)
            {
                BuildReceiver.Instance.OnBuildParsed -= HandleBuildParsed;
                BuildReceiver.Instance.OnParseError -= HandleParseError;
            }
            if (AssemblySystem.Instance != null)
            {
                AssemblySystem.Instance.OnComponentInstalled -= HandleComponentInstalled;
                AssemblySystem.Instance.OnComponentUninstalled -= HandleComponentUninstalled;
            }
            if (PCSimulator.Power.PowerSystem.Instance != null)
            {
                PCSimulator.Power.PowerSystem.Instance.OnPowerSequenceStarted -= HandlePowerStarted;
                PCSimulator.Power.PowerSystem.Instance.OnComputerBootSuccess -= HandlePowerSuccess;
                PCSimulator.Power.PowerSystem.Instance.OnPowerFailed -= HandleParseError;
            }
        }

        private void HandlePowerStarted()
        {
            statusNotification = "Iniciando sequencia de Power... Lendo BIOS e LEDs.";
        }

        private void HandlePowerSuccess()
        {
            statusNotification = "PC LIGADO COM SUCESSO! Tudo operante.";
            showReturnButton = true;
        }

        private void HandleBuildParsed(BuildConfiguration config)
        {
            showReturnButton = false;
            currentBuildId = config.BuildId;
            totalComponents = config.GetAllComponentIds().Count;
            installedComponentsCount = 0;
            statusNotification = $"Build '{config.BuildId}' carregada! Pecas na bancada: {totalComponents}";
        }

        private void HandleParseError(string error)
        {
            statusNotification = $"[ERRO] {error}";
        }

        private void HandleComponentInstalled(ComputerComponent component, AssemblySlot slot)
        {
            installedComponentsCount++;
            string cName = component.ComponentData != null ? component.ComponentData.DisplayName : component.name;
            statusNotification = $"Peca {cName} instalada no slot '{slot.SlotId}'";
            CheckAssemblyCompletion();
        }

        private void HandleComponentUninstalled(ComputerComponent component, AssemblySlot slot)
        {
            installedComponentsCount = Mathf.Max(0, installedComponentsCount - 1);
            statusNotification = $"Removido {component.name} do slot '{slot.SlotId}'";
        }

        private void CheckAssemblyCompletion()
        {
            if (totalComponents > 0 && installedComponentsCount >= totalComponents)
            {
                statusNotification = "Montagem Concluida! Pressione a tecla 'P' ou clique em Power para ligar o computador.";
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ChangeState(GameState.AssemblyCompleted);
                }
            }
        }

        private void OnGUI()
        {
            if (SceneManager.GetActiveScene().name == "MainMenu") return;
            if (PlayerPrefs.GetInt("DebugMode", 1) == 0) return;

            GUI.Box(new Rect(15, 15, 340, 185), "SIMULADOR DE MONTAGEM DE PC");

            string stateName = GameManager.Instance != null ? GameManager.Instance.CurrentState.ToString() : "N/A";
            GUI.Label(new Rect(25, 40, 320, 20), $"Estado Atual: {stateName}");
            GUI.Label(new Rect(25, 60, 320, 20), $"Build Ativa: {currentBuildId}");

            float percent = totalComponents > 0 ? ((float)installedComponentsCount / totalComponents) * 100f : 0f;
            GUI.Label(new Rect(25, 80, 320, 20), $"Progresso: {installedComponentsCount} / {totalComponents} ({percent:F0}%)");

            GUI.Label(new Rect(25, 105, 320, 45), $"Notificacao:\n{statusNotification}");

            GUI.Label(new Rect(25, 155, 320, 40), "Controles: [T] Simular JSON | [P] Power | [R] Rotacionar");

            if (showReturnButton)
            {
                if (GUI.Button(new Rect(15, 205, 340, 50), "VOLTAR AO MENU"))
                {
                    SceneManager.LoadScene("MainMenu");
                }
            }
        }
    }
}
