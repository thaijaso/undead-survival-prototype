using UndeadSurvivalGame.PlayerSystems;
using UnityEngine;

public class InteriorTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Interior Trigger Entered by Player");
            // Additional logic for when the player enters the interior trigger
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.IsInside = true;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Interior Trigger Exited by Player");
            // Additional logic for when the player exits the interior trigger
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.IsInside = false;
            }
        }
    }
}
