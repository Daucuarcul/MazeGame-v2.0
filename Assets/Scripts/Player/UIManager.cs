using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject powerUpButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Ascundem butonul la început
        if (powerUpButton != null)
            powerUpButton.SetActive(false);
    }

    public void SetPowerUpButtonActive(bool state)
    {
        //Debug.Log("🔘 Buton PowerUp: " + (state ? "ON" : "OFF")); // ✅ Afișează în consolă
        if (powerUpButton != null)
            powerUpButton.SetActive(state);
    }
}
