using UnityEngine;

[ExecuteAlways]
public class BallController : MonoBehaviour
{
    [SerializeField] private Material ProximityMaterial;

    private static int PlayerPosId = Shader.PropertyToID("_PlayerPosition");
    
    void Update()
    {
        Vector3 movement = Vector3.zero;
        
        if(Input.GetKey(KeyCode.A))
            movement += Vector3.left;
        if (Input.GetKey(KeyCode.W))
            movement += Vector3.forward;
        if (Input.GetKey((KeyCode.D)))
            movement += Vector3.right;
        if (Input.GetKey(KeyCode.S))
            movement += Vector3.back;
        transform.Translate(translation:Time.deltaTime*5*movement.normalized,Space.World);
        ProximityMaterial.SetVector(PlayerPosId,transform.position);
    }
}
