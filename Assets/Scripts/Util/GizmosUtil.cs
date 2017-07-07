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

using UnityEngine;

//Utilities for drawing different patterns via Gizmos
public static class GizmosUtil {
    public static void DrawDottedLine(Vector3 start, Vector3 end, int segments) {
        segments = Mathf.Clamp(segments, 1, 10000);
        float step = 1f / ((float)segments);
        for(float curr = 0f; curr < 1f; curr += step) {
            Gizmos.DrawLine(Vector3.Lerp(start, end, curr),
                            Vector3.Lerp(start, end, curr + (step/2f)));
        }
    }
    public static void DrawDottedLine(Vector3 start, Vector3 end, float segmentLength, int minSegmentCount, int maxSegmentCount) {
        DrawDottedLine(start, end, Mathf.Clamp((int)Mathf.Ceil((end-start).magnitude / segmentLength), minSegmentCount, maxSegmentCount));
    }

    public static void DrawCircle(Vector3 center, Quaternion rotation, float radius, int segments) {
        for(int i=0; i<segments+1; ++i) {
            float percent = (float)i / (float)segments;
            float nextPercent = (float)(i+1) / (float)segments;
            Gizmos.DrawLine(
                center + rotation * new Vector3(Mathf.Cos(percent * 2f * Mathf.PI) * radius,
                                                Mathf.Sin(percent * 2f * Mathf.PI) * radius,
                                                0f),
                center + rotation * new Vector3(Mathf.Cos(nextPercent * 2f * Mathf.PI) * radius,
                                                Mathf.Sin(nextPercent * 2f * Mathf.PI) * radius,
                                                0f));
                
        }
    }
}