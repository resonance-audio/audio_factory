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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Beginnings of the singleton class which manages rooms
public class GameManager : MonoBehaviour {

    //---------------------------------------------------------------------
    #region SERIALIZED_FIELDS
        [SerializeField] public MasterConfiguration config = null;
        [SerializeField] public string[] preloadedResourcePaths = null;
    #endregion SERIALIZED_FIELDS 

    //---------------------------------------------------------------------
    #region NON_SERIALIZED_FIELDS
        [NonSerialized] RoomDefinition _currentRoom = default(RoomDefinition);
        [NonSerialized] public List<GameObject> persistentObjects = new List<GameObject>();

        [NonSerialized] BaseRoomManager _previousRoomManager = null;
        [NonSerialized] BaseRoomManager _currentRoomManager = null;
        [NonSerialized] FogBlender _fogBlender = null;
        [NonSerialized] RoomLightingSettings _currentLightingSettings = default(RoomLightingSettings);
        [NonSerialized] RoomLightingSettings _previousLightingSettings = default(RoomLightingSettings);
        [NonSerialized] bool _hasTransitionedLightingForCurrentRoom = false;
        [NonSerialized] bool isComplete = false;
    #endregion NON_SERIALIZED_FIELDS

    //---------------------------------------------------------------------
    #region PROPERTIES
        public static GameManager instance { get; private set; }
        private static GameManager _staticAsset { get; set; }

        public RoomDefinition CurrentRoom {
            get {
                return _currentRoom;
            }
        }

        public BaseRoomManager CurrentRoomManager {
            get {
                return _currentRoomManager;
            }
        }

        public BaseRoomManager PreviousRoomManager {
            get {
                return _previousRoomManager;
            }
        }

        public bool IsComplete {
            get {
                return isComplete;
            }
            set {
                isComplete = value;
            }
        }
    #endregion PROPERTIES

    //---------------------------------------------------------------------
    #region METHODS

