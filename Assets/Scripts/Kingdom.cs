using System;
using UnityEngine;

[Serializable]
public class Kingdom {
	public string name;
	public int size;
	public int population;
	public int money;
	public Color color;
	public Texture2D flag;
	public Leader leader;

	public Kingdom(string name, int size, int population, int money, Color color, Texture2D flag, Leader leader) {
		this.name = name;
		this.size = size;
		this.population = population;
		this.money = money;
		this.color = color;
		this.flag = flag;
		this.leader = leader;
	}
}

