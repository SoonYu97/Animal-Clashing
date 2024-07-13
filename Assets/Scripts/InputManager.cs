using System;
using System.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class InputManager : MonoBehaviour
    {
        private VisualElement root;
        private VisualElement lane1;
        private VisualElement lane2;
        private VisualElement lane3;
        private Button lane1Player1SpawnButton;
        private Button lane2Player1SpawnButton;
        private Button lane3Player1SpawnButton;
        private Button lane1Player2SpawnButton;
        private Button lane2Player2SpawnButton;
        private Button lane3Player2SpawnButton;
        private Action lane1Player1SpawnButtonClicked;
        private Action lane2Player1SpawnButtonClicked;
        private Action lane3Player1SpawnButtonClicked;
        private Action lane1Player2SpawnButtonClicked;
        private Action lane2Player2SpawnButtonClicked;
        private Action lane3Player2SpawnButtonClicked;
        
        private LaneSystem laneSystem;

        private void Start()
        {
            laneSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<LaneSystem>();
            
            root = GetComponent<UIDocument>().rootVisualElement;

            lane1 = root.Q<VisualElement>("Lane1");
            lane2 = root.Q<VisualElement>("Lane2");
            lane3 = root.Q<VisualElement>("Lane3");
            
            lane1Player1SpawnButton = lane1.Q<Button>("Player1SpawnButton");
            lane2Player1SpawnButton = lane2.Q<Button>("Player1SpawnButton");
            lane3Player1SpawnButton = lane3.Q<Button>("Player1SpawnButton");
            lane1Player2SpawnButton = lane1.Q<Button>("Player2SpawnButton");
            lane2Player2SpawnButton = lane2.Q<Button>("Player2SpawnButton");
            lane3Player2SpawnButton = lane3.Q<Button>("Player2SpawnButton");

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP_8_1
#else
            lane2Player1SpawnButton.AddToClassList("focused");
            lane2Player2SpawnButton.AddToClassList("focused");
#endif

            lane1Player1SpawnButtonClicked = () => laneSystem.SpawnUnitFor(PlayerTag.Player1, 1);
            lane2Player1SpawnButtonClicked = () => laneSystem.SpawnUnitFor(PlayerTag.Player1, 2);
            lane3Player1SpawnButtonClicked = () => laneSystem.SpawnUnitFor(PlayerTag.Player1, 3);
            lane1Player2SpawnButtonClicked = () => laneSystem.SpawnUnitFor(PlayerTag.Player2, 1);
            lane2Player2SpawnButtonClicked = () => laneSystem.SpawnUnitFor(PlayerTag.Player2, 2);
            lane3Player2SpawnButtonClicked = () => laneSystem.SpawnUnitFor(PlayerTag.Player2, 3);
            
            lane1Player1SpawnButton.clicked += lane1Player1SpawnButtonClicked;
            lane2Player1SpawnButton.clicked += lane2Player1SpawnButtonClicked;
            lane3Player1SpawnButton.clicked += lane3Player1SpawnButtonClicked;
            lane1Player2SpawnButton.clicked += lane1Player2SpawnButtonClicked;
            lane2Player2SpawnButton.clicked += lane2Player2SpawnButtonClicked;
            lane3Player2SpawnButton.clicked += lane3Player2SpawnButtonClicked;
        }

        private void OnDestroy()
        {
            lane1Player1SpawnButton.clicked -= lane1Player1SpawnButtonClicked;
            lane2Player1SpawnButton.clicked -= lane2Player1SpawnButtonClicked;
            lane3Player1SpawnButton.clicked -= lane3Player1SpawnButtonClicked;
            lane1Player2SpawnButton.clicked -= lane1Player2SpawnButtonClicked;
            lane2Player2SpawnButton.clicked -= lane2Player2SpawnButtonClicked;
            lane3Player2SpawnButton.clicked -= lane3Player2SpawnButtonClicked;
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
                if (lane2Player1SpawnButton.focusable && lane3Player1SpawnButton.ClassListContains("focused"))
                {
                    SetButtonFocus(lane2Player1SpawnButton, lane3Player1SpawnButton);
                }
                else if (lane1Player1SpawnButton.focusable && lane2Player1SpawnButton.ClassListContains("focused"))
                {
                    SetButtonFocus(lane1Player1SpawnButton, lane2Player1SpawnButton);
                }
            }
            else if (keyboard.sKey.wasPressedThisFrame)
            {
                if (lane3Player1SpawnButton.focusable && lane2Player1SpawnButton.ClassListContains("focused"))
                {
                    SetButtonFocus(lane3Player1SpawnButton, lane2Player1SpawnButton);
                }
                else if (lane2Player1SpawnButton.focusable && lane1Player1SpawnButton.ClassListContains("focused"))
                {
                    SetButtonFocus(lane2Player1SpawnButton, lane1Player1SpawnButton);
                }
            }
            else if (keyboard.iKey.wasPressedThisFrame)
            {
                if (lane2Player2SpawnButton.focusable && lane3Player2SpawnButton.ClassListContains("focused"))
                {
                    SetButtonFocus(lane2Player2SpawnButton, lane3Player2SpawnButton);
                }
                else if (lane1Player2SpawnButton.focusable && lane2Player2SpawnButton.ClassListContains("focused"))
                {
                    SetButtonFocus(lane1Player2SpawnButton, lane2Player2SpawnButton);
                }
            }
            else if (keyboard.kKey.wasPressedThisFrame)
            {
                if (lane3Player2SpawnButton.focusable && lane2Player2SpawnButton.ClassListContains("focused"))
                {
                    SetButtonFocus(lane3Player2SpawnButton, lane2Player2SpawnButton);
                }
                else if (lane2Player2SpawnButton.focusable && lane1Player2SpawnButton.ClassListContains("focused"))
                {
                    SetButtonFocus(lane2Player2SpawnButton, lane1Player2SpawnButton);
                }
            }
            else if (keyboard.spaceKey.wasPressedThisFrame)
            {
                ClickFocusedButton(lane1Player1SpawnButton, lane2Player1SpawnButton, lane3Player1SpawnButton);
            }
            else if (keyboard.enterKey.wasPressedThisFrame)
            {
                ClickFocusedButton(lane1Player2SpawnButton, lane2Player2SpawnButton, lane3Player2SpawnButton);
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
            foreach (var button in buttons)
            {
                if (!button.ClassListContains("focused")) continue;
                using var e = new NavigationSubmitEvent();
                e.target = button;
                button.SendEvent(e);
            }
        }
    }
}