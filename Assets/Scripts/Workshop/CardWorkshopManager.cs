using Assets.Scripts.Service;
using System.Collections.Generic;
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
        [SerializeField] private WorkshopDraftManager draftPanel; 

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
            formPanel.OnSubmitClicked += HandleSubmitAsync;

            if (draftPanel != null)
                draftPanel.OnDraftSelected += formPanel.LoadFrom;

            _ = LoadWorkshopAsync();
        }

        private void OnDisable()
        {
            formPanel.OnFormChanged -= OnFormChanged;
            formPanel.OnSubmitClicked -= HandleSubmitAsync;

            if (draftPanel != null)
                draftPanel.OnDraftSelected -= formPanel.LoadFrom;
        }
        
        private void OnFormChanged(WorkshopCardDTO dto)
        {
            if (previewPanel != null)
                previewPanel.UpdatePreview(dto);
        }


        private async Task LoadWorkshopAsync()
        {
            long userId = AuthBootstrapper.CurrentUserId;

            List<WorkshopCardDTO> runtimeCards = null;
            List<WorkshopCardDTO> userWorkshopCards = null;

            try
            {
                var runtimeTask = _cardService.GetRuntimeCardsAsync();
                var userTask = _cardService.GetUserWorkshopCardsAsync(userId);

                await Task.WhenAll(runtimeTask, userTask);

                runtimeCards = runtimeTask.Result;
                userWorkshopCards = userTask.Result;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[CardWorkshopManager] Erro ao carregar dados: " + e);
                return;
            }

            Debug.Log($"[CardWorkshopManager] runtimeCards = {runtimeCards?.Count ?? 0}, drafts = {userWorkshopCards?.Count ?? 0}");

            formPanel.Init(runtimeCards ?? new List<WorkshopCardDTO>());

            if (draftPanel != null)
                draftPanel.Init(userWorkshopCards ?? new List<WorkshopCardDTO>());
        }

        // ASSINATURA ALINHADA COM event Action<WorkshopCardDTO, string>
        private async void HandleSubmitAsync(WorkshopCardDTO dto, string status)
        {
            long userId = AuthBootstrapper.CurrentUserId;
            dto.status = status;

            WorkshopCardDTO saved = null;

            try
            {
                saved = await _cardService.UpsertWorkshopCardAsync(userId, dto);

                formPanel.ClearForm();

                if (draftPanel != null)
                    draftPanel.AddOrUpdateCard(saved);
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

            // se ficou active, dar 1 cópia ao inventário
            if (saved.status == "active")
            {
                await _cardService.GrantCardToInventoryAsync(userId, saved.id, 4);
            }
        }

        public void ReturnToMenu()
        {
            MainMenuManager.workshop = false;
            SceneManager.UnloadSceneAsync("Workshop");
        }
    }
}
