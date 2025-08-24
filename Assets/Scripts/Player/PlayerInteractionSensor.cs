using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

public class PlayerInteractionSensor : MonoBehaviour
{
    public float arrowColliderRadius = 10f;
    public float buttonLineOfSightDistance = 5f;
    public float lineofSightRadius = 0.5f;
    private HashSet<ProximityUI> prevInteractables = new();
    private ProximityUI prevLineOfSightButton = null;

    // Update is called once per frame
    void Update()
    {
        ToggleArrowsForNearbyInteractables();
        ToggleButtonByLineOfSight();
    }

    // Show arrow UI for nearby interactables
    private void ToggleArrowsForNearbyInteractables()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, arrowColliderRadius, LayerMask.GetMask("Interactable"));
        HashSet<ProximityUI> curNearbyInteractables = new();

        // Enable arrows for interactables that are in range
        foreach (Collider hit in hits)
        {
            ProximityUI proximityUI = hit.GetComponent<ProximityUI>();
            if (proximityUI != null)
            {
                proximityUI.EnableArrow();
                curNearbyInteractables.Add(proximityUI);
            }
        }

        // Disable arrows for interactables that are no longer in range
        foreach (ProximityUI prevInteractable in prevInteractables)
        {
            if (!curNearbyInteractables.Contains(prevInteractable))
            {
                prevInteractable.DisableArrow();
            }
        }

        prevInteractables = curNearbyInteractables;
    }

    private void ToggleButtonByLineOfSight()
    {
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Center of the screen

        ProximityUI lineOfSightButton = null;

        if (Physics.SphereCast(ray, lineofSightRadius,out RaycastHit hit, buttonLineOfSightDistance, LayerMask.GetMask("Interactable")))
        {
            ProximityUI interactable = hit.collider.GetComponent<ProximityUI>();

            if (interactable != null && prevInteractables.Contains(interactable))
            {
                lineOfSightButton = interactable;
            }
        }

        if (prevLineOfSightButton != null && prevLineOfSightButton != lineOfSightButton)
        {
            prevLineOfSightButton.DisableButton();
        }

        lineOfSightButton?.EnableButton();
        prevLineOfSightButton = lineOfSightButton;
    }

    void OnDrawGizmos()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Vector3 start = ray.origin;
            Vector3 end = ray.origin + ray.direction * buttonLineOfSightDistance;

            // Draw the start sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(start, lineofSightRadius);

            // Draw the end sphere
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(end, lineofSightRadius);

            // Draw the line between start and end
            Gizmos.color = Color.red;
            Gizmos.DrawLine(start, end);
        }
    }
}
