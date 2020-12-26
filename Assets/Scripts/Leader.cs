using System;
using UnityEngine;

[Serializable]
public class Leader {
	public string name;
	public float popularity;
	public float rage;
	public float intelligence;
	public float vicious;
	public float religious;
	public float scientific;

	public Leader(string name, float popularity, float rage, float intelligence, float vicious, float religious, float scientific) {
		this.name = name;
		this.popularity = popularity;
		this.rage = rage;
		this.intelligence = intelligence;
		this.vicious = vicious;
		this.religious = religious;
		this.scientific = scientific;
	}

	public Leader(int seed) {
		System.Random random = new System.Random(seed);
		name = NameGenerator.GenerateName(random, 3, 7, 2);
		popularity = (float) random.NextDouble() * 2 - 1;
		rage = (float) random.NextDouble() * 2 - 1;
		intelligence = (float) random.NextDouble() * 2 - 1;
		vicious = (float) random.NextDouble() * 2 - 1;
		religious = (float) random.NextDouble() * 2 - 1;
		scientific = (float) random.NextDouble() * 2 - 1;
	}

	public Leader(Leader parent, int seed) {
		System.Random random = new System.Random(seed);
		name = NameGenerator.GenerateName(random, 3, 7, 2, parent.name[0].ToString());
		popularity = GetVariation(random, parent.popularity);
		rage = GetVariation(random, parent.rage);
		intelligence = GetVariation(random, parent.intelligence);
		vicious = GetVariation(random, parent.vicious);
		religious = GetVariation(random, parent.religious);
		scientific = GetVariation(random, parent.scientific);
	}

	float GetVariation(System.Random random, float org) {
		float variation = 0.01f;
		int mutation = Mathf.Clamp(random.Next(-50, 30), 1, 30);
		return Mathf.Clamp(((float) random.NextDouble() * variation * 2 - variation) * mutation, -1, 1);
	}
}

