using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
	//map
	private float[,] terrainMap;
	private int[,] kingdomMap;
	private List<float[,]> resourceMaps;

	//colors and textures
	private Color[] terrainColors;
	private Texture2D terrainTexture;

	private Color[] kingdomColors;
	private Texture2D kingdomTexture;

	private List<Color[]> resourceColors;
	private List<Texture2D> resourceTextures;

	//renderers
	public Renderer terrainRenderer;
	public Renderer kingdomRenderer;

	//general values
	public int width;
	public int height;

	public int seed;
	public float scale;
	public AnimationCurve falloffCurve;

	//terrain perlin noise values
	public int octaves;
	public float persistance;
	public float lacunarity;
	public Vector2 offset;

	//river values
	public float minRiverHeight;
	public int minRivers;
	public int maxRivers;
	public int minRiverThickness;
	public int maxRiverThickness;

	//border values
	public int borderRadius;
	public Color borderColor;

	//border wave values
	public int waveNum;
	public int waveRadius;
	public int waveSpacing;

	//kingdom values
	public int minKingdoms;
	public int maxKingdoms;
	public float minKingdomPopulationRatio;
	public float maxKingdomPopulationRatio;
	public int minKingdomMoney;
	public int maxKingdomMoney;

	//categories
	public List<Biome> biomes;
	private List<Kingdom> kingdoms;
	public List<Resource> resources;

	private void OnValidate() {
		if(width < 1) {
			width = 1;
		}
		if(height < 1) {
			height = 1;
		}
		if (scale <= 0) {
			scale = 0.0001f;
		}
		if(octaves < 1) {
			octaves = 1;
		}
		if(lacunarity < 0) {
			lacunarity = 0;
		}
		if(minRiverHeight < 0) {
			minRiverHeight = 0;
		}
		if(minRivers < 0) {
			minRivers = 0;
		}
		if(maxRivers < minRivers) {
			maxRivers = minRivers;
		}
		if(minRiverThickness < 1) {
			minRiverThickness = 1;
		}
		if(maxRiverThickness < minRiverThickness) {
			maxRiverThickness = minRiverThickness;
		}
		if(minKingdoms < 1) {
			minKingdoms = 1;
		}
		if(maxKingdoms < minKingdoms) {
			maxKingdoms = minKingdoms;
		}
		if(minKingdomPopulationRatio < 0) {
			minKingdomPopulationRatio = 0;
		}
		if(maxKingdomPopulationRatio < minKingdomPopulationRatio) {
			maxKingdomPopulationRatio = minKingdomPopulationRatio;
		}
		if(maxKingdomMoney < minKingdomMoney) {
			maxKingdomMoney = minKingdomMoney;
		}
	}

	public void GenerateTerrain() {
		PerlinNoise();
		//GenerateRivers();
		TerrainTexture();
		AddBorders();
		CellularAutomata();
		KingdomTexture();
		ResourcePerlinNoise();
		ResourceTexture();
		ApplyTextures();
	}

	public void GenerateKingdoms() {
		CellularAutomata();
		KingdomTexture();
		ApplyTextures();
	}

	public void GenerateResources() {
		ResourcePerlinNoise();
		ResourceTexture();
		ApplyTextures();
	}

	private void PerlinNoise() {
		terrainMap = new float[width, height];
		System.Random random = new System.Random(seed);
		Vector2[] octaveOffsets = new Vector2[octaves];
		for(int i = 0; i < octaves; i++) {
			float offsetX = (float) random.NextDouble() + offset.x;
			float offsetY = (float) random.NextDouble() + offset.y;
			octaveOffsets [i] = new Vector2(offsetX, offsetY);
		}
		float minHeight = float.MaxValue;
		float maxHeight = float.MinValue;
		for(int x = 0; x < width; x++) {
			for(int y = 0; y < height; y++) {
				float amplitude = 1;
				float frequency = 1;
				float noiseHeight = 0;
				for(int i = 0; i < octaves; i++) {
					float noiseX = ((float) x / width * scale * frequency) + octaveOffsets[i].x;
					float noiseY = ((float) y / height * scale * frequency) + octaveOffsets[i].y;
					float perlin = Mathf.PerlinNoise(noiseX, noiseY);
					noiseHeight += perlin * amplitude;
					amplitude *= persistance;
					frequency *= lacunarity;
				}
				if(noiseHeight > maxHeight) {
					maxHeight = noiseHeight;
				} else if(noiseHeight < minHeight) {
					minHeight = noiseHeight;
				}
				terrainMap[x, y] = noiseHeight;
			}
		}
		for(int x = 0; x < width; x++) {
			for(int y = 0; y < height; y++) {
				float dist = Vector2.Distance(new Vector2(width / 2, height / 2), new Vector2(x, y)) / Mathf.Max(width / 2, height / 2);
				float falloff = falloffCurve.Evaluate(dist);
				terrainMap[x, y] = Mathf.Clamp(Mathf.InverseLerp(minHeight, maxHeight, terrainMap[x, y]) - falloff, 0, 1);
			}
		}
	}

	private void TerrainTexture() {
		terrainColors = new Color[width * height];
		for(int x = 0; x < width; x++) {
			for(int y = 0; y < height; y++) {
				for(int i = 0; i < biomes.Count; i++) {
					double height = terrainMap[x, y];
					if(height >= biomes[i].minHeight && height <= biomes[i].maxHeight) {
						terrainColors[y * width + x] = biomes[i].color;
					}
				}
			}
		}
		terrainTexture = new Texture2D(width, height);
		terrainTexture.filterMode = FilterMode.Point;
		terrainTexture.wrapMode = TextureWrapMode.Clamp;
		terrainTexture.SetPixels(terrainColors);
	}

	private void GenerateRivers() {
		System.Random random = new System.Random(seed);
		int numRivers = random.Next(minRivers, maxRivers + 1);
		int riverThickness = random.Next(minRiverThickness, maxRiverThickness + 1);
		for(int i = 0; i < numRivers; i++) {
			bool isValid = false;
			int x;
			int y;
			do {
				x = random.Next(0, width);
				y = random.Next(0, height);
				isValid = (terrainMap[x, y] >= minRiverHeight);
			} while(!isValid);
			int deltaX = -1;
			int deltaY = -1;
			float minHeight = 1;
			for(int j = -1; j <= 1; j++) {
				for(int k = -1; k <= 1; k++) {
					if((j == 0 && k == 0) || (j != 0 && k != 0)) {
						continue;
					}
					if(terrainMap[x + j, y + k] < minHeight) {
						deltaX = j;
						deltaY = k;
						minHeight = terrainMap[x, y];
					}
				}
			}
			//Debug.Log(i + " " + numRivers + " " + deltaX + " " + deltaY + " " + x + " " + y);
			do {
				for(int j = -riverThickness; j <= riverThickness; j++) {
					if(deltaX == 0) {
						terrainMap[x + j, y] = biomes[0].maxHeight / 2;
					} else if(deltaY == 0) {
						terrainMap[x, y + j] = biomes[0].maxHeight / 2;
					}
				}
				int variation = random.Next(-1, 2);
				x += deltaX + (deltaX == 0 ? variation : 0);
				y += deltaY + (deltaY == 0 ? variation : 0);
			} while(terrainMap[x, y] > biomes[0].maxHeight);
		}
	}

	private void CellularAutomata() {
		kingdomMap = new int[width, height];
		kingdoms = new List<Kingdom>();
		System.Random random = new System.Random(seed);
		for(int x = 0; x < width; x++) {
			for(int y = 0; y < height; y++) {
				kingdomMap[x, y] = (IsEqualWater(terrainTexture.GetPixel(x, y))) ? -2 : -1;
			}
		}
		int numKingdoms = random.Next(minKingdoms, maxKingdoms + 1);
		List<Vector2> posToCheck = new List<Vector2>();
		for(int i = 0; i < numKingdoms; i++) {
			bool isValid = false;
			int x;
			int y;
			do {
				x = random.Next(0, width);
				y = random.Next(0, height);
				isValid = (kingdomMap[x, y] == -1);
			} while(!isValid);
			kingdomMap[x, y] = i;
			posToCheck.Add(new Vector2(x, y));
			kingdoms.Add(new Kingdom(
				NameGenerator.GenerateName(random, 4, 6, 1),
				1,
				0,
				random.Next(minKingdomMoney, maxKingdomMoney),
				new Color((float) random.NextDouble(), (float) random.NextDouble(), (float) random.NextDouble()),
				null,
				new Leader(seed)
			));
			Debug.Log(kingdoms[kingdoms.Count - 1].name);
		}
		while(posToCheck.Count != 0) {
			int x = (int) posToCheck[0].x;
			int y = (int) posToCheck[0].y;
			posToCheck.RemoveAt(0);
			if(kingdomMap[x, y] >= 0) {
				int index = kingdomMap[x, y];
				if(IsWithinBounds(x + 1, y) && kingdomMap[x + 1, y] == -1) {
					kingdomMap[x + 1, y] = index;
					kingdoms[index].size++;
					posToCheck.Add(new Vector2(x + 1, y));
				}
				if(IsWithinBounds(x - 1, y) && kingdomMap[x - 1, y] == -1) {
					kingdomMap[x - 1, y] = index;
					kingdoms[index].size++;
					posToCheck.Add(new Vector2(x - 1, y));
				}
				if(IsWithinBounds(x, y + 1) && kingdomMap[x, y + 1] == -1) {
					kingdomMap[x, y + 1] = index;
					kingdoms[index].size++;
					posToCheck.Add(new Vector2(x, y + 1));
				}
				if(IsWithinBounds(x, y - 1) && kingdomMap[x, y - 1] == -1) {
					kingdomMap[x, y - 1] = index;
					kingdoms[index].size++;
					posToCheck.Add(new Vector2(x, y - 1));
				}
			}
		}
		for(int i = 0; i < kingdoms.Count; i++) {
			float populationRatio = ((float) random.NextDouble() * (maxKingdomPopulationRatio - minKingdomPopulationRatio)) + minKingdomPopulationRatio;
			kingdoms[i].population = (int) (kingdoms[i].size * populationRatio);
			//Debug.Log(i + " " + kingdoms[i].size + " " + kingdoms[i].population + " " + kingdoms[i].color);
		}
	}

	private void ResourcePerlinNoise() {
		resourceMaps = new List<float[,]>();
		for(int i = 0; i < resources.Count; i++) {
			int octaves = resources[i].octaves;
			float persistance = resources[i].persistence;
			float lacunarity = resources[i].lacunarity;
			resourceMaps.Add(new float[width, height]);
			System.Random random = new System.Random(seed + 1 + i);
			Vector2[] octaveOffsets = new Vector2[octaves];
			for(int j = 0; j < octaves; j++) {
				float offsetX = (float)random.NextDouble();
				float offsetY = (float)random.NextDouble();
				octaveOffsets[j] = new Vector2(offsetX, offsetY);
			}
			for(int x = 0; x < width; x++) {
				for(int y = 0; y < height; y++) {
					float amplitude = 1;
					float frequency = 1;
					float noiseHeight = 0;
					for(int j = 0; j < octaves; j++) {
						float noiseX = ((float)x / width * scale * frequency) + octaveOffsets[j].x;
						float noiseY = ((float)y / height * scale * frequency) + octaveOffsets[j].y;
						float perlin = Mathf.PerlinNoise(noiseX, noiseY);
						noiseHeight += perlin * amplitude;
						amplitude *= persistance;
						frequency *= lacunarity;
					}
					noiseHeight /= octaves; 
					if (x == 50 && y == 50) {
						Debug.Log(noiseHeight);
                    }
					resourceMaps[i][x, y] = (noiseHeight > resources[i].threshold ? 0 : Mathf.InverseLerp(resources[i].minLerp, resources[i].maxLerp, 1 - noiseHeight));
				}
			}
		}
	}

	private void KingdomTexture() {
		kingdomColors = new Color[width * height];
		for(int x = 0; x < width; x++) {
			for(int y = 0; y < height; y++) {
				if(kingdomMap[x, y] < 0) {
					kingdomColors[y * width + x] = Color.clear;
				} else {
					Color color = kingdoms[kingdomMap[x, y]].color;
					kingdomColors[y * width + x] = new Color(color.r, color.g, color.b, 0.6f);
				}
			}
		}
		kingdomTexture = new Texture2D(width, height);
		kingdomTexture.filterMode = FilterMode.Point;
		kingdomTexture.wrapMode = TextureWrapMode.Clamp;
		kingdomTexture.SetPixels(kingdomColors);
	}

	private void ResourceTexture() {
		resourceColors = new List<Color[]>();
		resourceTextures = new List<Texture2D>();
		for(int i = 0; i < resources.Count; i++) {
			print(resourceMaps[i][0, 0]);
			resourceColors.Add(new Color[width * height]);
			for(int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					Color color = resources[i].color;
					resourceColors[i][y * width + x] = new Color(color.r, color.g, color.b, resourceMaps[i][x, y]);
				}
			}
			resourceTextures.Add(new Texture2D(width, height));
			resourceTextures[i].filterMode = FilterMode.Point;
			resourceTextures[i].wrapMode = TextureWrapMode.Clamp;
			resourceTextures[i].SetPixels(resourceColors[i]);
		}
	}

	private void AddBorders() {
		for(int x = 0; x < width; x++) {
			for(int y = 0; y < height; y++) {
				if(IsEqualWater(terrainTexture.GetPixel(x, y))) {
					CheckBorder(x, y);
				}
			}
		}
	}

	private void CheckBorder(int x, int y) {
		for(int i = x - borderRadius; i <= x + borderRadius; i++) {
			for(int j = y - (borderRadius * 2); j <= y + borderRadius; j++) {
				if(IsWithinBounds(i, j)) {
					Color neighbor = terrainTexture.GetPixel(i, j);
					if(IsEqualLand(neighbor)) {
						terrainTexture.SetPixel(x, y, borderColor);
						if(j >= y - borderRadius) {
							//AddWaves(x, y, i, j);
						}
						return;
					}
				}
			}
		}
	}

	private void AddWaves(int x, int y, int i, int j) {
		for(int currWave = 1; currWave <= waveNum; currWave++) {
			int xChange = (i == x) ? 0 : (int) Mathf.Sign(x - i);
			int k = (xChange * waveSpacing * currWave) + x;
			int yChange = (j == y) ? 0 : (int) Mathf.Sign(y - j);
			int l = (yChange * waveSpacing * currWave) + y;
			if(IsWithinBounds(k, 1) && IsEqualWater(terrainTexture.GetPixel(k, l))) {
				terrainTexture.SetPixel(k, l, borderColor);
			}
		}
	}

	private void ApplyTextures() {
		terrainTexture.Apply();
		terrainRenderer.sharedMaterial.mainTexture = terrainTexture;
		terrainRenderer.transform.localScale = new Vector3(terrainTexture.width, 1, terrainTexture.height);
		kingdomTexture.Apply();
		kingdomRenderer.sharedMaterial.mainTexture = kingdomTexture;
		kingdomRenderer.transform.localScale = new Vector3(terrainTexture.width, 1, terrainTexture.height);
		for(int i = 0; i < resources.Count; i++) {
			resourceTextures[i].Apply();
			resources[i].renderer.sharedMaterial.mainTexture = resourceTextures[i];
			resources[i].renderer.transform.localScale = new Vector3(terrainTexture.width, 1, terrainTexture.height);
		}
	}

	private bool IsEqual(Color a, Color b) {
		float thresh = 0.005f;
		if(Mathf.Abs(a.r - b.r) <= thresh && Mathf.Abs(a.g - b.g) <= thresh && Mathf.Abs(a.b - b.b) <= thresh && Mathf.Abs(a.a - b.a) <= thresh) {
			return true;
		}
		return false;
	}

	private bool IsEqualWater(Color a) {
		return IsEqual(a, biomes[0].color);
	}

	private bool IsEqualLand(Color a) {
		return !(IsEqualWater(a) || IsEqual(a, borderColor));
	}

	private bool IsWithinBounds(int x, int y) {
		return (x >= 0 && y >= 0 && x < width && y < height);
	}
}
