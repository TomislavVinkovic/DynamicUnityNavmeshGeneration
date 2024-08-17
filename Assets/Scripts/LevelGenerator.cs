using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public int width = 200;
	public int height = 200;

	public GameObject wall;

    public bool IsLevelGenerated { get; private set; } = false;

	// Use this for initialization
	void Start () {
	    GenerateLevel();
        IsLevelGenerated = true;
	}
	
	// Create a grid based level
	void GenerateLevel()
	{
		// Loop over the grid
		for (int x = 0; x <= width; x+=2)
		{
			for (int z = 0; z <= height; z+=2)
			{
				// Should we place a wall?
				if (Random.value > .8f)
				{
					// Spawn a wall
					Vector3 pos = new Vector3(x - width / 2f, 1.5f, z - height / 2f);
					Instantiate(wall, pos, Quaternion.identity, transform);
				}
			}
		}
	}
}
