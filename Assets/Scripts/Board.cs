using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoardStatus
{
    Idle,
    Moving,
}

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject tilePrefab;
    public GameObject[] dots;
    public GameObject[,] allDots;
    public BoardStatus boardStatus = BoardStatus.Idle;
    public GameObject destroyEffect;
    // Start is called before the first frame update
    void Start()
    {
        allDots = new GameObject[width, height];
        Setup();
    }

    private void Setup()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 position = new Vector2(i, j + 3);
                int dotToUse = Random.Range(0, dots.Length);

                // Make sure the board doesn't start with a match.
                if ((i > 1 && dots[dotToUse].tag == allDots[i - 1, j]?.tag && dots[dotToUse].tag == allDots[i - 2, j]?.tag) || (j > 1 && dots[dotToUse].tag == allDots[i, j - 1]?.tag && dots[dotToUse].tag == allDots[i, j - 2]?.tag))
                {
                    dotToUse = (dotToUse + Random.Range(1, dots.Length - 1)) % dots.Length;
                }

                GameObject dot = Instantiate(dots[dotToUse], position, Quaternion.identity);
                dot.name = "(" + i + "," + j + ")";
                allDots[i, j] = dot;
                StartCoroutine(DropDotAtAfter(i, j, .1f * j));
            }
        }
    }

    void Update()
    {
        DestroyMatches();
    }

    private IEnumerator DropDotAtAfter(int x, int y, float d)
    {
        yield return new WaitForSeconds(d);

        Dot dot = allDots[x, y].GetComponent<Dot>();
        dot.column = x;
        dot.row = y;
    }

    private IEnumerator DropDotAt(int x, int y)
    {
        yield return new WaitForEndOfFrame();

        Dot dot = allDots[x, y].GetComponent<Dot>();
        dot.column = x;
        dot.row = y;
    }

    private IEnumerator DestroyDot(GameObject dot, int x, int y)
    {
        GameObject particle = Instantiate(destroyEffect, dot.transform.position, Quaternion.identity);
        yield return new WaitForSeconds(.3f);
        Destroy(dot);
        Destroy(particle, .5f);
    }

    void DestroyMatches()
    {
        int movingPieces = 0;
        for (int i = 0; i < width; i++)
        {
            int destroyedDots = 0;
            bool generateNewDots = false;

            for (int j = 0; j < height; j++)
            {
                if (generateNewDots)
                {
                    int dotToUse = Random.Range(0, dots.Length);
                    Vector2 position = new Vector2(i, j + destroyedDots);

                    GameObject newDot = Instantiate(dots[dotToUse], position, Quaternion.identity);
                    newDot.name = "(" + i + "," + j + ")";
                    allDots[i, j] = newDot;

                    StartCoroutine(DropDotAtAfter(i, j, .3f));

                    continue;
                }

                GameObject dot = allDots[i, j];
                Dot dotDot = dot.GetComponent<Dot>();

                if (dot.transform.position.x != dotDot.column || dot.transform.position.y != dotDot.row)
                {
                    movingPieces++;
                }

                if (!dotDot.isMatched && destroyedDots == 0) continue;

                if (dotDot.isMatched)
                {
                    StartCoroutine(DestroyDot(dot, i, j));
                    destroyedDots += 1;
                    allDots[i, j] = null;
                }

                else if (!dotDot.isMatched && destroyedDots > 0)
                {
                    //dotDot.row -= destroyedDots;
                    allDots[i, j - destroyedDots] = dot;
                    dot.name = "(" + i + "," + (j - destroyedDots) + ")";
                    StartCoroutine(DropDotAtAfter(i, j - destroyedDots, .3f));
                }

                if (j == height - 1 && destroyedDots > 0)
                {
                    j -= destroyedDots;
                    generateNewDots = true;
                }

            }
        }

        if (movingPieces > 0) boardStatus = BoardStatus.Moving;
        else boardStatus = BoardStatus.Idle;
    }
}
