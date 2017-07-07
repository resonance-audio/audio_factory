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
using UnityEngine;

public static class ArrayUtil {
    public static class TypedStatics<T> {
        public static T[] Empty = new T[0];
    }

    internal static class _InternalTypedStatics<T> {
        private static HashSet<T> _globalHashSet = new HashSet<T>();
        internal static HashSet<T> GlobalHashSet { get { return _globalHashSet; } }
    }

    public static T[] Empty<T>() {
        return TypedStatics<T>.Empty;
    }

    //Return whether the superSet intersects with any of the given subSets
    public static bool HashContainsAny<T>(this IEnumerable<T> superSet, params IEnumerable<T>[] subSets) {
        if(superSet == null || subSets == null) return false;

        HashSet<T> table = _InternalTypedStatics<T>.GlobalHashSet;
        table.Clear();
        for(var itor = superSet.GetEnumerator(); itor.MoveNext();) {
            table.Add(itor.Current);
        }

        for(int s=0; s<subSets.Length; ++s) {
            if(subSets[s] == null) continue;
            for(var itor = subSets[s].GetEnumerator(); itor.MoveNext();) {
                if(table.Contains(itor.Current)) return true;
            }
        }

        return false;
    }

    //Return the object which corresponds to the lowest Vector3.Distance value to the given point
    public static T Closest<T>(this IEnumerable<T> objects, Vector3 point, Func<T,Vector3> selector, Func<T,bool> predicate = null) {
        float? min = null;
        T ret = default(T);

        for(var itor = objects.GetEnumerator(); itor.MoveNext();) {
            if(predicate != null && !predicate(itor.Current)) continue;

            float dist = Vector3.Distance(selector.Invoke(itor.Current), point);
            if(!min.HasValue || dist < min.Value) {
                ret = itor.Current;
                min = dist;
            }
        }

        return ret;
    }
    
    //Fisher-Yates Shuffle
    public static void Shuffle<T>(this IList<T> list) {
        int i = list.Count;
        T temp;
        while (i > 1) {
            int rand = UnityEngine.Random.Range(0, i);
            --i;
            temp = list[rand];
            list[rand] = list[i];
            list[i] = temp;
        }
    }
}