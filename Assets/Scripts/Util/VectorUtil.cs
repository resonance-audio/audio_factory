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

//Extension methods for Unity Vectors
public static class VectorUtil {

    //Return the vector with a modified X-component
    public static Vector3 WithX(this Vector3 vec, float x) {
        return new Vector3(x, vec.y, vec.z);
    }

    //Return the vector with a modified Y-component
    public static Vector3 WithY(this Vector3 vec, float y) {
        return new Vector3(vec.x, y, vec.z);
    }

    //Return the vector with a modified Z-component
    public static Vector3 WithZ(this Vector3 vec, float z) {
        return new Vector3(vec.x, vec.y, z);
    }

    //Return the XY component of a Vector3
    public static Vector2 XY(this Vector3 vec) {
        return new Vector2(vec.x, vec.y);
    }

    //Return the YZ component of a Vector3
    public static Vector2 YZ(this Vector3 vec) {
        return new Vector2(vec.y, vec.z);
    }

    //Return a XZ component of a Vector3
    public static Vector2 XZ(this Vector3 vec) {
        return new Vector2(vec.x, vec.z);
    }

    //Return the XYZ component of a Vector4
    public static Vector3 XYZ(this Vector4 vec) {
        return new Vector3(vec.x, vec.y, vec.z);
    }

    //Component-wise inverse of a Vector2
    public static Vector2 Inverse(this Vector2 vec) {
        return new Vector2(1f / vec.x, 1f/ vec.y);
    }

    //Component-wise inverse of a Vector3
    public static Vector3 Inverse(this Vector3 vec) {
        return new Vector3(1f / vec.x, 1f / vec.y, 1f / vec.z);
    }

    //Component-wise inverse of a Vector4
    public static Vector4 Inverse(this Vector4 vec) {
        return new Vector4(1f / vec.x, 1f / vec.y, 1f / vec.z, 1f / vec.w);
    }

    public static Vector2 Sin(this Vector2 vec) {
        return new Vector2(Mathf.Sin(vec.x), Mathf.Sin(vec.y));
    }

    public static Vector3 Sin(this Vector3 vec) {
        return new Vector3(Mathf.Sin(vec.x), Mathf.Sin(vec.y), Mathf.Sin(vec.z));
    }
    
    public static Vector4 Sin(this Vector4 vec) {
        return new Vector4(Mathf.Sin(vec.x), Mathf.Sin(vec.y), Mathf.Sin(vec.z), Mathf.Sin(vec.w));
    }

    public static Vector3 Mod(this Vector3 vec, float mod) {
        return new Vector3(
            vec.x > 0f ? (vec.x % mod) : (-(-vec.x % mod)),
            vec.y > 0f ? (vec.y % mod) : (-(-vec.y % mod)),
            vec.z > 0f ? (vec.z % mod) : (-(-vec.z % mod)));
    }

    public static Vector3 Mod(this Vector3 vec, Vector3 mod) {
        return new Vector3(
            vec.x > 0f ? (vec.x % mod.x) : (-(-vec.x % mod.x)),
            vec.y > 0f ? (vec.y % mod.y) : (-(-vec.y % mod.y)),
            vec.z > 0f ? (vec.z % mod.z) : (-(-vec.z % mod.z)));
    }
}