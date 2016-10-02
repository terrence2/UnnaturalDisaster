using UnityEngine;
using System;
using Random = UnityEngine.Random;


class BuildingCreator
{
    private String name_;
    private float minHeight_;
    private float minBase_;
    private float ratio;
    private Material mat;

    public BuildingCreator(String name, float floorHeight, int minFloors, float minBase, float ratio_, Material material)
    {
        name_ = name;
        minHeight_ = minFloors * floorHeight;
        minBase_ = minBase;
        ratio = ratio_;
        mat = material;
    }

    public Material material()
    {
        return mat;
    }

    public float minHeight()
    {
        return minHeight_;
    }

    public float minBase()
    {
        return minBase_;
    }

    public float heightForBaseSize(float baseSize)
    {
        return Mathf.Max(minHeight_, (baseSize - minBase_) / ratio);
    }

    public float baseSizeForHeight(float height)
    {
        return (height * ratio) + minBase_;
    }
}

public class CityCreator : MonoBehaviour {

    const float FloorHeight = 3; //m
    const float MinHeight = 2 * FloorHeight;

    public Material materialSkyscraper;
    public Material materialAppartment;
    public Material materialFactory;
    public Material materialHouse;

    public Material materialIntersection;
    public Material materialStreet;

    public float minSkyscraperBase = 20f;
    public float minBuildingSpacing = 1f;
    public float maxBuildingSpacing = 5f;
    public float blockWidth = 100f;
    public float blockLength = 200f;
    public float streetSize = 12f;

    BuildingCreator skyscraperCreator;
    BuildingCreator appartmentCreator;
    BuildingCreator factoryCreator;
    BuildingCreator houseCreator;

    void MakeBuilding(float x, float y, float h, float s, BuildingCreator creator)
    {
        if (h < creator.minHeight())
            Debug.Log("Under min height!");

        h = Mathf.Max(FloorHeight, h);

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(x + s / 2, h / 2.0f, y + s / 2);
        cube.transform.localScale = new Vector3(s, h, s);

        Material m = new Material(creator.material());
        m.SetTextureScale("_MainTex", new Vector2(3f * s / 5f, h / 3f));
        m.color = Random.ColorHSV(0.0f, 1.0f, 0.25f, 1.0f, 0.75f, 1.0f);
        cube.GetComponent<MeshRenderer>().material = m;
    }

    void PackBuildingsIntoArea(float x, float y, float w, float h, int level)
    {
        // Set a maximum limit on how much time we can take in generation.
        if (level > 7)
            return;

        // At a certain point we can't put in any usefully sized buildings.
        float maxBase = Mathf.Min(w, h);
        if (maxBase < houseCreator.minBase())
            return;

        // The size of our building is dependent on the distance from the city center. This is
        // both a minimum and maximum height.
        float d = Mathf.Sqrt(x * x + y * y);
        float maxHeight0 = 160f * (7f - Mathf.Log(d + 100f));
        float maxHeight = Mathf.Max(MinHeight, maxHeight0);
        float minHeight = Mathf.Max(MinHeight, maxHeight / 2.0f);
        float height = Random.Range(minHeight, maxHeight);

        // Our size range determines what sort of buildings we could make with this space.
        BuildingCreator creator = houseCreator;
        if (height > skyscraperCreator.minHeight())
        {
            creator = skyscraperCreator;
        }
        else if (height > appartmentCreator.minHeight())
        {
            creator = appartmentCreator;
        }
        else if (height > factoryCreator.minHeight())
        {
            creator = factoryCreator;
        }

        // Different building kinds have different squatnesses; use the building kind to find a base size.
        float baseSize = creator.baseSizeForHeight(height);

        // If the random size we pick won't actually fit because of the base size, we switch to a second
        // random algorithm that picks based on the available base size. We don't want to just clip to the
        // available base size, however, because this will lead to mostly max size buildings as we
        // recurse into a block.
        if (baseSize > maxBase)
        {
            // Get a new random base size.
            baseSize = Random.Range(Mathf.Max(5f, maxBase / 2f), maxBase);

            if (baseSize > factoryCreator.minBase())
                creator = factoryCreator;
            else if (baseSize > appartmentCreator.minBase())
                creator = appartmentCreator;
            else if (baseSize > skyscraperCreator.minBase())
                creator = skyscraperCreator;
            else
                creator = houseCreator;

            // Go the other direction to get a height from our base size.
            height = creator.heightForBaseSize(baseSize);
        }

        // Figure out how much room to leave between adjacent buildings here.
        float interspace = Random.Range(minBuildingSpacing, maxBuildingSpacing);

        // Now that we know what the building's size is, figure out what span to the next building so that we
        // can recurse while leaving random spacing between buildings.
        float s = baseSize + interspace;
        Vector4 area1;
        Vector4 area2;

        // Place at a random corner.
        int corner = (int)Mathf.Round(Random.Range(-0.49f, 3.49f));
        switch (corner)
        {
            case 0:
                MakeBuilding(x, y, height, baseSize, creator);
                if (h - baseSize > w - baseSize)
                {
                    area1 = new Vector4(x, y + s, w, h - s);
                    area2 = new Vector4(x + s, y, w - s, s);
                } else
                {
                    area1 = new Vector4(x + s, y, w - s, h);
                    area2 = new Vector4(x, y + s, s, h - s);
                }
                break;
            case 1:
                MakeBuilding(x + w - baseSize, y, height, baseSize, creator);
                if (h - baseSize > w - baseSize)
                {
                    area1 = new Vector4(x, y + s, w, h - s);
                    area2 = new Vector4(x, y, w - s, s);
                } else
                {
                    area1 = new Vector4(x, y, w - s, h);
                    area2 = new Vector4(x + w - s, y + s, s, h - s);
                }
                break;
            case 2:
                MakeBuilding(x, y + h - baseSize, height, baseSize, creator);
                if (h - baseSize > w - baseSize)
                {
                    area1 = new Vector4(x, y, w, h - s);
                    area2 = new Vector4(x + s, y + h - s, w - s, s);
                } else
                {
                    area1 = new Vector4(x + s, y, w - s, h);
                    area2 = new Vector4(x, y, s, h - s);
                }
                break;
            case 3:
                MakeBuilding(x + w - baseSize, y + h - baseSize, height, baseSize, creator);
                if (h - baseSize > w - baseSize)
                {
                    area1 = new Vector4(x, y, w, h - s);
                    area2 = new Vector4(x, y + h - s, w - s, s);
                } else
                {
                    area1 = new Vector4(x, y, w - s, h);
                    area2 = new Vector4(x + w - s, y, s, h - s);
                }
                break;
            default:
                area1 = new Vector4();
                area2 = new Vector4();
                break;
        }

        // Fill the holes we left with buildings.
        PackBuildingsIntoArea(area1[0], area1[1], area1[2], area1[3], level + 1);
        PackBuildingsIntoArea(area2[0], area2[1], area2[2], area2[3], level + 1);
    }

