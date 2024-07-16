using System.Globalization;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class InGameUIManager : MonoBehaviour
    {
        private EntityManager entityManager;
        private NativeArray<Entity> entityArray;
        private PlayerData playerData1;
        private PlayerData playerData2;
        
        private VisualElement root;
        
        private VisualElement player1;
        private VisualElement player2;

        private Label[] player1Units;
        private Label player1Lives;
        private Label[] player2Units;
        private Label player2Lives;
        

        private void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            root = GetComponent<UIDocument>().rootVisualElement;

            player1 = root.Q<VisualElement>("Player1");
            player1Units = new Label[5];
            player1Units[0] = player1.Q<Label>("Unit1");
            player1Units[1] = player1.Q<Label>("Unit2");
            player1Units[2] = player1.Q<Label>("Unit3");
            player1Units[3] = player1.Q<Label>("Unit4");
            player1Units[4] = player1.Q<Label>("Unit5");
            player1Lives = player1.Q<Label>("Life");
            
            player2 = root.Q<VisualElement>("Player2");
            player2Units = new Label[5];
            player2Units[0] = player2.Q<Label>("Unit1");
            player2Units[1] = player2.Q<Label>("Unit2");
            player2Units[2] = player2.Q<Label>("Unit3");
            player2Units[3] = player2.Q<Label>("Unit4");
            player2Units[4] = player2.Q<Label>("Unit5");
            player2Lives = player2.Q<Label>("Life");
            
            var query = new EntityQueryBuilder(Allocator.Temp).WithAll<PlayerData>().Build(entityManager);
            entityArray = query.ToEntityArray(Allocator.Persistent);
            foreach (var entity in entityArray)
            {
                if (entityManager.GetComponentData<PlayerData>(entity).Tag == PlayerTag.Player1)
                    playerData1 = entityManager.GetComponentData<PlayerData>(entity);
                if (entityManager.GetComponentData<PlayerData>(entity).Tag == PlayerTag.Player2)
                    playerData2 = entityManager.GetComponentData<PlayerData>(entity);
            }
            query.Dispose();
        }

        private void OnDestroy()
        {
            entityArray.Dispose();
        }

        private void Update()
        {
            ResetAllUnitsLabel();
            RefreshPlayerData();
            UpdateUnitsLabel();
            UpdateLives();
        }

        private void UpdateLives()
        {
            player1Lives.text = playerData1.Lives.ToString(CultureInfo.InvariantCulture);
            player2Lives.text = playerData2.Lives.ToString(CultureInfo.InvariantCulture);
        }

        private void UpdateUnitsLabel()
        {
            for (var idx = 0; idx < playerData1.Queue.Length; idx++)
            {
                var unitType = playerData1.Queue[idx];
                player1Units[idx].text = unitType switch
                {
                    0 => "Infantry",
                    1 => "Heavy",
                    2 => "Range",
                    _ => ""
                };
            }

            for (var idx = 0; idx < playerData2.Queue.Length; idx++)
            {
                var unitType = playerData2.Queue[idx];
                player2Units[idx].text = unitType switch
                {
                    0 => "Infantry",
                    1 => "Heavy",
                    2 => "Range",
                    _ => ""
                };
            }
        }

        private void RefreshPlayerData()
        {
            foreach (var entity in entityArray)
            {
                if (entityManager.GetComponentData<PlayerData>(entity).Tag == PlayerTag.Player1)
                    playerData1 = entityManager.GetComponentData<PlayerData>(entity);
                if (entityManager.GetComponentData<PlayerData>(entity).Tag == PlayerTag.Player2)
                    playerData2 = entityManager.GetComponentData<PlayerData>(entity);
            }
        }

        private void ResetAllUnitsLabel()
        {
            foreach (var label in player1Units)
            {
                label.text = "";
            }
            foreach (var label in player2Units)
            {
                label.text = "";
            }
        }
    }
}
