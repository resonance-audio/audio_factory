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

using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SplashSceneController : MonoBehaviour {

    [SerializeField] MasterConfiguration config;

    public Renderer fadeRenderer;
    float fadeTimer = 1;
    float fadeDuration = 2;
    bool fadeOut = false;

    public bool FadeOut
    {
        get { return fadeOut; }
        set {
            fadeOut = value;
            fadeTimer = Mathf.Clamp01(fadeTimer);
        }
    }

    float testTimer = 5;

    public GameObject noWarning;
    public GameObject headPhonesWarning;
    public GameObject blueToothWarning;

    void OnEnable() {
#if UNITY_ANDROID
        SetActivityInNativePlugin();
#endif
    }

    void Start() {
        ReadyToStart();
    }

    void Update() {
        if (GvrController.AppButtonDown) {
            Debug.Log("Open Legal Menu");
            SceneManager.LoadScene("LegalText");
        }

        testTimer -= Time.deltaTime;
        if (testTimer < 0) {
            testTimer = 1.0f;
            Debug.Log("Testing for pass");
            if (ReadyToStart()) {
                FadeOut = true;
            }
        }

        if (fadeTimer >= 0.0f && fadeTimer <= 1.0f) {
            fadeTimer += Time.deltaTime / (fadeOut ? fadeDuration : -fadeDuration);
            if (fadeTimer > 1.0f)
                GotoFirstScene();
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(fadeTimer));
            fadeRenderer.material.SetColor("_TintColor", Color.Lerp(Color.clear, Color.black, amount));
        }
    }

    public void GotoFirstScene() {
        UnityEngine.SceneManagement.SceneManager.LoadScene(config.roomConfiguration.FirstRoom.scenePath);
    }

    bool ReadyToStart() {

        if (Application.platform != RuntimePlatform.Android) {
            noWarning.SetActive(true);
            blueToothWarning.SetActive(false);
            headPhonesWarning.SetActive(false);
            Debug.Log("Not running on device");
            return true;           
        }

        bool headPhoneConnected = GetJavaObject().Call<bool>("isHeadphonePluggedIn");
        bool blueToothConnected = GetJavaObject().Call<bool>("isBluetoothAudioDevicePluggedIn");
        if (blueToothConnected) {
            noWarning.SetActive(false);
            blueToothWarning.SetActive(true);
            headPhonesWarning.SetActive(false);
            return false;
        }
        if (!headPhoneConnected) {
            noWarning.SetActive(false);
            blueToothWarning.SetActive(false);
            headPhonesWarning.SetActive(true);
            return false;
        }

        noWarning.SetActive(true);
        blueToothWarning.SetActive(false);
        headPhonesWarning.SetActive(false);
        return true;
    }


    private AndroidJavaObject javaObj = null;

    private AndroidJavaObject GetJavaObject() {
        if (javaObj == null) {
            javaObj = new AndroidJavaObject("com.google.vr.audio.androidheadphonedetection.HeadphoneDetection");
        }
        return javaObj;
    }

    private void SetActivityInNativePlugin() {
        // Retrieve current Android Activity from the Unity Player
        AndroidJavaClass jclass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = jclass.GetStatic<AndroidJavaObject>("currentActivity");

        // Pass reference to the current Activity into the native plugin,
        // using the 'setActivity' method that we defined in the ImageTargetLogger Java class
        GetJavaObject().Call("setActivity", activity);
        Debug.Log("Set Androind Activity+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_+_");
    }

}