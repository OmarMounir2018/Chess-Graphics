using UnityEngine;
using UnityEngine.SceneManagement;

public class RageQuit : MonoBehaviour {

	// Use this for initialization
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 300, 150), "E2lb ElTarbeza"))
        {
            foreach(SquareState square in LogicMaster.currentBoard)
            {
                Destroy(square.marker);
            }
            Rigidbody boardRB = GameObject.FindGameObjectWithTag("Board").GetComponent<Rigidbody>();
            boardRB.isKinematic = false;
            boardRB.useGravity = true;
            boardRB.AddForce(new Vector3(0, 100000, 0));
            boardRB.AddTorque(new Vector3(0, 0, 1500000));
        }
        if (GUI.Button(new Rect(10, 180, 300, 150), "Men El2wl"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
		if (GUI.Button(new Rect(10, 350, 300, 150), "Salaam"))
		{
			Application.Quit ();
		}
    }
}
