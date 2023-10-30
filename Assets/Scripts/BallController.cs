
using Unity.Mathematics;
using UnityEngine;
[ExecuteAlways]
public class BallController : MonoBehaviour
{
    [SerializeField] private Material ProximityMaterial;

    private static int PlayerPosId = Shader.PropertyToID("_PlayerPosition");
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
        Vector4 posit = new Vector4(transform.position.x, transform.position.y, transform.position.z, 0f);
        ProximityMaterial.SetVector(PlayerPosId,posit);
    }
}
