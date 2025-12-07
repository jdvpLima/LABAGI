using Assets.Scripts.Main_Menu;
using Assets.Scripts.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Workshop
{
    public class CardFormUI : MonoBehaviour
    {
        [Header("Fields")]
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_Dropdown suitDropdown;
        [SerializeField] private TMP_Dropdown rarityDropdown;
        [SerializeField] private TMP_Dropdown pointsDropdown;
        //[SerializeField] private TMP_InputField abilityInput;
        [SerializeField] private TMP_Dropdown triggerDropdown;
        [SerializeField] private TMP_Dropdown effectDropdown;
        [SerializeField] private TMP_InputField amountInput;
        [SerializeField] private TMP_Dropdown targetDropdown;
        [SerializeField] private Toggle oncePerGameToggle;
        //[SerializeField] private InputField abilityJsonInput;
        [SerializeField] private TMP_InputField flavorTextInput;

        [Header("Buttons")]
        [SerializeField] private Button saveDraftButton;
        [SerializeField] private Button submitButton;

        [Header("Preview")]
        [SerializeField] private CardPreviewUI previewUI;

        [Header("Word Filter")]
        [SerializeField] private LocalContentFilter contentFilter;
        [SerializeField] private TMP_Text validationMessageText;

        // Eventos usados pelo CardWorkshopManager
        public event Action<WorkshopCardDTO, string> OnSubmitClicked;
        public event Action<WorkshopCardDTO> OnFormChanged;

        private long _currentId = 0;
        private string _currentStatus = null;
        private bool _hasOffensiveContent;
        private bool _hasNonEnglishText;


        private void Awake()
        {
            // Listeners de alteração de campos
            nameInput.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            amountInput.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            flavorTextInput.onValueChanged.AddListener(_ => OnAnyFieldChanged());

            suitDropdown.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            rarityDropdown.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            triggerDropdown.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            effectDropdown.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            targetDropdown.onValueChanged.AddListener(_ => OnAnyFieldChanged());
            pointsDropdown.onValueChanged.AddListener(_ => OnAnyFieldChanged());

            oncePerGameToggle.onValueChanged.AddListener(_ => OnAnyFieldChanged());
        }
        public void Init(List<WorkshopCardDTO> runtimeCards)
        {
            validationMessageText.text = string.Empty;
            if (runtimeCards == null || runtimeCards.Count == 0)
            {
                Debug.LogWarning("[CardFormUI] Init chamado com lista vazia de runtimeCards.");
                return;
            }

            // Suits
            var suits = runtimeCards
                .Where(c => !string.IsNullOrEmpty(c.suit))
                .Select(c => c.suit)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
            SetDropdownOptions(suitDropdown, suits);

            // Rarities
            var rarities = runtimeCards
                .Where(c => !string.IsNullOrEmpty(c.rarity))
                .Select(c => c.rarity)
                .Distinct()
                .OrderBy(r => r)
                .ToList();
            SetDropdownOptions(rarityDropdown, rarities);

            // Triggers
            var triggers = runtimeCards
                .Where(c => !string.IsNullOrEmpty(c.trigger))
                .Select(c => c.trigger)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            SetDropdownOptions(triggerDropdown, triggers);

            // Effects
            var effects = runtimeCards
                .Where(c => !string.IsNullOrEmpty(c.effect))
                .Select(c => c.effect)
                .Distinct()
                .OrderBy(e => e)
                .ToList();
            SetDropdownOptions(effectDropdown, effects);

            // Targets
            var targets = runtimeCards
                .Where(c => !string.IsNullOrEmpty(c.target))
                .Select(c => c.target)
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            SetDropdownOptions(targetDropdown, targets);

            // Points
            var points = runtimeCards
                .Select(c => c.points)
                .Distinct()
                .OrderBy(p => p)
                .ToList();
            if (pointsDropdown != null)
            {
                pointsDropdown.ClearOptions();
                var opt = points
                    .Select(p => new TMPro.TMP_Dropdown.OptionData(p.ToString()))
                    .ToList();
                pointsDropdown.AddOptions(opt);
            }
        }
        private void SetDropdownOptions(TMP_Dropdown dropdown, List<string> values)
        {
            if (dropdown == null) return;

            dropdown.ClearOptions();
            var options = values.Select(v => new TMP_Dropdown.OptionData(v)).ToList();
            dropdown.AddOptions(options);

            if (values.Count > 0)
                dropdown.value = 0;
        }

        // Botão "Guardar rascunho"
        public void OnClickSubmitDraft()
        {
            if (_hasOffensiveContent || _hasNonEnglishText)
            {
                UpdateValidationState();
                return;
            }

            Submit("draft");
        }

        // Botão "Submeter para revisão"
        public void OnClickSubmitForReview()
        {
            if (_hasOffensiveContent || _hasNonEnglishText)
            {
                UpdateValidationState();
                return;
            }

            Submit("active");
        }

        private void Submit(string status)
        {
            var dto = BuildDto(status);

            // atualiza status interno para futuras edições
            _currentStatus = status;

            OnSubmitClicked?.Invoke(dto, status);
        }

        private void OnAnyFieldChanged()
        {
            var dto = BuildDto(_currentStatus);

            // Atualizar preview
            if (previewUI != null)
            {
                previewUI.UpdatePreview(dto);
            }

            // Atualizar estado de validação (liga/desliga botões e mostra aviso)
            UpdateValidationState();

            // Notificar manager (para draft list, etc.)
            OnFormChanged?.Invoke(dto);
        }
        private void UpdateValidationState()
        {
            _hasOffensiveContent = HasOffensiveContentInForm();
            _hasNonEnglishText = HasNonEnglishText();

            if (validationMessageText != null)
            {
                if (_hasOffensiveContent)
                {
                    AudioManager.Instance.PlayUiError();
                    validationMessageText.text =
                        "This card contains offensive language. Remove any offensive words before saving or submitting.";
                }
                else if (_hasNonEnglishText)
                {
                    AudioManager.Instance.PlayUiError();
                    validationMessageText.text =
                        "This card must be written in English only. Please remove any non-English text.";
                }
                else
                {
                    validationMessageText.text = string.Empty;
                }
            }

            // Se quiseres bloquear os botões também em caso de não-inglês:
            bool canInteract = !_hasOffensiveContent && !_hasNonEnglishText;

            if (saveDraftButton != null)
                saveDraftButton.interactable = canInteract;

            if (submitButton != null)
                submitButton.interactable = canInteract;
        }
        /// <summary>
        /// Constrói o DTO do formulário atual, incluindo ability e abilityJson.
        /// </summary>
        private WorkshopCardDTO BuildDto(string statusOverride)
        {
            int points = 0;
            int.TryParse(GetDropdownText(pointsDropdown), out points);

            int amount = 0;
            int.TryParse(amountInput.text, out amount);

            string suit = GetDropdownText(suitDropdown);
            string rarity = GetDropdownText(rarityDropdown);
            string trigger = GetDropdownText(triggerDropdown);
            string effect = GetDropdownText(effectDropdown);
            string target = GetDropdownText(targetDropdown);

            // Construir um CardDto temporário só para alimentar os builders
            var cardForAbility = new WorkshopCardDTO
            {
                id = _currentId,
                name = nameInput.text,
                suit = suit,
                rarity = rarity,
                points = points,
                ability = null, // vai ser preenchido pelos builders
                trigger = trigger,
                effect = effect,
                amount = amount,
                target = target,
                oncePerGame = oncePerGameToggle.isOn,
                abilityJson = null,
                expansionCode = "wks",
                flavorText = flavorTextInput.text,
                status = statusOverride ?? _currentStatus,
            };

            string humanReadableAbility = AbilityTextBuilder.Build(cardForAbility);
            string abilityJson = AbilityJsonBuilder.Build(cardForAbility);

            var dto = new WorkshopCardDTO
            {
                id = _currentId,
                name = nameInput.text,
                suit = suit,
                rarity = rarity,
                points = points,
                ability = humanReadableAbility,
                trigger = trigger,
                effect = effect,
                amount = amount,
                target = target,
                oncePerGame = oncePerGameToggle.isOn,
                abilityJson = abilityJson,
                expansionCode = "wks",
                flavorText = flavorTextInput.text,
                status = statusOverride ?? _currentStatus
            };

            return dto;
        }

        private static string GetDropdownText(TMP_Dropdown ddl)
        {
            if (ddl == null || ddl.options == null || ddl.options.Count == 0)
                return string.Empty;

            int idx = ddl.value;
            if (idx < 0 || idx >= ddl.options.Count)
                return string.Empty;

            return ddl.options[idx].text;
        }

        /// <summary>
        /// Limpa o formulário para criar uma nova carta.
        /// </summary>
        public void ClearForm()
        {
            _currentId = 0;
            _currentStatus = null;

            nameInput.text = string.Empty;
            amountInput.text = "0";
            flavorTextInput.text = string.Empty;

            if (suitDropdown != null && suitDropdown.options.Count > 0) suitDropdown.value = 0;
            if (rarityDropdown != null && rarityDropdown.options.Count > 0) rarityDropdown.value = 0;
            if (triggerDropdown != null && triggerDropdown.options.Count > 0) triggerDropdown.value = 0;
            if (effectDropdown != null && effectDropdown.options.Count > 0) effectDropdown.value = 0;
            if (targetDropdown != null && targetDropdown.options.Count > 0) targetDropdown.value = 0;
            if (pointsDropdown != null && targetDropdown.options.Count > 0) targetDropdown.value = 0;

            oncePerGameToggle.isOn = false;

            OnAnyFieldChanged();

            saveDraftButton.interactable = false;
            submitButton.interactable = false;
        }

        /// <summary>
        /// Carrega a carta recebida (ex: devolvida da API) para o formulário.
        /// </summary>
        public void LoadFrom(WorkshopCardDTO dto)
        {
            if (dto == null)
            {
                ClearForm();
                return;
            }

            _currentId = dto.id;
            _currentStatus = dto.status;

            nameInput.text = dto.name;
            amountInput.text = dto.amount.ToString();
            flavorTextInput.text = dto.flavorText ?? string.Empty;

            SetDropdownValueByText(suitDropdown, dto.suit);
            SetDropdownValueByText(rarityDropdown, dto.rarity);
            SetDropdownValueByText(triggerDropdown, dto.trigger);
            SetDropdownValueByText(effectDropdown, dto.effect);
            SetDropdownValueByText(targetDropdown, dto.target);
            SetDropdownValueByText(pointsDropdown, dto.points.ToString());

            oncePerGameToggle.isOn = dto.oncePerGame;

            // aqui o texto da ability vem do dto, mas também podemos reconstruir
            var rebuilt = BuildDto(_currentStatus);
            if (previewUI != null)
            {
                previewUI.UpdatePreview(rebuilt);
            }

            OnFormChanged?.Invoke(rebuilt);
        }

        private static void SetDropdownValueByText(TMP_Dropdown ddl, string text)
        {
            if (ddl == null || ddl.options == null) return;
            int index = ddl.options.FindIndex(o => o.text == text);
            if (index >= 0)
                ddl.value = index;
        }

        private bool HasOffensiveContentInForm()
        {
            if (contentFilter == null)
                return false;

            if (contentFilter.ContainsOffensiveContent(nameInput.text))
                return true;

            if (contentFilter.ContainsOffensiveContent(flavorTextInput.text))
                return true;

            return false;
        }
        private bool HasNonEnglishText()
        {
            string unknownWord = string.Empty;
            if (contentFilter == null) return false; // ou true, se quiseres ser mais estrito

            if (!contentFilter.IsStrictEnglish(nameInput.text, out unknownWord))
                return true;

            if (!contentFilter.IsStrictEnglish(flavorTextInput.text, out unknownWord))
                return true;

            return false;
        }
    }
}
    