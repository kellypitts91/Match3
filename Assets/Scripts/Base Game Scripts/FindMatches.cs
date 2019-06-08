using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour
{
    private Board board;
    public List<GameObject> currentMatches = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start() {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches() {
        StartCoroutine(FindAllMatchesCo());
    }

    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2, Dot dot3) {
        List<GameObject> currentDots = new List<GameObject>();
        if(dot1.isRowBomb) {
            currentDots.Union(GetRowPieces(dot1.row));
        }
        if(dot2.isRowBomb) {
            currentDots.Union(GetRowPieces(dot2.row));
        }
        if(dot3.isRowBomb) {
            currentDots.Union(GetRowPieces(dot3.row));
        }
        // UnionRowPieces(dot1);
        // UnionRowPieces(dot2);
        // UnionRowPieces(dot3);
        return currentDots;
    }

    private void UnionRowPieces(Dot dot) {
        if(dot.isRowBomb) {
            currentMatches.Union(GetRowPieces(dot.row));
        }
    }

    private List<GameObject> IsColumnBomb(Dot dot1, Dot dot2, Dot dot3) {
        List<GameObject> currentDots = new List<GameObject>();
         if(dot1.isColumnBomb) {
            currentDots.Union(GetColumnPieces(dot1.column));
        }
         if(dot2.isColumnBomb) {
            currentDots.Union(GetColumnPieces(dot2.column));
        }
         if(dot3.isColumnBomb) {
            currentDots.Union(GetColumnPieces(dot3.column));
        }
        // UnionColumnPieces(dot1);
        // UnionColumnPieces(dot2);
        // UnionColumnPieces(dot3);
        return currentDots;
    }

    private void UnionColumnPieces(Dot dot) {
        if(dot.isColumnBomb) {
            currentMatches.Union(GetColumnPieces(dot.column));
        }
    }

    private void MatchNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3) {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
    }

    private void AddToListAndMatch(GameObject dot) {
        if(!currentMatches.Contains(dot)) {
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3) {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isAdjacentBomb) {
            currentDots.Union(GetAdjacentPieces(dot1.column, dot1.row));
        }

        if (dot2.isAdjacentBomb) {
            currentDots.Union(GetAdjacentPieces(dot2.column, dot2.row));
        }

        if (dot3.isAdjacentBomb) {
            currentDots.Union(GetAdjacentPieces(dot3.column, dot3.row));
        }
        
        // UnionAdjacentPieces(dot1);
        // UnionAdjacentPieces(dot2);
        // UnionAdjacentPieces(dot3);
        return currentDots;
    }

    private void UnionAdjacentPieces(Dot dot) {
        if(dot.isAdjacentBomb) {
            currentMatches.Union(GetAdjacentPieces(dot.column, dot.row));
        }
    }

    private IEnumerator FindAllMatchesCo() {
        yield return new WaitForSeconds(.2f);
        for(int i = 0; i < board.width; i++) {
            for(int j = 0; j < board.height; j++) {
                GameObject currentDot = board.allDots[i,j];
                if(currentDot != null) {
                    Dot currentDotDot = currentDot.GetComponent<Dot>();
                    if(i > 0 && i < board.width-1) {
                        GameObject leftDot = board.allDots[i-1,j];
                        GameObject rightDot = board.allDots[i+1,j];
                        if(leftDot != null && rightDot != null) {
                            Dot leftDotDot = leftDot.GetComponent<Dot>();
                            Dot rightDotDot = rightDot.GetComponent<Dot>();
                            if(leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag) {
                                currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot));
                                currentMatches.Union(IsColumnBomb(leftDotDot, currentDotDot, rightDotDot));
                                currentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rightDotDot));
                                MatchNearbyPieces(leftDot, currentDot, rightDot);
                            }
                        }
                    }
                    if(j > 0 && j < board.height-1) {
                        GameObject upDot = board.allDots[i,j+1];
                        GameObject downDot = board.allDots[i,j-1];
                        if(upDot != null && downDot != null) {
                            Dot upDotDot = upDot.GetComponent<Dot>();
                            Dot downDotDot = downDot.GetComponent<Dot>();
                            if(upDot.tag == currentDot.tag && downDot.tag == currentDot.tag) {
                                currentMatches.Union(IsColumnBomb(upDotDot, currentDotDot, downDotDot));
                                currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot));
                                currentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot));
                                MatchNearbyPieces(upDot, currentDot, downDot);
                            }
                        }
                    }
                }
            }
        }
    }

    List<GameObject> GetAdjacentPieces(int column, int row) {
        List<GameObject> dots = new List<GameObject>();
        for(int i = column-1; i <= column+1; i++) {
            for(int j = row-1; j <= row+1; j++) {
                //Check if piece inside board
                if(i >= 0 && i < board.width && j >=0 && j < board.height) {
                    if(board.allDots[i, j] != null) {
                        dots.Add(board.allDots[i,j]);
                        board.allDots[i,j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
        return dots;
    }

    public void MatchPiecesOfColor(string color) {
        for(int i = 0; i < board.width; i++) {
            for(int j = 0; j < board.height; j++) {
                //Check if that piece exist
                if(board.allDots[i,j] != null) {
                    //Check the tag on that dot
                    if(board.allDots[i,j].tag == color) {
                        //Set the dot to be matched
                        board.allDots[i,j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
    } 

    List<GameObject> GetColumnPieces(int column) {
        List<GameObject> dots = new List<GameObject>();
        for(int i = 0; i < board.height; i++) {
            if(board.allDots[column, i] != null) {
                Dot dot = board.allDots[column, i].GetComponent<Dot>();
                if(dot.isRowBomb) {
                    dots.Union(GetRowPieces(i)).ToList();
                }
                dots.Add(board.allDots[column, i]);
                dot.isMatched = true;
            }
        }
        return dots;
    } 

    List<GameObject> GetRowPieces(int row) {
        List<GameObject> dots = new List<GameObject>();
        for(int i = 0; i < board.width; i++) {
            if(board.allDots[i, row] != null) {
                Dot dot = board.allDots[i, row].GetComponent<Dot>();
                if(dot.isColumnBomb) {
                    dots.Union(GetColumnPieces(i)).ToList();
                }
                dots.Add(board.allDots[i, row]);
                dot.isMatched = true;
            }
        }
        return dots;
    } 

    public void CheckBombs() {
        //Did the player move something?
        if(board.currentDot != null) {
            //Is the piece the moved matched?
            if(board.currentDot.isMatched) {
                //make it unmatched
                board.currentDot.isMatched = false;
                if((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45) ||
                    (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135)){
                    board.currentDot.MakeRowBomb();
                } else {
                    board.currentDot.MakeColumnBomb();
                }
            } 
            //Is the other piece matched?
            else if(board.currentDot.otherDot != null) {
                Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>();
                //Is the other dot matched?
                if(otherDot.isMatched) {
                    //Make it unmatched
                    otherDot.isMatched = false;
                    if((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45) ||
                        (board.currentDot.swipeAngle < -135 || board.currentDot.swipeAngle >= 135)){
                        otherDot.MakeRowBomb();
                    } else {
                        otherDot.MakeColumnBomb();
                    }
                }
            }
        }
    }
}
