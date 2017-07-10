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

public static class ColorUtil {
    //replace a color's red value
    public static Color WithRed(this Color col, float red) {
        return new Color(red, col.g, col.b, col.a);
    }
    //replace a color's green value
    public static Color WithGreen(this Color col, float green) {
        return new Color(col.r, green, col.b, col.a);
    }
    //replace a color's blue value
    public static Color WithBlue(this Color col, float blue) {
        return new Color(col.r, col.g, blue, col.a);
    }
    //replace a color's alpha value
    public static Color WithAlpha(this Color col, float alpha) {
        return new Color(col.r, col.g, col.b, alpha);
    }
    //invert a color to 1-divided-by-color
    public static Color Inverse(this Color col, bool invertAlpha = false) {
        return new Color(1f / col.r, 1f / col.g, 1f / col.b, invertAlpha ? (1f/col.a) : col.a);
    }
    //Vector3 scale with Color
    public static Color Scale(this Color col, Vector3 scale) {
        return new Color(col.r * scale.x, col.g * scale.y, col.b * scale.z, col.a);
    }
    //Vector4 scale with Color
    public static Color Scale(this Color col, Vector4 scale) {
        return new Color(col.r * scale.x, col.g * scale.y, col.b * scale.z, col.a * scale.w);
    }
    //set a color's r, g, and b values to their average
    public static Color GreyScale(this Color col, bool preserveAlpha = true) {
        if(preserveAlpha) {
            float val = (col.r + col.g + col.b) / 3f;
            return new Color(val, val, val, col.a);
        }else{
            float val = (col.r + col.g + col.b + col.a) / 4f;
            return new Color(val, val, val, val);
        }
    }
    
    //http://wiki.unity3d.com/index.php?title=HexConverter
    // Note that Color32 and Color implictly convert to each other. You may pass a Color object to this method without first casting it.
    public static string ColorToHex(Color32 color) {
        string hex = "#" + color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
        return hex;
    }
    
    public static Color HexToColor(string hex) {
        if(hex == null || hex.Length == 0) return new Color(0f, 0f, 0f, 0f);
        
        //if the text begins with a hash character, start on index 1, otherwise start on 0
        int start = (hex.Length > 0 && hex[0] == '#') ? 1 : 0;
        byte r = byte.Parse(hex.Substring(start,2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(start+2,2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(start+4,2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r,g,b, 255);
    }
}