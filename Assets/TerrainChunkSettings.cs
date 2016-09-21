using UnityEngine;

public class TerrainChunkSettings
{
  public int length;
  public int height;
  public int spacing;
  public int treeDensinity;
  public int detailsDensinity;
  public GameObject[] prefabs;
  public GameObject[] details;

  public TerrainChunkSettings(int length, int height, int spacing, int treeDensinity,
			      int detailsDensinity, GameObject[] prefabs, GameObject[] details)
  {
    this.length = length;
    this.height = height;
    this.spacing = spacing;
    this.treeDensinity = treeDensinity;
    this.detailsDensinity = detailsDensinity;
    this.prefabs = prefabs;
    this.details = details;
    // prototypes = new TreePrototype[prefabs.Length];
    // for (int i = 0; i < prototypes.Length; ++i)
    // {
      // prototypes[i] = new TreePrototype();
      // prototypes[i].prefab = prefabs[i];
    // }
  }
}
