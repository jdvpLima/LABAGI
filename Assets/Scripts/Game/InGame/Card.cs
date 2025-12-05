using Assets.Scripts.Model;
using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class Card
{
    public long CardId { get; private set; }
    public string Name { get; private set; } = "";
	public string Suit { get; private set; } = "";
	public int Points { get; private set; } = 0;
	public string Rarity { get; private set; } = "";

	public string FlavourText { get; private set; } = "";

	public List<string> Actions { get; private set; } = new List<string>();

    public Card(string name, string suit, int points = 0, string rarity = "", string flavourText = null)
    {
        Name = name;
        Suit = suit;
        Points = points;
        Rarity = rarity;
        if (flavourText != null) FlavourText = flavourText;
    }

    // Factory para criar a Card a partir do CardDto
    public static Card FromDto(CardDto dto)
    {
        // Mapeamento básico de campos
        var flavour = dto.flavorText ?? dto.flavorText ?? "";
        var card = new Card(dto.name ?? $"Card {dto.cardId}", dto.suit ?? "", dto.points, dto.rarity ?? "", flavour);

        card.CardId = dto.cardId;

        // Popula Actions:
        // 1) if abilityJson contém um array de strings — desserializa
        // 2) else, se effect/ability/trigger existir — adiciona como ação única (ou separa por ';')
        if (!string.IsNullOrEmpty(dto.abilityJson))
        {
            try
            {
                // tenta desserializar para List<string>
                var actions = JsonConvert.DeserializeObject<List<string>>(dto.abilityJson);
                if (actions != null)
                    card.Actions.AddRange(actions);
                else
                    card.Actions.Add(dto.abilityJson);
            }
            catch
            {
                // fallback: guarda o raw json como uma única ação
                card.Actions.Add(dto.abilityJson);
            }
        }
        else
        {
            // juntar possíveis campos que a API devolve
            if (!string.IsNullOrEmpty(dto.effect))
                card.Actions.Add(dto.effect);
            if (!string.IsNullOrEmpty(dto.ability))
                card.Actions.Add(dto.ability);
            if (!string.IsNullOrEmpty(dto.trigger))
                card.Actions.Add(dto.trigger);
        }

        return card;
    }
}
