using UnityEngine;

public static class DisplayInfobox
{
    public static Rect DisplayInfoBox(Vector3 position, string text)
    {
        Vector3 worldPosition = position + Vector3.up * 2f;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        if (screenPosition.z > 0)
        {
            string labelText = text;
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(labelText));
            float padding = 16f;
            float width = size.x + padding;
            float height = size.y + padding / 2;
            float x = screenPosition.x - width / 2;
            float y = Screen.height - screenPosition.y - height / 2;

            return new Rect(x, y, width, height);
        }

        return Rect.zero;
    }
}