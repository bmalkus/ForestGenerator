using System.Collections.Generic;
using UnityEngine;
using Utils;
using System;
using System.Linq;

public class GameController : MonoBehaviour
{
	public int radius = 7;

	public GameObject mainObject;
	public TerrainManager terrainManager;

	private List<Vector2i> circleCoords = new List<Vector2i>();

	private Vector2i previousChunkPos = new Vector2i(-999, 999);

	void Start () // {{{
	{
		for (int x = -radius; x <= radius; ++x)
			for (int z = -radius; z <= radius; ++z)
				if (x*x + z*z < radius*radius)
					circleCoords.Add(new Vector2i(x, z));

		Vector2i currChunkPos = terrainManager.GetChunkPosition(mainObject.transform.position);
		terrainManager.AddChunk(currChunkPos);
	}
	// }}}

	void Update () // {{{
	{
		var currChunkPos = terrainManager.GetChunkPosition(mainObject.transform.position);

		if (!currChunkPos.Equals(previousChunkPos))
		{
			previousChunkPos = currChunkPos;

			var needed = new List<Vector2i>();

			foreach (var c in circleCoords)
				needed.Add(new Vector2i(currChunkPos.x + c.x, currChunkPos.z + c.z));

			var current = terrainManager.GetExisting();

			var toRemove = current.Except(needed);
			foreach (var pos in toRemove)
				terrainManager.RemoveChunk(pos);

			var toAdd = needed.Except(current);
			foreach (var pos in toAdd)
				terrainManager.AddChunk(pos);
		}
	}
	// }}}
}
