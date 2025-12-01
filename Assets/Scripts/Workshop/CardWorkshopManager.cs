using Assets.Scripts.Model;
using Assets.Scripts.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Workshop
{
    public class CardWorkshopManager : MonoBehaviour
    {

        [Header("Panels")]
        [SerializeField] private CardFormUI formPanel;
        [SerializeField] private CardPreviewUI previewPanel;
        [SerializeField] private WorkshopDraftManager draftPanel; // opcional

        private CardService _cardService;

        private void Awake()
        {
            Debug.Log("[CardWorkshopManager] Awake - scene = " + SceneManager.GetActiveScene().name);

            _cardService = new CardService();
        }

        private void OnEnable()
        {
            Debug.Log("[CardWorkshopManager] OnEnable - scene = " + SceneManager.GetActiveScene().name);

            formPanel.OnFormChanged += OnFormChanged;
            formPanel.OnSubmitClicked += HandleSubmit;

            LoadWorkshop();
        }

        private void OnDisable()
        {
            formPanel.OnFormChanged -= OnFormChanged;
            formPanel.OnSubmitClicked -= HandleSubmit;
        }

        private void OnFormChanged(WorkshopCardDTO dto)
        {
            if (previewPanel != null)
                previewPanel.UpdatePreview(dto);
        }

        private async void LoadWorkshop()
        {
            long userId = AuthBootstrapper.CurrentUserId;

            List<WorkshopCardDTO> cards = null;
            List<WorkshopCardDTO> draftCards = null;

            try
            {
                cards = await _cardService.GetRuntimeCardsAsync();
                draftCards = await _cardService.GetUserWorkshopCardsAsync(userId);
                               
            }
            catch (System.Exception e)
            {
                Debug.LogError("[CardWorkshopManager] Erro ao carregar dados: " + e);
                return;
            }

            formPanel.Init(cards ?? new List<WorkshopCardDTO>());

            if (draftPanel != null)
                draftPanel.Init(draftCards ?? new List<WorkshopCardDTO>());
        }

        // ASSINATURA ALINHADA COM event Action<WorkshopCardDTO, string>
        private void HandleSubmit(WorkshopCardDTO dto, string status)
        {
            long userId = AuthBootstrapper.CurrentUserId;
            dto.status = status;

            WorkshopCardDTO saved = null;

            try
            {
                saved = _cardService.UpsertWorkshopCard(userId, dto);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[CardWorkshopManager] Erro em UpsertWorkshopCardAsync: " + e);
                return;
            }

            if (saved == null)
            {
                Debug.LogError("[CardWorkshopManager] Upsert devolveu null.");
                return;
            }

            formPanel.LoadFrom(saved);

            if (draftPanel != null)
                draftPanel.AddOrUpdateCard(saved);
        }
    }

}
