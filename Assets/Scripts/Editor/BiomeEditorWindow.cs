using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BiomeEditorWindow : EditorWindow {
	public delegate void OnChangeDelegate(List<Biome> biomes);
	private OnChangeDelegate onChangeDelegate;

	private int padding = 10;
	private Vector2 keySize = new Vector2(20, 10);

	private List<Biome> biomes = new List<Biome>();

	private Rect preview;
	private Rect settings;
	private Rect[] keys;
	private int selectedKey = -1;
	private bool isMouseDown = false;
	private bool shouldRepaint;

	private void OnGUI() {
		Draw();
		HandleInput();
		if(shouldRepaint) {
			shouldRepaint = false;
			onChangeDelegate(biomes);
			Repaint();
		}
	}

	private void Draw() {
		preview = new Rect(padding, padding, 50, position.height - (padding * 2));
		GUI.DrawTexture(preview, GetTexture((int) preview.height));
		keys = new Rect[biomes.Count];
		for(int i = 0; i < biomes.Count; i++) {
			Biome biome = biomes[i];
			Rect key = new Rect(preview.xMax + (padding / 2), preview.y + (preview.height * biome.minHeight) - (keySize.y / 2f), keySize.x, keySize.y);
			if(i == selectedKey) {
				EditorGUI.DrawRect(new Rect(key.x - 2, key.y - 2, key.width + 4, key.height + 4), Color.black);
			}
			EditorGUI.DrawRect(key, biome.color);
			keys[i] = key;
		}
		settings = new Rect(keys[0].xMax + (padding * 2), padding, position.width - keys[0].xMax - (padding * 4), position.height - (padding * 2));
		if(selectedKey != -1) {
			GUILayout.BeginArea(settings);
			EditorGUI.BeginChangeCheck();
			Biome biome = biomes[selectedKey];
			string name = EditorGUILayout.TextField(biome.name);
			float minHeight = EditorGUILayout.FloatField(biome.minHeight);
			EditorGUILayout.FloatField(biome.maxHeight);
			Color color = EditorGUILayout.ColorField(biome.color);
			if(EditorGUI.EndChangeCheck()) {
				selectedKey = MoveBiome(selectedKey, name, minHeight, color);
				onChangeDelegate(biomes);
			}
			GUILayout.EndArea();
		}
	}

	private void HandleInput() {
		Event e = Event.current;
		if(e.type == EventType.MouseDown && e.button == 0) {
			for(int i = 0; i < keys.Length; i++) {
				if(keys[i].Contains(e.mousePosition)) {
					selectedKey = i;
					isMouseDown = true;
					shouldRepaint = true;
					break;
				}
			}
			if(!isMouseDown && (settings.x > e.mousePosition.x || selectedKey == -1)) {
				float value = Mathf.InverseLerp(preview.y, preview.yMax, e.mousePosition.y);
				selectedKey = AddBiome("", value, Color.white);
				shouldRepaint = true;
			}
		}
		if(e.type == EventType.MouseUp && e.button == 0) {
			isMouseDown = false;
		}
		if(isMouseDown && e.type == EventType.MouseDrag && e.button == 0)	{
			float value = Mathf.InverseLerp(preview.y, preview.yMax, e.mousePosition.y);
			selectedKey = MoveBiome(selectedKey, value);
			shouldRepaint = true;
		}
		if(e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete) {
			RemoveBiome(selectedKey);
			if(selectedKey >= biomes.Count) {
				biomes[selectedKey - 1].maxHeight = 1;
				selectedKey--;
			} else if(selectedKey != 0) {
				biomes[selectedKey - 1].maxHeight = biomes[selectedKey].minHeight;
			}
			shouldRepaint = true;
		}
	}

	private int MoveBiome(int i, float value) {
		Color color = biomes[i].color;
		string name = biomes[i].name;
		int newI = MoveBiome(i, name, value, color);
		return newI;
	}

	private int MoveBiome(int i, string name, float value, Color color) {
		RemoveBiome(i);
		return AddBiome(name, value, color);
	}

	private int AddBiome(string name, float value, Color color) {
		for(int i = 0; i < biomes.Count; i++)	{
			if(value < biomes[i].minHeight) {
				if(i > 0) {
					biomes[i - 1].maxHeight = value;
				}
				biomes.Insert(i, new Biome(name, value, biomes[i].minHeight, color));
				return i;
			}
		}
		biomes[biomes.Count - 1].maxHeight = value;
		biomes.Add(new Biome(name, value, 1, color));
		return biomes.Count - 1;
	}

	private void RemoveBiome(int i) {
		if(biomes.Count > 1) {
			biomes.RemoveAt(i);
		}
	}

	public void SetBiomes(List<Biome> biomes) {
		if(biomes.Count == 0) {
			biomes.Add(new Biome("", 0, 1, Color.white));
		}
		this.biomes = biomes;
		selectedKey = -1;
		isMouseDown = false;
	}
		
	public void OnChange(OnChangeDelegate onChangeDelegate) {
		this.onChangeDelegate = onChangeDelegate;
	}

	private void OnEnable()	{
		titleContent.text = "Biome Editor";
		position.Set(position.x, position.y, 250, 400);
		minSize = new Vector2(250, 400);
		maxSize = new Vector2(250, 400);
	}

	private void OnDisable() {
		UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
	}

	private Texture GetTexture(int height) {
		Texture2D texture = new Texture2D(1, height);
		Color[] colors = new Color[height];
		for(int i = 0; i < height; i++) {
			for(int j = 0; j < biomes.Count; j++) {
				if(biomes[j].maxHeight > ((float) i / (height - 1))) {
					colors[height - 1 - i] = biomes[j].color;
					break;
				}
			}
		}
		texture.SetPixels(colors);
		texture.Apply();
		return texture;
	}
}

