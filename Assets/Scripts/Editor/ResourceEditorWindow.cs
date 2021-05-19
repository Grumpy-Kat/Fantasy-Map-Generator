using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ResourceEditorWindow : EditorWindow {
    public delegate void OnChangeDelegate(List<Resource> resources);
    private OnChangeDelegate onChangeDelegate;

    private List<Resource> resources = new List<Resource>();

    private Vector2 scrollPos;
    private bool shouldRepaint;

    private void OnGUI() {
        Draw();
        if (shouldRepaint) {
            shouldRepaint = false;
            onChangeDelegate(resources);
            Repaint();
        }
    }

    private void Draw() {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayoutOption[] options = new GUILayoutOption[] {
            GUILayout.Width(233),
        };

        for (int i = 0; i < resources.Count; i++) {
            // Add fields for all the possible values
            EditorGUI.BeginChangeCheck();
            Resource resource = resources[i];
            resource.name = EditorGUILayout.TextField(resource.name, options);
            resource.threshold = Mathf.Clamp(EditorGUILayout.FloatField(resource.threshold, options), 0, 1);
            resource.minLerp = Mathf.Clamp(EditorGUILayout.FloatField(resource.minLerp, options), 0, 1);
            resource.maxLerp = Mathf.Clamp(EditorGUILayout.FloatField(resource.maxLerp, options), 0, 1);
            resource.octaves = Mathf.Clamp(EditorGUILayout.IntField(resource.octaves, options), 0, int.MaxValue);
            resource.persistence = EditorGUILayout.FloatField(resource.persistence, options);
            resource.lacunarity = Mathf.Clamp(EditorGUILayout.FloatField(resource.lacunarity, options), 0, float.MaxValue);
            resource.color = EditorGUILayout.ColorField(resource.color, options);
            resource.renderer = (Renderer)EditorGUILayout.ObjectField(resource.renderer, typeof(Renderer), true, options);
            if (EditorGUI.EndChangeCheck()) {
                onChangeDelegate(resources);
            }

            // Create delete resource button
            if (GUILayout.Button("Delete", options)) {
                RemoveResource(i);
            }

            // Spacing
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
        options = new GUILayoutOption[] {
            GUILayout.Width(245),
        };

        // Create new resource button
        if (GUILayout.Button("Add Resource", options)) {
            AddResource();
        }
    }

    private void AddResource() {
        resources.Add(new Resource("New Resource", 0.5f, 0.3f, 0.7f, 1, 0.5f, 1f, Color.black, null));
        shouldRepaint = true;
    }

    private void RemoveResource(int i) {
        if (resources.Count > 1) {
            resources.RemoveAt(i);
            shouldRepaint = true;
        }
    }

    public void SetResources(List<Resource> resources) {
        this.resources = resources;
    }

    public void OnChange(OnChangeDelegate onChangeDelegate) {
        this.onChangeDelegate = onChangeDelegate;
    }

    private void OnEnable() {
        titleContent.text = "Resource Editor";
        position.Set(position.x, position.y, 255, 450);
        minSize = new Vector2(255, 450);
        maxSize = new Vector2(255, 450);
    }

    private void OnDisable() {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}

