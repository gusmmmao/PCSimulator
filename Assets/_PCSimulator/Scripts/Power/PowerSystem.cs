using System;
using System.Collections;
using UnityEngine;
using PCSimulator.Core;
using PCSimulator.Assembly;

namespace PCSimulator.Power
{
    /// <summary>
    /// Sistema responsável por validar as condições elétricas e sequenciar a inicialização (Power On).
    /// </summary>
    public class PowerSystem : MonoBehaviour
    {
        public static PowerSystem Instance { get; private set; }

        public event Action OnPowerSequenceStarted;
        public event Action OnComputerBootSuccess;
        public event Action<string> OnPowerFailed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private bool isPoweredOn = false;

        public bool TryPressPowerButton()
        {
            if (isPoweredOn) return false;

            Debug.Log("[PowerSystem] Botão Power pressionado. Validando montagem...");

            if (!ValidateRequiredComponents(out string failureReason))
            {
                Debug.LogError($"[PowerSystem Error] Falha na energização: {failureReason}");
                OnPowerFailed?.Invoke(failureReason);
                return false;
            }

            isPoweredOn = true;
            StartCoroutine(PowerSequenceRoutine());
            return true;
        }

        private bool ValidateRequiredComponents(out string failureReason)
        {
            // Valida se o GameManager existe e o sistema está pronto
            failureReason = string.Empty;

            var slots = FindObjectsByType<AssemblySlot>(FindObjectsSortMode.None);
            if (slots.Length == 0)
            {
                failureReason = "Nenhum slot cadastrado no sistema.";
                return false;
            }

            foreach (var slot in slots)
            {
                if (!slot.IsOccupied)
                {
                    failureReason = $"Slot '{slot.SlotId}' está vazio. Todos os componentes devem estar instalados.";
                    return false;
                }
            }

            return true;
        }

        private IEnumerator PowerSequenceRoutine()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.PowerSequence);
            }

            OnPowerSequenceStarted?.Invoke();
            Debug.Log("[PowerSystem] Lendo LEDs, iniciando rotação das ventoinhas e iluminação RGB...");

            yield return new WaitForSeconds(1.5f);

            Debug.Log("[PowerSystem] BIOS inicializada. Computador ligado com sucesso (BEEP)!");

            // Feedback visual: Muda a cor dos componentes instalados para parecerem "Ligados"
            var slots = FindObjectsByType<AssemblySlot>(FindObjectsSortMode.None);
            foreach (var slot in slots)
            {
                if (slot.IsOccupied && slot.InstalledComponent != null)
                {
                    var renderer = slot.InstalledComponent.GetComponentInChildren<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        Color currentColor = renderer.material.color;
                        // Torna a cor mais vibrante (RGB mode) e adiciona emissão
                        renderer.material.EnableKeyword("_EMISSION");
                        renderer.material.SetColor("_EmissionColor", currentColor * 1.5f);
                    }
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.ComputerRunning);
            }

            OnComputerBootSuccess?.Invoke();
        }
    }
}

