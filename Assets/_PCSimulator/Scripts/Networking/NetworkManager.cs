using System;
using System.Collections;
using UnityEngine;

namespace PCSimulator.Networking
{
    /// <summary>
    /// Gerenciador de conexão de rede/WebSocket isolado da camada de gameplay.
    /// Suporta escuta de eventos, reconexão e simulação local de recepção de JSON.
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [Header("Configurações de Conexão")]
        [SerializeField] private string serverUrl = "ws://localhost:8080/build";
        [SerializeField] private bool autoConnectOnStart = false;

        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<string> OnRawMessageReceived;

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
            if (autoConnectOnStart)
            {
                Connect();
            }
        }

        public void Connect()
        {
            Debug.Log($"[NetworkManager] Conectando ao servidor em {serverUrl}...");
            // Em ambiente real, inicializa a conexão com o WebSocket.
            // Por hora, emite o evento de conexão concluída.
            OnConnected?.Invoke();
        }

        public void Disconnect()
        {
            Debug.Log("[NetworkManager] Desconectando do servidor...");
            OnDisconnected?.Invoke();
        }

        /// <summary>
        /// Método utilitário para disparar manualmente uma mensagem JSON para testes ou debug.
        /// </summary>
        public void SimulateReceiveJson(string jsonPayload)
        {
            Debug.Log("[NetworkManager Debug] Simulando recebimento de payload JSON de Build...");
            OnRawMessageReceived?.Invoke(jsonPayload);

            if (BuildReceiver.Instance != null)
            {
                BuildReceiver.Instance.ProcessBuildJson(jsonPayload);
            }
        }
    }
}
