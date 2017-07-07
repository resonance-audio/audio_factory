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

public static class GlobalMaterialPropertyBlocks {

    static Dictionary<string, MaterialPropertyBlock> _cache = new Dictionary<string, MaterialPropertyBlock>();
    
    public static MaterialPropertyBlock Get(string name) {
        if(string.IsNullOrEmpty(name)) {
            throw new ArgumentNullException("name");
        }

        MaterialPropertyBlock ret;
        if(!_cache.TryGetValue(name, out ret)) {
            ret = new MaterialPropertyBlock();
            _cache[name] = ret;
        }

        return ret;
    }
}