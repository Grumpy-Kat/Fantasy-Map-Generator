﻿using System;
using UnityEngine;

[Serializable]
public class Resource {
    public string name;

    // Changing these settings alters the properties of the resources
    // For instance, for a more rare resource, decrease the threshold
    // For more thin veins, increase the lacunarity and persistance
    public float threshold;
    public float minLerp;
    public float maxLerp;
    public int octaves;
    public float persistence;
    public float lacunarity;

    public Color color;
    public Renderer renderer;

    public Resource(string name, float threshold, float minLerp, float maxLerp, int octaves, float persistence, float lacunarity, Color color, Renderer renderer) {
        this.name = name;
        this.threshold = threshold;
        this.minLerp = minLerp;
        this.maxLerp = maxLerp;
        this.octaves = octaves;
        this.persistence = persistence;
        this.lacunarity = lacunarity;
        this.color = color;
        this.renderer = renderer;
    }
}
