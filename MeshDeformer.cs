using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour{

    Mesh deformingMesh;
    Vector3[] originalVertices, displacedVertices;
    Vector3[] vertexVelocities;
    public float springForce = 20f;
    public float damping = 1.8f;
    public float mass = 4f;
    float uniformScale = 1f;
    public float force = 10f;
    public float forceOffset = 0.1f;

    Collider other;

    void Start()
    {
        deformingMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = deformingMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
        vertexVelocities = new Vector3[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }
    }
     
    void Update(){
       uniformScale = transform.localScale.x;

        if (other.gameObject.CompareTag("Plane"))
        {
            for (int i = 0; i < displacedVertices.Length; i++)
            {
                UpdateVertex(i);
            }
        }
        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();
    }

    void UpdateVertex(int i)
    {
        Vector3 velocity = vertexVelocities[i];
        Vector3 displacement = displacedVertices[i] - originalVertices[i];
        displacement *= uniformScale;

        velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime;

        vertexVelocities[i] = velocity;
        displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);

    }

    internal void AddDeformingForce(Vector3 point, float force){
        point = transform.InverseTransformPoint(point);
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            AddForceToVertex(i, point, force);
        }

    }

    void AddForceToVertex(int i, Vector3 point, float force) {
        Vector3 pointToVertex = displacedVertices[i] - point;
        pointToVertex *= uniformScale;
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        float velocity = attenuatedForce * Time.deltaTime;
        vertexVelocities[i] += pointToVertex.normalized * velocity;
    }

    void OnCollisionEnter(Collision col)
    {

        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(inputRay, out hit))
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            if (deformer)
            {
                Vector3 point = hit.point;
                point += hit.normal * forceOffset;
                deformer.AddDeformingForce(point, force);
            }
        }

        if (col.gameObject.name == "Plane")
        {
            Debug.Log("Points colliding: " + col.contacts.Length);
            Debug.Log("First point that collided: " + col.contacts[0].point);
        }
    }
}
