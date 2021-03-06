using UnityEngine;
using System.Collections;



namespace Vuforia
{
	public class TargetData : MonoBehaviour, ITrackableEventHandler {

		public string type = "ENTER A TYPE";
		public double value = 0;

		public float world_x = 0;
		public float world_y = 0;
		public float world_z = 0;

		public float screen_x = 0;
		public float screen_y = 0;

		public bool isActive = false;

		private ImageTargetBehaviour mImageTargetBehaviour = null;
		private TrackableBehaviour mTrackableBehaviour = null;




		void Start () {

			mTrackableBehaviour = GetComponent<TrackableBehaviour>();
			if (mTrackableBehaviour)
			{
				mTrackableBehaviour.RegisterTrackableEventHandler(this);
			}

			mImageTargetBehaviour = GetComponent<ImageTargetBehaviour>();
			if (mImageTargetBehaviour == null)
			{
				Debug.Log ("ImageTargetBehaviour not found ");
			}
				
		}

		public void OnTrackableStateChanged(
			TrackableBehaviour.Status previousStatus,
			TrackableBehaviour.Status newStatus)
		{
			if (newStatus == TrackableBehaviour.Status.DETECTED ||
				newStatus == TrackableBehaviour.Status.TRACKED ||
				newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
			{
				Debug.Log ("FOUND:" + this.gameObject.name);
				isActive = true;
			}
			else
			{
				Debug.Log ("LOST:" + this.gameObject.name);
				isActive = false;
			}
		}   

		// Update is called once per frame
		void Update () {
			if (mImageTargetBehaviour == null)
			{
				Debug.Log ("ImageTargetBehaviour not found");
				return;
			}

			Vector2 targetSize = mImageTargetBehaviour.GetSize();
			float targetAspect = targetSize.x / targetSize.y;

			// We define a point in the target local reference 
			// we take the bottom-left corner of the target, 
			// just as an example
			// Note: the target reference plane in Unity is X-Z, 
			// while Y is the normal direction to the target plane
			Vector3 pointOnTarget = new Vector3(-0.5f, 0, -0.5f/targetAspect); 

			// We convert the local point to world coordinates
			Vector3 targetPointInWorldRef = transform.TransformPoint(pointOnTarget);

			// We project the world coordinates to screen coords (pixels)
			Vector3 screenPoint = Camera.main.WorldToScreenPoint(targetPointInWorldRef);

			world_x = targetPointInWorldRef.x;
			world_y = targetPointInWorldRef.y;
			world_z = targetPointInWorldRef.z;

			screen_x = screenPoint.x;
			screen_y = screenPoint.y;
		}
	}
}