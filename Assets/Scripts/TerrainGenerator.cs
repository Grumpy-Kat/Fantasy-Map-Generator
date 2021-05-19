using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    // 2D representation of all possible maps
    private float[,] terrainMap;
    private int[,] kingdomMap;
    private List<float[,]> resourceMaps;

    // Colors and textures of each map type
    private Color[] terrainColors;
    private Texture2D terrainTexture;

    private Color[] kingdomColors;
    private Texture2D kingdomTexture;

    private List<Color[]> resourceColors;
    private List<Texture2D> resourceTextures;

    // Renderers
    public Renderer terrainRenderer;
    public Renderer kingdomRenderer;

    // General map values
    public int width;
    public int height;

    public int seed;
    public float scale;
    public AnimationCurve falloffCurve;

    // Terrain perlin noise values
    public int octaves;
    public float persistance;
    public float lacunarity;
    public Vector2 offset;

    // River values, currently unused
    public float minRiverHeight;
    public int minRivers;
    public int maxRivers;
    public int minRiverThickness;
    public int maxRiverThickness;

    // Border values
    public int borderRadius;
    public Color borderColor;

    // Border wave values
    public int waveNum;
    public int waveRadius;
    public int waveSpacing;

    // Kingdom values
    public int minKingdoms;
    public int maxKingdoms;
    public float minKingdomPopulationRatio;
    public float maxKingdomPopulationRatio;
    public int minKingdomMoney;
    public int maxKingdomMoney;

    // Data
    public List<Biome> biomes;
    private List<Kingdom> kingdoms;
    public List<Resource> resources;

    private void OnValidate() {
        // Check values to make sure within valid range

        // Width and height can not be 0 or negative
        if (width < 1) {
            width = 1;
        }
        if (height < 1) {
            height = 1;
        }
        // Scale can not be equal to or less than 0 because the map will be completely flat
        if (scale <= 0) {
            scale = 0.0001f;
        }

        // Need at least one octave for perlin noise
        if (octaves < 1) {
            octaves = 1;
        }
        // Negative or 0 frequency break perlin noise
        if (lacunarity < 0) {
            lacunarity = 0;
        }

        // Rivers can not go to negative height
        if (minRiverHeight < 0) {
            minRiverHeight = 0;
        }
        // Number of rivers can not be negative and max needs to be greater than min
        if (minRivers < 0) {
            minRivers = 0;
        }
        if (maxRivers < minRivers) {
            maxRivers = minRivers;
        }
        // River thickness can not be 0 or negative and max needs to be greater than min
        if (minRiverThickness < 1) {
            minRiverThickness = 1;
        }
        if (maxRiverThickness < minRiverThickness) {
            maxRiverThickness = minRiverThickness;
        }

        // Need at least one kingdom and max needs to be greater than min
        if (minKingdoms < 1) {
            minKingdoms = 1;
        }
        if (maxKingdoms < minKingdoms) {
            maxKingdoms = minKingdoms;
        }
        // Kingdom can not have negative people and max needs to be greater than min
        if (minKingdomPopulationRatio < 0) {
            minKingdomPopulationRatio = 0;
        }
        if (maxKingdomPopulationRatio < minKingdomPopulationRatio) {
            maxKingdomPopulationRatio = minKingdomPopulationRatio;
        }
        // Max wealth of kingdom needs to be greater than min 
        if (maxKingdomMoney < minKingdomMoney) {
            maxKingdomMoney = minKingdomMoney;
        }
    }

    /// <summary>
    /// Calls all methods necessary for generating terrain map in correct order. Require regeneration of kingdoms.
    /// </summary>
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

    /// <summary>
    /// Calls all methods necessary for generating kingdoms map in correct order. Does not require regeneration of terrain.
    /// </summary>
    public void GenerateKingdoms() {
        CellularAutomata();
        KingdomTexture();
        ApplyTextures();
    }

    /// <summary>
    /// Calls all methods necessary for generating resource maps in correct order. Does not require regeneration of terrain.
    /// </summary>
    public void GenerateResources() {
        ResourcePerlinNoise();
        ResourceTexture();
        ApplyTextures();
    }

    /// <summary>
    /// Uses perlin noise to generate terrain map.
    /// </summary>
    private void PerlinNoise() {
        // Set up values, assume generating brand new map
        terrainMap = new float[width, height];
        System.Random random = new System.Random(seed);
        
        // Set up octaves
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offsetX = (float)random.NextDouble() + offset.x;
            float offsetY = (float)random.NextDouble() + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // The actual 2D perlin noise generation
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++) {
                    float noiseX = ((float)x / width * scale * frequency) + octaveOffsets[i].x;
                    float noiseY = ((float)y / height * scale * frequency) + octaveOffsets[i].y;
                    float perlin = Mathf.PerlinNoise(noiseX, noiseY);
                    noiseHeight += perlin * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noiseHeight > maxHeight) {
                    maxHeight = noiseHeight;
                } else if (noiseHeight < minHeight) {
                    minHeight = noiseHeight;
                }
                terrainMap[x, y] = noiseHeight;
            }
        }

        // Normalize terrain and use fall off curve to adjust terrain, in this case to make terrain an island
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float dist = Vector2.Distance(new Vector2(width / 2, height / 2), new Vector2(x, y)) / Mathf.Max(width / 2, height / 2);
                float falloff = falloffCurve.Evaluate(dist);
                terrainMap[x, y] = Mathf.Clamp(Mathf.InverseLerp(minHeight, maxHeight, terrainMap[x, y]) - falloff, 0, 1);
            }
        }
    }

    /// <summary>
    /// Set terrain colors based on height and create texture for terrain map.
    /// </summary>
    private void TerrainTexture() {
        // Assign terrain colors based on height
        terrainColors = new Color[width * height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                for (int i = 0; i < biomes.Count; i++) {
                    double height = terrainMap[x, y];
                    if (height >= biomes[i].minHeight && height <= biomes[i].maxHeight) {
                        terrainColors[y * width + x] = biomes[i].color;
                    }
                }
            }
        }

        // Create texture and set pixels for terrain map
        terrainTexture = new Texture2D(width, height);
        terrainTexture.filterMode = FilterMode.Point;
        terrainTexture.wrapMode = TextureWrapMode.Clamp;
        terrainTexture.SetPixels(terrainColors);
    }

    /// <summary>
    /// Generate rivers.
    /// TODO: improve, might be helped with using different colors to differentiate between shallow and deep water.
    /// </summary>
    private void GenerateRivers() {
        // Set up values
        System.Random random = new System.Random(seed);
        int numRivers = random.Next(minRivers, maxRivers + 1);
        int riverThickness = random.Next(minRiverThickness, maxRiverThickness + 1);

        // Create each river
        for (int i = 0; i < numRivers; i++) {
            bool isValid = false;
            int x;
            int y;

            // Find starting point of river that is not already below possible river height
            do {
                x = random.Next(0, width);
                y = random.Next(0, height);
                isValid = (terrainMap[x, y] >= minRiverHeight);
            } while (!isValid);

            int deltaX = -1;
            int deltaY = -1;
            float minHeight = 1;
            // Decide what direction for river to flow
            for (int j = -1; j <= 1; j++) {
                for (int k = -1; k <= 1; k++) {
                    // Don't flow on same point or diagonally
                    if ((j == 0 && k == 0) || (j != 0 && k != 0)) {
                        continue;
                    }
                    if (terrainMap[x + j, y + k] < minHeight) {
                        deltaX = j;
                        deltaY = k;
                        minHeight = terrainMap[x, y];
                    }
                }
            }

            do {
                for (int j = -riverThickness; j <= riverThickness; j++) {
                    if (deltaX == 0) {
                        terrainMap[x + j, y] = biomes[0].maxHeight / 2;
                    } else if (deltaY == 0) {
                        terrainMap[x, y + j] = biomes[0].maxHeight / 2;
                    }
                }
                int variation = random.Next(-1, 2);
                x += deltaX + (deltaX == 0 ? variation : 0);
                y += deltaY + (deltaY == 0 ? variation : 0);
            } while (terrainMap[x, y] > biomes[0].maxHeight);
        }
    }

    /// <summary>
    /// Uses cellular automata to generate kingdom borders.
    /// Assumes there are at least as many land pixels as kingdoms.
    /// </summary>
    private void CellularAutomata() {
        // Set up values
        kingdomMap = new int[width, height];
        kingdoms = new List<Kingdom>();
        System.Random random = new System.Random(seed);

        // Decide whether it would be valid to have kingdom at each point or not
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                kingdomMap[x, y] = (IsEqualWater(terrainTexture.GetPixel(x, y))) ? -2 : -1;
            }
        }
        // Decide number of kingdoms
        int numKingdoms = random.Next(minKingdoms, maxKingdoms + 1);

        // Find valid spawnpoints for each kingdom
        List<Vector2> posToCheck = new List<Vector2>();
        for (int i = 0; i < numKingdoms; i++) {
            bool isValid = false;
            int x;
            int y;
            do {
                x = random.Next(0, width);
                y = random.Next(0, height);
                isValid = (kingdomMap[x, y] == -1);
            } while (!isValid);

            // Found valid spawn point, so declare as starting positon of kingdom
            kingdomMap[x, y] = i;
            posToCheck.Add(new Vector2(x, y));

            // Create kingdom with random name, money, color, and leader
            kingdoms.Add(
                new Kingdom(
                    NameGenerator.GenerateName(random, 4, 6, 1),
                    1,
                    0,
                    random.Next(minKingdomMoney, maxKingdomMoney),
                    new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()),
                    null,
                    new Leader(seed)
                )
            );
        }

        // Cellular automata to expand borders of kingdom until it hits the borders of its land mass or another kingdom
        while (posToCheck.Count != 0) {
            int x = (int)posToCheck[0].x;
            int y = (int)posToCheck[0].y;
            posToCheck.RemoveAt(0);
            if (kingdomMap[x, y] >= 0) {
                int index = kingdomMap[x, y];
                if (IsWithinBounds(x + 1, y) && kingdomMap[x + 1, y] == -1) {
                    kingdomMap[x + 1, y] = index;
                    kingdoms[index].size++;
                    posToCheck.Add(new Vector2(x + 1, y));
                }
                if (IsWithinBounds(x - 1, y) && kingdomMap[x - 1, y] == -1) {
                    kingdomMap[x - 1, y] = index;
                    kingdoms[index].size++;
                    posToCheck.Add(new Vector2(x - 1, y));
                }
                if (IsWithinBounds(x, y + 1) && kingdomMap[x, y + 1] == -1) {
                    kingdomMap[x, y + 1] = index;
                    kingdoms[index].size++;
                    posToCheck.Add(new Vector2(x, y + 1));
                }
                if (IsWithinBounds(x, y - 1) && kingdomMap[x, y - 1] == -1) {
                    kingdomMap[x, y - 1] = index;
                    kingdoms[index].size++;
                    posToCheck.Add(new Vector2(x, y - 1));
                }
            }
        }

        // Set each kingdom population randomly based on its size
        // TODO:  some kingdoms have lower or higher population density, currently this is generated, but it could be interesting to set it based on kingdom traits
        /// For instance, kingdoms that depend on agriculture have lower population density and kingdoms that have many large trading centers could higher density
        for (int i = 0; i < kingdoms.Count; i++) {
            float populationRatio = ((float)random.NextDouble() * (maxKingdomPopulationRatio - minKingdomPopulationRatio)) + minKingdomPopulationRatio;
            kingdoms[i].population = (int)(kingdoms[i].size * populationRatio);
        }
    }

    /// <summary>
    /// Set kingdom colors based on kingdom locations and create texture for kingdom map.
    /// </summary>
    private void KingdomTexture() {
        // Assign kingdom colors based on kingdom location
        kingdomColors = new Color[width * height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (kingdomMap[x, y] < 0) {
                    kingdomColors[y * width + x] = Color.clear;
                } else {
                    Color color = kingdoms[kingdomMap[x, y]].color;
                    kingdomColors[y * width + x] = new Color(color.r, color.g, color.b, 0.6f);
                }
            }
        }

        // Create texture and set pixels for kingdom map
        kingdomTexture = new Texture2D(width, height);
        kingdomTexture.filterMode = FilterMode.Point;
        kingdomTexture.wrapMode = TextureWrapMode.Clamp;
        kingdomTexture.SetPixels(kingdomColors);
    }

    /// <summary>
    /// Uses perlin noise to generate resource maps.
    /// </summary>
    private void ResourcePerlinNoise() {
        resourceMaps = new List<float[,]>();
        for (int i = 0; i < resources.Count; i++) {
            // Retrieve settings from resource data class
            int octaves = resources[i].octaves;
            float persistance = resources[i].persistence;
            float lacunarity = resources[i].lacunarity;

            // Set up values, assume generating brand new map
            resourceMaps.Add(new float[width, height]);
            System.Random random = new System.Random(seed + 1 + i);

            // Set up octaves
            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int j = 0; j < octaves; j++) {
                float offsetX = (float)random.NextDouble();
                float offsetY = (float)random.NextDouble();
                octaveOffsets[j] = new Vector2(offsetX, offsetY);
            }

            // The actual 2D perlin noise generation
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    for (int j = 0; j < octaves; j++) {
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

    /// <summary>
    /// Set resource colors based on resource locations and create texture for resource maps.
    /// </summary>
    private void ResourceTexture() {
        resourceColors = new List<Color[]>();
        resourceTextures = new List<Texture2D>();
        for (int i = 0; i < resources.Count; i++) {
            // Assign resource colors based on resource locations
            resourceColors.Add(new Color[width * height]);
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    Color color = resources[i].color;
                    resourceColors[i][y * width + x] = new Color(color.r, color.g, color.b, resourceMaps[i][x, y]);
                }
            }

            // Create texture and set pixels for kingdom map
            resourceTextures.Add(new Texture2D(width, height));
            resourceTextures[i].filterMode = FilterMode.Point;
            resourceTextures[i].wrapMode = TextureWrapMode.Clamp;
            resourceTextures[i].SetPixels(resourceColors[i]);
        }
    }

    /// <summary>
    /// Create map borders for decoration.
    /// </summary>
    private void AddBorders() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                // If water, check if border can be placed here.
                if (IsEqualWater(terrainTexture.GetPixel(x, y))) {
                    CheckBorder(x, y);
                }
            }
        }
    }

    /// <summary>
    /// Checks if border can be placed here. If it can, set the map to the border color.
    /// </summary>
    /// <param name="x">X coordinate to check</param>
    /// <param name="y">Y coordinate to check</param>
    private void CheckBorder(int x, int y) {
        for (int i = x - borderRadius; i <= x + borderRadius; i++) {
            for (int j = y - (borderRadius * 2); j <= y + borderRadius; j++) {
                if (IsWithinBounds(i, j)) {
                    Color neighbor = terrainTexture.GetPixel(i, j);
                    if (IsEqualLand(neighbor)) {
                        terrainTexture.SetPixel(x, y, borderColor);
                        if (j >= y - borderRadius) {
                            //AddWaves(x, y, i, j);
                        }
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds wave to map.
    /// </summary>
    /// <param name="x">X coordinate to add waves to</param>
    /// <param name="y">Y cooridnate to add waves to</param>
    /// <param name="i"></param>
    /// <param name="j"></param>
    private void AddWaves(int x, int y, int i, int j) {
        for (int currWave = 1; currWave <= waveNum; currWave++) {
            int xChange = (i == x) ? 0 : (int)Mathf.Sign(x - i);
            int k = (xChange * waveSpacing * currWave) + x;
            int yChange = (j == y) ? 0 : (int)Mathf.Sign(y - j);
            int l = (yChange * waveSpacing * currWave) + y;
            if (IsWithinBounds(k, 1) && IsEqualWater(terrainTexture.GetPixel(k, l))) {
                terrainTexture.SetPixel(k, l, borderColor);
            }
        }
    }

    /// <summary>
    /// Finalize and set textures to Unity objects.
    /// </summary>
    private void ApplyTextures() {
        // Finalize and set texture to terrain object
        terrainTexture.Apply();
        terrainRenderer.sharedMaterial.mainTexture = terrainTexture;
        terrainRenderer.transform.localScale = new Vector3(terrainTexture.width, 1, terrainTexture.height);

        // Finalize and set texture to kigndom object
        kingdomTexture.Apply();
        kingdomRenderer.sharedMaterial.mainTexture = kingdomTexture;
        kingdomRenderer.transform.localScale = new Vector3(terrainTexture.width, 1, terrainTexture.height);

        // Finalize and set texture to resource objects
        for (int i = 0; i < resources.Count; i++) {
            resourceTextures[i].Apply();
            resources[i].renderer.sharedMaterial.mainTexture = resourceTextures[i];
            resources[i].renderer.transform.localScale = new Vector3(terrainTexture.width, 1, terrainTexture.height);
        }
    }

    /// <summary>
    /// Checks whether two colors are approximately equal.
    /// Due to image compression and floating point math, the same or extremely colors can be seen as not equal.
    /// This method checks to see if each color component is within a set of threshold of each other.
    /// </summary>
    /// <param name="a">First color to check</param>
    /// <param name="b">Second color to check</param>
    /// <returns>Whether two colors are approximately equal.</returns>
    private bool IsEqual(Color a, Color b) {
        const float thresh = 0.005f;
        if (Mathf.Abs(a.r - b.r) <= thresh && Mathf.Abs(a.g - b.g) <= thresh && Mathf.Abs(a.b - b.b) <= thresh && Mathf.Abs(a.a - b.a) <= thresh) {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if given color is part of water.
    /// </summary>
    /// <param name="a">Color to check</param>
    /// <returns>Whether color is water or not.</returns>
    private bool IsEqualWater(Color a) {
        return IsEqual(a, biomes[0].color);
    }

    /// <summary>
    /// Checks if given color is part of land.
    /// This done by seeing if it is not water and not border. As of now, any color that is not that, can be assumed to be on land.
    /// Note: Does not check if actually part of allowed colors. 
    /// </summary>
    /// <param name="a">Color to check</param>
    /// <returns>Whether color is land or not.</returns>
    private bool IsEqualLand(Color a) {
        return !(IsEqualWater(a) || IsEqual(a, borderColor));
    }

    /// <summary>
    /// Helper method to see if coordinate is part of map bounds.
    /// </summary>
    /// <param name="x">X value of coordinate</param>
    /// <param name="y">Y value of coordinate</param>
    /// <returns>Whether x and y are part of map bounds.</returns>
    private bool IsWithinBounds(int x, int y) {
        return (x >= 0 && y >= 0 && x < width && y < height);
    }
}
