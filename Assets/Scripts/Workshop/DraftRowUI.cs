using Assets.Scripts.Service;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Workshop
{
    public class DraftRowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Button deleteButton;

        // ID da carta
        private long _cardId;
        private CardService _cardService;
        public event Action<long> OnDeleteClicked;

        private void Awake()
        {
            _cardService = new CardService();

            if (deleteButton != null)
                deleteButton.onClick.AddListener(OnClickDelete);
        }

        public void Setup(WorkshopCardDTO draft, Action onClick)
        {
            if (draft == null)
            {
                nameText.text = "";
                selectButton.onClick.RemoveAllListeners();
                return;
            }

            var label = string.IsNullOrEmpty(draft.name) ? "(sem nome)" : draft.name;
            if (!string.IsNullOrEmpty(draft.status))
                label += $" [{draft.status}]";

            nameText.text = label;
            SetCardId(draft.id);

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onClick?.Invoke());
        }

        public void SetCardId(long cardId)
        {
            _cardId = cardId;
        }

        private void OnClickDelete()
        {
            deleteButton.interactable = false;

            // Apagar linha ASYNC
            _ = DeleteCardAsync();
        }

        private async Task DeleteCardAsync()
        {
            try
            {
                await _cardService.DeleteCardAsync(AuthBootstrapper.CurrentUserId, _cardId);
                Destroy(gameObject); // Apagar linha
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to delete card: " + e);
                deleteButton.interactable = true; // volta a ativar o botão se falhar
            }
        }
    }
}