    void MakeIntersection(float x, float y)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(x, 0.05f, y);
        cube.transform.localScale = new Vector3(streetSize, 0.1f, streetSize);

        Material m = new Material(materialIntersection);
        cube.GetComponent<MeshRenderer>().material = m;
    }

    void MakeStreetNS(float x, float y, float len)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(x + streetSize / 2f, 0.05f, y + len / 2f);
        cube.transform.localScale = new Vector3(streetSize, 0.1f, len);

        Material m = new Material(materialStreet);
        cube.GetComponent<MeshRenderer>().material = m;
    }

    void MakeStreetEW(float x, float y, float len)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(x + len / 2f, 0.05f, y + streetSize / 2f);
        cube.transform.localScale = new Vector3(streetSize, 0.1f, len);
        cube.transform.Rotate(Vector3.up, 90f);

        Material m = new Material(materialStreet);
        cube.GetComponent<MeshRenderer>().material = m;
    }

    void MakeBlock(float x, float y)
    {
        // Insert street corner into corner and lanes at top and left sides.
        MakeIntersection(x + streetSize / 2f, y + streetSize / 2f);
        MakeStreetNS(x, y + streetSize, blockLength - streetSize);
        MakeStreetEW(x + streetSize, y, blockWidth - streetSize);

        // Put skyscrapers into the rest of the available area.
        PackBuildingsIntoArea(x + streetSize, y + streetSize, blockWidth - streetSize, blockLength - streetSize, 0);
    }

	// Use this for initialization
	void Start () {
        skyscraperCreator = new BuildingCreator("Skyscraper", 3f, 20, 20f, 1f / 7.5f, materialSkyscraper);
        appartmentCreator = new BuildingCreator("Appartment", 3.5f, 10, 30f, 4f / 50f, materialAppartment);
        factoryCreator = new BuildingCreator("Factory", 4f, 2, 40f, 4f / 80f, materialFactory);
        houseCreator = new BuildingCreator("House", 3f, 1, 5f, 4f / 8f, materialHouse);

        float radius = 1000f;
        float nblocks = Mathf.Floor((radius * 2f) / blockWidth);
        float n = nblocks / 2f;
        for (float offsetX = -(blockWidth * n); offsetX < blockWidth * n; offsetX += blockWidth) {
            for (float offsetY = -(blockLength * n); offsetY < blockLength * n; offsetY += blockLength)
            {
                float r = Mathf.Sqrt(offsetX * offsetX + offsetY * offsetY);
                if (r > radius)
                    continue;
                MakeBlock(offsetX, offsetY);
            }
        }

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
