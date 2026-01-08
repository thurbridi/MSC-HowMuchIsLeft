using UnityEngine;

namespace HowMuchIsLeft
{
    internal static class ItemContentDescription
    {
        static GameObject contentDescription;
        static TextMesh foregroundText;
        static TextMesh shadowText;


        static ItemContentDescription()
        {
            CreateContentDescription();
        }

        static void CreateContentDescription()
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

        public static void SetText(string text)
        {
            foregroundText.text = text;
            shadowText.text = text;
        }

        public static void ClearText()
        {
            foregroundText.text = "";
            shadowText.text = "";
        }
    }
}
