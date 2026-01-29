using UnityEngine;

public class CarMeshSelector : MonoBehaviour
{

    public Mesh[] meshes;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    private void Start()
    {
        SelectRandomCar();
    }

    public void SelectRandomCar()
    {
        SetCarMesh(Random.Range(0, meshes.Length));
    }

    public void SetCarMesh(int mesh)
    {
        meshFilter.mesh = meshes[mesh];
    }
}
