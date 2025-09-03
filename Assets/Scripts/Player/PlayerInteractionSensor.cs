using System.Collections.Generic;
using UndeadSurvivalGame.Gameplay;
using UndeadSurvivalGame.UI;
using UnityEngine;

namespace UndeadSurvivalGame.PlayerSystems
{
    public class InteractionSensor : MonoBehaviour
    {
        public IInteractable CurrentInteractable { get; private set; } = null;
        public float arrowColliderRadius = 10f;
        public float buttonLineOfSightDistance = 5f;
        public float lineofSightRadius = 0.5f;
        private HashSet<ProximityUI> activeArrowsUI = new();
        private ProximityUI focusedProximityUI = null;

        // Update is called once per frame
        void Update()
        {
            ToggleArrowsForNearbyInteractables();
            ToggleButtonAndTextByLineOfSight();
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
                if (proximityUI != null && proximityUI.gameObject != null)
                {
                    proximityUI.ShowArrowIndicator();
                    curNearbyInteractables.Add(proximityUI);
                }
            }

            // Hide arrows for all except the focused one if there is a focused interactable 
            if (focusedProximityUI != null)
            {
                foreach (ProximityUI activeArrowUI in activeArrowsUI)
                {
                    if (activeArrowUI != focusedProximityUI && activeArrowUI != null)
                    {
                        activeArrowUI.HideArrowIndicator();
                    }
                }
            }


            // Disable arrows for interactables that are no longer in range
            foreach (ProximityUI activeArrowUI in activeArrowsUI)
            {
                if (activeArrowUI == null)
                    continue; // Skip destroyed objects

                if (!curNearbyInteractables.Contains(activeArrowUI))
                {
                    if (activeArrowUI != null)
                    {
                        activeArrowUI.HideArrowIndicator();
                    }
                }
            }

            activeArrowsUI = curNearbyInteractables;
            activeArrowsUI.RemoveWhere(ui => ui == null); // Clean up any null references
        }

        private void ToggleButtonAndTextByLineOfSight()
        {
            Camera cam = Camera.main;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Center of the screen

            ProximityUI curProximityUI = null;

            if (Physics.SphereCast(ray, lineofSightRadius, out RaycastHit hit, buttonLineOfSightDistance, LayerMask.GetMask("Interactable")))
            {
                ProximityUI proximityUI = hit.collider.GetComponent<ProximityUI>();

                if (proximityUI != null && activeArrowsUI.Contains(proximityUI))
                {
                    curProximityUI = proximityUI;
                }
            }

            if (focusedProximityUI != null && focusedProximityUI != curProximityUI)
            {
                focusedProximityUI.HidePickupButton();
                focusedProximityUI.HideTextBackground();
                ResetItemPickupText(focusedProximityUI);
            }

            CurrentInteractable = curProximityUI?.GetComponent<IInteractable>();
            curProximityUI?.ShowPickupButton();
            curProximityUI?.ShowTextBackground();
            focusedProximityUI = curProximityUI;
        }

        private void ResetItemPickupText(ProximityUI proximityUI)
        {
            ItemPickupInteractable itemPickup = proximityUI.GetComponent<ItemPickupInteractable>();

            if (itemPickup != null && itemPickup.itemStack != null)
            {
                proximityUI.DisplayPickupPrompt(itemPickup.itemStack.item.itemName, itemPickup.itemStack.quantity);
            }
            else
            {
                Debug.LogWarning($"ResetItemPickupText(): {proximityUI.name} has no ItemPickupInteractable component.");
            }
        }

        public void RemoveProximityUIRefs(ProximityUI proximityUI)
        {
            if (activeArrowsUI.Contains(proximityUI))
            {
                activeArrowsUI.Remove(proximityUI);
            }

            if (focusedProximityUI == proximityUI)
            {
                focusedProximityUI = null;
                CurrentInteractable = null;
            }
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
}
