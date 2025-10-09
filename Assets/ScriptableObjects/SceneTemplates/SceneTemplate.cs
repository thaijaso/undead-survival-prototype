using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/SceneTemplate", fileName = "SceneTemplate")]
public class SceneTemplate : ScriptableObject
{
    [Tooltip("List of prefabs to instantiate when applying this template.")]
    public List<GameObject> prefabs = new List<GameObject>();
}
