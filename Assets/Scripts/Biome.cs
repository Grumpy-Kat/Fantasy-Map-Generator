using System;
using UnityEngine;

[Serializable]
public class Biome {
	public string name;
	public float minHeight;
	public float maxHeight;
	public Color color;

	public Biome(string name, float minHeight, float maxHeight, Color color) {
		this.name = name;
		this.minHeight = minHeight;
		this.maxHeight = maxHeight;
		this.color = color;
	}
}

