// Source 
// https://forum.unity.com/threads/utility-ai-discussion.607561/#post-4072711
// 2019 01 07

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class evaluateCurves : MonoBehaviour {

	public AnimCurveEx0 AnimCurveEx0;

	public float[] curveValues;

	[Range(0.0f, 1.0f)] public float timeExample;

	public string highest;
	public float highValue;

	int i;
	int x;

	void Start () {
		curveValues = new float[AnimCurveEx0.labels.Length];
	}

	void Update () {

		//get values
		for (i = 0; i < curveValues.Length; i++) {

			curveValues [i] = AnimCurveEx0.factors [i].curve.Evaluate (timeExample);
		}

		//analyze values
		x = 0;
		for (i = 1; i < curveValues.Length; i++) {

			if (curveValues [i] > curveValues [x]) {
				x = i;
			} 

			if (i == curveValues.Length-1) {

				highest = AnimCurveEx0.factors [x].label;
				highValue = curveValues [x];
			}
			print ("analyzed");
		}

	}
}
