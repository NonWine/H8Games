using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace H8.UI.EditorTools
{
    /// <summary>
    /// Re-authors a World Space canvas in pixel units.
    ///
    /// StylizedGraphic's presets are pixel numbers (shadowOffsetY 10, bevel 6,
    /// cornerRadius 28...). A World Space canvas whose RectTransform is a couple
    /// of world units treats those as WORLD units, so a 10px shadow lands four
    /// element-widths away and the mesh padding blows up ~20x. Presets cannot be
    /// hand-tuned around it either: ApplyPreset() rewrites the depth values on
    /// every OnValidate.
    ///
    /// Fix: multiply the whole canvas subtree by <see cref="Scale"/> and divide
    /// the canvas transform by the same amount. Final world size is unchanged,
    /// but the numbers the shader sees are now pixels, exactly like a Screen
    /// Space canvas.
    /// </summary>
    static class WorldCanvasPixelUnits
    {
        // Matches Canvas.referencePixelsPerUnit, so 1 world unit == 100 px.
        const float Scale = 100f;

        // Anything at or below this is already converted; re-running would shrink it again.
        const float AlreadyConvertedScale = 0.5f;

        [MenuItem("Tools/H8 UI/Convert World Canvas To Pixel Units", true)]
        static bool Validate() => Selection.GetFiltered<Canvas>(SelectionMode.Editable).Length > 0;

        [MenuItem("Tools/H8 UI/Convert World Canvas To Pixel Units")]
        static void Convert()
        {
            Canvas[] canvases = Selection.GetFiltered<Canvas>(SelectionMode.Editable);
            int done = 0, skipped = 0;

            foreach (Canvas canvas in canvases)
            {
                if (canvas.renderMode != RenderMode.WorldSpace)
                {
                    Debug.LogWarning($"[{canvas.name}] not a World Space canvas — skipped.", canvas);
                    skipped++;
                    continue;
                }

                if (canvas.transform.localScale.x <= AlreadyConvertedScale)
                {
                    Debug.LogWarning($"[{canvas.name}] already in pixel units (scale " +
                                     $"{canvas.transform.localScale.x}) — skipped.", canvas);
                    skipped++;
                    continue;
                }

                ConvertOne(canvas);
                done++;
            }

            Debug.Log($"World canvas -> pixel units: {done} converted, {skipped} skipped.");
        }

        static void ConvertOne(Canvas canvas)
        {
            var canvasRect = (RectTransform)canvas.transform;
            Vector2 worldSizeBefore = canvasRect.rect.size * canvasRect.lossyScale.x;

            Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "World canvas to pixel units");

            // Children first: their values are read relative to the canvas, and the
            // canvas transform must stay at its old scale while they are rewritten.
            foreach (RectTransform rt in canvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt == canvasRect) continue;
                rt.sizeDelta = rt.sizeDelta * Scale;
                rt.anchoredPosition3D = rt.anchoredPosition3D * Scale;
                EditorUtility.SetDirty(rt);
            }

            // Font size is in canvas units too, so it has to ride along or the
            // text keeps its old point size inside a 100x bigger rect.
            foreach (TextMeshProUGUI text in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                text.fontSize *= Scale;
                text.fontSizeMin *= Scale;
                text.fontSizeMax *= Scale;
                EditorUtility.SetDirty(text);
            }

            // Colliders on the canvas object are in the same local space, so the
            // /Scale below would shrink the trigger out of existence.
            foreach (Collider collider in canvas.GetComponents<Collider>())
            {
                switch (collider)
                {
                    case BoxCollider box:
                        box.size *= Scale;
                        box.center *= Scale;
                        break;
                    case SphereCollider sphere:
                        sphere.radius *= Scale;
                        sphere.center *= Scale;
                        break;
                    default:
                        Debug.LogWarning($"[{canvas.name}] {collider.GetType().Name} not rescaled — " +
                                         "check it by hand.", collider);
                        break;
                }

                EditorUtility.SetDirty(collider);
            }

            canvasRect.sizeDelta = canvasRect.sizeDelta * Scale;
            canvasRect.localScale = canvasRect.localScale / Scale;
            EditorUtility.SetDirty(canvasRect);

            Vector2 worldSizeAfter = canvasRect.rect.size * canvasRect.lossyScale.x;
            if ((worldSizeAfter - worldSizeBefore).sqrMagnitude > 1e-4f)
            {
                Debug.LogWarning($"[{canvas.name}] world size drifted {worldSizeBefore} -> " +
                                 $"{worldSizeAfter}. Undo and check for a non-uniform parent scale.", canvas);
            }

            Canvas.ForceUpdateCanvases();
        }
    }
}
