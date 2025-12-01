using Assets.Scripts.Model;
using Assets.Scripts.Service;
using System.Text;

namespace Assets.Scripts.Workshop
{
    public static class AbilityTextBuilder
    {
        public static string Build(WorkshopCardDTO dto)
        {
            if (dto == null)
                return string.Empty;

            var prefix = BuildTriggerPrefix(dto.trigger, dto.oncePerGame);
            var effectPart = BuildEffectChunk(dto.effect, dto.target, dto.amount);

            var sb = new StringBuilder();

            if (!string.IsNullOrEmpty(prefix))
            {
                sb.Append(prefix);
                if (!prefix.EndsWith(":") && !prefix.EndsWith(",")) sb.Append(" ");
            }

            sb.Append(effectPart);

            return sb.ToString();
        }

        private static string BuildTriggerPrefix(string trigger, bool oncePerGame)
        {
            if (string.IsNullOrEmpty(trigger))
                return string.Empty;

            // once_per_game tem prioridade
            if (oncePerGame || trigger == "once_per_game")
            {
                return "Once per game:";
            }

            switch (trigger)
            {
                case "on_accept_accept":
                    return "If both Accept,";
                case "on_accept_refuse":
                    return "If you Accept and opponent Refuses,";
                case "on_refuse_refuse":
                    return "If both Refuse,";
                case "on_reveal":
                    return "After reveal,";
                case "on_points":
                    return "After scoring,";
                case "on_choice":
                    return "Before choices,";
                default:
                    return string.Empty; // sem prefixo
            }
        }

        private static string BuildEffectChunk(string effect, string target, int amount)
        {
            if (string.IsNullOrEmpty(effect))
                return string.Empty;

            if (amount < 1) amount = 1;

            switch (effect)
            {
                case "draw":
                    return BuildDrawText(target, amount);

                case "reduce_burnout":
                    return BuildReduceBurnoutText(target, amount);

                case "gain_points":
                    return BuildGainPointsText(target, amount);

                case "set_hand_cap":
                    return $"set next round hand cap to {amount}.";

                case "hold_overdraw":
                    return $"keep up to {amount} cards until your next play.";

                case "peek":
                    return BuildPeekText(target, amount);

                case "reorder_top":
                    return BuildReorderTopText(target, amount);

                case "prevent_burnout":
                    return "prevent 1 Burnout gain you would take this round.";

                case "innovation_bonus":
                    return "gain Innovation bonus.";

                case "opponent_draw":
                    return $"opponent draws {Plural(amount, "card", "cards")}.";

                case "both_draw":
                    return $"both players draw {Plural(amount, "card", "cards")}.";

                case "set_next_value":
                    return $"set your next revealed value to +{amount}.";

                case "swap_with_top":
                    return "swap a card in hand with the top of your deck.";

                case "none":
                    return "apply a pacing-only effect.";

                default:
                    // fallback genérico (para efeitos novos que apareçam)
                    return $"{effect} ({target}) x{amount}.";
            }
        }

        private static string BuildDrawText(string target, int amount)
        {
            var cards = Plural(amount, "card", "cards");
            switch (target)
            {
                case "self":
                    return $"draw {cards}.";
                case "opponent":
                    return $"opponent draws {cards}.";
                case "both":
                    return $"both players draw {cards}.";
                default:
                    return $"draw {cards}.";
            }
        }

        private static string BuildReduceBurnoutText(string target, int amount)
        {
            switch (target)
            {
                case "self":
                    return $"reduce your Burnout by {amount} (min 0).";
                case "opponent":
                    return $"opponent reduces Burnout by {amount} (min 0).";
                case "both":
                    return $"both reduce Burnout by {amount} (min 0).";
                default:
                    return $"reduce Burnout by {amount} (min 0).";
            }
        }

        private static string BuildGainPointsText(string target, int amount)
        {
            var pts = $"+{amount}";
            switch (target)
            {
                case "self":
                    return $"gain {pts}.";
                case "opponent":
                    return $"opponent gains {pts}.";
                case "both":
                    return $"both gain {pts}.";
                default:
                    return $"gain {pts}.";
            }
        }

        private static string BuildPeekText(string target, int amount)
        {
            var cards = Plural(amount, "card", "cards");
            switch (target)
            {
                case "deck":
                    return $"look at the top {cards} of your deck.";
                case "opponent":
                    return $"peek at opponent’s top {cards}.";
                default:
                    return $"look at the top {cards}.";
            }
        }

        private static string BuildReorderTopText(string target, int amount)
        {
            var cards = Plural(amount, "card", "cards");
            switch (target)
            {
                case "deck":
                    return $"reorder the top {cards} of your deck.";
                default:
                    return $"reorder the top {cards}.";
            }
        }

        private static string Plural(int n, string singular, string plural)
        {
            return n == 1 ? $"1 {singular}" : $"{n} {plural}";
        }
    }
}
