using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Utils;

public class TerrainChunk
{
  public Vector2i pos;
  public Vector3 worldPosition;

  private TerrainChunkSettings settings;

  private PerlinNoise perlinNoise;

  public bool ready;

  private Action[] stages;
  private int currentStage = 0;

  private Vector3[] vertices;
  private int[] triangles;

  private GameObject gameObject;

  private System.Random rand;

  private float[,] heightmap;
  private Terrain terrain;

  private int seed;

  public TerrainChunk(TerrainChunkSettings settings, PerlinNoise perlinNoise, int seed, Vector2i pos) // {{{
  {
    this.pos = pos;
    this.settings = settings;
    this.perlinNoise = perlinNoise;
    this.seed = seed;
    rand = new System.Random((pos.x * 10000 + pos.z * 587) * seed);

    stages = new Action[]{
      CreateMesh,
      CreateTerrain,
      AddTrees,
      AddDetails,
    };

    gameObject = new GameObject("Terrain");
    gameObject.transform.position = new Vector3(pos.x * (settings.length - 1) * settings.spacing + (settings.length % 2 == 0 && pos.z % 2 == 0 ? settings.spacing / 2 : 0),
						0,
						pos.z * (settings.length - 1) * settings.spacing);
    worldPosition = gameObject.transform.position;
  }
  // }}}

  public bool NextStage() // {{{
  {
    stages[currentStage]();
    return ++currentStage >= stages.Length;
  }
  // }}}

  public void PrecalculateMesh() // {{{
    // Adapted from: http://kobolds-keep.net/?p=33
  {
    var verts = new List<Vector3[]>();
    var tris = new List<int>();
    var uvs = new List<Vector2>();

    int len = settings.length;
    int spacing = settings.spacing;

    heightmap = new float[len, len];

    for (int z = 0; z < len; z++)
    {
      verts.Add(new Vector3[len]);
      for (int x = 0; x < len; x++)
      {
	var current_point = new Vector3();
	current_point.x = (x * spacing) - (len / 2f*spacing);
	current_point.z = (z * spacing) - (len / 2f*spacing);
	if (x > 1 && x < len - 1 && z > 1 && z < len - 1)
	{
	  current_point.x += ((float)rand.NextDouble() - 0.5f) * spacing;
	  current_point.z += ((float)rand.NextDouble() - 0.5f) * spacing;
	}

	int offset = z % 2;
	if (offset == 1)
	{
	  current_point.x -= spacing * 0.5f;
	}

	float height = GetHeight(current_point.x + worldPosition.x,
				 current_point.z + worldPosition.z);
	current_point.y = height * settings.height;
	heightmap[z, x] = height;

	verts[z][x] = current_point;

	int current_x = x + (1-offset);

	if (x >= 1 && z > 0)
	{
	  tris.Add(x + z*len);
	  tris.Add((current_x-1) + (z-1)*len);
	  tris.Add((x-1) + z*len);
	}

	if ((x > 0 || (x == 0 && offset == 0)) && z > 0 && current_x < len)
	{
	  tris.Add(x + z*len);
	  tris.Add(current_x + (z-1)*len);
	  tris.Add((current_x-1) + (z-1)*len);
	}
      }
    }

    // Unfold the 2d array of verticies into a 1d array.
    var unfolded_verts = new Vector3[len*len];
    int i = 0;
    foreach (Vector3[] v in verts)
    {
      v.CopyTo(unfolded_verts, i * len);
      i++;
    }

    // Transform so there are no common vertices
    triangles = tris.ToArray();
    vertices = new Vector3[triangles.Length];
    for (i = 0; i < triangles.Length; i++) {
      vertices[i] = unfolded_verts[triangles[i]];
      triangles[i] = i;
    }

    ready = true;
  }
  // }}}

  private void CreateMesh() // {{{
  {
    gameObject.AddComponent<MeshFilter>();

    // Generate the mesh object.
    var ret = new Mesh();
    ret.vertices = vertices;
    ret.triangles = triangles;
    // ret.uv = uvs.ToArray();

    ret.RecalculateBounds();
    ret.RecalculateNormals();

    // Assign the mesh object and update it.
    gameObject.GetComponent<MeshFilter>().sharedMesh = ret;

    // var meshCollider = gameObject.AddComponent<MeshCollider>();
    // meshCollider.sharedMesh = ret;

    MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
    // renderer.material.shader = Shader.Find ("Particles/Additive");
    var tex = new Texture2D(1, 1);
    var darkness = (byte)rand.Next(35, 50);
    var greenish = new Color32(darkness, (byte)rand.Next(95, 125), darkness, 255);
    tex.SetPixel(0, 0, greenish);
    tex.Apply();

    renderer.material.mainTexture = tex;
    renderer.material.color = Color.black;
    gameObject.isStatic = true;
  }
  // }}}

