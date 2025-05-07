using UnityEngine;

public class PowerUpButtonHandler : MonoBehaviour
{
    public HeroMovement hero;

    private void Awake()
    {
        if (hero == null)
        {
            hero = FindObjectOfType<HeroMovement>();
            Debug.Log("⚠️ PowerUpButtonHandler: Hero setat automat în Awake(): " + hero?.name);
        }
    }

    private void Start()
    {
        if (hero == null)
        {
            hero = FindObjectOfType<HeroMovement>();
            Debug.Log("⚠️ PowerUpButtonHandler: Hero setat automat în Start(): " + hero?.name);
        }
    }


    public void OnPowerUpClick()
    {
        if (hero != null)
        {
            hero.ExploreNearestBranch();
        }
        else
        {
            Debug.LogWarning("❌ PowerUpButtonHandler: Hero e NULL!");
        }
    }
}
