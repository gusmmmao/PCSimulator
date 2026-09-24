using System;
using UnityEngine;
using Newtonsoft.Json;
using PCSimulator.Data;

namespace PCSimulator.Networking
{
    /// <summary>
    /// Receptor e validador de mensagens de Build enviadas em formato JSON.
    /// Sanitiza o payload, converte para FBuildConfiguration e dispara o evento OnBuildParsed.
    /// </summary>
    public class BuildReceiver : MonoBehaviour
    {
        public static BuildReceiver Instance { get; private set; }

        public event Action<BuildConfiguration> OnBuildParsed;
        public event Action<string> OnParseError;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public bool ProcessBuildJson(string jsonPayload)
        {
            if (string.IsNullOrWhiteSpace(jsonPayload))
            {
                const string error = "[BuildReceiver Error] Payload JSON vazio ou nulo recebido.";
                Debug.LogError(error);
                OnParseError?.Invoke(error);
                return false;
            }

            try
            {
                BuildConfiguration config = JsonConvert.DeserializeObject<BuildConfiguration>(jsonPayload);
                if (config == null)
                {
                    const string error = "[BuildReceiver Error] Falha ao desserializar JSON de Build.";
                    Debug.LogError(error);
                    OnParseError?.Invoke(error);
                    return false;
                }

                Debug.Log($"[BuildReceiver] Build '{config.BuildId}' processada com sucesso. Total de peças: {config.GetAllComponentIds().Count}");
                OnBuildParsed?.Invoke(config);
                return true;
            }
            catch (Exception ex)
            {
                string error = $"[BuildReceiver Exception] Erro ao interpretar JSON: {ex.Message}";
                Debug.LogError(error);
                OnParseError?.Invoke(error);
                return false;
            }
        }
    }
}
