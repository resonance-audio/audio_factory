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
using System.Linq;
using UnityEngine;

//This component auto-generates a set of points based upon a sequence of jointed capsule colliders, and sends that point set to a line-renderer.
[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class Rope : BaseSplinePath {

    //The minimum number of segments allowed
    const int SEGMENT_MIN = 3;

    //The maximum number of segments allowed
    const int SEGMENT_MAX = 100;

    //Time in-frames after OnEnable, in which the {lineRendererLerp} is ignored, and 1.0 is used instead
    const int NO_RENDERER_LERP_GRACE_PERIOD = 5;

//-----------------------------------------------------------------------------------------
#region SERIALIED_FIELDS
    //Whether to create/destroy/reset elements on update in the editor
    [SerializeField] public bool autoGenerate = true;

    [SerializeField] public AnimationCurve droopX = new AnimationCurve();
    [SerializeField] public AnimationCurve droopY = new AnimationCurve();
    [SerializeField] public AnimationCurve droopZ = new AnimationCurve();

    //Whether colliders should be enabled on the rope segments
    [SerializeField] public bool collisionEnabled = false;  
    [SerializeField] public bool isKinematic = false;
    //Whether to manually constrain the distance between the start/end points
    [SerializeField] public bool hardLimitLength = true;
    //Whether to manually constrain the distance between the start/end points
    [SerializeField] public float hardLimitSlack = 0f;
    //The number of steps to do in the curving of the renderer
    [RangeAttribute(0, 20)]
    [SerializeField] public int splineDetail = 1;
    //Number of rope segments
    [SerializeField] public int segments = 5;
    //Parent to spawn rope segments under
    [SerializeField] public Transform root = null;
    //The starting rigidbody of the rope
    [SerializeField] public Rigidbody start = null;
    //The ending rigidbody of the rope
    [SerializeField] public Rigidbody end = null;
    //Rope radius
    [SerializeField] public float radius = 0.1f;
    //The total length of gap-space between colliders
    [SerializeField] public float gap = 0.1f;
    //Rigidbody drag
    [SerializeField] public float drag = 1f;
    //Fraction at which the rope positions will lerp to their target (1 == instant)
    [RangeAttribute(0.01f, 1f)]
    [SerializeField] public float lineRendererLerp = 1f;
    [SerializeField] public int refreshFrequency = 1;
#endregion SERIALIZED_FIELDS


//-----------------------------------------------------------------------------------------
#region NON_SERIALIZED_FIELDS
    //array of positions to keep around so we don't have to keep allocating every frame
    [NonSerialized] Vector3[] _lineRendererPositionCache = null;
    [NonSerialized] Vector3[] _childrenCache = null;
    [NonSerialized] List<RopeSegment> _segmentCache = new List<RopeSegment>();
    [NonSerialized] List<Joint> _jointCache = new List<Joint>();
    [NonSerialized] int _refreshFrame = 0;
    [NonSerialized] int framesSinceEnable = 0;
#endregion NON_SERIALIZED_FIELDS


//-----------------------------------------------------------------------------------------
#region METHODS
    public override Vector3 Evaluate(float time) {

        if(_lineRendererPositionCache == null || _lineRendererPositionCache.Length == 0) {
            RefreshLineRenderer();
        }

        if(time == 1f) return _lineRendererPositionCache[_lineRendererPositionCache.Length-1];
        time %= 1f;
        if(time < 0f) time = 1f + time;
        if(_lineRendererPositionCache == null || _lineRendererPositionCache.Length == 0) return transform.position;
        if(time == 0f) return _lineRendererPositionCache[0];

        float totalLength = 0f;
        for(int v=0; v<_lineRendererPositionCache.Length-1; ++v) {
            totalLength += (_lineRendererPositionCache[v+1] - _lineRendererPositionCache[v]).magnitude;
        }

        float requestedLength = totalLength * time;
        float currLen = 0f;
        for(int v=0; v<_lineRendererPositionCache.Length-1; ++v) {
            float len = (_lineRendererPositionCache[v+1] - _lineRendererPositionCache[v]).magnitude;
            if((currLen + len) > requestedLength) {
                float currTime = currLen / requestedLength;
                float lengthNeeded = requestedLength * (1f - currTime);
                float fractionOfNextSegment = lengthNeeded / (_lineRendererPositionCache[v+1] - _lineRendererPositionCache[v]).magnitude;
                return Vector3.Lerp(_lineRendererPositionCache[v], _lineRendererPositionCache[v+1], fractionOfNextSegment);
            }else{
                currLen += len;
                continue;
            }
        }

        return _lineRendererPositionCache[_lineRendererPositionCache.Length-1];
    }

    void Awake() {

        if(root == null) {
            root = transform;
        }

        if(start == null) {
            start = new GameObject("Start").AddComponent<Rigidbody>();
            start.isKinematic = true;
            start.useGravity = false;
            start.gameObject.AddComponent<SphereCollider>();
            start.transform.SetParent(root);
            start.transform.localPosition = Vector3.zero;
        }

        if(end == null) {
            end = new GameObject("End").AddComponent<Rigidbody>();
            end.isKinematic = true;
            end.useGravity = false;
            end.gameObject.AddComponent<SphereCollider>();
            end.transform.SetParent(root);
            end.transform.localPosition = Vector3.down * 1f;
        }
    }

    //The visual length of the rope. Will fluctuate depending on joint elasticity.
    public override float TotalLength {
        get {

            //return the sum of the length between each segment
            if(_lineRendererPositionCache != null && _lineRendererPositionCache.Length > 0) {
                float ret = 0f;
                for(int i=0; i<_lineRendererPositionCache.Length-1; ++i) {
                    ret += (_lineRendererPositionCache[i] - _lineRendererPositionCache[i+1]).magnitude;
                }
                return ret;

            //otherwise, just return the expected length
            }else{
                return RuntimeLength;
            }
        }
    }

    //The expected length of the rope. May be different than the actual length, which is influenced by joint elasticity
    public float RuntimeLength {
        get {
            //gap is a constant length, and the radius is added to include the start/end sphere radii
            float ret = gap + radius + hardLimitSlack;

            _segmentCache.Clear();
            (root ?? transform).GetComponentsInChildren<RopeSegment>(true, _segmentCache);
            //add each segment's joint distances
            for(int s=0; s<_segmentCache.Count; ++s) {
                var seg = _segmentCache[s];
                _jointCache.Clear();
                seg.GetComponents<Joint>(_jointCache);
                if(_jointCache != null && _jointCache.Count >= 2) {
                    ret += Vector3.Distance(_jointCache[0].anchor, _jointCache[1].anchor);
                }
            }
            return ret;
        }
    }

    //The physical length of each segment, NOT including any gap spacing
    public float SegmentLength {
        get {
            return (Vector3.Distance(start.transform.position, end.transform.position) - gap - (2f * radius)) / (float)segments;
        }
    }

    //Create and setup a new segment
    RopeSegment CreateNewChild() {
        GameObject obj = new GameObject("Segment");
        obj.transform.SetParent(root);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        var capsule = obj.AddComponent<CapsuleCollider>();
        capsule.radius = radius;
        capsule.height = SegmentLength;
        obj.AddComponent<Rigidbody>();
        var ret = obj.AddComponent<RopeSegment>();
        return ret;
    }

    //Create/destroy joints on an object to the specified count
    // NOTE: count is per-segment; IE: usually 2, one connecting the previous segment, one connecting the next segment
    ConfigurableJoint[] GenerateJoints(GameObject obj, int count) {
        ConfigurableJoint[] joints = obj.GetComponents<ConfigurableJoint>();

        if(joints == null) {
            for(int i=0; i<count; ++i) {
                var newJoint = obj.AddComponent<ConfigurableJoint>();
                newJoint.xMotion = ConfigurableJointMotion.Locked;
                newJoint.yMotion = ConfigurableJointMotion.Limited;
                newJoint.zMotion = ConfigurableJointMotion.Locked;
            }
        }else{
            if(joints.Length < count) {
                for(int i=0; i<count-joints.Length; ++i) {
                    var newJoint = obj.AddComponent<ConfigurableJoint>();
                    newJoint.xMotion = ConfigurableJointMotion.Locked;
                    newJoint.yMotion = ConfigurableJointMotion.Limited;
                    newJoint.zMotion = ConfigurableJointMotion.Locked;
                }
            }else if(joints.Length > count) {
                for(int i=0; i<joints.Length-count; ++i) {
                    try {
                        if(Application.isPlaying) Component.Destroy(joints[i]);
                        else Component.DestroyImmediate(joints[i]);
                    } catch(Exception) {
                    }
                }
            }
        }

        return obj.GetComponents<ConfigurableJoint>();
    }

    //Set up joints on all children to connect to one another
    void ConnectJoints(List<RopeSegment> children) {

        if(start.GetComponent<ConfigurableJoint>() == null) {
            start.gameObject.AddComponent<ConfigurableJoint>();
        }

        if(end.GetComponent<ConfigurableJoint>() == null) {
            end.gameObject.AddComponent<ConfigurableJoint>();
        }

        for(int i=0; i<children.Count; ++i) {
            ConfigurableJoint[] joints = GenerateJoints(children[i].gameObject, 2);
            if(i==0) {
                joints[0].connectedBody = start;
                joints[1].connectedBody = children[i+1].GetComponent<Rigidbody>();
            }else if(i==children.Count-1) {
                joints[0].connectedBody = children[i-1].GetComponent<Rigidbody>();
                joints[1].connectedBody = end;
            }else{
                joints[0].connectedBody = children[i-1].GetComponent<Rigidbody>();
                joints[1].connectedBody = children[i+1].GetComponent<Rigidbody>();
            }
            
            joints[0].axis = Vector3.up;
            joints[0].anchor = Vector3.down * (SegmentLength - (gap/(float)segments)) * 0.5f;
            joints[1].axis = Vector3.up;
            joints[1].anchor = Vector3.up * (SegmentLength - (gap/(float)segments)) * 0.5f;
        }
    }

    //Create/destroy segments to the specified count
    List<RopeSegment> GetChildren(int count) {
        List<RopeSegment> children = new List<RopeSegment>(root.GetComponentsInChildren<RopeSegment>(true));
        for(int i=children.Count-1; i>=0; --i) {
            if(children[i] == null) {
                children.RemoveAt(i);
                continue;
            }
        }

        while(children.Count < count) {
            var child = CreateNewChild();
            children.Add(child);
        }

        while(children.Count > count) {
            children[children.Count-1].SafeDestroy();
            children.RemoveAt(children.Count-1);
        }

        return children;
    }

    //Create missing segments, or destroy existing/orphaned segments
    List<RopeSegment> GeneratePhysicsElements() {
        var children = GetChildren(segments);
        ConnectJoints(children);
        MoveChildren(children);
        return children;
    }

    //Properly move/orient the rope segments
    void MoveChildren(List<RopeSegment> children) {

        // Quaternion rotationDir = Quaternion.LookRotation((end.transform.position - start.transform.position).normalized, (root??transform).rotation * Vector3.up) * Quaternion.AngleAxis(90f, Vector3.right);

        Vector3[] pathPoints = new Vector3[segments+1];
        pathPoints[0] = start.position;
        pathPoints[pathPoints.Length-1] = end.position;

        //iterate from 2nd point to 2nd-to-last point (first and last are always fixed positions)
        for(int i=1; i<pathPoints.Length-1; ++i) {
            float percent = (float)(i) / (float)(pathPoints.Length-1);
            
            Vector3 droop = (droopX == null ? 0f : droopX.Evaluate(percent)) * Vector3.right
                          + (droopY == null ? 0f : droopY.Evaluate(percent)) * Vector3.up
                          + (droopZ == null ? 0f : droopZ.Evaluate(percent)) * Vector3.forward;

            pathPoints[i] = Vector3.Lerp(start.position, end.position, percent) + droop;
        }

        for(int i=0; i<children.Count; ++i) {
            Vector3 left = pathPoints[i];
            Vector3 right = pathPoints[i+1];
            children[i].Initialize(radius, (left-right).magnitude);

            children[i].transform.position = (pathPoints[i] + pathPoints[i+1]) / 2f;
            children[i].transform.rotation = Quaternion.LookRotation((right-left).normalized, Vector3.down) * Quaternion.AngleAxis(90f, Vector3.right);
        }
    }

    //Refresh all relevent properties on the rope's line renderer
    void RefreshLineRenderer() {
        _segmentCache.Clear();
        root.GetComponentsInChildren<RopeSegment>(true, this._segmentCache);
        
        if(_childrenCache == null || _childrenCache.Length != (_segmentCache.Count+2)) {
            _childrenCache = new Vector3[_segmentCache.Count+2];
        }

        for(int i=0; i<_segmentCache.Count; ++i) {
            _childrenCache[i+1] = _segmentCache[i].transform.position - 
                (_segmentCache[i].transform.rotation 
                 * Vector3.up 
                 * _segmentCache[i].GetOrAddComponent<CapsuleCollider>().height/2f);
        }
        _childrenCache[0] = start.position;
        _childrenCache[_childrenCache.Length-1] = end.position;

        if(_childrenCache == null || _childrenCache.Length == 0) {
            return;
        }

        var rend = GetComponent<LineRenderer>();
        int childrenLen = ((_childrenCache.Length-1) * splineDetail) - splineDetail + 1;
        if(rend == null) {
            throw new NullReferenceException("null renderer");
        }

        if(_lineRendererPositionCache == null || _lineRendererPositionCache.Length != childrenLen) {
            _lineRendererPositionCache = new Vector3[childrenLen];
        }
        _lineRendererPositionCache[_lineRendererPositionCache.Length-1] = end.position;

        float lerp = Application.isPlaying && (framesSinceEnable > NO_RENDERER_LERP_GRACE_PERIOD) ? lineRendererLerp : 1f;

        _lineRendererPositionCache[0] = start.transform.position;
        _lineRendererPositionCache[_lineRendererPositionCache.Length-1] = end.transform.position;

        for(int i=0; i<_childrenCache.Length-2; ++i) {
            int len = _childrenCache.Length-1;
            int i1 = Mathf.Clamp(i+0, 0, len);
            int i2 = Mathf.Clamp(i+1, 0, len);
            int i3 = Mathf.Clamp(i+2, 0, len);
            int i4 = Mathf.Clamp(i+3, 0, len);

            Vector3 p1 = _childrenCache[i1];
            Vector3 p2 = _childrenCache[i2];
            Vector3 p3 = _childrenCache[i3];
            Vector3 p4 = _childrenCache[i4];

            for(int s=0; s<splineDetail; ++s) {
                float splineFrac = (float)s / (float)(splineDetail);
                Vector3 newPos = GetCatmullRomPosition(splineFrac, p1, p2, p3, p4);
                Vector3 prev = _lineRendererPositionCache[i*splineDetail+s];
                _lineRendererPositionCache[i*splineDetail+s] = Vector3.Lerp(prev, newPos, lerp);
            }
        }

        rend.positionCount = _lineRendererPositionCache.Length;
        rend.SetPositions(_lineRendererPositionCache);
        rend.startWidth = radius * 2f;
        rend.endWidth   = radius * 2f;
    }

	//Returns a position between 4 Vector3 with Catmull-Rom spline algorithm
	//http://www.iquilezles.org/www/articles/minispline/minispline.htm
	Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		//The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
		Vector3 a = 2f * p1;
		Vector3 b = p2 - p0;
		Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

		//The cubic polynomial: a + b * t + c * t^2 + d * t^3
		Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));

		return pos;
	}

    //Refresh the radius of colliders and the line-renderer
    void RefreshRadii() {
        
        //set the line renderer width
        var rend = ComponentUtilities.GetOrAddComponent<LineRenderer>(this);
        rend.startWidth = radius * 2f;
        rend.endWidth = radius * 2f;

        //set collision radii
        if(start != null) {
            ComponentUtilities.GetOrAddComponent<SphereCollider>(start).radius = radius;
        }
        if(end != null) {
            ComponentUtilities.GetOrAddComponent<SphereCollider>(end).radius = radius;
        }
        foreach(RopeSegment seg in (root??transform).GetComponentsInChildren<RopeSegment>(true)) {
            if(seg != null) {
                var coll = seg.GetOrAddComponent<CapsuleCollider>();
                coll.radius = radius;
                coll.height = SegmentLength;
            }
        }
    }

    //Refresh the enabled-state on all colliders
    void RefreshCollision() {
        if(start != null) {
            start.GetOrAddComponent<SphereCollider>().enabled = collisionEnabled;
            start.GetOrAddComponent<Rigidbody>().drag = drag;
        }
        if(end != null) {
            end.GetOrAddComponent<SphereCollider>().enabled = collisionEnabled;
            end.GetOrAddComponent<Rigidbody>().drag = drag;
        }
        foreach(RopeSegment seg in (root??transform).GetComponentsInChildren<RopeSegment>(true)) {
            if(seg != null) {
                seg.GetOrAddComponent<CapsuleCollider>().enabled = collisionEnabled;
                seg.GetOrAddComponent<Rigidbody>().drag = drag;
                seg.GetOrAddComponent<Rigidbody>().isKinematic = isKinematic;
            }
        }
    }

    //We need to reinforce the rope's constraints on the start/end points manually, to ensure that there is no funny business going on with our joints
    public void ConstrainEndpoints() {

        if(end != null && start != null) {
            float maxDist = RuntimeLength;
            float dist = Vector3.Distance(end.transform.position, start.transform.position);
            if(dist > maxDist) {

                if(!start.isKinematic
                && end.isKinematic) {

                    //normalized direction vector
                    Vector3 direction = (start.transform.position - end.transform.position).normalized;
                    start.transform.position = end.transform.position + (direction * maxDist);

                    float dot = Vector3.Dot(start.velocity, direction);
                    //float normDot = Vector3.Dot(start.velocity.normalized, direction);
                    Vector3 newVel = start.velocity - (direction * dot);
                    //float deltaMagnitude = newVel.magnitude - start.velocity.magnitude;
                    //Vector3 conservedVelocity = (newVel.normalized * normDot * (-deltaMagnitude));
                    start.velocity = newVel;// + conservedVelocity;

                } else {
                    //normalized direction vector
                    Vector3 direction = (end.transform.position - start.transform.position).normalized;
                    end.transform.position = start.transform.position + (direction * maxDist);

                    float dot = Vector3.Dot(end.velocity, direction);
                    //float normDot = Vector3.Dot(end.velocity.normalized, direction);
                    Vector3 newVel = end.velocity - (direction * dot);
                    //float deltaMagnitude = newVel.magnitude - end.velocity.magnitude;
                    //Vector3 conservedVelocity = (newVel.normalized * normDot * (-deltaMagnitude));
                    end.velocity = newVel;// + conservedVelocity;
                }
            }
        }
    }

    //OnValidate, keep values within reasonable bounds
    void OnValidate() {

        //default droop curves to have at least two points
        if(droopX == null || droopX.length < 2 || droopX.keys.Count(k => k.time == 0f) == droopX.length) {
            var keys = new Keyframe[2];
            keys[0] = new Keyframe(0f, 0f);
            keys[1] = new Keyframe(1f, 0f);
            droopX = new AnimationCurve(keys);
        }
        if(droopY == null || droopY.length < 2 || droopY.keys.Count(k => k.time == 0f) == droopY.length) {
            var keys = new Keyframe[2];
            keys[0] = new Keyframe(0f, 0f);
            keys[1] = new Keyframe(1f, 0f);
            droopY = new AnimationCurve(keys);
        }
        if(droopZ == null || droopZ.length < 2 || droopZ.keys.Count(k => k.time == 0f) == droopZ.length) {
            var keys = new Keyframe[2];
            keys[0] = new Keyframe(0f, 0f);
            keys[1] = new Keyframe(1f, 0f);
            droopZ = new AnimationCurve(keys);
        }

        if(hardLimitSlack < 0f) hardLimitSlack = 0f;
        if(gap < 0f) gap = 0f;
        segments = Mathf.Clamp(segments, SEGMENT_MIN, SEGMENT_MAX);

        float segLen = SegmentLength;
        if(radius <= 0f) {
            radius = 0.0001f;
        }else if(radius > (segLen / 2f)) {
            radius = segLen / 2f;
        }
    }

    void OnEnable() {
        framesSinceEnable = 0;
        _refreshFrame = UnityEngine.Random.Range(0, 1000);
        if(autoGenerate) {
            GeneratePhysicsElements();
            MoveChildren(root.GetComponentsInChildren<RopeSegment>(true).ToList());
            RefreshCollision();
            RefreshRadii();
        }
    }

    void Update() {
        ++framesSinceEnable;
        //only do the auto-generate stuff if we have specified to do so; it may be useful to copy ropes from play-mode and paste them into the scene later, so we don't want to be destroying its orientations in editor-mode
        if(!Application.isPlaying) {
            if(autoGenerate) {
                //regenerate the segments if needed
                GeneratePhysicsElements();
                //move and rotate the segments
                MoveChildren(root.GetComponentsInChildren<RopeSegment>(true).ToList());
                //create colliders if we need to
                RefreshCollision();
                //refresh the radius of all colliders/lineRenderers
                RefreshRadii();
            }
            
            RefreshLineRenderer();
        }else{
            if(refreshFrequency >= 2) {
                ++_refreshFrame;
                if(_refreshFrame >= refreshFrequency) {
                    _refreshFrame = _refreshFrame % refreshFrequency;
                    RefreshLineRenderer();
                }
            }else{
                RefreshLineRenderer();
            }
        }
    }

    void LateUpdate() {
        //no need to do this in editor as we want to be able to drag the endpoints around freely
        if(Application.isPlaying && hardLimitLength) {
            ConstrainEndpoints();
        }
    }
#endregion METHODS
}