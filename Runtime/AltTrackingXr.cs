using System;
using UnityEngine;
using Antilatency.SDK;
using Antilatency.DeviceNetwork;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AltTrackingXr : AltTracking {
    public Camera XrCamera;
    public UnityEngine.SpatialTracking.TrackedPoseDriver HmdPoseDriver;
    public float MinimalAQualityToAlign = 0.075f;
    public float BQuality = 0.6f;

    private bool _lerpPosition;
    private bool _lerpRotation;

    private Antilatency.TrackingAlignment.ILibrary _alignmentLibrary;
    private Antilatency.TrackingAlignment.ITrackingAlignment _alignment;

    private bool _altInitialPositionApplied = false;

    private Transform _aSpace;
    private Transform _bSpace;
    private Transform _b;

    protected override NodeHandle GetAvailableTrackingNode() {
        return GetFirstIdleTrackerNode();
    }

    protected override Pose GetPlacement() {
        var result = Pose.identity;

        using (var localStorage = Antilatency.SDK.StorageClient.GetLocalStorage()) {
            if (localStorage == null) {
                return result;
            }

            var placementCode = localStorage.read("placement", "default");

            if (string.IsNullOrEmpty(placementCode)) {
                Debug.LogError("Failed to get placement code");
                result = Pose.identity;
            } else {
                result = _trackingLibrary.createPlacement(placementCode);
            }

            return result;
        }
    }

    protected virtual void OnFocusChanged(bool focus) {
        if (focus) {
            StartTrackingAlignment();
        } else {
            StopTrackingAlignment();
        }
    }

    private void StartTrackingAlignment() {
        if (_alignment != null) {
            StopTrackingAlignment();
        }

        var placement = GetPlacement();
        _alignment = _alignmentLibrary.createTrackingAlignment(placement.rotation, ExtrapolationTime);
        _altInitialPositionApplied = false;
    }

    private void StopTrackingAlignment() {
        if (_alignment == null) {
            return;
        }

        _alignment.Dispose();
        _alignment = null;
    }

    private void OnApplicationFocus(bool focus) {
        OnFocusChanged(focus);
    }

    private void OnApplicationPause(bool pause) {
        OnFocusChanged(!pause);
    }

    protected override void Awake() {
        base.Awake();

        _alignmentLibrary = Antilatency.TrackingAlignment.Library.load();

        var placement = GetPlacement();
        _alignment = _alignmentLibrary.createTrackingAlignment(placement.rotation, ExtrapolationTime);

        if (XrCamera == null) {
            XrCamera = GetComponentInChildren<Camera>();
            if (XrCamera == null) {
                Debug.LogError("XR Camera is not setted and no cameras has been found in children gameobjects");
                enabled = false;
                return;
            } else {
                Debug.LogWarning("XR Camera: " + XrCamera.gameObject.name);
            }
        }

        _lerpPosition = HmdPoseDriver.trackingType == UnityEngine.SpatialTracking.TrackedPoseDriver.TrackingType.PositionOnly ||
                        HmdPoseDriver.trackingType == UnityEngine.SpatialTracking.TrackedPoseDriver.TrackingType.RotationAndPosition;

        _lerpRotation = HmdPoseDriver.trackingType == UnityEngine.SpatialTracking.TrackedPoseDriver.TrackingType.RotationOnly ||
                        HmdPoseDriver.trackingType == UnityEngine.SpatialTracking.TrackedPoseDriver.TrackingType.RotationAndPosition;

        _aSpace = transform.parent;
        _bSpace = transform;
        _b = XrCamera.transform;
    }

    protected override void Update() {
        base.Update();

        var centerEye = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.CenterEye);

        if (!centerEye.isValid) {
            Debug.LogWarning("Center eye device is not valid");
            return;
        }

        // Do nothing if HMD is not putted on because some devices send invalid tracking data in such state.
        if (!centerEye.TryGetFeatureValue(UnityEngine.XR.CommonUsages.userPresence, out var userPresence) || !userPresence) {
            return;
        }

        if (!centerEye.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trackingState, out var state)) {
            return;
        }

        var bPositionReceived = centerEye.TryGetFeatureValue(UnityEngine.XR.CommonUsages.centerEyePosition, out var bPosition) && state.HasFlag(UnityEngine.XR.InputTrackingState.Position);
        var bRotationReceived = centerEye.TryGetFeatureValue(UnityEngine.XR.CommonUsages.centerEyeRotation, out var bRotation) && state.HasFlag(UnityEngine.XR.InputTrackingState.Rotation);

        var curTime = Time.realtimeSinceStartup;
        
        bool altTrackingActive;
        Antilatency.Alt.Tracking.State trackingState;

        altTrackingActive = GetRawTrackingState(out trackingState);

        // If Alt is disconnected, we have nothing to do.
        if (!altTrackingActive) {
            return;
        }

        if (_lerpRotation && _alignment != null && trackingState.stability.stage == Antilatency.Alt.Tracking.Stage.Tracking6Dof && bRotationReceived && trackingState.stability.value >= MinimalAQualityToAlign) {
            var result = _alignment.update(trackingState.pose.rotation, bRotation, curTime); 
            
            ExtrapolationTime = result.timeBAheadOfA;
            _placement.rotation = result.rotationARelativeToB;
            _bSpace.localRotation = result.rotationBSpace;
        }

        altTrackingActive = GetTrackingState(out trackingState);
        if (!altTrackingActive || trackingState.stability.stage == Antilatency.Alt.Tracking.Stage.InertialDataInitialization) {
            return;
        }

        if (!_lerpRotation) {
            _bSpace.localRotation = trackingState.pose.rotation;
            _b.localRotation = Quaternion.identity;
        }

        if (_lerpPosition) {
            if (trackingState.stability.stage == Antilatency.Alt.Tracking.Stage.Tracking6Dof && bPositionReceived && trackingState.stability.value >= MinimalAQualityToAlign) {
                var a = trackingState.pose.position;
                var bSpace = _bSpace.localPosition;
                var b = _aSpace.InverseTransformPoint(_bSpace.TransformPoint(bPosition));

                Vector3 averagePositionInASpace;

                if (!_altInitialPositionApplied) {
                    averagePositionInASpace = a;
                    _altInitialPositionApplied = true;
                } else {
                    averagePositionInASpace = (b * BQuality + a * trackingState.stability.value) / (trackingState.stability.value + BQuality);
                }

                _bSpace.localPosition += averagePositionInASpace - b;
            }
        } else {
            _bSpace.localPosition = trackingState.pose.position;
            _b.localPosition = Vector3.zero;
        }
    }
}