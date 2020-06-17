using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Video;
using UnityEngine.Experimental.Audio;
using UnityEngine.Video;
using Unity.Collections;

public class DanbiAudioProcessor : MonoBehaviour {

  public AudioSampleProvider provider { get; set; }

  void OnDestroy() {
    provider.Dispose();
    provider = null;
  }

  public void OnVideoPrepared(VideoPlayer preparedVideoPlayer) {
    if (provider != null) {
      Debug.LogError($"AudioSampleProvider can't be initialzed before this line", this);
    }
    // in order to receive the audio samples during playback to initialized AudioSampleProvider.
    provider = preparedVideoPlayer.GetAudioSampleProvider(0);

    if (provider == null) {
      Debug.LogError($"Retrieving AudioSampleProvider failed!", this);
    }

    // bind to the event to sample frames.
    provider.sampleFramesAvailable += OnProviderSampleFramesAreAvailable;
    // Enables sampleFramesAvailable events.
    provider.enableSampleFramesAvailableEvents = false;
    // Then the free sample count falls below this threshold, AudioSampleProvider.sampleFramesAvailable event
    // and the associated native is emitted.
    provider.freeSampleFrameCountLowThreshold = provider.maxSampleFrameCount / 4;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="provider">Provider emitting the event.</param>
  /// <param name="sampleFramesCount">How many sample frames are available, or were dropped, depending on the event.</param>
  void OnProviderSampleFramesAreAvailable(AudioSampleProvider provider, uint sampleFramesCount) {
    using (var audioBuf = new NativeArray<float>((int)sampleFramesCount * provider.channelCount, Allocator.Temp)) {
      // UnityEngine.Experimental.Audio.AudioSampleProvider.ConsumeSampleFrames(
      //   NativeArray<float> sampleFrames) -> uint
      //
      //  param (sampleFrames) -> a buf where the consumed smaples wil lbe transferred.
      //  return (uint) -> How many samples were written into the buffer passed in.
      // 
      //  If AudioSampleProvider.enableSilencePadding = true, then the buf passed in as a parameter
      //  will be completely filled with and padded with silence if there're are less ample frames available.
      //  Otherwise, the extra sample frame in the buf will be left intact!
      uint totalProvidedSamplesCnt = provider.ConsumeSampleFrames(audioBuf);
      Debug.Log($"SetupSoftwareAUdioOutput.Available got {totalProvidedSamplesCnt} sample counts in total!", this);
    }
  }
};
