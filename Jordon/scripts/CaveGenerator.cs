using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    public GameObject start, end;
    public float floorLength, floorWidth;
    public float roomSpacing;
    public float cycleChance;

    public Material viewMat;
    public Vector2 n1, n2, offset;
    Texture2D view;
    public Sprite floor, wall;
    public int resolution, size, numRooms;
    public float roomSize, roomSizeVariance, hallwaySize, squareFactor;
    public float smoothFactor;
    [SerializeField] public List<Room> rooms;
    public List<Hallway> hallways;


    void Start()
    {
        System.Random random = new System.Random(0);
        
        view = new Texture2D(resolution, resolution);
        viewMat = transform.GetComponent<Renderer>().material;
        viewMat.mainTexture = view;

        int rooms_wide = (int)floorWidth / (int)roomSpacing;
        int rooms_long = (int)floorLength / (int)roomSpacing;

        Random.InitState(0);

        rooms.Add(new Room(new Vector2(0, 0), roomSize + roomSizeVariance*((float)random.NextDouble() - .5f), smoothFactor, squareFactor));
        rooms.Add(new Room(new Vector2(0, roomSpacing),roomSize + roomSizeVariance * ((float)random.NextDouble() - .5f), smoothFactor, squareFactor));
        rooms.Add(new Room(new Vector2(0, 2*roomSpacing), roomSize + roomSizeVariance * ((float)random.NextDouble() - .5f), smoothFactor));
        rooms.Add(new Room(new Vector2(roomSpacing, roomSpacing), roomSize + roomSizeVariance * ((float)random.NextDouble() - .5f), smoothFactor, squareFactor));
        rooms.Add(new Room(new Vector2(roomSpacing, 2*roomSpacing), roomSize + roomSizeVariance * ((float)random.NextDouble() - .5f), smoothFactor, squareFactor));
        rooms.Add(new Room(new Vector2(-roomSpacing, roomSpacing), roomSize + roomSizeVariance * ((float)random.NextDouble() - .5f), smoothFactor, squareFactor));

        hallways = new List<Hallway>();
        hallways.Add(new Hallway(rooms[0], rooms[1], hallwaySize));
        hallways.Add(new Hallway(rooms[1], rooms[2], hallwaySize));
        hallways.Add(new Hallway(rooms[1], rooms[3], hallwaySize));
        hallways.Add(new Hallway(rooms[3], rooms[4], hallwaySize));
        hallways.Add(new Hallway(rooms[1], rooms[5], hallwaySize));

        start.transform.position = rooms[0].position;
        end.transform.position = rooms[4].position;
       

        float[] positions = new float[40];
        float[] sizes = new float[20];
        float[] smooth = new float[20];
        float[] square = new float[20];

        

        for (int i = 0; i < rooms.Count; i++)
        {
            positions[i * 2] = rooms[i].position.x;
            positions[i * 2 + 1] = rooms[i].position.y;
            sizes[i] = rooms[i].size;
            smooth[i] = rooms[i].smoothFactor;
            square[i] = rooms[i].squareFactor;

        }

        viewMat.SetInt("_num_rooms", rooms.Count);
        viewMat.SetFloatArray("_room_positions", positions);
        viewMat.SetFloatArray("_room_sizes", sizes);
        viewMat.SetFloatArray("_room_smooth", smooth);
        viewMat.SetFloatArray("_room_square", square);

        viewMat.SetInt("_num_hallways", hallways.Count+1);

        float[] hallwayInedexes = new float[hallways.Count*2];
        float[] hallwayWidths = new float[hallways.Count];
        for (int  i = 0;  i < hallways.Count;  i++)
        {
            hallwayInedexes[2*i] = rooms.IndexOf(hallways[i].room1);
            hallwayInedexes[2*i+1] = rooms.IndexOf(hallways[i].room2);
            hallwayWidths[i] = hallways[i].width;
        }
        viewMat.SetFloatArray("_hallways", hallwayInedexes);

        viewMat.SetFloatArray("_hallways_width", new float[] { hallwaySize, hallwaySize, hallwaySize, hallwaySize, hallwaySize, hallwaySize });

        setSpritesAsTextures();




        gameObject.GetComponent<ColiderGenerator>().init();

    }

    // Update is called once per frame
    void Update()
    {
        //generateMap(rooms);
    }

    public float getValue(Vector2 pos)
    {
        float value = rooms[0].getValue(pos);

        foreach (Room room in rooms)
        {
            value = smoothMin(value, room.getValue(pos), room.smoothFactor);
            
        }
        foreach (Hallway hallway in hallways)
        {
            value = smoothMin(value, hallway.getValue(pos), 0f);
        }
        return value;
    }

    void setSpritesAsTextures()
    {
        Texture2D texture_from_sprite = new Texture2D((int)floor.rect.width, (int)floor.rect.height);
        texture_from_sprite.filterMode = FilterMode.Point;
        Color[] pixels = floor.texture.GetPixels((int)floor.textureRect.x,
                                                (int)floor.textureRect.y,
                                                (int)floor.textureRect.width,
                                                (int)floor.textureRect.height);
        texture_from_sprite.SetPixels(pixels);
        texture_from_sprite.Apply();
        viewMat.SetTexture("_MainTex", texture_from_sprite);

        texture_from_sprite = new Texture2D((int)wall.rect.width, (int)wall.rect.height);
        texture_from_sprite.filterMode = FilterMode.Point;
        pixels = floor.texture.GetPixels((int)wall.textureRect.x,
                                                (int)wall.textureRect.y,
                                                (int)wall.textureRect.width,
                                                (int)wall.textureRect.height);
        texture_from_sprite.SetPixels(pixels);
        texture_from_sprite.Apply();
        viewMat.SetTexture("_WallTex", texture_from_sprite);
    }
    float smoothMin(float a, float b, float c)
    {
        if(c == 0f)
        {
            return Mathf.Min(a, b);
        }
        float h = Mathf.Max(c - Mathf.Abs(a - b), 0f) / c;
        return Mathf.Min(a, b) - h * h * h * c * (1f / 6f);
    }
}
[System.Serializable]
public class Room
{
    public Room(Vector2 position, float size, float smoothFactor, float squareFactor = 2f)
    {
        this.size = size;
        this.position = position;
        this.smoothFactor = smoothFactor;
        this.squareFactor = squareFactor;
        hallways = new List<Hallway>();
    }
    public float size, smoothFactor, squareFactor;
    public Vector2 position;
    public List<Hallway> hallways;

    public float getValue(Vector2 coord)
    {
        Vector2 diff = coord - position;
        float value1 = Mathf.Pow(Mathf.Pow(Mathf.Abs(diff.x), squareFactor) + Mathf.Pow(Mathf.Abs(diff.y), squareFactor), 1 / squareFactor) / size - 1;
        return value1;
    }
    

}
public class Hallway
{
    public Room room1, room2;
    public Hallway(Room r1, Room r2, float width)
    {
        room1 = r1;
        room2 = r2;
        this.width = width;
    }
    
    public float width;
    public Vector2 getClosestPoint(Vector2 coordinate)
    {
        Vector2 line = room1.position - room2.position;
        Vector2 closestPoint;
        float pointOnLine = Vector2.Dot(line.normalized, coordinate - room2.position);
        pointOnLine = Mathf.Clamp(pointOnLine, 0f, line.magnitude);
        closestPoint = pointOnLine * line.normalized + room2.position;
        return closestPoint;
       
    }
    public float getValue(Vector2 coord)
    {
        Vector2 closest = getClosestPoint(coord);
        return (closest - coord).magnitude / width - 1;
    }
    
}
