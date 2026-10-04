using UnityEngine;
using UnityEngine.InputSystem;

public class GameSpeedController : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float normalSpeed = 1f;
    [SerializeField] private float fastSpeed = 2f;

    private bool isFastSpeed = false;

    private void Update()
    {
        // Check if a physical keyboard is connected and the Space key was pressed this frame
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ToggleSpeed();
        }
    }

    public void ToggleSpeed()
    {
        isFastSpeed = !isFastSpeed;
        Time.timeScale = isFastSpeed ? fastSpeed : normalSpeed;

        Debug.Log($"[GameSpeedController] Time Scale set to: {Time.timeScale}x");
    }

    private void OnDisable()
    {
        // Reset back to standard timescale when switching scenes or pausing
        Time.timeScale = 1f;
    }
}