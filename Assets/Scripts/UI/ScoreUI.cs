using UnityEngine;

namespace UI
{
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

			Vector4 scores = Scores.Instance.totalScore + new Vector4(0.001f,0.001f,0.001f,0.001f);
			float totalScores = scores.x + scores.y + scores.z + scores.w;
			int yelowScore = (int)( 10 * ( scores.x ) );
			int redScore = (int)( 10 * ( scores.y ) );
			int greenScore = (int)( 10 * ( scores.z ) );
			int blueScore = (int)( 10 * ( scores.w ) );

			GUI.DrawTexture (new Rect (40, 20, yelowScore, 30), sliderYellow);
			GUI.DrawTexture (new Rect (40, 60, redScore, 30), sliderRed);
			GUI.DrawTexture (new Rect (40, 100, greenScore, 30), sliderGreen);
			GUI.DrawTexture (new Rect (40, 140, blueScore, 30), sliderBlue);

		}
	}
}
