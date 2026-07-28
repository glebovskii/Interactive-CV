using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Dissolver.Demo
{
    public class DemoController : MonoBehaviour
    {
        public Button next;
        public Button prev;
        public List<ExampleHolder> examples;
        public CameraMover _camera;

        private int currentExample = 0;

        private void Awake()
        {
            next.onClick.AddListener(NextBtnClick);
            prev.onClick.AddListener(PrevBtnClick);
        }

        private void NextBtnClick()
        {
            currentExample++;
            if (currentExample >= examples.Count)
                currentExample = 0;

            _camera.MoveTo(examples[currentExample]);
        }

        private void PrevBtnClick()
        {
            currentExample--;
            if (currentExample < 0)
                currentExample = examples.Count - 1;

            _camera.MoveTo(examples[currentExample]);
        }
    }
}