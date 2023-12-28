using System.Collections;
using UnityEngine;

public class Agent : MonoBehaviour {
    [Range(0.5f, 20.0f)]
    public float maxDecisionInterval = 10.0f;
    [Range(0.1f, 10.0f)]
    public float speed = 1.0f;
    [Range(10.0f, 360*4.0f)]
    public float maxRotationSpeed = 360.0f;
    public bool drawDebugGizmos = false;
    public MeshCollider platform;

    private float _direction = 1.0f;
    private Vector3 _circleCenter;
    private float _circleRadius;
    
    void Start() {
        StartCoroutine(ChooseNextCircle());
    }

    private IEnumerator ChooseNextCircle() {
        while (true) {
            // Subtract the Agent radius to avoid having half the agent off the platform
            _circleRadius = Random.Range(float.Epsilon, MaxRadius(_direction)-GetComponent<Collider>().bounds.extents.x*0.5f);
            _circleCenter = transform.position + _direction*transform.right * _circleRadius;

            yield return new WaitForSeconds(Random.Range(float.Epsilon, maxDecisionInterval));
            _direction *= -1;
        }
    }

    void FixedUpdate() {
        // Cap the maximun rotation speed to avoid ultra fast spinning on small circles
        float degreePerSecond = Mathf.Min(360.0f * speed/(2.0f*_circleRadius*Mathf.PI), maxRotationSpeed);
        Quaternion rotation = Quaternion.AngleAxis(_direction * degreePerSecond * Time.fixedDeltaTime, Vector3.up);

        transform.position = rotation*(transform.position - _circleCenter) + _circleCenter;
        transform.rotation *= rotation;
    }

    private float MaxRadius(float direction) {
        // Apply the inverse of the platform rotation to get back to an axis aligned coordinate system
        Quaternion invRotation = Quaternion.Inverse(platform.transform.rotation);
        Vector3 position = invRotation*transform.position;
        Vector3 right = invRotation*transform.right;

        // Get the extends of the platform from the mesh as the collider bounds are AA in world space
        Vector3 extents = Vector3.Scale(platform.sharedMesh.bounds.extents, platform.transform.lossyScale);
        Vector3 min = invRotation*platform.transform.position - extents;
        Vector3 max = invRotation*platform.transform.position + extents;

        // Get the max radius for each edge, then return the minimun one
        float x1 = (position.x-min.x)/(1.0f-right.x*direction);
        float x2 = (max.x-position.x)/(1.0f+right.x*direction);
        float z1 = (position.z-min.z)/(1.0f-right.z*direction);
        float z2 = (max.z-position.z)/(1.0f+right.z*direction);

        return Mathf.Min(Mathf.Min(x1, x2), Mathf.Min(z1, z2));
    }

    void OnDrawGizmos() {
        if (drawDebugGizmos) {
            Vector3 offset = (platform.transform.position.y+0.01f)*Vector3.up;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_circleCenter+offset, _circleRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position+transform.right*MaxRadius(1.0f)+offset, MaxRadius(1.0f));
            Gizmos.DrawWireSphere(transform.position-transform.right*MaxRadius(-1.0f)+offset, MaxRadius(-1.0f));
        }
    }
}