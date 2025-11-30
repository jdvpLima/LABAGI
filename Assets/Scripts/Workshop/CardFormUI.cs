using Assets.Scripts.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Workshop
{
    public class CardFormUI: MonoBehaviour
    {
        [Header("API")]
        [SerializeField] private CardWorkshopApiClient apiClient;

        [Header("Fields")]
        
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField amountInput;
        [SerializeField] private TMP_InputField flavorTextInput;

        [SerializeField] private TMP_Dropdown suitDropdown;
        [SerializeField] private TMP_Dropdown rarityDropdown;        
        [SerializeField] private TMP_Dropdown pointsDropdown;
        [SerializeField] private TMP_Dropdown triggerDropdown;
        [SerializeField] private TMP_Dropdown effectDropdown;
        [SerializeField] private TMP_Dropdown targetDropdown;        
        [SerializeField] private TMP_Dropdown expansionDropdown;

        [Header("Preview Panel")]
        [SerializeField] private Image suitImg;
        [SerializeField] private TMP_Text cardNameLbl;
        [SerializeField] private TMP_Text previewSuitLbl;
        [SerializeField] private TMP_Text previewRarityLbl;
        [SerializeField] private TMP_Text previewAbilityLbl;
        [SerializeField] private TMP_Text previewPointsLbl;
        [SerializeField] private TMP_Text previewFlavorLbl;

        [SerializeField] private Toggle oncePerGameToggle;

        [Header("Buttons")]
        [SerializeField] private Button saveDraftButton;
        [SerializeField] private Button submitButton;

        public event Action<CardDto, string> OnSubmitClicked;   // (card, status)
        public event Action<CardDto> OnFormChanged;

        private long _currentCardId;
        private readonly TMP_Text _pointsLabel;
        private string _abilityJson;


        private void Awake()
        {
            if (saveDraftButton != null)
                saveDraftButton.onClick.AddListener(() => Submit("draft"));

            if (submitButton != null)
                submitButton.onClick.AddListener(() => Submit("active"));

            if (nameInput != null)
                nameInput.onValueChanged.AddListener(_ => NotifyChanged());

            if (pointsDropdown != null)
                pointsDropdown.onValueChanged.AddListener(_ => NotifyChanged());

            if (triggerDropdown != null)
                triggerDropdown.onValueChanged.AddListener(_ => NotifyChanged());

            if (effectDropdown != null)
                effectDropdown.onValueChanged.AddListener(_ => NotifyChanged());

            if (amountInput != null)
            {
                amountInput.onValueChanged.AddListener(_ => NotifyChanged());
                amountInput.onEndEdit.AddListener(_ => ValidateAmount());
            }

            if (targetDropdown != null)
                targetDropdown.onValueChanged.AddListener(_ => NotifyChanged());

            if (suitDropdown != null)
                suitDropdown.onValueChanged.AddListener(_ => NotifyChanged());

            if (rarityDropdown != null)
                rarityDropdown.onValueChanged.AddListener(_ => NotifyChanged());

            if (oncePerGameToggle != null)
                oncePerGameToggle.onValueChanged.AddListener(_ => NotifyChanged());

            if (flavorTextInput != null)
                flavorTextInput.onValueChanged.AddListener(_ => NotifyChanged());

            if (expansionDropdown != null)
                expansionDropdown.onValueChanged.AddListener(_ => NotifyChanged());

            if (pointsDropdown != null)
                pointsDropdown.onValueChanged.AddListener(OnPointsDropdownChanged);
        }
        private void ValidateAmount()
        {
            if (!int.TryParse(amountInput.text, out var value))
            {
                value = 1; // default
            }

            if (value < 1) value = 1;
            if (value > 4) value = 4;

            amountInput.text = value.ToString();
        }

        private void Start()
        {
            // carrega valores dinâmicos para os dropdowns a partir do /api/Cards/runtime
            if (apiClient != null)
                StartCoroutine(InitDynamicDropdowns());
            else
                Debug.LogWarning("[CardFormUI] apiClient não está ligado; dropdowns vão usar opções estáticas.");
        }

        public void LoadFrom(CardDto dto)
        {
            _currentCardId = dto.cardId;

            nameInput.text = dto.name;
            previewAbilityLbl.text = dto.ability;
            flavorTextInput.text = dto.flavorText;
            amountInput.text = dto.amount.ToString();
            oncePerGameToggle.isOn = dto.oncePerGame;

            suitDropdown.value = suitDropdown.options.FindIndex(o => o.text == dto.suit);
            rarityDropdown.value = rarityDropdown.options.FindIndex(o => o.text == dto.rarity);
            triggerDropdown.value = triggerDropdown.options.FindIndex(o => o.text == dto.trigger);
            effectDropdown.value = effectDropdown.options.FindIndex(o => o.text == dto.effect);
            targetDropdown.value = targetDropdown.options.FindIndex(o => o.text == dto.target);
            expansionDropdown.value = expansionDropdown.options.FindIndex(o => o.text == dto.expansionCode);

            if (pointsDropdown != null)
            {
                var txt = dto.points.ToString();
                var idx = pointsDropdown.options.FindIndex(o => o.text == txt);
                if (idx >= 0)
                    pointsDropdown.value = idx;
            }

            UpdateAbilityFields(dto);
            UpdatePreview(dto);
            OnFormChanged?.Invoke(dto);
        }

        public void ClearForm()
        {
            _currentCardId = 0;

            nameInput.text = "";
            previewAbilityLbl.text = "";
            _abilityJson = string.Empty;
            flavorTextInput.text = "";
            amountInput.text = "0";
            oncePerGameToggle.isOn = false;

            // dropdowns podem ficar nos valores default (0)
            if (pointsDropdown != null)
                pointsDropdown.value = 0;

            NotifyChanged();
        }

        private CardDto BuildDto()
        {
            var dto = BuildDtoCoreWithoutAbility();
            UpdateAbilityFields(dto);
            return dto;
        }
        private void UpdateAbilityFields(CardDto dto)
        {
            if (dto == null) return;

            dto.ability = AbilityTextBuilder.Build(dto);
            dto.abilityJson = AbilityJsonBuilder.Build(dto);

            if (previewAbilityLbl != null)
                previewAbilityLbl.text = dto.ability ?? string.Empty;

            // nada de UI para abilityJson aqui
            _abilityJson = dto.abilityJson;
        }
        private CardDto BuildDtoCoreWithoutAbility()
        {

            int amount;
            if (!int.TryParse(amountInput.text, out amount))
                amount = 1;
            if (amount < 1) amount = 1;
            if (amount > 4) amount = 4;

            return new CardDto
            {
                cardId = _currentCardId,
                name = nameInput.text.Trim(),
                suit = suitDropdown.options[suitDropdown.value].text,
                rarity = rarityDropdown.options[rarityDropdown.value].text,
                points = Convert.ToInt32(pointsDropdown.options[pointsDropdown.value].text),
                trigger = triggerDropdown.options[triggerDropdown.value].text,
                effect = effectDropdown.options[effectDropdown.value].text,
                amount = amount,
                target = targetDropdown.options[targetDropdown.value].text,
                oncePerGame = oncePerGameToggle.isOn,
                flavorText = flavorTextInput.text,
                expansionCode = expansionDropdown.options[expansionDropdown.value].text
            };
        }

        private void NotifyChanged()
        {
            var dto = BuildDtoCoreWithoutAbility();
            UpdateAbilityFields(dto);   // gera ability e abilityJson
            UpdatePreview(dto);         // atualiza o painel de preview
            OnFormChanged?.Invoke(dto);
        }

        private void Submit(string status)
        {
            var dto = BuildDto();
            OnSubmitClicked?.Invoke(dto, status);
        }

        // =========================
        //  Dinamic dropdown logic
        // =========================

        private IEnumerator InitDynamicDropdowns()
        {
            List<CardDto> cards = null;
            string error = null;

            yield return apiClient.GetRuntimeCards(
                list => cards = list,
                err => error = err
            );

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError("[CardFormUI] Erro ao carregar /api/Cards/runtime: " + error);
                yield break;
            }

            if (cards == null || cards.Count == 0)
            {
                Debug.LogWarning("[CardFormUI] /api/Cards/runtime devolveu lista vazia.");
                yield break;
            }

            var suits = DistinctStrings(cards.Select(c => c.suit));
            var rarities = DistinctStrings(cards.Select(c => c.rarity));
            var triggers = DistinctStrings(cards.Select(c => c.trigger));
            var effects = DistinctStrings(cards.Select(c => c.effect));
            var targets = DistinctStrings(cards.Select(c => c.target));
            var points = cards.Select(c => c.points)
                              .Distinct()
                              .OrderBy(p => p)
                              .Select(p => p.ToString())
                              .ToList();

            SetDropdownOptions(suitDropdown, suits);
            SetDropdownOptions(rarityDropdown, rarities);
            SetDropdownOptions(triggerDropdown, triggers);
            SetDropdownOptions(effectDropdown, effects);
            SetDropdownOptions(targetDropdown, targets);
            SetDropdownOptions(pointsDropdown, points);
        }

        private static List<string> DistinctStrings(IEnumerable<string> source)
        {
            return source
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .OrderBy(s => s)
                .ToList();
        }

        private static void SetDropdownOptions(TMP_Dropdown dropdown, List<string> values)
        {
            if (dropdown == null || values == null) return;

            dropdown.ClearOptions();
            dropdown.AddOptions(values);
        }

        private void OnPointsDropdownChanged(int index)
        {
            if (pointsDropdown == null) return;
            if (index < 0 || index >= pointsDropdown.options.Count) return;

            if (previewPointsLbl != null)
                previewPointsLbl.text = pointsDropdown.options[index].text;
        }

        private void UpdatePreview(CardDto dto)
        {
            if (dto == null) return;

            if (cardNameLbl != null)
                cardNameLbl.text = dto.name ?? string.Empty;

            if (previewSuitLbl != null)
                previewSuitLbl.text = dto.suit ?? string.Empty;

            if (previewRarityLbl != null)
                previewRarityLbl.text = dto.rarity ?? string.Empty;

            if (previewPointsLbl != null)
                previewPointsLbl.text = dto.points.ToString();

            if (previewFlavorLbl != null)
                previewFlavorLbl.text = dto.flavorText ?? string.Empty;

            // abilityLbl já é tratado em UpdateAbilityFields(dto)

            // Atualizar imagem do suit
            if (suitImg != null)
            {
                var sprite = LoadSuitSprite(dto.suit);
                if (sprite != null)
                    suitImg.sprite = sprite;
            }
        }
        private Sprite LoadSuitSprite(string suitName)
        {
            if (string.IsNullOrEmpty(suitName))
                return null;

            // Exemplo: suitName = "Analitycal" → Resources/Suits/Analitycal
            return Resources.Load<Sprite>($"Suits/{suitName}");
        }

    }
}
