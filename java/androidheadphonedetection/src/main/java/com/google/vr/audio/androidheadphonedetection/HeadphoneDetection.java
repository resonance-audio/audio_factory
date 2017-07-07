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

package com.google.vr.audio.androidheadphonedetection;

import android.app.Activity;
import android.content.Context;
import android.media.AudioDeviceInfo;
import android.media.AudioManager;
import android.os.Build;

/**
 * This class is created by the native |HeadphoneDetection| class to retrieve audio-related device
 * information from Android SDK.
 */
public class HeadphoneDetection {
    private Context context;

    public void setActivity(Activity activity) {
        context = activity.getApplicationContext();
    }

    /**
     * Returns true if a headphone is connected.
     */
    public boolean isHeadphonePluggedIn() {
        // AudioDeviceInfo is only supported on API 23+.
        AudioManager audioManager = (AudioManager) context.getSystemService(Context.AUDIO_SERVICE);
        if (Build.VERSION.SDK_INT <= Build.VERSION_CODES.N_MR1) {
            return audioManager.isWiredHeadsetOn();
        }
        AudioDeviceInfo[] AudioDeviceInfos = audioManager.getDevices(AudioManager.GET_DEVICES_OUTPUTS);
        for (AudioDeviceInfo AudioDeviceInfo : AudioDeviceInfos) {
            if (AudioDeviceInfo.getType() == AudioDeviceInfo.TYPE_WIRED_HEADPHONES) {
                return true;
            }
        }
        return false;
    }

    /**
     * Returns true if a bluetooth audio device is connected.
     */
    public boolean isBluetoothAudioDevicePluggedIn() {
        // AudioDeviceInfo is only supported on API 23+.
        AudioManager audioManager = (AudioManager) context.getSystemService(Context.AUDIO_SERVICE);
        if (Build.VERSION.SDK_INT <= Build.VERSION_CODES.N_MR1) {
            return audioManager.isBluetoothA2dpOn();
        }
        AudioDeviceInfo[] AudioDeviceInfos = audioManager.getDevices(AudioManager.GET_DEVICES_OUTPUTS);
        for (AudioDeviceInfo AudioDeviceInfo : AudioDeviceInfos) {
            if (AudioDeviceInfo.getType() == AudioDeviceInfo.TYPE_BLUETOOTH_A2DP) {
                return true;
            }
        }
        return false;
    }
    
}