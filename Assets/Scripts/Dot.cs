using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Board variables")]
    public int column;
    public int row;
    public int targetX;
    public int targetY;
    public bool isMatched = false;
    private Vector2 firstTouch;
    private Vector2 finalTouch;
    private enum SwipeDirections
    {
        UP, DOWN, LEFT, RIGHT
    }
    private SwipeDirections swipeDirection;
    private Board board;
    private GameObject otherDot;
    private Vector2 tempPosition;
    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
        targetX = (int)transform.position.x;
        targetY = (int)transform.position.y;
        column = targetX;
        row = targetY;
    }

    // Update is called once per frame
    void Update()
    {
        targetX = column;
        targetY = row;

        if (Mathf.Abs(targetX - transform.position.x) > .1)
        {
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .5f);
        }
        else if (Mathf.Abs(targetX - transform.position.x) > 0)
        {
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = tempPosition;
            board.allDots[column, row] = this.gameObject;
            if (otherDot)
            {
                Dot otherDotDot = otherDot.GetComponent<Dot>();
                board.allDots[otherDotDot.column, otherDotDot.row] = otherDot;
                FindMatches();
                otherDotDot.FindMatches();

                if (!isMatched && !otherDotDot.isMatched)
                {
                    StartCoroutine(RestorePosition());
                }
                else
                {
                    string newName = otherDot.name;
                    otherDot.name = name;
                    name = newName;
                }
                otherDot = null;


            }
        }

        if (Mathf.Abs(targetY - transform.position.y) > .1)
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .5f);
        }
        else if (Mathf.Abs(targetY - transform.position.y) > 0)
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
            board.allDots[column, row] = this.gameObject;
            if (otherDot)
            {
                Dot otherDotDot = otherDot.GetComponent<Dot>();
                board.allDots[otherDotDot.column, otherDotDot.row] = otherDot;
                FindMatches();
                otherDotDot.FindMatches();

                if (!isMatched && !otherDotDot.isMatched)
                {

                    StartCoroutine(RestorePosition());

                }

                otherDot = null;


            }
            else
            {
                FindMatches();
            }
        }
    }

    private IEnumerator RestorePosition()
    {
        Dot otherDotDot = otherDot.GetComponent<Dot>();
        yield return new WaitForSeconds(.1f);
        int tempCol = column;
        int tempRow = row;
        column = otherDotDot.column;
        row = otherDotDot.row;
        otherDotDot.column = tempCol;
        otherDotDot.row = tempRow;
    }

    private void OnMouseDown()
    {
        if (board.boardStatus == BoardStatus.Moving) return;
        firstTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (firstTouch == null || board.boardStatus == BoardStatus.Moving) return;
        finalTouch = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Math.Sqrt(MathF.Pow(finalTouch.y - firstTouch.y, 2) + MathF.Pow(finalTouch.x - firstTouch.x, 2)) > .1f)
        {
            DetectSwipeDirection();
            MovePieces();
        }
    }

    void DetectSwipeDirection()
    {
        float swipeAngle = Mathf.Atan2(finalTouch.y - firstTouch.y, finalTouch.x - firstTouch.x) * 180 / Mathf.PI;
        if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            swipeDirection = SwipeDirections.UP;
            return;
        }
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            swipeDirection = SwipeDirections.RIGHT;
            return;
        }
        if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            swipeDirection = SwipeDirections.LEFT;
            return;
        }
        if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            swipeDirection = SwipeDirections.DOWN;
            return;
        }
    }

    void MovePieces()
    {
        if (swipeDirection == SwipeDirections.UP)
        {
            // Up swipe
            otherDot = board.allDots[column, row + 1];
            otherDot.GetComponent<Dot>().row -= 1;
            row += 1;
        }

        else if (swipeDirection == SwipeDirections.RIGHT)
        {
            // Right Swipe
            otherDot = board.allDots[column + 1, row];
            otherDot.GetComponent<Dot>().column -= 1;
            column += 1;
        }

        else if (swipeDirection == SwipeDirections.LEFT)
        {
            // Left Swipe
            otherDot = board.allDots[column - 1, row];
            otherDot.GetComponent<Dot>().column += 1;
            column -= 1;
        }

        else if (swipeDirection == SwipeDirections.DOWN)
        {
            // Down Swipe
            otherDot = board.allDots[column, row - 1];
            otherDot.GetComponent<Dot>().row += 1;
            row -= 1;
        }
    }


    void FindMatchAt(int x, int y, bool vertical = false)
    {
        Dot centerDot = board.allDots[x, y].GetComponent<Dot>();
        if (!vertical)
        {
            if (x > 0 && x < board.width - 1)
            {

                Dot leftDot = board.allDots[x - 1, y].GetComponent<Dot>();
                Dot rightDot = board.allDots[x + 1, y].GetComponent<Dot>();

                if (leftDot.tag == centerDot.tag && rightDot.tag == centerDot.tag)
                {
                    centerDot.isMatched = true;
                    leftDot.isMatched = true;
                    rightDot.isMatched = true;
                }
            }

            return;
        }

        if (y > 0 && y < board.height - 1)
        {
            Dot downDot1 = board.allDots[column, y - 1].GetComponent<Dot>();
            Dot upDot1 = board.allDots[column, y + 1].GetComponent<Dot>();

            if (upDot1.tag == centerDot.tag && downDot1.tag == centerDot.tag)
            {
                centerDot.isMatched = true;
                upDot1.isMatched = true;
                downDot1.isMatched = true;
            }
        }

    }

    public void FindMatches()
    {
        FindMatchAt(column, row, false);
        if (column > 0)
            FindMatchAt(column - 1, row, false);
        if (column < board.width - 1)
            FindMatchAt(column + 1, row, false);

        FindMatchAt(column, row, true);
        if (row > 0)
            FindMatchAt(column, row - 1, true);
        if (row < board.height - 1)
            FindMatchAt(column, row + 1, true);
    }

}
