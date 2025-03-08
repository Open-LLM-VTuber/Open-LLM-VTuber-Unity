using Live2D.Cubism.Framework.Json;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Live2D.Cubism.Framework.Expression;

namespace Live2D 
{
    public class DynamicExpressionSetup : MonoBehaviour
    {
        [SerializeField] private string modelJsonPath;

        private CubismExpressionController _cubismExpressionController;

        public void Initialize(string jsonPath, CubismExpressionController cubismExpressionController)
        {
            modelJsonPath = jsonPath;
            _cubismExpressionController = cubismExpressionController;
            StartSetup(); // 调用初始化逻辑
        }

        void StartSetup()
        {
            // 加载 model3.json 并解析表情
            LoadExpressions();
        }

        public void SetExpression(int index) 
        {
            _cubismExpressionController.CurrentExpressionIndex = index;
        }

        private void LoadExpressions()
        {
            var modelJson = CubismModel3Json.LoadAtPath(modelJsonPath, FileManager.LoadAssetAtPath);
            var expressions = modelJson.FileReferences.Expressions;

            if (expressions != null)
            {
                var tempExpressionList = new List<CubismExpressionData> ();

                string modelJsonDir = Path.GetDirectoryName(modelJsonPath);

                for (int i = 0; i < expressions.Length; i++)
                {
                    var exp3JsonString = File.ReadAllText(Path.Combine(modelJsonDir, expressions[i].File));
                    CubismExp3Json cubismExp3Json = CubismExp3Json.LoadFrom(exp3JsonString);
                    var exp3Instance = CubismExpressionData.CreateInstance(cubismExp3Json);
                    tempExpressionList.Add(exp3Instance);   
                }

                _cubismExpressionController.ExpressionsList = ScriptableObject.CreateInstance<CubismExpressionList>();
                _cubismExpressionController.ExpressionsList.CubismExpressionObjects = tempExpressionList.ToArray();
            }
            else
            {
                Debug.LogWarning("Expressions data in model3.json is invalid or missing.");
            }
        }
    }


}
