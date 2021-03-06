using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Vuforia
{
	public class Controller : MonoBehaviour {
		private GameObject robot1;
		private GameObject robot2;

		private static int maxObjects = 20;

		private GameObject[] objects;
		private TargetData[] objectData;
		private string[] objectTypes = new string[maxObjects];
		private double[] objectValues = new double[maxObjects];
		private Vector3[] objectWorldPos = new Vector3[maxObjects];
		private Vector2[] objectScreenPos = new Vector2[maxObjects];
		private string[] objectNames = new string[maxObjects];
		private int voltageSourceIndex;
		private int groundIndex;



		private List<int> activeIndexes = new List<int> ();

		//private ParticleSystem[] psystems = new ParticleSystem[20];

		//private List<LineRenderer> lineRenderers = new List<LineRenderer> ();

		GameObject[] electricity = new GameObject[20];
		LineRenderer[] lineRenderers = new LineRenderer[20];

		private Vector3 p;
		private Vector3 b;

		public struct line
		{
			public line(Vector3 x, Vector3 y) {
				this.start = x;
				this.end = y;
			}
			public Vector3 start;
			public Vector3 end;
		}

		private List<line> lines = new List<line>();

		public class element
		{
			private string myName;
			private string myType;
			private double myValue;
			private Vector3 myThreeDPos;
			private Vector2 myTwoDPos;
			public double current;
			public double power;
			public double voltage;



			public element(string name, string type, double value, Vector3 ThreeDPos, Vector3 TwoDPos){
				myName = name;
				myType = type;
				myValue = value;
				myThreeDPos = ThreeDPos;
				myTwoDPos = TwoDPos;
				current = 1;
				power = 0;
				voltage = 0;
			}


			public void print(){
				Debug.Log ("Test PRINT!");
				//Debug.Log (myName + ": (" + (string)myType + "," + (string)myThreeDPos.ToString + "," + (string)myTwoDPos.ToString + ")");
			}

			public string getName(){
				return myName;
			}
			public string getType(){
				return myType;
			}
			public double getValue(){
				return myValue;
			}
			public Vector3 getThreeDPos(){
				return myThreeDPos;
			}
			public Vector2 getTwoDPos(){
				return myTwoDPos;
			}

		}

		public class ElementComparer: IComparer<element>
		{
			public int Compare(element e1, element e2)  {
				if (e1.getTwoDPos ().x > e2.getTwoDPos ().x) {
					return 1;
				} else if (e1.getTwoDPos ().x < e2.getTwoDPos ().x) {
					return -1;
				} else {
					return 0;
				}

			}
		}

		private List<element> elements = new List<element>();


		float Distance(float x1, float y1, float z1, float x2, float y2, float z2)
		{
			return (Mathf.Sqrt (Mathf.Pow (x2 - x1, 2) + Mathf.Pow (y2 - y1, 2) + Mathf.Pow (z2 - z1, 2)));
		}

		void straight(element e1,element e2) 
		{
			lines.Add(new line(e1.getThreeDPos(),e2.getThreeDPos()));
		}

		void elbow(element e1,element e2)
		{
			Debug.Log ("ELBOWING");
			Vector3 b = e2.getThreeDPos() - e1.getThreeDPos();
			Vector3 midbottom = e1.getThreeDPos() + Vector3.Project(0.5F * b,p);
			Vector3 midtop = midbottom + b - Vector3.Project(b,p);
			lines.Add(new line(e1.getThreeDPos(), midbottom));
			lines.Add(new line(midbottom, midtop));
			lines.Add(new line(midtop, e2.getThreeDPos()));
		}

		void branch(element e1,element e2,element e3)
		{
			Debug.Log ("BRANCHING");
			Vector3 b2 = e2.getThreeDPos() - e1.getThreeDPos();
			Vector3 b3 = e3.getThreeDPos() - e1.getThreeDPos();
			Vector3 midbottom2 = e1.getThreeDPos() + Vector3.Project(0.5F * b2, p);
			Vector3 midbottom3 = e1.getThreeDPos() + Vector3.Project(0.5F * b3, p);
			Vector3 midbottom;
			if ((midbottom2 - e1.getThreeDPos()).magnitude < (midbottom3 - e1.getThreeDPos()).magnitude) {
				midbottom = midbottom2;
			} else {
				midbottom = midbottom3;
			}
			Vector3 midtop2 = midbottom + b2 - Vector3.Project(b2,p);
			Vector3 midtop3 = midbottom + b3 - Vector3.Project(b3,p);
			lines.Add(new line(e1.getThreeDPos(), midbottom));
			lines.Add(new line(midbottom, midtop2));
			lines.Add(new line(midbottom, midtop3));
			lines.Add(new line(midtop2, e2.getThreeDPos()));
			lines.Add(new line(midtop3, e3.getThreeDPos()));
		}

		void rev_branch(element e2,element e3,element e1)
		{
			Vector3 b2 = e2.getThreeDPos() - e1.getThreeDPos();
			Vector3 b3 = e3.getThreeDPos() - e1.getThreeDPos();
			Vector3 midbottom2 = e1.getThreeDPos() + Vector3.Project(0.5F * b2, p);
			Vector3 midbottom3 = e1.getThreeDPos() + Vector3.Project(0.5F * b3, p);
			Vector3 midbottom;
			if ((midbottom2 - e1.getThreeDPos()).magnitude < (midbottom3 - e1.getThreeDPos()).magnitude) {
				midbottom = midbottom2;
			} else {
				midbottom = midbottom3;
			}
			Vector3 midtop2 = midbottom + b2 - Vector3.Project(b2,p);
			Vector3 midtop3 = midbottom + b3 - Vector3.Project(b3,p);
			lines.Add(new line(midbottom, e1.getThreeDPos()));
			lines.Add(new line(midtop2, midbottom));
			lines.Add(new line(midtop3, midbottom));
			lines.Add(new line(e2.getThreeDPos(), midtop2));
			lines.Add(new line(e3.getThreeDPos(), midtop3));
		}

		void lines_helper(List<element> ele)
		{
			for (int i = 0; i < ele.Count - 1; i++) 
			{
				straight (ele[i], ele [i + 1]);
			}
		}

		void determine_lines ()
		{
			lines.Clear();

			// exit if sorted list doesn't begin with source and ends with ground
			if (elements.Count == 0 || !(elements [0].getType () == "VoltageSource" && elements [elements.Count - 1].getType () == "Ground")) { 
				Debug.Log ("No valid Circuit!");
				return;
			}

			double epsilon = 4.0;
			int i = 0;

			// helps keep track of parallel circuits later on
			List<element> ypos = new List<element>();
			List<element> yneg = new List<element>();

			// setup for circuit solving calculuations
			double r_eq = 0;
			foreach (element e in elements) {
				e.current = 1;
			}

			while (i < elements.Count - 1) {
				// add resistances of resistors in series
				if (elements [i].getType () == "Resistor")
					r_eq += elements [i].getValue ();
				// assuming abs(elements[i].getTwoDPos().y) <= 3
				if (Mathf.Abs(elements[i + 1].getTwoDPos().y) <= epsilon) 
				{
					straight(elements[i],elements[i+1]);
					i += 1;
				} 
				else 
				{
					int begin = i;
					ypos.Clear();
					yneg.Clear();
					i += 1;
					while (Mathf.Abs(elements[i].getTwoDPos().y) > epsilon) {
						if (elements[i].getTwoDPos().y >= 0) {
							ypos.Add(elements[i]);
						} else {
							yneg.Add(elements[i]);
						}
						i += 1;
					}
					lines_helper(ypos);
					lines_helper(yneg);
					if (ypos.Count > 0 && yneg.Count > 0) {
						branch (elements [begin], ypos [0], yneg [0]);
						rev_branch(ypos[ypos.Count - 1], yneg[yneg.Count - 1], elements[i]);
						double ypos_r_eq = 0.0;
						foreach (element e in ypos)
							ypos_r_eq += e.getValue ();
						double yneg_r_eq = 0.0;
						foreach (element e in yneg)
							yneg_r_eq += e.getValue ();
						r_eq += 1 / ((1 / ypos_r_eq) + (1 / yneg_r_eq));
						foreach (element e in ypos)
							e.current = yneg_r_eq / (ypos_r_eq + yneg_r_eq);
						foreach (element e in yneg)
							e.current = ypos_r_eq / (ypos_r_eq + yneg_r_eq);
					} else if (ypos.Count > 0 && yneg.Count == 0) {
						elbow (ypos [ypos.Count - 1], elements [i]);
						elbow (elements [begin], ypos [0]);
						foreach (element e in ypos)
							r_eq += e.getValue ();
					} else {
						elbow (elements [begin], yneg [0]);
						elbow (yneg [yneg.Count - 1], elements [i]);
						foreach (element e in yneg)
							r_eq += e.getValue ();
					}
				}	

			}
			if (r_eq != 0) {
				double circuit_current = elements [0].getValue () / r_eq;
				foreach (element e in elements) {
					e.current *= circuit_current;
					e.power = e.current * e.current * e.getValue ();
					e.voltage = e.current * e.getValue();
				}
				Debug.Log ("r_eq: " + r_eq);
				Debug.Log ("circuit current: " + circuit_current);
			}
		}



		void Awake () {
			for (int i = 0; i < lineRenderers.Length; i++) {
				lineRenderers [i] = new GameObject ("LineRenderer").AddComponent<LineRenderer> () as LineRenderer;
			}

		}
		// Use this for initialization

		void Start () {
			CameraDevice.Instance.SetFocusMode (CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
			//Create an array of all of the Vuforia Objects
			objects = GameObject.FindGameObjectsWithTag ("VuforiaObject"); //A tag that I defined

			objectData = new TargetData[objects.Length]; //Allocate the right amount of memory of objectData
			//Loop through that array and assign all of the targetData
			for (int i = 0; i < objects.Length; i++){
				objectData [i] = objects [i].GetComponent<TargetData> ();
				objectNames [i] = objects [i].gameObject.name;
				objectTypes [i] = objectData [i].type;
				objectValues [i] = objectData [i].value;
			}
			/*for (int i = 0; i < psystems.Length; i++) {
				psystems [i] = gameObject.AddComponent <ParticleSystem>() as ParticleSystem;
			}*/
			for (int i = 0; i < electricity.Length; i++) {
				electricity [i] = Instantiate (UnityEngine.GameObject.Find ("electricityMaster"));
			}

		}

		void OnPaused(bool paused){
			if (!paused) {
				CameraDevice.Instance.SetFocusMode (CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
			}
		}



		// Update is called once per frame
		void Update () {
			for (int i = 0; i < objectData.Length; i++) {
				if (objectData [i].isActive) {
					if (!(activeIndexes.Contains(i))){
						activeIndexes.Add (i);
					}
				}
				else{
					if ((activeIndexes.Contains (i))) {
						activeIndexes.Remove (i);
					}
				}
			}

			//Default both indexes to -1, or nonexistant
			voltageSourceIndex = -1;
			groundIndex = -1;
			for (int i = 0; i < activeIndexes.Count; i++) {
				if (objectTypes [activeIndexes [i]] == "VoltageSource") {
					voltageSourceIndex = activeIndexes [i];
				} else if (objectTypes [activeIndexes [i]] == "Ground") {
					groundIndex = activeIndexes [i];
				}

				objectWorldPos[activeIndexes[i]] = new Vector3(objectData[activeIndexes[i]].world_x,objectData[activeIndexes[i]].world_y,objectData[activeIndexes[i]].world_z);
				objectScreenPos[activeIndexes[i]] = new Vector2(objectData[activeIndexes[i]].screen_x, objectData [activeIndexes[i]].screen_y);
			}

			elements.Clear (); //empty out the elements array

			if ((voltageSourceIndex >= 0) && (groundIndex >= 0)) {


				//lineRenderer.enabled = true;
				//WE HAVE A VALID CIRCUIT

				//Draw a line between the voltage source and ground
				//lineRenderer.SetPosition (0, objects [voltageSourceIndex].transform.position);
				//lineRenderer.SetPosition (1, objects [groundIndex].transform.position);

				//Calculate "path" vector
				p = (objectWorldPos [groundIndex] - objectWorldPos [voltageSourceIndex]);

				element vs = new element (objectNames [voltageSourceIndex], objectTypes [voltageSourceIndex],
					objectValues [voltageSourceIndex], objects [voltageSourceIndex].transform.position, Vector2.zero);
				element gnd = new element (objectNames [groundIndex], objectTypes [groundIndex],
					objectValues [groundIndex], objects [groundIndex].transform.position, new Vector2(p.magnitude,0));

				elements.Add (vs);
				elements.Add (gnd);

				Vector3 posCross = Vector3.zero;
				double x = -1;
				double y = -1;

				for (int i = 0; i < activeIndexes.Count; i++) {
					if (objectTypes[activeIndexes[i]] == "Resistor") {
						//If we find a resistor object
						//Compute the vector from the source to the resistor
						Vector3 b = objects[activeIndexes[i]].transform.position - objects[voltageSourceIndex].transform.position;
						Vector3 pCrossb = Vector3.Cross (p, b);

						double pDotb = Vector3.Dot(p,b);
						if (pCrossb != Vector3.zero && posCross == Vector3.zero)
						{
							posCross = pCrossb.normalized;
						}
						x = pDotb/p.magnitude;
						y = pCrossb.magnitude/p.magnitude;
						if (posCross != Vector3.zero && Vector3.Dot(pCrossb,posCross)<0)
						{
							y *= -1;
						}
						element r = new element (objectNames [activeIndexes [i]], objectTypes [activeIndexes [i]], objectValues [activeIndexes [i]],
							objects [activeIndexes [i]].transform.position, new Vector2 ((float)x, (float)y));
						elements.Add (r);
					}
				}

			} else {
				//lineRenderer.enabled = false;
			}

			//Elements list is all set up and ready to be used!
			//Comparer elementComparer = new Comparer

			ElementComparer elementComparer = new ElementComparer ();
			elements.Sort (elementComparer); //Sort all the elements based on their x value

			determine_lines ();

			for (int i = 0; i < activeIndexes.Count; i++) {
				if (objectTypes [activeIndexes [i]] == "Resistor") {
					GameObject obj = GameObject.Find (objectNames [activeIndexes [i]]);
					obj.GetComponentInChildren<TextMesh> ().text = "";
				}
			}

			for (int i = 0; i < elements.Count; i++) {
				if (elements[i].getType() == "Resistor") {
					GameObject obj = GameObject.Find (elements [i].getName ());
					obj.GetComponentInChildren<TextMesh> ().text = Math.Round(elements [i].current,2).ToString() + "A\n" + Math.Round(elements[i].voltage,2) + "V";
				}

			}

			for(int i = 0; i < lineRenderers.Length; i++) {
				lineRenderers[i].enabled = false;
			}

			for (int i = 0; i < lines.Count; i++){
				Vector3 start = lines[i].start;
				Vector3 end = lines[i].end;
				//lineRenderers [i].SetPosition (0, start);
				//lineRenderers [i].SetPosition (1, end);

				lineRenderers [i].enabled = true;
			}

			for (int i = 0; i < electricity.Length; i++) {
				electricity [i].transform.position = Vector3.zero;
			}

			for (int i = 0; i < lines.Count; i++) {
				electricity[i].transform.position = lines [i].start;
				electricity [i].transform.rotation = Quaternion.FromToRotation (Vector3.forward, lines [i].end - lines [i].start);
				ParticleSystem ps = electricity [i].GetComponent<ParticleSystem>();

				ps.startLifetime = (lines [i].end - lines [i].start).magnitude / ps.startSpeed;

			}
				

			Debug.Log ("# of lines: " + lines.Count);

		}

		void OnGUI(){
			GUIStyle debugStyle = new GUIStyle (GUI.skin.GetStyle ("label"));
			debugStyle.fontSize = 48;
			string guiString = "ResistARv1";


			GUILayout.Label (guiString,debugStyle);
		}
	}
}
