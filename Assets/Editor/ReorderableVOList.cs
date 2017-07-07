/* Copyright 2017 Google Inc. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

﻿using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(ElevatorVOSequencer))]
public class ReorderableVOList : Editor {
    
    private ReorderableList reorderableList;

    private ElevatorVOSequencer ElevatorVOSequencer
    {
        get
        {
            return target as ElevatorVOSequencer;
        }
    }

    private void OnEnable() {
        reorderableList = new ReorderableList(ElevatorVOSequencer.VOEvents, typeof(ElevatorVOSequencer.VOEvent), true, true, true, true);
        if (ElevatorVOSequencer.VOEvents.Count < 1) {
            AddItem(reorderableList);
        }
        // This could be used aswell, but I only advise this your class inherrits from UnityEngine.Object or has a CustomPropertyDrawer
        // Since you'll find your item using: serializedObject.FindProperty("list").GetArrayElementAtIndex(index).objectReferenceValue
        // which is a UnityEngine.Object
        // reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("list"), true, true, true, true);

        // Add listeners to draw events
        reorderableList.drawHeaderCallback += DrawHeader;
        reorderableList.drawElementCallback += DrawElement;

        reorderableList.onAddCallback += AddItem;
        reorderableList.onRemoveCallback += RemoveItem;
    }

    private void OnDisable() {
        // Make sure we don't get memory leaks etc.
        reorderableList.drawHeaderCallback -= DrawHeader;
        reorderableList.drawElementCallback -= DrawElement;

        reorderableList.onAddCallback -= AddItem;
        reorderableList.onRemoveCallback -= RemoveItem;
    }


    private void DrawHeader(Rect rect) {
        GUI.Label(rect, "Voiceover Events");
    }


    private void DrawElement(Rect rect, int index, bool active, bool focused) {
        ElevatorVOSequencer.VOEvent item = ElevatorVOSequencer.VOEvents[index];

        EditorGUI.BeginChangeCheck();

        int eventFieldWidth = Mathf.Min(300, Mathf.FloorToInt(rect.width * 0.8f));

        item.clip = 
            (AudioClip)EditorGUI.ObjectField(new Rect(rect.x, rect.y, eventFieldWidth, rect.height), item.clip, typeof(AudioClip), allowSceneObjects: true
        );

        item.delay = 
            EditorGUI.FloatField(new Rect(rect.x + eventFieldWidth, rect.y, rect.width - eventFieldWidth, rect.height), item.delay);
        if (EditorGUI.EndChangeCheck()) {
            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }

    private void AddItem(ReorderableList list) {
        ElevatorVOSequencer.VOEvents.Add(new ElevatorVOSequencer.VOEvent());

        EditorUtility.SetDirty(target);
    }

    private void RemoveItem(ReorderableList list) {
        ElevatorVOSequencer.VOEvents.RemoveAt(list.index);

        EditorUtility.SetDirty(target);
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        // Actually draw the list in the inspector
        reorderableList.DoLayoutList();
    }
}
