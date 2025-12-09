using UnityEngine;

public static class CardColorUtil
{
	// Converts HEX to Unity Color
	public static Color Hex(string hex)
	{
		Color c;
		if (ColorUtility.TryParseHtmlString(hex, out c))
			return c;
		return Color.white;
	}

	// Rarity colors based on card points
	public static Color GetRarityColor(int points)
	{
		switch (points)
		{
			case 5: return Hex("#FFF156"); // LEGENDARY (YELLOW)
			case 4: return Hex("#F89AFF"); // UNIQUE (PINK)
			case 3: return Hex("#99D3FF"); // RARE (LIGHT BLUE)
			case 1:
			case 2:
			default: return Hex("#CCCCCC"); // COMMON (GREY)
		}
	}

	// Suit colors
	public static Color GetSuitColor(string suit)
	{
		if (suit == null) return Hex("#CCCCCC");

		switch (suit.ToLower())
		{
			case "analytical": return Hex("#4C94F8"); // Analytical (BLUE)
			case "creative": return Hex("#4DBC61"); // Creative (GREEN)
			case "structured": return Hex("#ED834A"); // Structured (ORANGE)
			case "social": return Hex("#BC4D4D"); // Social (RED)
			case "adaptive": return Hex("#724DBC"); // Structured (PURPLE)
			default: return Hex("#CCCCCC"); // UNKOWN (GREY)
		}
	}
}
