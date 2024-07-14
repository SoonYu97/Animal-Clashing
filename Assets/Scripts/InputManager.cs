using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class InputManager : MonoBehaviour
    {
        public int LaneCount = 5;
        
        private VisualElement root;
        private VisualElement[] lanes;
        private Button[][] spawnButtons;
        private Action[][] spawnButtonClicked;
        
        private LaneSystem laneSystem;

        private void Start()
        {
            laneSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<LaneSystem>();
            
            root = GetComponent<UIDocument>().rootVisualElement;

            lanes = new VisualElement[LaneCount];
            spawnButtons = new Button[2][];
            spawnButtonClicked = new Action[2][];
            spawnButtons[0] = new Button[LaneCount];
            spawnButtons[1] = new Button[LaneCount];
            spawnButtonClicked[0] = new Action[LaneCount];
            spawnButtonClicked[1] = new Action[LaneCount];
            
            for (var i = 0; i < lanes.Length; i++)
            {
                var laneCount = i + 1;
                lanes[i] = root.Q <VisualElement>($"Lane{laneCount}");
                spawnButtons[0][i] = lanes[i].Q<Button>($"Player1SpawnButton");
                spawnButtons[1][i] = lanes[i].Q<Button>($"Player2SpawnButton");
                spawnButtonClicked[0][i] = () => laneSystem.SpawnUnitFor(PlayerTag.Player1, laneCount);
                spawnButtonClicked[1][i] = () => laneSystem.SpawnUnitFor(PlayerTag.Player2, laneCount);
                spawnButtons[0][i].clicked += spawnButtonClicked[0][i];
                spawnButtons[1][i].clicked += spawnButtonClicked[1][i];
            }

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

        private void Update()
        {
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP_8_1
            ManageTouch();
#else
            ManageKeyboard();
#endif
        }
        
        private void ManageKeyboard()
        {
            var keyboard = Keyboard.current;

            if (keyboard.wKey.wasPressedThisFrame)
            {
                var player1FocusedButton = FindFocusedButtonIndex(spawnButtons[0]);
                if (player1FocusedButton != 0)
                {
                    SetButtonFocus(spawnButtons[0][player1FocusedButton - 1], spawnButtons[0][player1FocusedButton]);
                }
            }
            else if (keyboard.sKey.wasPressedThisFrame)
            {
                var player1FocusedButton = FindFocusedButtonIndex(spawnButtons[0]);
                if (player1FocusedButton != LaneCount - 1)
                {
                    SetButtonFocus(spawnButtons[0][player1FocusedButton + 1], spawnButtons[0][player1FocusedButton]);
                }
            }
            else if (keyboard.iKey.wasPressedThisFrame)
            {
                var player2FocusedButton = FindFocusedButtonIndex(spawnButtons[1]);
                if (player2FocusedButton != 0)
                {
                    SetButtonFocus(spawnButtons[1][player2FocusedButton - 1], spawnButtons[1][player2FocusedButton]);
                }
            }
            else if (keyboard.kKey.wasPressedThisFrame)
            {
                var player2FocusedButton = FindFocusedButtonIndex(spawnButtons[1]);
                if (player2FocusedButton != LaneCount - 1)
                {
                    SetButtonFocus(spawnButtons[1][player2FocusedButton + 1], spawnButtons[1][player2FocusedButton]);
                }
            }
            else if (keyboard.spaceKey.wasPressedThisFrame)
            {
                ClickFocusedButton(spawnButtons[0]);
            }
            else if (keyboard.enterKey.wasPressedThisFrame)
            {
                ClickFocusedButton(spawnButtons[1]);
            }
        }

        private static void SetButtonFocus(Button newFocusButton, Button oldFocusButton)
        {
            oldFocusButton.RemoveFromClassList("focused");
            newFocusButton.AddToClassList("focused");
            newFocusButton.Focus();
        }

        private static void ClickFocusedButton(params Button[] buttons)
        {
            var button = buttons[FindFocusedButtonIndex(buttons)];
            using var e = new NavigationSubmitEvent();
            e.target = button;
            button.SendEvent(e);
            // foreach (var button in buttons)
            // {
            //     if (!button.ClassListContains("focused")) continue;
            //     using var e = new NavigationSubmitEvent();
            //     e.target = button;
            //     button.SendEvent(e);
            // }
        }

        private static int FindFocusedButtonIndex(params Button[] buttons)
        {
            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (!button.ClassListContains("focused")) continue;
                return i;
            }

            return -1;
        }
    }
}