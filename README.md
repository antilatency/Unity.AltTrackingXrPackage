# Unity.AltTrackingXrPackage

This package includes integration of headset tracking with [Antilatency tracking system](https://antilatency.com/), [Tracking Alignment library](https://developers.antilatency.com/Software/Libraries/Antilatency_Tracking_Alignment_Library_en.html) and [Unity XR](https://docs.unity3d.com/Manual/XR.html) platform.

The link for Unity Package Manager: https://github.com/antilatency/Unity.AltTrackingXrPackage.git

**To work correctly, your project should have the [Antilatency SDK](https://developers.antilatency.com/Sdk/Configurator_ru.html#{%22Release%22:%224.0.0%22,%22Target%22:%22Unity%22,%22TargetSettings%22:{%22MathTypes%22:%22UnityEngine.Math%22,%22UnityVersion%22:%222019.x%22,%22UnityComponents%22:true,%22Components%22:{%22AltTrackingComponents%22:true,%22AltEnvironmentComponents%22:true,%22BracerComponents%22:true,%22DeviceNetworkComponents%22:true,%22StorageClientComponents%22:true}},%22Libraries%22:{%22AltEnvironmentSelector%22:true,%22AltEnvironmentArbitrary2D%22:true,%22AltEnvironmentHorizontalGrid%22:true,%22AltEnvironmentPillars%22:true,%22AltEnvironmentAdditionalMarkers%22:true,%22DeviceNetwork%22:true,%22AltTracking%22:true,%22Bracer%22:true,%22HardwareExtensionInterface%22:true,%22RadioMetrics%22:true,%22TrackingAlignment%22:true,%22StorageClient%22:true,%22StereoGlasses%22:false,%22IllumetryDisplay%22:false},%22OS%22:{%22WindowsDesktop%22:{%22x86%22:true,%22x64%22:true},%22WindowsUWP%22:{%22x64%22:true,%22armeabi-v7a%22:true,%22arm64-v8a%22:true},%22Android%22:{%22aar%22:true},%22Linux%22:{%22x86_64%22:true}}}) installed.**


## Package structure
    .
    └── Packages
        └── Alt Tracking XR
            ├── Prefabs
            |   └── AltTrackingXrRig.prefab     # an example of a prepared prefab; you can drag it to the scene and it is ready to work
            └── Runtime
                └── AltTrackingXr.cs    # an example of a script that integrates the Antilatency system into the Unity XR

## Samples

The package contains Samples: prepared scenes with the prefab implementation. You can add this scenes into your project Assets by click `Import` in Unity Package Manager:
* [Alt Tracking XR Sample](./Samples~/AltTrackingXrSample) - Standard implementation of [`AltTrackingXrRig.prefab`](./Prefabs).

## Adding other Unity XR plugins
This package pulls up standard Unity XR plugins (Windows MR, Oculus and etc.).

You can add support for other headsets using external XR plugins:
* For Vive Focus 3 headsets, add [Vive Wave XR plugin](https://hub.vive.com/storage/docs/en-us/UnityXR/UnityXRSdk.html)
* For Pico Neo 2/3 headsets, add [Pico Unity XR Platform SDK](https://developer.pico-interactive.com/sdk)
