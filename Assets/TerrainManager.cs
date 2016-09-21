using System.Linq;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class TerrainManager : MonoBehaviour
{
	public int terrainSeed;

	public int terrainChunkLength = 40;
	public int terrainChunkHeight = 30;
	public int terrainChunkFaceSpacing = 4;

	public int treeDensinity = 8;

	public GameObject[] trees;

	public int maxGenerating = 4;

	private TerrainChunkSettings settings;
	private PerlinNoise perlinNoise;

	private Dictionary<Vector2i, TerrainChunk> currChunks = new Dictionary<Vector2i, TerrainChunk>();
	private Dictionary<Vector2i, TerrainChunk> awaiting = new Dictionary<Vector2i, TerrainChunk>();
	private Dictionary<Vector2i, TerrainChunk> generating = new Dictionary<Vector2i, TerrainChunk>();
	private HashSet<Vector2i> unneededChunks = new HashSet<Vector2i>();

	public void Start() // {{{
	{
		perlinNoise = new PerlinNoise(terrainSeed);
		Random.InitState(terrainSeed);
		settings = new TerrainChunkSettings(terrainChunkLength, terrainChunkHeight, terrainChunkFaceSpacing, treeDensinity);
	}
	// }}}

	public void Update() // {{{
	{
		ProcessGenerating();
		ProcessAwaiting();
		ProcessUnneeded();
	}
	// }}}

	public void ProcessGenerating() // {{{
	{
		var tmpGenerating = generating.ToList();
		foreach (var item in tmpGenerating)
		{
			if (item.Value.ready)
			{
				if (item.Value.NextStage())
				{
					currChunks[item.Key] = item.Value;
					generating.Remove(item.Key);
					break;
				}
			}
		}
	}
	// }}}

	public void ProcessAwaiting() // {{{
	{
		if (generating.Count < maxGenerating)
		{
			var toGenerate = awaiting.Take(maxGenerating - generating.Count).ToList();
			foreach (var item in toGenerate)
			{
				new Thread(new ThreadStart(item.Value.PrecalculateMesh)).Start();
				generating[item.Key] = item.Value;
				awaiting.Remove(item.Key);
			}
		}
	}
	// }}}

	public void ProcessUnneeded() // {{{
	{
		var toDelete = unneededChunks.ToList();
		foreach (var pos in toDelete)
		{
			if (!generating.ContainsKey(pos))
			{
				unneededChunks.Remove(pos);
				currChunks[pos].Destroy();
				currChunks.Remove(pos);
			}
		}
	}
	// }}}

	public void AddChunk(Vector2i pos) // {{{
	{
		if (!awaiting.ContainsKey(pos) && !generating.ContainsKey(pos))
		{
			var chunk = new TerrainChunk(settings, perlinNoise, terrainSeed, this, pos);
			awaiting[pos] = chunk;
		}
		else if (unneededChunks.Contains(pos))
			unneededChunks.Remove(pos);
	}
	// }}}

	public void RemoveChunk(Vector2i pos) // {{{
	{
		if (generating.ContainsKey(pos) || currChunks.ContainsKey(pos))
			unneededChunks.Add(pos);
		else if (awaiting.ContainsKey(pos))
			awaiting.Remove(pos);
	}
	// }}}

	public List<Vector2i> GetExisting() // {{{
	{
		return currChunks.Keys.ToList();
	}
	// }}}

	public Vector2i GetChunkPosition(Vector3 worldPosition) // {{{
	{
		var x = (int)Mathf.Floor(worldPosition.x / (settings.spacing * (settings.length - 1)));
		var z = (int)Mathf.Floor(worldPosition.z / (settings.spacing * (settings.length - 1)));

		return new Vector2i(x, z);
	}
	// }}}
}
