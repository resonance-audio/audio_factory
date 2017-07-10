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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//A simple Catmull-Rom spline implementation, ordered by sibling index
[ExecuteInEditMode]
public class SplinePath : BaseSplinePath {

//-----------------------------------------------------------------------------------------
#region SERIALIZED_FIELDS
    //How many steps are involved in the point-to-point interpolation
    [SerializeField] int splineDetail = 2;
    //The color the spline appears in the editor
    [SerializeField] public Color editorColor = Color.white;
    //Whether the path is looping or broken
    [SerializeField] bool loop = true;
    //How to update the positions of the path at runtime
    [SerializeField] public Mode mode = Mode.Dynamic;
    //The total length of the path
    [SerializeField, ReadOnly] float totalLength = 0f;
    //Optional renderer for the path
    [SerializeField] LineRenderer lineRenderer = null;
#endregion SERIALIZED_FIELDS

//-----------------------------------------------------------------------------------------
#region NON_SERIALIZED_FIELDS
    //positions of the SplinePoints
    [NonSerialized] List<Vector3> points = null;
    //list of the actual SplinePoints
    [NonSerialized] List<SplinePoint> childrenCache = null;
    //list of the vertices
    [NonSerialized] List<Vector3> vertexCacheList = null;
    //array of the vertices; should be identical to vertexCacheList;
    //I'm implementing both to avoid certain runtime allocations
    [NonSerialized] Vector3[] vertexCacheArray = null;
    [NonSerialized] Vector3 _lastGeneratedPosition = Vector3.zero;
    [NonSerialized] Vector3 _lastGeneratedEulers = Vector3.zero;
    [NonSerialized] Vector3 _lastGeneratedScale = Vector3.zero;
#endregion NON_SERIALIZED_FIELDS

//-----------------------------------------------------------------------------------------
#region INNER_DEFINITIONS
    public enum Mode {
        //No generation is done at runtime
        Static = 0,
        //Spline points are re-generated every frame
        Dynamic = 1,
    }
#endregion INNER_DEFINITIONS

#region METHODS

    //The total length between the vertices that get generated.
    //It is IMPORTANT TO NOTE that since the 'splineDetail' field affects the vertices which are generated, this property is also affected by it.
    public override float TotalLength {
        get {
            return totalLength;
        }
    }
    
    //Evaluate the position of this path at a certain 'time', IE: a value between 0 and 1
    // A value of 0 should always correspond to the first point
    // If the path is looping, a value of 1 should wrap to the first point
    // If the path is NOT looping, a value of 1 should clamp to the last point
    public override Vector3 Evaluate(float time) {
        //we're overriding from BaseSplinePath.cs, which is also implemented by Rope.cs
        if(time == 1f) return vertexCacheList[vertexCacheList.Count-1];
        time %= 1f;
        if(time < 0f) time = 1f + time;
        if(time == 0f) return vertexCacheList[0];

        float requestedLength = totalLength * time;
        float currLen = 0f;
        for(int v=0; v<vertexCacheList.Count-1; ++v) {
            float len = (vertexCacheList[v+1] - vertexCacheList[v]).magnitude;
            if((currLen + len) > requestedLength) {
                float currTime = currLen / requestedLength;
                float lengthNeeded = requestedLength * (1f - currTime);
                float fractionOfNextSegment = lengthNeeded / (vertexCacheList[v+1] - vertexCacheList[v]).magnitude;
                return Vector3.Lerp(vertexCacheList[v], vertexCacheList[v+1], fractionOfNextSegment);
            }else{
                currLen += len;
                continue;
            }
        }

        return vertexCacheList[vertexCacheList.Count-1];
    }

    //Does all of the vertex-generating. Called on each update if the mode is set to Dynamic, otherwise, only gets called when the SplinePath itself is moved.
    public void Generate() {

        _lastGeneratedPosition = transform.position;
        _lastGeneratedEulers = transform.eulerAngles;
        _lastGeneratedScale = transform.lossyScale;
        points = points ?? new List<Vector3>();
        points.Clear();

        //if we're in editor or have never cached the child points, do so now
        if(childrenCache == null || !Application.isPlaying) {
            childrenCache = new List<SplinePoint>(GetComponentsInChildren<SplinePoint>());
        }

        //if we're in editor, do all of the point cleanup/renaming
        #if UNITY_EDITOR
        _ReorderChildren();
        #endif

        for(int c=0; c<childrenCache.Count; ++c) {
            if(childrenCache[c] != null) {
                points.Add(childrenCache[c].transform.position);
            }
        }

        //not really a cache; just to prevent allocation
        vertexCacheList = vertexCacheList ?? new List<Vector3>();
        vertexCacheList.Clear();

        AddSpline(vertexCacheList, loop ? points[points.Count-1] : points[0], points[0], points[1], points[2]);
        for(int i=0; i<points.Count-3; ++i) {
            AddSpline(vertexCacheList, points[i], points[i+1], points[i+2], points[i+3]);
            if(i == points.Count-4) {
                if(loop) {
                    AddSpline(vertexCacheList, points[i+1] ,points[i+2], points[i+3], points[0]);
                    AddSpline(vertexCacheList, points[i+2], points[i+3], points[0], points[1]);
                    vertexCacheList.Add(points[0]);
                }else{
                    AddSpline(vertexCacheList, points[i+1] ,points[i+2], points[i+3], points[i+3]);
                    vertexCacheList.Add(points[points.Count-1]);
                }
            }
        }

        totalLength = 0f;
        for(int v=0; v<vertexCacheList.Count-1; ++v) {
            totalLength += (vertexCacheList[v+1] - vertexCacheList[v]).magnitude;
        }
        if(loop) {
            totalLength += (vertexCacheList[0] - vertexCacheList[vertexCacheList.Count-1]).magnitude;
        }
    }

