using System.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class StartMenuUIManager : MonoBehaviour
    {
        [SerializeField] private UIDocument UIDocument;
        private VisualElement root;
        private Label gameEndLabel;
        private Button startButton;
        private Button settingButton;
        private Button quitButton;
        private GameStateManagerSystem gameStateManagerSystem;
        
        private void Start()
        {
            root = GetComponent<UIDocument>().rootVisualElement;
            gameEndLabel = root.Q<Label>("GameEnd");
            startButton = root.Q<Button>("Start");
            settingButton = root.Q<Button>("Setting");
            quitButton = root.Q<Button>("Bye");
            startButton.RegisterCallback<PointerUpEvent>(_ => StartGame());
            quitButton.RegisterCallback<PointerUpEvent>(_ => QuitGame());
            gameEndLabel.text = "";

            gameStateManagerSystem =
                World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<GameStateManagerSystem>();
            gameStateManagerSystem.GameEnd += OnGameEnd;
        }

        private void OnDisable()
        {
            if (gameStateManagerSystem != null)
            {
                gameStateManagerSystem.GameEnd -= OnGameEnd;
            }
        }

        private void OnGameEnd(object sender, string gameEndString)
        {
            root.style.display = DisplayStyle.Flex;
            Debug.Log(gameEndString);
            gameEndLabel.text = gameEndString;
            StartCoroutine(nameof(ResetGameEndLabel));
        }

        private IEnumerator ResetGameEndLabel()
        {
            yield return new WaitForSeconds(3f);
            gameEndLabel.text = "";
            
        }

        private void StartGame()
        {
            gameStateManagerSystem.StartGame();
            root.style.display = DisplayStyle.None;
        }

        private static void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}