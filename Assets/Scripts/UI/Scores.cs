using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scores : Singleton<Scores>
{
    public Vector4 totalScore = Vector4.zero;
    public Dictionary<int, Vector4> allScores = new Dictionary<int, Vector4>();
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine( TallyScores() );
    }

    IEnumerator TallyScores()
    {
        while( true )
        {
			yield return new WaitForEndOfFrame();
            totalScore = Vector4.zero;
            foreach (var score in allScores)
            {
                totalScore.x += score.Value.x;
                totalScore.y += score.Value.y;
                totalScore.w += score.Value.w;
                totalScore.z += score.Value.z;
            }
            yield return new WaitForSeconds (1.0f);
        }
    }
}
