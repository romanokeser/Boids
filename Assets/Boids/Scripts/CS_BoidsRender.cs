using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CS_BoidsRender : MonoBehaviour
{
    [SerializeField] private CS_Boids csBoids;
    [SerializeField] private Mesh instanceMesh;
    [SerializeField] private Material instanceRenderMaterial;
    [SerializeField] public Vector3 boidScale = new Vector3(0.2f, 0.3f, 0.6f);

    private bool _supportsInstancing;
    private uint _instanceMeshIndexCount;
    private uint _boidsCount;

    private Bounds _simulationBounds;

    private readonly uint[] _args = new uint[5] { 0, 0, 0, 0, 0 };

    private ComputeBuffer _argsBuffer;


    private static readonly int BoidDataBuffer = Shader.PropertyToID("_BoidDataBuffer");
    private static readonly int Scale = Shader.PropertyToID("_BoidScale");

    private void Awake()
    {
        //csBoids = GetComponent<CS_Boids>();
    }

    private void Start()
    {
        InitValues();
        GetSimulationBounds();
    }

    private void InitValues()
    {
        _supportsInstancing = SystemInfo.supportsInstancing;
        _instanceMeshIndexCount = (instanceMesh != null ? instanceMesh.GetIndexCount(0) : 0);
        _boidsCount = (uint)csBoids.GetBoidsCount();

        _argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint),
            ComputeBufferType.IndirectArguments);
    }

    private void Update()
    {
        if (instanceRenderMaterial == null || csBoids == null || !_supportsInstancing)
            return;

        RenderInstancedMesh();
    }

    private void OnDisable()
    {
        _argsBuffer?.Release();
    }

    private void RenderInstancedMesh()
    {
        // Update the arguments buffer
        _args[0] = _instanceMeshIndexCount;
        _args[1] = _boidsCount;
        _argsBuffer.SetData(_args);

        // Create a MaterialPropertyBlock
        var propertyBlock = new MaterialPropertyBlock();

        // Set the boid data buffer and scale property in the property block
        propertyBlock.SetBuffer(BoidDataBuffer, csBoids.GetBoidsData());
        propertyBlock.SetVector(Scale, boidScale);

        // Draw the mesh using GPU instancing with the property block
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceRenderMaterial, _simulationBounds, _argsBuffer, 0, propertyBlock);
    }

    private void GetSimulationBounds()
    {
        // Define the bounding area
        _simulationBounds = new Bounds
        (
            csBoids.GetSimulationCenter(),
            csBoids.GetSimulationDimensions()
        );
    }
}
