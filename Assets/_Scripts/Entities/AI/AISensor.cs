using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PlayerLoop;

// a class to manage the sensor of an agent (ie its sight cone)
public class AISensor : MonoBehaviour
{
    public bool drawDebug = false;
    public float distance = 10;
    public float angle = 30;
    public float height = 1;
    public Color meshColor = Color.red;
    public float scanInterval = 1f/30f;
    public LayerMask layersOfInterest;
    public List<GameObject> inSightObjects {get; private set;}

    private Collider[] colliders = new Collider[50];
    private Mesh mesh;
    private int count;
    private float scanTimer;
    private LayerMask layersThatOcclude;

    // Start is called before the first frame update
    void Start()
    {
        layersThatOcclude = UtilsLayers.getCollidingLayerMask(gameObject.layer);
        foreach(int layer in UtilsLayers.getLayersInMask(layersOfInterest))
        {
            UtilsLayers.removeLayer(ref layersThatOcclude, layer);
        }
        
        inSightObjects = new List<GameObject>();        
    }

    // Update is called once per frame
    void Update()
    {
        scanTimer -= Time.deltaTime;
        if(scanTimer < 0)
        {
            scan();
            scanTimer += scanInterval;
        }
    }

    public bool isInSight(GameObject obj)
    {
        Vector3 origin = transform.position;
        Vector3 dest = obj.transform.position;
        Vector3 direction = dest - origin;

        if(direction.y < -1f || direction.y > height)
        {
            return false;
        }

        direction.y = 0f;
        float deltaAngle = Vector3.Angle(direction, transform.forward);
        if(deltaAngle > angle)
        {
            return false;
        }

        origin.y += height / 2;
        dest.y = origin.y;
        if(Physics.Linecast(origin, dest, layersThatOcclude))
        {
            return false;
        }

        return true;
    }

    public int filter(GameObject[] buffer, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        int count = 0;

        foreach(GameObject obj in inSightObjects)
        {
            if(obj.layer == layer)
            {
                buffer[count++] = obj;
            }

            if(buffer.Length == count)
            {
                break;
            }
        }

        return count;
    }

    private void scan()
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, layersOfInterest, QueryTriggerInteraction.Collide);

        inSightObjects.Clear();
        for(int i = 0; i < count; ++i)
        {
            GameObject obj = colliders[i].gameObject;
            if(isInSight(obj))
            {
                inSightObjects.Add(obj);
            }
        }
        inSightObjects.RemoveAll(obj => !obj);
    }

    private Mesh createWedgeMesh()
    {
        Mesh mesh = new Mesh();

        int segments = 10;
        int numTriangles = (segments * 4) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 botCenter = Vector3.zero;
        Vector3 botLeft = Quaternion.Euler(0, -angle, 0) * Vector3.forward * distance;
        Vector3 botRight = Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;

        Vector3 topCenter = botCenter + Vector3.up * height;
        Vector3 topLeft = botLeft + Vector3.up * height;
        Vector3 topRight = botRight + Vector3.up * height;

        int vert = 0;

        // keep normals pointing outwards
        // left side
        vertices[vert++] = botCenter;
        vertices[vert++] = botLeft;
        vertices[vert++] = topLeft;
        
        vertices[vert++] = topLeft;
        vertices[vert++] = topCenter;
        vertices[vert++] = botCenter;

        // right side
        vertices[vert++] = botCenter;
        vertices[vert++] = topCenter;
        vertices[vert++] = topRight;
        
        vertices[vert++] = topRight;
        vertices[vert++] = botRight;
        vertices[vert++] = botCenter;


        float currentAngle = -angle;
        float deltaAngle = (angle * 2) / segments;
        for(int i = 0; i < segments; ++i)
        {
            botLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance;
            botRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance;

            topLeft = botLeft + Vector3.up * height;
            topRight = botRight + Vector3.up * height;
            
            // far side
            vertices[vert++] = botLeft;
            vertices[vert++] = botRight;
            vertices[vert++] = topRight;
            
            vertices[vert++] = topRight;
            vertices[vert++] = topLeft;
            vertices[vert++] = botLeft;

            // top
            vertices[vert++] = topCenter;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;

            // bottom
            vertices[vert++] = botCenter;
            vertices[vert++] = botRight;
            vertices[vert++] = botLeft;

            currentAngle += deltaAngle;
        }

        for(int i = 0; i < numVertices; ++i)
        {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    private void OnValidate()
    {
        mesh = createWedgeMesh();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(mesh)
        {
            Gizmos.color = meshColor;
            Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
        }
        
        Gizmos.DrawWireSphere(transform.position, distance);

        if(!drawDebug)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        for(int i = 0; i < count; ++i)
        {
            Gizmos.DrawSphere(colliders[i].transform.position, 0.2f);
        }

        Gizmos.color = Color.green;
        foreach(GameObject obj in inSightObjects)
        {
            Gizmos.DrawSphere(obj.transform.position, 0.2f);
        }
    }
#endif
}
