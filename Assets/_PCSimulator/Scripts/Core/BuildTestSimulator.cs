using UnityEngine;
using PCSimulator.Networking;
using PCSimulator.Power;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PCSimulator.Core
{
    /// <summary>
    /// Utilitário de teste para disparar o envio de um JSON de teste e validar todo o fluxo
    /// com suporte dual ao New Input System e Legacy Input.
    /// </summary>
    public class BuildTestSimulator : MonoBehaviour
    {
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
