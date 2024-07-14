using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class InputManager : MonoBehaviour
    {
        public int LaneCount = 5;
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

            moveUpPlayer1.performed += ctx => MovePlayer1Up();
            moveDownPlayer1.performed += ctx => MovePlayer1Down();
            moveUpPlayer2.performed += ctx => MovePlayer2Up();
            moveDownPlayer2.performed += ctx => MovePlayer2Down();
            clickPlayer1.performed += ctx => ClickFocusedButton(spawnButtons[0]);
            clickPlayer2.performed += ctx => ClickFocusedButton(spawnButtons[1]);
        }

        private void OnEnable()
        {
            moveUpPlayer1.Enable();
            moveDownPlayer1.Enable();
            moveUpPlayer2.Enable();
            moveDownPlayer2.Enable();
            clickPlayer1.Enable();
            clickPlayer2.Enable();
        }

        private void OnDisable()
        {
            moveUpPlayer1.Disable();
            moveDownPlayer1.Disable();
            moveUpPlayer2.Disable();
            moveDownPlayer2.Disable();
            clickPlayer1.Disable();
            clickPlayer2.Disable();
        }

        private void Start()
        {
            InitializeLaneSystem();
            InitializeUIElements();
            InitializeButtonClickEvents();
            SetInitialFocus();
        }

        private void InitializeLaneSystem()
        {
            laneSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<LaneSystem>();
        }

        private void InitializeUIElements()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            lanes = new VisualElement[LaneCount];
            spawnButtons = new Button[2][];
            spawnButtonClicked = new Action[2][];
            spawnButtons[0] = new Button[LaneCount];
            spawnButtons[1] = new Button[LaneCount];
            spawnButtonClicked[0] = new Action[LaneCount];
            spawnButtonClicked[1] = new Action[LaneCount];
        }

        private void InitializeButtonClickEvents()
        {
            for (var i = 0; i < lanes.Length; i++)
            {
                var laneCount = i + 1;
                lanes[i] = root.Q<VisualElement>($"Lane{laneCount}");
                InitializeButtonForLane(laneCount, i);
            }
        }

        private void InitializeButtonForLane(int laneCount, int index)
        {
            spawnButtons[0][index] = lanes[index].Q<Button>($"Player1SpawnButton");
            spawnButtons[1][index] = lanes[index].Q<Button>($"Player2SpawnButton");
            spawnButtonClicked[0][index] = () => laneSystem.SpawnUnitFor(PlayerTag.Player1, laneCount);
            spawnButtonClicked[1][index] = () => laneSystem.SpawnUnitFor(PlayerTag.Player2, laneCount);
            spawnButtons[0][index].clicked += spawnButtonClicked[0][index];
            spawnButtons[1][index].clicked += spawnButtonClicked[1][index];
        }

        private void SetInitialFocus()
        {
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP_8_1
#else
            var midLaneIndex = LaneCount % 2 == 0 ? (LaneCount / 2) : (LaneCount - 1) / 2;
            spawnButtons[0][midLaneIndex].AddToClassList("focused");
            spawnButtons[1][midLaneIndex].AddToClassList("focused");
#endif
        }

        private void OnDestroy()
        {
            for (var i = 0; i < lanes.Length; i++)
            {
                spawnButtons[0][i].clicked -= spawnButtonClicked[0][i];
                spawnButtons[1][i].clicked -= spawnButtonClicked[1][i];
            }
        }

        private void MovePlayer1Up()
        {
            var player1FocusedButton = FindFocusedButtonIndex(spawnButtons[0]);
            if (player1FocusedButton != 0)
            {
                SetButtonFocus(spawnButtons[0][player1FocusedButton - 1], spawnButtons[0][player1FocusedButton]);
            }
        }

        private void MovePlayer1Down()
        {
            var player1FocusedButton = FindFocusedButtonIndex(spawnButtons[0]);
            if (player1FocusedButton != LaneCount - 1)
            {
                SetButtonFocus(spawnButtons[0][player1FocusedButton + 1], spawnButtons[0][player1FocusedButton]);
            }
        }

        private void MovePlayer2Up()
        {
            var player2FocusedButton = FindFocusedButtonIndex(spawnButtons[1]);
            if (player2FocusedButton != 0)
            {
                SetButtonFocus(spawnButtons[1][player2FocusedButton - 1], spawnButtons[1][player2FocusedButton]);
            }
        }

        private void MovePlayer2Down()
        {
            var player2FocusedButton = FindFocusedButtonIndex(spawnButtons[1]);
            if (player2FocusedButton != LaneCount - 1)
            {
                SetButtonFocus(spawnButtons[1][player2FocusedButton + 1], spawnButtons[1][player2FocusedButton]);
            }
        }

        private static void SetButtonFocus(Button newFocusButton, Button oldFocusButton)
        {
            oldFocusButton.RemoveFromClassList("focused");
            newFocusButton.AddToClassList("focused");
            newFocusButton.Focus();
        }

        private static void ClickFocusedButton(Button[] buttons)
        {
            var button = buttons[FindFocusedButtonIndex(buttons)];
            using var e = new NavigationSubmitEvent();
            e.target = button;
            button.SendEvent(e);
        }

        private static int FindFocusedButtonIndex(Button[] buttons)
        {
            for (var i = 0; i < buttons.Length; i++)
            {
                if (buttons[i].ClassListContains("focused"))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
