using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor {
	private TerrainGenerator terrainGenerator;

	private SerializedProperty width;
	private SerializedProperty height;
	private SerializedProperty seed;
	private SerializedProperty scale;
	private SerializedProperty falloffCurve;
	private SerializedProperty terrainRenderer;
	private SerializedProperty kingdomRenderer;

	private SerializedProperty octaves;
	private SerializedProperty persistance;
	private SerializedProperty lacunarity;
	private SerializedProperty offset;

	private SerializedProperty minRiverHeight;
	private SerializedProperty minRivers;
	private SerializedProperty maxRivers;
	private SerializedProperty minRiverThickness;
	private SerializedProperty maxRiverThickness;

	private SerializedProperty borderRadius;
	private SerializedProperty borderColor;

	private SerializedProperty waveNum;
	private SerializedProperty waveRadius;
	private SerializedProperty waveSpacing;

	private SerializedProperty minKingdoms;
	private SerializedProperty maxKingdoms;
	private SerializedProperty minKingdomPopulationRatio;
	private SerializedProperty maxKingdomPopulationRatio;
	private SerializedProperty minKingdomMoney;
	private SerializedProperty maxKingdomMoney;

	private void OnEnable() {
		terrainGenerator = (TerrainGenerator) target;
		width = serializedObject.FindProperty("width");
		height = serializedObject.FindProperty("height");
		seed = serializedObject.FindProperty("seed");
		scale = serializedObject.FindProperty("scale");
		falloffCurve = serializedObject.FindProperty("falloffCurve");
		terrainRenderer = serializedObject.FindProperty("terrainRenderer");
		kingdomRenderer = serializedObject.FindProperty("kingdomRenderer");

		octaves = serializedObject.FindProperty("octaves");
		persistance = serializedObject.FindProperty("persistance");
		lacunarity = serializedObject.FindProperty("lacunarity");
		offset = serializedObject.FindProperty("offset");

		minRiverHeight = serializedObject.FindProperty("minRiverHeight");
		minRivers = serializedObject.FindProperty("minRivers");
		maxRivers = serializedObject.FindProperty("maxRivers");
		minRiverThickness = serializedObject.FindProperty("minRiverThickness");
		maxRiverThickness = serializedObject.FindProperty("maxRiverThickness");

		borderRadius = serializedObject.FindProperty("borderRadius");
		borderColor = serializedObject.FindProperty("borderColor");

		waveNum = serializedObject.FindProperty("waveNum");
		waveRadius = serializedObject.FindProperty("waveRadius");
		waveSpacing = serializedObject.FindProperty("waveSpacing");

		minKingdoms = serializedObject.FindProperty("minKingdoms");
		maxKingdoms = serializedObject.FindProperty("maxKingdoms");
		minKingdomPopulationRatio = serializedObject.FindProperty("minKingdomPopulationRatio");
		maxKingdomPopulationRatio = serializedObject.FindProperty("maxKingdomPopulationRatio");
		minKingdomMoney = serializedObject.FindProperty("minKingdomMoney");
		maxKingdomMoney = serializedObject.FindProperty("maxKingdomMoney");
	}

	public override void OnInspectorGUI() {
		//terrain properties
		EditorGUI.BeginChangeCheck();

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(width);
		EditorGUILayout.PropertyField(height);
		EditorGUILayout.PropertyField(seed);
		EditorGUILayout.PropertyField(scale);
		EditorGUILayout.PropertyField(falloffCurve);
		EditorGUILayout.PropertyField(terrainRenderer);

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(octaves);
		EditorGUILayout.PropertyField(persistance);
		EditorGUILayout.PropertyField(lacunarity);
		EditorGUILayout.PropertyField(offset);

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(minRiverHeight);
		EditorGUILayout.PropertyField(minRivers);
		EditorGUILayout.PropertyField(maxRivers);
		EditorGUILayout.PropertyField(minRiverThickness);
		EditorGUILayout.PropertyField(maxRiverThickness);

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(borderRadius);
		EditorGUILayout.PropertyField(borderColor);

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(waveNum);
		EditorGUILayout.PropertyField(waveRadius);
		EditorGUILayout.PropertyField(waveSpacing);

		serializedObject.ApplyModifiedProperties();
		if(EditorGUI.EndChangeCheck()) {
			GenerateTerrain();
		}

		//kingdom properties
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(kingdomRenderer);

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(minKingdoms);
		EditorGUILayout.PropertyField(maxKingdoms);
		EditorGUILayout.PropertyField(minKingdomPopulationRatio);
		EditorGUILayout.PropertyField(maxKingdomPopulationRatio);
		EditorGUILayout.PropertyField(minKingdomMoney);
		EditorGUILayout.PropertyField(maxKingdomMoney);

		serializedObject.ApplyModifiedProperties();
		if(EditorGUI.EndChangeCheck()) {
			GenerateKingdoms();
		}

		//biome properties
		EditorGUILayout.Space();
		GUILayout.Label(terrainGenerator.biomes.Count + " Biomes");
		if(GUILayout.Button("Edit Biomes")) {
			BiomeEditorWindow window = EditorWindow.GetWindow<BiomeEditorWindow>();
			window.SetBiomes(terrainGenerator.biomes);
			window.OnChange(SetBiomes);
		}
		/*if(GUILayout.Button("Clear Biomes")) {
			terrainGenerator.biomes.Clear();
		}*/

		//resource properties
		EditorGUILayout.Space();
		GUILayout.Label(terrainGenerator.resources.Count + " Resources");
		if(GUILayout.Button("Edit Resources")) {
			ResourceEditorWindow window = EditorWindow.GetWindow<ResourceEditorWindow>();
			window.SetResources(terrainGenerator.resources);
			window.OnChange(SetResources);
		}
		/*if(GUILayout.Button("Clear Resources")) {
			terrainGenerator.resources.Clear();
		}*/

		//buttons
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		EditorGUILayout.Space();
		if(GUILayout.Button("Generate Terrain")) {
			GenerateTerrain();
		}
		if(GUILayout.Button("Generate Kingdoms")) {
			GenerateKingdoms();
		}
		if (GUILayout.Button("Generate Resources")) {
			GenerateResources();
		}
		EditorGUILayout.Space();
	}

	public void SetBiomes(List<Biome> biomes) {
		terrainGenerator.biomes = biomes;
		Repaint();
	}

	public void SetResources(List<Resource> resources) {
		terrainGenerator.resources = resources;
		Repaint();
	}

	public void GenerateTerrain() {
		terrainGenerator.GenerateTerrain();
	}

	public void GenerateKingdoms() {
		terrainGenerator.GenerateKingdoms();
	}

	public void GenerateResources() {
		terrainGenerator.GenerateResources();
	}
}
