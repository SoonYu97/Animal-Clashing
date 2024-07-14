using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class RebindInputManager : MonoBehaviour
    {
        public InputActionAsset InputActions;

        private VisualElement root;
        private VisualElement[] lanes;
        private Button[][] spawnButtons;
        private Action[][] spawnButtonClicked;

        private LaneSystem laneSystem;
        private InputAction moveUpPlayer1;
        private InputAction moveDownPlayer1;
        private InputAction moveUpPlayer2;
        private InputAction moveDownPlayer2;
        private InputAction clickPlayer1;
        private InputAction clickPlayer2;

        private void Awake()
        {
            InitializeInputActions();
        }

        private void InitializeInputActions()
        {
            var gameplayActionMap = InputActions.FindActionMap("Gameplay");

            moveUpPlayer1 = gameplayActionMap.FindAction("MoveUpPlayer1");
            moveDownPlayer1 = gameplayActionMap.FindAction("MoveDownPlayer1");
            moveUpPlayer2 = gameplayActionMap.FindAction("MoveUpPlayer2");
            moveDownPlayer2 = gameplayActionMap.FindAction("MoveDownPlayer2");
            clickPlayer1 = gameplayActionMap.FindAction("ClickPlayer1");
            clickPlayer2 = gameplayActionMap.FindAction("ClickPlayer2");
        }

        private void Start()
        {
            SetupRebindingUI();
        }

        private void SetupRebindingUI()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            var rebindMenu = root.Q<VisualElement>("RebindMenu");

            var rebindMoveUpPlayer1Button = rebindMenu.Q<Button>("RebindMoveUpPlayer1Button");
            var rebindMoveDownPlayer1Button = rebindMenu.Q<Button>("RebindMoveDownPlayer1Button");
            var rebindSubmitPlayer1Button = rebindMenu.Q<Button>("RebindSubmitPlayer1Button");
            var rebindMoveUpPlayer2Button = rebindMenu.Q<Button>("RebindMoveUpPlayer2Button");
            var rebindMoveDownPlayer2Button = rebindMenu.Q<Button>("RebindMoveDownPlayer2Button");
            var rebindSubmitPlayer2Button = rebindMenu.Q<Button>("RebindSubmitPlayer2Button");

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP_8_1
#else
            SetupRebindButton(rebindMoveUpPlayer1Button, moveUpPlayer1, "Move Up (Player 1)");
            SetupRebindButton(rebindMoveDownPlayer1Button, moveDownPlayer1, "Move Down (Player 1)");
            SetupRebindButton(rebindSubmitPlayer1Button, clickPlayer1, "Click (Player 1)");
            SetupRebindButton(rebindMoveUpPlayer2Button, moveUpPlayer2, "Move Up (Player 2)");
            SetupRebindButton(rebindMoveDownPlayer2Button, moveDownPlayer2, "Move Down (Player 2)");
            SetupRebindButton(rebindSubmitPlayer2Button, clickPlayer2, "Click (Player 2)");
#endif
        }


        private void SetupRebindButton(Button button, InputAction action, string actionName)
        {
            button.text =
                $"{actionName}: {InputControlPath.ToHumanReadableString(action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice)}";
            button.clicked += () =>
            {
                button.text =
                    $"{actionName}: ?";
                StartRebind(action, () =>
                {
                    button.text =
                        $"{actionName}: {InputControlPath.ToHumanReadableString(action.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice)}";
                    Debug.Log($"Rebind Complete: {actionName}");
                });
            };
        }

        private void OnDestroy()
        {
        }

        // Method to start the rebinding process
        private static void StartRebind(InputAction actionToRebind, Action onComplete)
        {
            actionToRebind.Disable();

            actionToRebind.PerformInteractiveRebinding()
                .WithControlsExcluding("Mouse")
                .OnComplete(operation =>
                {
                    actionToRebind.Enable();
                    operation.Dispose();
                    onComplete?.Invoke();
                })
                .Start();
        }
    }
}