using UnityEngine;

public class GridGenerator : MonoBehaviour
{
	// функция создания 2D массива на основе шаблона
	public static T[,] Create2DGrid<T>(T sample, int width, int height, float size, Transform parent) where T : Object
	{
		T[,] field = new T[width, height];

		float posX = -size * width / 2f - size / 2f;
		float posY = size * height / 2f - size / 2f;

		float Xreset = posX;

		int z = 0;

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				posX += size;
				field[x, y] = Instantiate(sample, new Vector3(posX, posY, 0), Quaternion.identity, parent) as T;
				field[x, y].name = "Tile-" + z;
				z++;
			}
			posY -= size;
			posX = Xreset;
		}

		return field;
	}
}
