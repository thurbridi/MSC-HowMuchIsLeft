using UnityEngine;

namespace HowMuchIsLeft
{
    internal class ItemContentDescription
    {
        private static ItemContentDescription _instance;
        private GameObject contentDescription;
        private TextMesh foregroundText;
        private TextMesh shadowText;

        internal static ItemContentDescription Instance()
        {
            _instance = _instance ?? new ItemContentDescription();
            return _instance;
        }

        internal void SetText(string text)
        {
            foregroundText.text = text;
            shadowText.text = text;
        }

        internal void ClearText()
        {
            foregroundText.text = "";
            shadowText.text = "";
        }

        private void CreateContentDescription()
        {
            GameObject partName = GameObject.Find("GUI/Indicators/Partname");

            contentDescription = GameObject.Instantiate(partName);

            GameObject.Destroy(contentDescription.GetComponent<PlayMakerFSM>());

            contentDescription.name = "ContentDescription";
            contentDescription.transform.parent = partName.transform.parent;
            contentDescription.transform.localPosition = new Vector3(0.0f, -0.21f, 0.0f);

            foregroundText = contentDescription.GetComponent<TextMesh>();
            shadowText = contentDescription.transform.GetChild(0).GetComponent<TextMesh>();

            foregroundText.characterSize = 0.05f;
            shadowText.characterSize = 0.05f;
        }

        private ItemContentDescription()
        {
            CreateContentDescription();
        }
    }
}
