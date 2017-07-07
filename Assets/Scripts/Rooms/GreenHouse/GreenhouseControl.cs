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

public class GreenhouseControl : MonoBehaviour {

    ShowerHead[] showerHeads;
    Pipe[] pipes;
    BambooSpawner bamboo;
    OverheadLights overheadLights;
    PlantGroup[] plants;

    public AnimationCurve pipeFallOff;
    public AnimationCurve handleShakeCurve;

    int currentPipe;

    [SerializeField]
    public GvrAudioSettings handleAudioSettings;


    void Start() {
        FindPlants();

        FindBamboo();
    }

	public void Init() {
        FindShowerHeads();
        FindPipes();
        FindOverheadLights();
	}

    void FindBamboo() {
        bamboo = GameObject.Find("Bamboo").GetComponent<BambooSpawner>();
        bamboo.Init(this);
    }

    void FindShowerHeads() {
        showerHeads = new ShowerHead[3];
        for(int i = 0; i < showerHeads.Length; i++) {
            GameObject newShowerHead = GameObject.Find("ShowerHead" + (i + 1));
            GameObject newShowerHeadObj = GameObject.Find("SHOWERHEAD_0" + (i + 1));
            ShowerHead head = newShowerHead.GetComponent<ShowerHead>();
            head.AddRenderer(newShowerHeadObj.GetComponent<Renderer>());
            //head.AddFallOffCurve(pipeFallOff);
            showerHeads[i] = head;
            head.AddBamboo(bamboo.GetGroup(i));
        }        
    }

    void FindPipes() {
        pipes = new Pipe[3];
        for(int i = 0; i < pipes.Length; i++) {
            GameObject pipe = GameObject.Find(string.Format("Pipe{0}", (i + 1)));
            pipes[i] = pipe.GetComponent<Pipe>();

            GameObject pipeAObj = GameObject.Find(string.Format("FUNNEL/FUNNEL_PIPES_{0}", (i + 1)));
            //[bh 02-20-2017] warning cleanup
            //GameObject pipeBObj = GameObject.Find(string.Format("SHOWER_PIPES/SHOWER_PIPE_0{0}", (i + 1)));
            pipes[i].AddMaterials(pipeAObj.GetComponent<Renderer>().material);

            GameObject pathAObj = GameObject.Find(string.Format("Pipe{0}Path", (i + 1)));
            
            //[bh 02-20-2017] warning cleanup
            //GameObject pathBObj = GameObject.Find(string.Format("Pipe{0}Path", (i + 1)));
            pipes[i].AddPaths(pathAObj.GetComponent<SplinePath>());

            pipes[i].AddShowerHead(showerHeads[i]);
        }          
    }
        
    void FindOverheadLights() {
        overheadLights = GameObject.Find("OverheadLights").GetComponent<OverheadLights>();
    }

    void FindPlants() {
        plants = new PlantGroup[3];
        GameObject plantGroup;

        for(int i = 0; i < plants.Length; i++) {  
            plantGroup = GameObject.Find(string.Format("GREENHOUSE_14_GROUNDCOVER_UNITY/CROUNDCOVER_0{0}", (i + 1)));
            plants[i] = plantGroup.GetComponent<PlantGroup>();
        }
    }

    public PlantGroup GetPlantGroup(int id) {
        return plants[id];
    }
	
	void Update () {



	}

    public Pipe GetCurrentPipe() {
        Debug.Log("Getting pipe number " + currentPipe);
        return pipes[currentPipe];
    }

    public void NextPipeOn()
    {
        pipes[currentPipe].PipeOn = true;
        currentPipe++;
        Debug.Log("CurrentPipe is " + currentPipe);

    }

    public void BambooDone(int groupID)
    {
        if (groupID == 2)
        {
            overheadLights.LightsOn = true;
        }
    }
}
