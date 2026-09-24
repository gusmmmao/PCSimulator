using UnityEngine;

namespace PCSimulator.Core
{
    /// <summary>
    /// Câmera Fixa com Visão Superior (Top-Down / Overhead View) posicionada estrategicamente
    /// sobre a bancada de montagem para controle de precisão.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Posicionamento Fixo da Câmera")]
        [SerializeField] private Vector3 fixedPosition = new Vector3(0f, 0.85f, -0.25f);
        [SerializeField] private Vector3 fixedRotation = new Vector3(65f, 0f, 0f);

        private void Start()
        {
            ApplyFixedTopDownView();
        }

        private void LateUpdate()
        {
            // Mantém a câmera perfeitamente fixa e alinhada acima da bancada
            transform.position = fixedPosition;
            transform.rotation = Quaternion.Euler(fixedRotation);
        }

        [ContextMenu("Aplicar Visão Superior")]
        public void ApplyFixedTopDownView()
        {
            transform.position = fixedPosition;
            transform.rotation = Quaternion.Euler(fixedRotation);
        }
    }
}
