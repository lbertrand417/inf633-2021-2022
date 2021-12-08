using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws a basic oscilloscope type graph in a GUI.Window()
/// Michael Hutton May 2020
/// This is just a basic 'as is' do as you wish...
/// Let me know if you use it as I'd be interested if people find it useful.
/// I'm going to keep experimenting wih the GL calls...eg GL.LINES etc 
/// </summary>
public class Graph : MonoBehaviour
{

    Material mat;
    private Rect windowRect = new Rect(20, 20, 512, 256);

    // A list of random values to draw
    private List<float> values;
    private List<float> predValues;
    private List<float> speedValues;

    // The list the drawing function uses...
    private List<float> drawValues = new List<float>();
    private List<float> predDrawValues = new List<float>();
    private List<float> speedDrawValues = new List<float>();

    // List of Windows
    private bool showWindow0 = false;

    // Genetic alg.
    private GeneticAlgo genetic_algo = null;
    private int counter = 0;
    // Start is called before the first frame update
    void Start()
    {
        genetic_algo = GetComponent<GeneticAlgo>();
        mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        // Should check for material but I'll leave that to you..

        // Fill a list with ten random values
        values = new List<float>();
        predValues = new List<float>();
        speedValues = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        counter++;
        if (counter == 10)
        {
            values.Add(genetic_algo.getAnimalCount()/3f);
            speedValues.Add(genetic_algo.getAverageSpeed()*300f);
            predValues.Add(genetic_algo.getPredatorCount()/ 3f);
            counter = 0;
        }
    }

    private void OnGUI()
    {
        // Create a GUI.toggle to show graph window
        showWindow0 = GUI.Toggle(new Rect(10, 10, 100, 20), showWindow0, "Show Graph");

        if (showWindow0)
        {
            // Set out drawValue list equal to the values list 
            drawValues = values;
            predDrawValues = predValues;
            speedDrawValues = speedValues;
            windowRect = GUI.Window(0, windowRect, DrawGraph, "Population Evolution");
        }

    }


    void DrawGraph(int windowID)
    {
        // Make Window Draggable
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));

        // Draw the graph in the repaint cycle
        if (Event.current.type == EventType.Repaint)
        {
            GL.PushMatrix();

            GL.Clear(true, false, Color.black);
            mat.SetPass(0);

            // Draw a black back ground Quad 
            GL.Begin(GL.QUADS);
            GL.Color(Color.grey);
            GL.Vertex3(4, 4, 0);
            GL.Vertex3(windowRect.width - 4, 4, 0);
            GL.Vertex3(windowRect.width - 4, windowRect.height - 4, 0);
            GL.Vertex3(4, windowRect.height - 4, 0);
            GL.End();

            // Draw the lines of the graph
            GL.Begin(GL.LINES);
            GL.Color(Color.white);

            int valueIndex = drawValues.Count - 1;
            for (int i = (int)windowRect.width - 4; i > 3; i--)
            {
                float y1 = 0;
                float y2 = 0;
                if (valueIndex > 0)
                {
                    y2 = drawValues[valueIndex];
                    y1 = drawValues[valueIndex - 1];
                }
                GL.Vertex3(i, windowRect.height - 4 - y2, 0);
                GL.Vertex3((i - 1), windowRect.height - 4 - y1, 0);
                valueIndex -= 1;
            }
            GL.End();

            //Predators
            GL.Begin(GL.LINES);
            GL.Color(Color.red);

            valueIndex = predDrawValues.Count - 1;
            for (int i = (int)windowRect.width - 4; i > 3; i--)
            {
                float y1 = 0;
                float y2 = 0;
                if (valueIndex > 0)
                {
                    y2 = predDrawValues[valueIndex];
                    y1 = predDrawValues[valueIndex - 1];
                }
                GL.Vertex3(i, windowRect.height - 4 - y2, 0);
                GL.Vertex3((i - 1), windowRect.height - 4 - y1, 0);
                valueIndex -= 1;
            }
            GL.End();

            // Draw a black back ground Quad 
            GL.Begin(GL.QUADS);
            GL.Color(Color.grey);
            GL.Vertex3(4, windowRect.height + 4, 0);
            GL.Vertex3(windowRect.width - 4, windowRect.height + 4, 0);
            GL.Vertex3( windowRect.width - 4, windowRect.height + windowRect.height - 4, 0);
            GL.Vertex3( 4, windowRect.height + windowRect.height - 4, 0);
            GL.End();

            // Draw the lines of the graph
            GL.Begin(GL.LINES);
            GL.Color(Color.blue);

            valueIndex = speedDrawValues.Count - 1;
            for (int i = (int)windowRect.width - 4; i > 3; i--)
            {
                float y1 = 0;
                float y2 = 0;
                if (valueIndex > 0)
                {
                    y2 = speedDrawValues[valueIndex];
                    y1 = speedDrawValues[valueIndex - 1];
                }
                GL.Vertex3(i, windowRect.height + windowRect.height - 4 - y2, 0);
                GL.Vertex3((i - 1), windowRect.height + windowRect.height - 4 - y1, 0);
                valueIndex -= 1;
            }
            GL.End();

            GL.PopMatrix();
        }
    }
}