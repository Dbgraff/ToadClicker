using UnityEngine;
using UnityEngine.InputSystem;

public class ToadClickHandler : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private ParticleSystem clickParticles;

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Debug.Log("🐸 Клик по жабе через Input System!");
                    ToadGameManager.Instance?.OnToadClick();
                }
            }
        }
    }
}
