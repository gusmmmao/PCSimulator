using System;
using UnityEngine;

namespace PCSimulator.Core
{
    public enum GameState
    {
        WaitingForBuild,
        LoadingBuild,
        BuildReady,
        AssemblyInProgress,
        AssemblyCompleted,
        PowerSequence,
        ComputerRunning
    }

    /// <summary>
    /// Gerenciador central de estado do simulador de montagem de PCs.
    /// Mantém desacoplamento utilizando padrão de eventos C#.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Estado Atual")]
        [SerializeField] private GameState currentState = GameState.WaitingForBuild;

        public GameState CurrentState => currentState;

        public event Action<GameState, GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            GameState previousState = currentState;
            currentState = newState;

            Debug.Log($"[GameManager] Estado alterado: {previousState} -> {currentState}");
            OnGameStateChanged?.Invoke(previousState, currentState);
        }
    }
}
