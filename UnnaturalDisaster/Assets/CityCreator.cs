using UnityEngine;
using System.Collections;

public class CityCreator : MonoBehaviour {
    const float FloorHeight = 3; //m

    public Material materialSkyscraper;

    void MakeBuilding(int floors, float baseSize)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, floors * FloorHeight / 2.0f, baseSize);
        cube.transform.localScale = new Vector3(baseSize, floors * FloorHeight, baseSize);

        Material m = new Material(materialSkyscraper);
        m.SetTextureScale("_MainTex", new Vector2(3.0f * baseSize / 5.0f, floors * FloorHeight / 3.0f));
        m.color = Random.ColorHSV(0.0f, 1.0f, 0.25f, 1.0f, 0.75f, 1.0f);
        cube.GetComponent<MeshRenderer>().material = m;
    }

    void MakeBlock()
    {
        // We have an area of 200x80 we need to fill. The max height of buildings is exponential up to 500m or so, random based on distance from center.
        int floors = 10;
        float baseSize = 50.0f; //m

    }

	// Use this for initialization
	void Start () {
        MakeBlock();

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
