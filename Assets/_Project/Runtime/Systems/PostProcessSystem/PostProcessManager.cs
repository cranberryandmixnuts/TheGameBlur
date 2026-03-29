using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class PostProcessManager : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private Volume volume;
    [SerializeField] private Transform player;

    private void Update()
    {
        float distance = Vector3.Distance(camera.transform.position, player.transform.position);
        volume.profile.TryGet(out DepthOfField dof);
        dof.focusDistance.value = distance;
    }
}
