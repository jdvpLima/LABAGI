using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupSwitcher : MonoBehaviour
{
    [Tooltip("Parent GameObjects for each group (each contains the 7 TMP texts). Order matters.")]
    public List<GameObject> groups = new List<GameObject>();

    [Tooltip("Buttons that show the corresponding group. Order must match 'groups'.")]
    public List<Button> buttons = new List<Button>();

    [Tooltip("Index of the group to show on Start (-1 = show none).")]
    public int startIndex = 0;

    void Awake()
    {
        // Basic validation / auto-correction
        if (groups == null) groups = new List<GameObject>();
        if (buttons == null) buttons = new List<Button>();

        // If user forgot to assign buttons but the buttons are children of this object, auto-find them.
        if (buttons.Count == 0)
        {
            var found = GetComponentsInChildren<Button>(true);
            foreach (var b in found) buttons.Add(b);
        }

        // Attach listeners. Use min length to avoid out-of-range.
        int pairCount = Mathf.Min(groups.Count, buttons.Count);
        for (int i = 0; i < pairCount; i++)
        {
            int idx = i; // capture local copy for closure
            if (buttons[i] != null)
                buttons[i].onClick.AddListener(() => ShowGroup(idx));
        }
    }

    void Start()
    {
        if (startIndex >= 0 && startIndex < groups.Count)
            ShowGroup(startIndex);
        else if (startIndex == -1)
            ShowNone();
        else if (groups.Count > 0)
            ShowGroup(0); // fallback
    }

    /// <summary>Show only the group at index and hide every other group.</summary>
    public void ShowGroup(int index)
    {
        if (groups == null || groups.Count == 0) return;

        for (int i = 0; i < groups.Count; i++)
        {
            var g = groups[i];
            if (g == null) continue;
            g.SetActive(i == index);
        }
    }

    /// <summary>Hide all groups.</summary>
    public void ShowNone()
    {
        if (groups == null) return;
        foreach (var g in groups) if (g != null) g.SetActive(false);
    }
}
