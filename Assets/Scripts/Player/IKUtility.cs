using RootMotion.FinalIK;
using UnityEngine;

public static class IKUtility
{
    // Helper to map string effector names to FullBodyBipedEffector enum
    public static FullBodyBipedEffector? StringToFullBodyBipedEffector(string effectorName)
    {
        switch (effectorName.ToLowerInvariant())
        {
            case "body": return FullBodyBipedEffector.Body;
            case "left shoulder": return FullBodyBipedEffector.LeftShoulder;
            case "right shoulder": return FullBodyBipedEffector.RightShoulder;
            case "left thigh": return FullBodyBipedEffector.LeftThigh;
            case "right thigh": return FullBodyBipedEffector.RightThigh;
            case "left hand": return FullBodyBipedEffector.LeftHand;
            case "right hand": return FullBodyBipedEffector.RightHand;
            case "left foot": return FullBodyBipedEffector.LeftFoot;
            case "right foot": return FullBodyBipedEffector.RightFoot;
            default:
                Debug.LogWarning($"[IKUtility] Unknown effector name '{effectorName}' for FullBodyBipedEffector mapping.");
                return null;
        }
    }
}
