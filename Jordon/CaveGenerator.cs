using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    

    public Material viewMat;
    public Vector2 n1, n2, offset;
    Texture2D view;
    public int resolution, size, numRooms;
    public float roomSize, hallwaySize, squareFactor;
    public float smoothFactor
    {
        get { return _smoothFactor; }
        set { _smoothFactor = Mathf.Clamp(value, 0, 10); }
    }
    [SerializeField] private float _smoothFactor;
    [SerializeField] public List<Room> rooms;


    void Start()
    {
        
        view = new Texture2D(resolution, resolution);
        viewMat = transform.GetComponent<Renderer>().material;
        viewMat.mainTexture = view;
        rooms.Add(new Room(new Vector2(0,0), roomSize, smoothFactor));

        for (int i = 0; i < numRooms; i++)
        {

            Room room = new Room(new Vector2(size * 3 / 4, size * (i + 1f) / (numRooms + 1f) - size/2f), roomSize/2, smoothFactor, 3);
            room.hallways.Add(new Hallway(rooms[0], room, hallwaySize));
            rooms.Add(room);

        }
        gameObject.GetComponent<ColiderGenerator>().init();

    }

    // Update is called once per frame
    void Update()
    {
        generateMap(rooms);
    }

    public float getValue(Vector2 pos)
    {
        float value = rooms[0].getValue(pos);

        foreach (Room room in rooms)
        {
            value = smoothMin(value, room.getValue(pos), room.smoothFactor);
            foreach (Hallway hallway in room.hallways)
            {
                value = smoothMin(value, hallway.getValue(pos), room.smoothFactor);
            }
        }
        return value;
    }

    void randomizeTexture()
    {
        
        float[,] values = new float[resolution, resolution];

        Color[] colors = new Color[resolution * resolution];
        int index = 0;
        for (int i = 0; i < values.GetLength(0); i++)
        {
            for (int j = 0; j < values.GetLength(1); j++)
            {
                values[i, j] = Random.value;
                colors[index] = new Color(values[i, j], values[i, j], values[i, j]);
                index++;
            }
        }

        view.SetPixels(colors);
        view.filterMode = FilterMode.Point;
        view.Apply();

       
    }
    void generateMap(List<Room> rooms)
    {
        float[,] values = new float[resolution, resolution];

        Color[] colors = new Color[resolution * resolution];
        int index = 0;
        for (int i = 0; i < values.GetLength(0); i++)
        {
            for (int j = 0; j < values.GetLength(1); j++)
            {

                Vector2 pos = new Vector2(i, j) / resolution * size + new Vector2(transform.position.x, transform.position.y) - new Vector2(transform.localScale.x, transform.localScale.y)/2 + offset;

                float value = getValue(pos);


                values[i, j] = value > 0f ? .2f : .7f;
                colors[index] = new Color(values[i, j], values[i, j], values[i, j]);
                index++;
            }
        }

        view.SetPixels(colors);
        view.filterMode = FilterMode.Point;
        view.Apply();

    }

    public float getValue(Vector2 pos)
    {
        float value = rooms[0].getValue(pos);

        foreach (Room room in rooms)
        {
            value = smoothMin(value, room.getValue(pos), room.smoothFactor);
            foreach (Hallway hallway in room.hallways)
            {
                value = smoothMin(value, hallway.getValue(pos), room.smoothFactor);
            }
        }
        return value;
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
