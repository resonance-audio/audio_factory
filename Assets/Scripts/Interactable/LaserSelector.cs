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

public class LaserSelector : MonoBehaviour {

    private static LaserSelector instance = null;

    public static LaserSelector Instance
    {
        get { return instance; }
    }

    private void CreateInstance() {
        if (instance == null)
        {
            instance = this;
        }
    }

    public enum LaserType { Straight, Curved }
    LaserType currentLaserType = LaserType.Straight;

    LineRenderer straightLaser;

    Transform target;
    Transform reticle;
    SplinePath laserPath;

    bool hideAll;

    void Awake()
    {
        CreateInstance();

    }

    void Start()
    {
        straightLaser = transform.Find("Laser").GetComponent<LineRenderer>();
        reticle = transform.Find("Laser/Reticle");

    }
	
	void LateUpdate () {
        if (hideAll)
            return;

	    if (!target) {
            currentLaserType = LaserType.Straight;
            straightLaser.enabled = true;
        }

        if (currentLaserType == LaserType.Straight)
            return;

	}

    public static void SetTarget(Transform newTarget)
    {
        if (instance == null)
            return;
        if (!newTarget)
        {
            LaserSelector.ClearTarget();
            return;
        }
        instance.currentLaserType = LaserType.Curved;
        instance.target = newTarget;
        instance.straightLaser.enabled = false;
        instance.laserPath.enabled = true;
        instance.reticle.GetComponent<Renderer>().enabled = false;
    }

    public static void ClearTarget()
    {
        if (instance == null)
            return;
        instance.currentLaserType = LaserType.Straight;
        instance.target = null;
        instance.straightLaser.enabled = true;
        instance.laserPath.enabled = false;
        instance.reticle.GetComponent<Renderer>().enabled = true;
    }

    public static void HideLaser() {
        if (instance == null)
            return;
        instance.hideAll = true;
        instance.currentLaserType = LaserType.Straight;
        instance.target = null;
        instance.straightLaser.enabled = false;
        instance.laserPath.enabled = false;
        instance.reticle.GetComponent<Renderer>().enabled = false;
    }
    public static void ShowLaser() {
        if (instance == null)
            return;
        instance.hideAll = false;
        instance.currentLaserType = LaserType.Straight;
        LaserSelector.ClearTarget();
    }

    public static Ray GetReticleRay() {
        return instance.GetRay();
    }

    public Ray GetRay() {
        return new Ray(Camera.main.transform.position, reticle.position - Camera.main.transform.position);
    }
  
}
