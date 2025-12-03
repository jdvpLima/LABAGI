using System.Collections.Generic;

[System.Serializable]
public class Card
{
	public string Name { get; private set; } = "";
	public string Suit { get; private set; } = "";
	public int Points { get; private set; } = 0;
	public string Rarity { get; private set; } = "";

	public string FlavourText { get; private set; } = "";

	public List<string> Actions { get; private set; } = new List<string>();

	public Card(string name, string suit)
	{
		Name = name;
		Suit = suit;
	}
}
