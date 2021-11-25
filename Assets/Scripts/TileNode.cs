using UnityEngine;

public class TileNode : MonoBehaviour {

	public SpriteRenderer spriteRenderer;
	public int id { get; set; }
	public bool ready { get; set; }
	public int x { get; set; }
	public int y { get; set; }
}