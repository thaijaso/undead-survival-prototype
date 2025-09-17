using System.Collections.Generic;
using UndeadSurvivalGame.Gameplay;
using UndeadSurvivalGame.UI;
using UnityEngine;

namespace UndeadSurvivalGame.PlayerSystems
{
    public class InteractionSensor : MonoBehaviour
    {
        public IInteractable CurrentInteractable { get; private set; } = null;
        public float ArrowDetectionRadius = 10f;
        public float PickupButtonDistance = 5f;
        public float LineOfSightSphereRadius = 0.5f;
        private HashSet<ProximityUI> NearbyProximityUIs = new();
        private ProximityUI focusedProximityUI = null;
        public HashSet<ProximityUI> ProximityUIs => NearbyProximityUIs;

        private void Update()
        {
            ToggleArrowsForNearbyInteractables();
            ToggleButtonAndTextByLineOfSight();
        }

        // Show arrow UI for nearby interactables
        private void ToggleArrowsForNearbyInteractables()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, ArrowDetectionRadius, LayerMask.GetMask("Interactable"));
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
                foreach (ProximityUI activeArrowUI in NearbyProximityUIs)
                {
                    if (activeArrowUI != focusedProximityUI && activeArrowUI != null)
                    {
                        activeArrowUI.HideArrowIndicator();
                    }
                }
            }


            // Disable arrows for interactables that are no longer in range
            foreach (ProximityUI activeArrowUI in NearbyProximityUIs)
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

            NearbyProximityUIs = curNearbyInteractables;
            NearbyProximityUIs.RemoveWhere(ui => ui == null); // Clean up any null references
        }

        private void ToggleButtonAndTextByLineOfSight()
        {
            Camera cam = Camera.main;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Center of the screen

            ProximityUI curProximityUI = null;

            if (Physics.SphereCast(ray, LineOfSightSphereRadius, out RaycastHit hit, PickupButtonDistance, LayerMask.GetMask("Interactable")))
            {
                ProximityUI proximityUI = hit.collider.GetComponent<ProximityUI>();

                if (proximityUI != null && NearbyProximityUIs.Contains(proximityUI))
                {
                    curProximityUI = proximityUI;
                }
            }

            if (focusedProximityUI != null && focusedProximityUI != curProximityUI)
            {
                focusedProximityUI.HidePickupButton();
                focusedProximityUI.HideContent();
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
                proximityUI.SetPickupPrompt(itemPickup.itemStack.item.ItemName, itemPickup.itemStack.quantity);
            }
            else
            {
                Debug.LogWarning($"ResetItemPickupText(): {proximityUI.name} has no ItemPickupInteractable component.");
            }
        }

        public void RemoveProximityUIRefs(ProximityUI proximityUI)
        {
            if (NearbyProximityUIs.Contains(proximityUI))
            {
                NearbyProximityUIs.Remove(proximityUI);
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
                Vector3 end = ray.origin + ray.direction * PickupButtonDistance;

                // Draw the start sphere
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(start, LineOfSightSphereRadius);

                // Draw the end sphere
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(end, LineOfSightSphereRadius);

                // Draw the line between start and end
                Gizmos.color = Color.red;
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