    void Update() {
        if(!Application.isPlaying 
        || mode == Mode.Dynamic 
        || transform.position != _lastGeneratedPosition
        || transform.eulerAngles != _lastGeneratedEulers
        || transform.lossyScale != _lastGeneratedScale) {
            //we're comparing global transform values here, which can be lossy, BUT they should be consistent
            //Worst-case, we call Generate() some extra times when we don't need to due to precision loss.
            Generate();
        }

        if(lineRenderer != null) {
            if(vertexCacheList != null && vertexCacheList.Count > 0) {
                
                //copy the contents of the vertexCache list to the array
                if(vertexCacheArray == null || vertexCacheArray.Length != vertexCacheList.Count) {
                    vertexCacheArray = new Vector3[vertexCacheList.Count];
                }
                for(int v=0; v<vertexCacheList.Count; ++v) vertexCacheArray[v] = vertexCacheList[v];

                lineRenderer.positionCount = vertexCacheArray.Length;
                lineRenderer.SetPositions(vertexCacheArray);
            }else{
                lineRenderer.SetPositions(ArrayUtil.Empty<Vector3>());
            }
        }
    }

	//Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
	//http://www.iquilezles.org/www/articles/minispline/minispline.htm
	Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		//The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
		Vector3 a = 2f * p1;
		Vector3 b = p2 - p0;
		Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

		//The cubic polynomial: a + b * t + c * t^2 + d * t^3
		Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

		return pos;
	}

    bool CompareEquals(IEnumerable<Transform> a, IEnumerable<Transform> b) {
        if((a == null) != (b == null)) return false;

        var aItor = a.GetEnumerator();
        var bItor = b.GetEnumerator();

        for(;;) {
            bool aDone = !aItor.MoveNext();
            bool bDone = !bItor.MoveNext();
            
            if(aDone != bDone) return false; //one enumerable is shorter than the other
            if(aDone) return true; //both iterators completed
            if(aItor.Current != bItor.Current) return false; //one of the elements does not line up
        }
    }

    void AddSpline(List<Vector3> buffer, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4) {
        for(int s=0; s<splineDetail; ++s) {
            float splineFrac = (float)s / (float)splineDetail;
            buffer.Add(GetCatmullRomPosition(splineFrac, p1, p2, p3, p4));
        }
    }
#endregion METHODS

//-----------------------------------------------------------------------------------------
#region EDITOR_METHODS
#if UNITY_EDITOR

    void OnValidate() {
        points = points ?? new List<Vector3>();
        Generate();
    }

    //Attempts to re-name all contained SplinePoints to their respective sibling indices.
    //Also attempts to detect when we've recently cloned a SplinePoint, and it needs to be moved in the hierarchy.
    void _ReorderChildren() {
        if(!Application.isPlaying) {

            //enforce that we have at least 4 children
            while(childrenCache.Count < 4) {
                //this will get re-named afterwards anyways
                GameObject newPoint = new GameObject("newPoint");
                newPoint.transform.SetParent(transform);
                newPoint.transform.localPosition = Vector3.zero;
                childrenCache.Add(newPoint.AddComponent<SplinePoint>());
            }

            //if we clone a point in the middle of the children, we want to move it next to the original point in the hierarchy, NOT the at end of the siblings, which is the default Unity behavior
            bool changedHierarchy = false;
            for(int c=0; c<childrenCache.Count; ++c) {
                //Unity follows a naming convention of "ObjectName (x)"; we don't care about 'x', just whether it is a clone or not
                if(childrenCache[c].name.Contains("(") && childrenCache[c].name.Contains(")")) {

                    //we've determined that this point is a clone, so use the closest point to this one as the neighbor
                    SplinePoint closest = childrenCache.Closest(
                        childrenCache[c].transform.position, 
                        child => child.transform.position, 
                        child => child != childrenCache[c]);

                    //move the clone next to the closest child in the hierarchy
                    if(closest != null) {
                        childrenCache[c].transform.SetSiblingIndex(closest.transform.GetSiblingIndex() + 1);
                        changedHierarchy = true;
                    }
                }
            }

            //if we moved a child around in the hierarchy, we need to re-call GetComponentsInChildren so that we get the new appropriate order of children
            if(changedHierarchy) {
                childrenCache.Clear();
                GetComponentsInChildren<SplinePoint>(childrenCache);
            }

            //finally name each child according to its order in the hierarchy
            for(int c=0; c<childrenCache.Count; ++c) {
                childrenCache[c].name = "p_" + c.ToString();
            }
        }
    }

    void OnDrawGizmos() {
        //if we have a line-renderer, we probably don't need gizmos
        if(lineRenderer == null) {
            Color col = Gizmos.color;

            GameObject[] objs = UnityEditor.Selection.gameObjects;
            var points = gameObject.GetComponentsInChildren<SplinePoint>();
            GameObject[] children = new GameObject[points.Length];
            for(int p=0; p<points.Length; ++p) children[p] = points[p].gameObject;

            if(objs.Contains(this.gameObject) || objs.HashContainsAny(children)) {
                Gizmos.color = editorColor.WithAlpha(1f);
            }else{
                Gizmos.color = editorColor.WithAlpha(0.25f);
            }

            for(int i=0; i< vertexCacheList.Count-1; ++i) {
                Gizmos.DrawLine(vertexCacheList[i], vertexCacheList[i+1]);
            }

            Gizmos.color = col;
        }
    }
#endif
#endregion EDITOR_METHODS
}