using UnityEngine;

public class TerrainChunkSettings
{
  public int length;
  public int height;
  public int spacing;
  public int treeDensinity;

  public TerrainChunkSettings(int length, int height, int spacing, int treeDensinity)
  {
    this.length = length;
    this.height = height;
    this.spacing = spacing;
    this.treeDensinity = treeDensinity;
  }
}
