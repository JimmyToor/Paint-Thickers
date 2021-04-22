using UnityEngine;
using System.Collections;

public class ScoreUI : MonoBehaviour {

	private Texture2D sliderYellow;
	private Texture2D sliderRed;
	private Texture2D sliderGreen;
	private Texture2D sliderBlue;
	void Start () {
		sliderYellow = Resources.Load<Texture2D>("sliderYellow");
		sliderRed = Resources.Load<Texture2D>("sliderRed");
		sliderGreen = Resources.Load<Texture2D>("sliderGreen");
		sliderBlue = Resources.Load<Texture2D>("sliderBlue");
	}

	void OnGUI () {

		Vector4 scores = Scores.instance.totalScore + new Vector4(0.001f,0.001f,0.001f,0.001f);
		float totalScores = scores.x + scores.y + scores.z + scores.w;
		int yelowScore = (int)( 512 * ( scores.x / totalScores ) );
		int redScore = (int)( 512 * ( scores.y / totalScores ) );
		int greenScore = (int)( 512 * ( scores.z / totalScores ) );
		int blueScore = (int)( 512 * ( scores.w / totalScores ) );

		GUI.DrawTexture (new Rect (40, 20, yelowScore, 30), sliderYellow);
		GUI.DrawTexture (new Rect (40, 60, redScore, 30), sliderRed);
		GUI.DrawTexture (new Rect (40, 100, greenScore, 30), sliderGreen);
		GUI.DrawTexture (new Rect (40, 140, blueScore, 30), sliderBlue);

	}
}
