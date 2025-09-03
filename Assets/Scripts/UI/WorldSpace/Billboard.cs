using UnityEngine;

namespace UndeadSurvivalGame.UI
{ 
    public class Billboard : MonoBehaviour
    {
        void LateUpdate()
        {
            // Keep the billboard facing the camera
            if (Camera.main)
            {
                transform.forward = Camera.main.transform.forward;
            }
        }
    }
}
