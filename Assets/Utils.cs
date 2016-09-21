using LibNoise.Unity.Generator;
using LibNoise.Unity.Operator;
using LibNoise.Unity;


namespace Utils
{
public class Vector2i
{
  public int x, z;

  public Vector2i(int x, int z)
  {
    this.x = x;
    this.z = z;
  }

  public override bool Equals(object obj)
  {
    var other = obj as Vector2i;
    return other != null && x == other.x && z == other.z;
  }

  public override int GetHashCode()
  {
    return x + 1000 * z;
  }
}

public class PerlinNoise
{
  private ModuleBase noiseProvider;

  public PerlinNoise(int seed)
  {
    var baseFlat = new Billow();
    baseFlat.Seed = seed;
    baseFlat.Frequency = 0.005f;
    var flatTerrain = new ScaleBias(0.09f, -0.75f, baseFlat);
    // var terraced = new Terrace(false, expHills);
    // terraced.Add(-1f);
    // terraced.Add(-0.35f);
    // terraced.Add(0.15f);
    // terraced.Add(0.75f);

    var denseBillow = new Billow();
    denseBillow.Seed = seed;
    denseBillow.Frequency = 0.01f;
    var expBillow = new Exponent(0.3f, denseBillow);
    var scaled = new ScaleBias(0.3f, -0.85f, expBillow);

    var terrainType = new Perlin();
    terrainType.Seed = seed;
    terrainType.Frequency = 0.005f;
    terrainType.Persistence = 0.15f;
    var flatAndScaled = new Select(0f, 1000f, 0.125f, flatTerrain, scaled);
    flatAndScaled.Controller = terrainType;

    var hills = new Billow();
    hills.Seed = seed;
    hills.Frequency = 0.002f;
    var expHills = new Exponent(2.5f, hills);
    var clamped = new Clamp(-1f, 0.75f, expHills);

    noiseProvider = new Blend(flatAndScaled, clamped, new Const(0.5f));
  }

  public float GetValue(float x, float z)
  {
    return (float)(noiseProvider.GetValue(x, 0, z) / 2f) + 0.5f;
  }
}
}
