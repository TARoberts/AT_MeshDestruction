using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshSlice : MonoBehaviour
{
    private bool edgeSet = false;
    private Vector3 edgeVertex = Vector3.zero;
    private Vector2 edgeUV = Vector2.zero;
    private Plane edgePlane = new Plane();
    private Vector3 pos1, pos2, pos3;

    public int CutCascades = 1;
    public bool flip = false;

    public float ExplodeForce = 400;
    private Camera cam;
    private Plane testPlane;
    private string lastHit;

    public float planeOffset = 3;
    private float timer = 1f;
    public GameObject orb;

    // Start is called before the first frame update
    void Start()
    {
        orb = GameObject.FindGameObjectWithTag("orb");
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        flip = Follow.flip;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            pos1 = Input.mousePosition;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.name == this.GetComponent<Collider>().name)
                {
                    Debug.Log(hit.collider.name);
                    lastHit = hit.collider.name;
                }
                pos1 = hit.point;
                SetPlane();
            }
            
        }
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log(lastHit);

            if (lastHit == GetComponent<Collider>().name)
            {
                DestroyMesh();
            }
        }

        //if (Input.GetMouseButtonDown(1) && timer <= 0)
        //{
        //    timer = 1f;
        //    flip = !flip;
        //}
        
        //if (timer > 0f)
        //{
        //    timer -= Time.deltaTime;
        //}
    }

    private void SetPlane()
    {
        pos2 = pos1;
        pos2.z += planeOffset;

        if (!flip)
        {
            pos2.y += planeOffset;
        }
        else
        {
            pos2.x += planeOffset;
        }

        pos3 = pos1;
        //pos3.y -= planeOffset;
        pos3.z += planeOffset;

        GameObject orb1 = Instantiate(orb, pos1, transform.rotation);
        orb1.AddComponent<TimerKill>();

        GameObject orb2 = Instantiate(orb, pos2, transform.rotation);
        orb2.AddComponent<TimerKill>();

        GameObject orb3 = Instantiate(orb, pos3, transform.rotation);
        orb3.AddComponent<TimerKill>();
    }

    private void DestroyMesh()
    {
        Mesh originalMesh = this.gameObject.GetComponent<MeshFilter>().mesh;
        originalMesh.RecalculateBounds();
        List<MeshPartsClass> parts = new List<MeshPartsClass>();
        List<MeshPartsClass> subParts = new List<MeshPartsClass>();

        MeshPartsClass mainPart = new MeshPartsClass()
        {
            UV = originalMesh.uv,
            Verts = originalMesh.vertices,
            Norms = originalMesh.normals,
            Triangles = new int[originalMesh.subMeshCount][],
            Bounds = originalMesh.bounds
        };
        for (int i = 0; i < originalMesh.subMeshCount; i++)
            mainPart.Triangles[i] = originalMesh.GetTriangles(i);

        parts.Add(mainPart);

        for (int c = 0; c < CutCascades; c++)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                Bounds bounds = parts[i].Bounds;
                bounds.Expand(0.5f);


                //Plane plane = new Plane(UnityEngine.Random.onUnitSphere, new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                //                                                                   UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                //                                                                   UnityEngine.Random.Range(bounds.min.z, bounds.max.z)));

                //float fuck = Vector3.Distance(pos1, pos2);

                //Plane plane = new Plane(pos1, new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                //                                                                   UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                //                                                                   UnityEngine.Random.Range(bounds.min.z, bounds.max.z)));

                //pos2 = pos1 + bounds.max;

                //pos1.Normalize();

                //Debug.Log(pos1);
                //Debug.Log(pos2);
                //Plane plane = new Plane(pos1, pos2);

                pos1 = transform.InverseTransformPoint(pos1);
                pos2 = transform.InverseTransformPoint(pos2);
                pos3 = transform.InverseTransformPoint(pos3);

                Plane plane = new Plane(pos1, pos2, pos3);

                subParts.Add(GenerateMesh(parts[i], plane, true));
                subParts.Add(GenerateMesh(parts[i], plane, false));
            }
            parts = new List<MeshPartsClass>(subParts);
            subParts.Clear();
        }

        for (int i = 0; i < parts.Count; i++)
        {
            parts[i].MakeGameobject(this);
            parts[i].OriginalModel.GetComponent<Rigidbody>().AddForceAtPosition(parts[i].Bounds.center * ExplodeForce, transform.position);
        }

        Destroy(gameObject);
    }

    private MeshPartsClass GenerateMesh(MeshPartsClass original, Plane plane, bool left)
    {
        MeshPartsClass partMesh = new MeshPartsClass() { };
        Ray ray1 = new Ray();
        Ray ray2 = new Ray();


        for (int i = 0; i < original.Triangles.Length; i++)
        {
            int[] triangles = original.Triangles[i];
            edgeSet = false;

            for (int j = 0; j < triangles.Length; j = j + 3)
            {
                bool sideA = plane.GetSide(original.Verts[triangles[j]]) == left;
                bool sideB = plane.GetSide(original.Verts[triangles[j + 1]]) == left;
                bool sideC = plane.GetSide(original.Verts[triangles[j + 2]]) == left;

                int sideCount = (sideA ? 1 : 0) +
                                (sideB ? 1 : 0) +
                                (sideC ? 1 : 0);
                if (sideCount == 0)
                {
                    continue;
                }
                if (sideCount == 3)
                {
                    partMesh.AddTris(i,
                                         original.Verts[triangles[j]], original.Verts[triangles[j + 1]], original.Verts[triangles[j + 2]],
                                         original.Norms[triangles[j]], original.Norms[triangles[j + 1]], original.Norms[triangles[j + 2]],
                                         original.UV[triangles[j]], original.UV[triangles[j + 1]], original.UV[triangles[j + 2]]);
                    continue;
                }

                //cut points
                int singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;

                ray1.origin = original.Verts[triangles[j + singleIndex]];
                Vector3 dir1 = original.Verts[triangles[j + ((singleIndex + 1) % 3)]] - original.Verts[triangles[j + singleIndex]];
                ray1.direction = dir1;
                plane.Raycast(ray1, out float enter1);
                float lerp1 = enter1 / dir1.magnitude;

                ray2.origin = original.Verts[triangles[j + singleIndex]];
                Vector3 dir2 = original.Verts[triangles[j + ((singleIndex + 2) % 3)]] - original.Verts[triangles[j + singleIndex]];
                ray2.direction = dir2;
                plane.Raycast(ray2, out float enter2);
                float lerp2 = enter2 / dir2.magnitude;

                //first vertex = ancor
                AddEdge(i,
                        partMesh,
                        left ? plane.normal * -1f : plane.normal,
                        ray1.origin + ray1.direction.normalized * enter1,
                        ray2.origin + ray2.direction.normalized * enter2,
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                if (sideCount == 1)
                {
                    partMesh.AddTris(i,
                                        original.Verts[triangles[j + singleIndex]],
                                        ray1.origin + ray1.direction.normalized * enter1,
                                        ray2.origin + ray2.direction.normalized * enter2,
                                        original.Norms[triangles[j + singleIndex]],
                                        Vector3.Lerp(original.Norms[triangles[j + singleIndex]], original.Norms[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        Vector3.Lerp(original.Norms[triangles[j + singleIndex]], original.Norms[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                                        original.UV[triangles[j + singleIndex]],
                                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));
                    
                    continue;
                }

                if (sideCount == 2)
                {
                    partMesh.AddTris(i,
                                        ray1.origin + ray1.direction.normalized * enter1,
                                        original.Verts[triangles[j + ((singleIndex + 1) % 3)]],
                                        original.Verts[triangles[j + ((singleIndex + 2) % 3)]],
                                        Vector3.Lerp(original.Norms[triangles[j + singleIndex]], original.Norms[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        original.Norms[triangles[j + ((singleIndex + 1) % 3)]],
                                        original.Norms[triangles[j + ((singleIndex + 2) % 3)]],
                                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        original.UV[triangles[j + ((singleIndex + 1) % 3)]],
                                        original.UV[triangles[j + ((singleIndex + 2) % 3)]]);
                    partMesh.AddTris(i,
                                        ray1.origin + ray1.direction.normalized * enter1,
                                        original.Verts[triangles[j + ((singleIndex + 2) % 3)]],
                                        ray2.origin + ray2.direction.normalized * enter2,
                                        Vector3.Lerp(original.Norms[triangles[j + singleIndex]], original.Norms[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        original.Norms[triangles[j + ((singleIndex + 2) % 3)]],
                                        Vector3.Lerp(original.Norms[triangles[j + singleIndex]], original.Norms[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        original.UV[triangles[j + ((singleIndex + 2) % 3)]],
                                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));
                    continue;
                }


            }
        }

        partMesh.FillArrays();

        return partMesh;
    }

    private void AddEdge(int subMesh, MeshPartsClass partMesh, Vector3 normal, Vector3 vertex1, Vector3 vertex2, Vector2 uv1, Vector2 uv2)
    {
        if (!edgeSet)
        {
            edgeSet = true;
            edgeVertex = vertex1;
            edgeUV = uv1;
        }
        else
        {
            edgePlane.Set3Points(edgeVertex, vertex1, vertex2);

            partMesh.AddTris(subMesh,
                                edgeVertex,
                                edgePlane.GetSide(edgeVertex + normal) ? vertex1 : vertex2,
                                edgePlane.GetSide(edgeVertex + normal) ? vertex2 : vertex1,
                                normal,
                                normal,
                                normal,
                                edgeUV,
                                uv1,
                                uv2);
        }
    }

    public class MeshPartsClass
    {
        private List<Vector3> Verticies = new List<Vector3>();
        private List<Vector3> Normals = new List<Vector3>();
        private List<List<int>> TrianglesList = new List<List<int>>();
        private List<Vector2> UVLists = new List<Vector2>();
        public Vector3[] Verts, Norms;
        public int[][] Triangles;
        public Vector2[] UV;
        public GameObject OriginalModel;
        public Bounds Bounds = new Bounds();

        public MeshPartsClass()
        {

        }

        public void AddTris(int submesh, Vector3 vert1, Vector3 vert2, Vector3 vert3, Vector3 normal1, Vector3 normal2, Vector3 normal3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            if (TrianglesList.Count - 1 < submesh)
                TrianglesList.Add(new List<int>());

            TrianglesList[submesh].Add(Verticies.Count);
            Verticies.Add(vert1);
            TrianglesList[submesh].Add(Verticies.Count);
            Verticies.Add(vert2);
            TrianglesList[submesh].Add(Verticies.Count);
            Verticies.Add(vert3);
            Normals.Add(normal1);
            Normals.Add(normal2);
            Normals.Add(normal3);
            UVLists.Add(uv1);
            UVLists.Add(uv2);
            UVLists.Add(uv3);

            Bounds.min = Vector3.Min(Bounds.min, vert1);
            Bounds.min = Vector3.Min(Bounds.min, vert2);
            Bounds.min = Vector3.Min(Bounds.min, vert3);
            Bounds.max = Vector3.Min(Bounds.max, vert1);
            Bounds.max = Vector3.Min(Bounds.max, vert2);
            Bounds.max = Vector3.Min(Bounds.max, vert3);
        }

        public void FillArrays()
        {
            Verts = Verticies.ToArray();
            Norms = Normals.ToArray();
            UV = UVLists.ToArray();
            Triangles = new int[TrianglesList.Count][];
            for (int i = 0; i < TrianglesList.Count; i++)
                Triangles[i] = TrianglesList[i].ToArray();
        }

        public void MakeGameobject(MeshSlice original)
        {
            int nameRand = UnityEngine.Random.Range(0, 10);
            OriginalModel = new GameObject(original.name + " " + nameRand);
            OriginalModel.transform.position = original.transform.position;
            OriginalModel.transform.rotation = original.transform.rotation;
            OriginalModel.transform.localScale = original.transform.localScale;
            OriginalModel.AddComponent<CullTiny>();

            Mesh mesh = new Mesh();
            mesh.name = original.GetComponent<MeshFilter>().mesh.name;

            mesh.vertices = Verts;
            mesh.normals = Norms;
            mesh.uv = UV;
            for (int i = 0; i < Triangles.Length; i++)
                mesh.SetTriangles(Triangles[i], i, true);
            Bounds = mesh.bounds;

            MeshRenderer renderer = OriginalModel.AddComponent<MeshRenderer>();
            renderer.materials = original.GetComponent<MeshRenderer>().materials;

            MeshFilter filter = OriginalModel.AddComponent<MeshFilter>();
            filter.mesh = mesh;

            MeshCollider collider = OriginalModel.AddComponent<MeshCollider>();

            Rigidbody rigidbody = OriginalModel.AddComponent<Rigidbody>();
            MeshSlice meshSlice = OriginalModel.AddComponent<MeshSlice>();
            meshSlice.CutCascades = original.CutCascades;
            meshSlice.ExplodeForce = original.ExplodeForce;
            meshSlice.flip = original.flip;

            //float size = collider.bounds.size.x + collider.bounds.size.y + collider.bounds.size.z;
            //if (size < 1)
            //{
            //    Debug.Log("Culling Object of size " + size);
            //    return;
            //}
            collider.convex = true;
        }

    }
}