  private void CreateTerrain() // {{{
  {
    var terrainData = new TerrainData();
    terrainData.heightmapResolution = settings.length;
    terrainData.alphamapResolution = settings.length;
    terrainData.thickness = 5f;
    terrainData.SetHeights(0, 0, heightmap);
    terrainData.size = new Vector3(settings.length * settings.spacing, settings.height, settings.length * settings.spacing);
    // terrainData.treePrototypes = settings.prototypes;

    var newTerrainGameObject = Terrain.CreateTerrainGameObject(terrainData);
    newTerrainGameObject.transform.parent = gameObject.transform;
    newTerrainGameObject.transform.position = new Vector3(worldPosition.x - settings.length * settings.spacing/2f,
							  0f,
							  worldPosition.z - settings.length * settings.spacing/2f);
    terrain = newTerrainGameObject.GetComponent<Terrain>();
    terrain.drawHeightmap = false;
    terrain.Flush();
  }
  // }}}

  private void AddTrees() // {{{
  {
    UnityEngine.Random.seed = (pos.x * 10000 + pos.z * 587) * seed;
    int trees = UnityEngine.Random.Range((int)(settings.treeDensinity * 0.6f), (int)(settings.treeDensinity * 1.4f) + 1);

    for (int i = 0; i < trees; ++i)
    {
      float x = (float) rand.NextDouble();
      float z = (float) rand.NextDouble();

      var steepness = terrain.terrainData.GetSteepness(x, z);

      if (steepness * steepness > UnityEngine.Random.Range(40f, 80f))
      {
	continue;
      }

      var pos = new Vector3(x, 0f, z);
      pos.x = (pos.x - 0.5f) * settings.length * settings.spacing + worldPosition.x;
      pos.z = (pos.z - 0.5f) * settings.length * settings.spacing + worldPosition.z;
      pos.y = terrain.SampleHeight(pos) + gameObject.transform.position.y;

      var treeInd = UnityEngine.Random.Range(0, settings.prefabs.Length);
      var tree = GameObject.Instantiate(settings.prefabs[treeInd], pos, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360),  0), gameObject.transform);

      var hScale = UnityEngine.Random.Range(5f, 11f);
      var wScale = hScale + UnityEngine.Random.Range(-1f, 1f);
      tree.transform.localScale = new Vector3(wScale, hScale, wScale);
      // var meshFilter = treeGO.AddComponent<MeshFilter>();
      // treeGO.transform.parent = gameObject.transform;


      // meshFilter.mesh = manager.trees[UnityEngine.Random.Range(0, manager.trees.Length)];
      // treeGO.transform.position = new Vector3(x, 0, z);
      // float x = (UnityEngine.Random.Range(0f, 1f));
      // float z = (UnityEngine.Random.Range(0f, 1f));
      // var tree = new TreeInstance();
      // tree.position = new Vector3(x, 0f, z);
      // tree.heightScale = UnityEngine.Random.Range(1.5f, 6f);
      // tree.widthScale = tree.heightScale + UnityEngine.Random.Range(-1f, 1f);
      // tree.lightmapColor = Color.white;
      // tree.color = Color.white;
      // tree.prototypeIndex = UnityEngine.Random.Range(0, settings.prototypes.Length);
      // tree.rotation = UnityEngine.Random.Range(0f, 2f * (float) Math.PI);
      // terrain.AddTreeInstance(tree);
    }
  }
  // }}}

  private void AddDetails() // {{{
  {
    UnityEngine.Random.seed = (pos.x * 10000 + pos.z * 587) * seed * 4532;
    int grass = UnityEngine.Random.Range((int)(settings.detailsDensinity * 0.6f), (int)(settings.detailsDensinity * 1.4f) + 1);

    for (int i = 0; i < grass; ++i)
    {
      float x = (float) rand.NextDouble();
      float z = (float) rand.NextDouble();

      var pos = new Vector3(x, 0f, z);
      pos.x = (pos.x - 0.5f) * settings.length * settings.spacing + worldPosition.x;
      pos.z = (pos.z - 0.5f) * settings.length * settings.spacing + worldPosition.z;
      pos.y = terrain.SampleHeight(pos) + gameObject.transform.position.y - 0.5f;

      var detInd = UnityEngine.Random.Range(0, settings.details.Length);
      var det = GameObject.Instantiate(settings.details[detInd], pos, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360),  0), gameObject.transform);

      var hScale = UnityEngine.Random.Range(5f, 11f);
      var wScale = hScale + UnityEngine.Random.Range(-1f, 1f);
      det.transform.localScale = new Vector3(wScale, hScale, wScale);
    }
  }
  // }}}

  public void Destroy() // {{{
  {
    GameObject.Destroy(gameObject);
  }
  // }}}

  private float GetHeight(float x, float z) // {{{
  {
    var f = perlinNoise.GetValue(x, z);
    // Debug.Log(f);
    return f;
  }
  // }}}

  public override bool Equals(object obj) // {{{
  {
    if (!(obj is TerrainChunk))
      return false;
    var other = (TerrainChunk) obj;
    return pos.x == other.pos.x && pos.z == other.pos.z;
  }
  // }}}

  public override int GetHashCode() // {{{
  {
    return pos.x + 10000 * pos.z;
  }
  // }}}
}
