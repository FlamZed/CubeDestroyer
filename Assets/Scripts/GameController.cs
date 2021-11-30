using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {
	[SerializeField] private float speed = 5.5f; // скорость движения объектов
	[SerializeField] private LayerMask layerMask; // маска узла (префаба)
	[SerializeField] private Sprite[] sprite; // набор спрайтов/id
	[SerializeField] private int gridWidth = 7; // ширина игрового поля
	[SerializeField] private int gridHeight = 10; // высота игрового поля
	[SerializeField] private TileNode sampleObject; // образец узла (префаб)
	[SerializeField] private float sampleSize = 1; // размер узла (ширина и высота)

	private TileNode[,] grid;
	private TileNode[] nodeArray;
	private Vector3[,] position;
	private TileNode current;
	private List<TileNode> lines;
	private bool isMove;

	private void Start()
	{
		// создание игрового поля (2D массив) с заданными параметрами
		grid = GridGenerator.Create2DGrid<TileNode>(sampleObject, gridWidth, gridHeight, sampleSize, transform);

		SetupField();
	}

	private void Update()
	{
		MoveNodes();

		//if (isMove) return;

		Control();
	}

	private void SetupField() // стартовые установки, подготовка игрового поля
	{
		position = new Vector3[gridWidth, gridHeight];
		nodeArray = new TileNode[gridWidth * gridHeight];

		int i = 0;
		int id = -1;
		int step = 0;

		for(int y = 0; y < gridHeight; y++)
		{
			for(int x = 0; x < gridWidth; x++)
			{
				int j = Random.Range(0, sprite.Length);
				if(id != j) id = j; else step++;
				if(step > 2)
				{
					step = 0;
					id = (id + 1 < sprite.Length-1) ? id + 1 : id - 1;
				}

				grid[x, y].x = x;
				grid[x, y].y = y;
				grid[x, y].id = id;
				grid[x, y].spriteRenderer.sprite = sprite[id];
				grid[x, y].gameObject.SetActive(true);
				position[x, y] = grid[x, y].transform.position;
				nodeArray[i] = grid[x, y];
				i++;
			}
		}

		current = null;
	}

    private void MoveNodes() // передвижение узлов и заполнение поля, после проверки совпадений
    {
        //if (!isMove) return;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, 0] == null)
                {
                    bool check = true;

                    for (int i = 0; i < gridWidth; i++)
                    {
                        if (grid[i, 0] == null)
                        {
                            grid[i, 0] = GetFree(position[i, 0]);
                        }
                    }

                    for (int i = 0; i < nodeArray.Length; i++)
                    {
                        if (!nodeArray[i].gameObject.activeSelf) check = false;
                    }

					if (check)
                    {
                        isMove = false;
                        GridUpdate();
                    }
                }

                if (grid[x, y] != null)
                {
					if (y + 1 < gridHeight && grid[x, y].gameObject.activeSelf && grid[x, y + 1] == null)
					{
						grid[x, y].transform.position = Vector3.MoveTowards(grid[x, y].transform.position, position[x, y + 1], speed * Time.deltaTime);

						if (grid[x, y].transform.position == position[x, y + 1])
						{
							grid[x, y + 1] = grid[x, y];
							grid[x, y] = null;
						}
					}
				}
            }
        }
    }

	private TileNode GetFree(Vector3 pos) // возвращает неактивный узел
	{
		for(int i = 0; i < nodeArray.Length; i++)
		{
			if(!nodeArray[i].gameObject.activeSelf)
			{
				int j = Random.Range(0, sprite.Length);
				nodeArray[i].id = j;
				nodeArray[i].spriteRenderer.sprite = sprite[j];
				nodeArray[i].transform.position = pos;
				nodeArray[i].gameObject.SetActive(true);
				return nodeArray[i];
			}
		}

		return null;
	}

	private void GridUpdate() // обновление игрового поля с помощью рейкаста
	{
		for(int y = 0; y < gridHeight; y++)
		{
			for(int x = 0; x < gridWidth; x++)
			{
				RaycastHit2D hit = Physics2D.Raycast(position[x, y], Vector2.zero, Mathf.Infinity, layerMask);

				if(hit.transform != null)
				{
					grid[x, y] = hit.transform.GetComponent<TileNode>();
					grid[x, y].x = x;
					grid[x, y].y = y;	
				}
			}
		}
	}

	private void Control() // управление ЛКМ
	{
		if(Input.GetMouseButtonDown(0))
		{
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, layerMask);
			if (hit.collider != null)
			{
				current = hit.transform.GetComponent<TileNode>();

				if (IsLine(current))
				{
					isMove = true;

					for (int i = 0; i < lines.Count; i++)
					{
						if (current.id == lines[i].id)
						{
							lines[i].gameObject.SetActive(false);
							grid[lines[i].x, lines[i].y] = null;
						}
					}

					GridUpdate();
				}
			}
		}

#if UNITY_EDITOR
		if (Input.GetMouseButtonDown(1))
        {
			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, layerMask);
			if (hit.collider != null)
			{
				current = hit.transform.GetComponent<TileNode>();
				Debug.LogError(current.x + " " + current.y + " " + current.id);
			}
        }
#endif
	}

	private bool IsLine(TileNode tile) // поиск совпадений по горизонтали и вертикали
	{
		lines = new List<TileNode>();
		TileNode currentNode = tile;
		List<TileNode> tempNode = new List<TileNode>();

		CheckNeighbor(currentNode.x, currentNode.y, currentNode.id);

		while (tempNode.Count != lines.Count)
        {
			tempNode.Add(currentNode);

			for (int i = 0; i < lines.Count; i++)
            {
                if (!tempNode.Contains(lines[i]))
                {
                    currentNode = lines[i];
                    break;
                }
            }
			CheckNeighbor(currentNode.x, currentNode.y, currentNode.id);
		}

		return (lines.Count > 2) ? true : false;
	}

	private void CheckNeighbor(int x, int y, int id)
    {
		var currentID = id;

		var rightCombination = x + 1;
		var leftCombination = x - 1;
		var topCombination = y - 1;
		var downCombination = y + 1;

		if (rightCombination < gridWidth && grid[rightCombination, y] != null && grid[rightCombination, y].id == currentID)
		{
            if (!lines.Contains(grid[rightCombination, y]))
            {
				lines.Add(grid[rightCombination, y]);
			}
		}
		if (topCombination >= 0 && grid[x, topCombination] != null && grid[x, topCombination].id == currentID)
		{
			if (!lines.Contains(grid[x, topCombination]))
			{
				lines.Add(grid[x, topCombination]);
			}
		}
		if (leftCombination >= 0 && grid[leftCombination, y] != null && grid[leftCombination, y].id == currentID)
		{
			if (!lines.Contains(grid[leftCombination, y]))
			{
				lines.Add(grid[leftCombination, y]);
			}
		}
		if (downCombination < gridHeight && grid[x, downCombination] != null && grid[x, downCombination].id == currentID)
		{
			if (!lines.Contains(grid[x, downCombination]))
			{
				lines.Add(grid[x, downCombination]);
			}
		}

		if (!lines.Contains(grid[x, y]))
			lines.Add(grid[x, y]);
	}
}