        public static void Init(Vector3 origin) {
            if(instance != null) {
                return;
            }
            if(_staticAsset == null) {
                var obj = Resources.Load("GameManager");
                UnityEngine.Debug.Log("Loaded GameManager asset: " + (obj != null ? obj.GetInstanceID().ToString() : "NULL"));
                _staticAsset = Resources.FindObjectsOfTypeAll(typeof(GameManager)).FirstOrDefault() as GameManager;
            }
            if(_staticAsset != null) {
                instance = (GameObject.Instantiate(_staticAsset.gameObject, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<GameManager>();
            }

            foreach(var obj in GameManager.instance.config.persistentGameObjects) {
                var inst = GameObject.Instantiate(obj, origin, Quaternion.identity);
                DontDestroyOnLoad(inst);
                instance.persistentObjects.Add((GameObject)inst);
            }

            //See SplashSceneController.cs
            //Resources.LoadAll("");
        }

        void Awake() {

            DontDestroyOnLoad(transform.gameObject);
            if(instance != null && instance != this) {
                UnityEngine.Debug.LogError("There is more than RoomManager in the game hierarchy (and there shouldn't be)");
            }else{
                instance = this;
            }
            _fogBlender = gameObject.GetOrAddComponent<FogBlender>();
            
        }

        void OnDestroy() {
            instance = null;
        }

        public void ReEnablePersistentObjects() {
            if(persistentObjects != null) {
                foreach(var obj in persistentObjects) {
                    obj.SetActive(false);
                    obj.SetActive(true);
                }
            }
        }

        void OnEnable() {
            
            if(config == null) {
                throw new NullReferenceException("MasterConfiguration is not set in GameManager");
            }else if(config.roomConfiguration == null) {
                throw new NullReferenceException("MasterConfiguration exists, but has no RoomConfiguration");
            }else if(config.roomConfiguration.FirstRoom == null) {
                throw new NullReferenceException("First room is null. Is GlobalGameData set up correctly?");
            }
            
            var startingScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var startingRoomDefinition = SceneToRoom(startingScene);

            _currentRoom = startingRoomDefinition;
            _currentRoomManager = FindObjectOfType(typeof(BaseRoomManager)) as BaseRoomManager;
            if(Camera.main != null && _currentRoomManager != null){
                Camera.main.backgroundColor = _currentRoomManager.cameraClearColor;
            }
        }

        RoomDefinition SceneToRoom(UnityEngine.SceneManagement.Scene scene) {
            if(!scene.IsValid()) return null;
            if(config == null) throw new NullReferenceException("GameManager config is null; can't resolve room scene");
            if(config.roomConfiguration == null) throw new NullReferenceException("GameManager room configuration is null; can't resolve room scene");
            if(config.roomConfiguration.rooms == null || config.roomConfiguration.rooms.Length == 0) {
                UnityEngine.Debug.LogError("Config/RoomConfiguration exists for GameManager, but there are no RoomDefinitions set up in it");
                return null;
            }

            var ret = config.roomConfiguration.rooms.FirstOrDefault(rm => scene.path.Contains(rm.scenePath) || string.Equals(scene.path, rm.scenePath, StringComparison.InvariantCultureIgnoreCase));
            if(ret == null) {
                UnityEngine.Debug.LogError("We are starting in a scene which is not normally accessible via the current RoomConfiguration. Scene-transitioning logic may be unavailable.");
            }
            return ret;
        }

        public void LoadRoom(RoomDefinition room) {
            UnityEngine.Debug.Log("LoadRoom: " + (room != null ? room.name : "NULL"));
            if(room == null) throw new ArgumentNullException("room");
            if(string.IsNullOrEmpty(room.scenePath)) throw new ArgumentException("room must have a non-null path");

            _previousRoomManager = _currentRoomManager;
            _currentRoomManager = null;

            StartCoroutine(_LoadRoom_Inner(room));
        }

        public void UnloadPreviousRoom() {
            if(_previousRoomManager == null) return;

            _previousRoomManager.gameObject.SetActive(false);
            //this is an async operation, but we don't really care when it finishes
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_previousRoomManager.gameObject.scene);
            _previousRoomManager = null;
        }

        public void StartLightingTransition() {
            if(!_hasTransitionedLightingForCurrentRoom) {
                _fogBlender.SetSettings(_previousLightingSettings, _currentLightingSettings);
                _fogBlender.SetBlendTexture(_currentRoomManager.pedestalTexture);
                _hasTransitionedLightingForCurrentRoom = true;
            }
        }

        IEnumerator _LoadRoom_Inner(RoomDefinition room) {

            var previousScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            
            //get the lighting settings for the current scene
            _previousLightingSettings = RoomLightingSettings.CreateFromCurrentRoom();

            var loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(room.scenePath, UnityEngine.SceneManagement.LoadSceneMode.Additive);

            while(!loadOperation.isDone) {
                yield return new WaitForEndOfFrame();
            }
            
            //HACK-ish: do not break the "Assets/.../*.unity" convention for scenes
            //  For whatever reason, the naming is very particular for GetSceneByPath()
            var nextScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/"+room.scenePath+".unity");
            if(nextScene.IsValid()) {
                _hasTransitionedLightingForCurrentRoom = false;
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(nextScene);
                //get the lighting settings for the current room
                _currentLightingSettings = RoomLightingSettings.CreateFromCurrentRoom();
                RoomLightingSettings.Set(_previousLightingSettings, null);
            }else{
                _currentLightingSettings = _previousLightingSettings;
            }

            var allRoomManagers = FindObjectsOfType(typeof(BaseRoomManager));
            foreach(var obj in allRoomManagers) {
                if(obj != null && (obj is BaseRoomManager)) {
                    var man = (obj as BaseRoomManager);
                    if(man != null && man.gameObject.scene.IsValid()) {
                        if(man.gameObject.scene == nextScene) {
                            _currentRoomManager = man;
                        }else if(man.gameObject.scene == previousScene) {
                            _previousRoomManager = man;
                        }
                    }
                }
            }

            _currentRoom = room;

            if(_previousRoomManager != null && _currentRoomManager != null) {
                float originalRoomYPosition = _currentRoomManager.transform.position.y;
                _currentRoomManager.transform.position = Vector3.up *  (_previousRoomManager.topPlane.transform.position.y + (_currentRoomManager.transform.position.y - _currentRoomManager.bottomPlane.transform.position.y));

                if(originalRoomYPosition != _currentRoomManager.transform.position.y) {
                    
                    UnityEngine.Debug.LogWarning("The Y-position for (" + _currentRoomManager.GetType().Name + ") does not correspond to its position as saved in the scene asset. The scene-manager will be moved, but objects marked as static may break.");
                }
            }
        }



    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus && isComplete) {
            IsComplete = false;
            Destroy(GameObject.Find("PhoneRemote"));
            Destroy(GameObject.Find("GameManager(Clone)"));
            Destroy(GameObject.Find("P_Elevator(Clone)"));
            UnityEngine.SceneManagement.SceneManager.LoadScene("Splash");
        }
    }

    #endregion METHODS
}
