using UnityEngine;
using System.Collections;

public class FindingTest : MonoBehaviour {

    [GameObject( "Directional Light" )]
    private GameObject mainLight;

    [GameObject( "Main Camera" )]
    public GameObject Camera;

    [GameObject( "Player" )]
    public GameObject Player { get { return player; } }
    private GameObject player;

    [GameObject( "Xilo", true )]
    public GameObject Xilo;

    // Use this for initialization
    void Start() {
        this.FindGameObjects();
    }

    // Update is called once per frame
    void Update() {

    }
}
