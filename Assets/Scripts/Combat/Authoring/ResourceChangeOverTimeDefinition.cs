using Combat;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Resource Change Over Time")]
public class ResourceChangeOverTimeDefinition: ScriptableObject
{
    public ResourceChangeRequest Request;
    public float FrequencyPerSecond;
    public float Duration;
}
