using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState {
    wait,
    move,
    win,
    lose,
    pause
}

public enum TileKind {
    Breakable,
    Blank,
    Lock,
    Icing,
    Chocolate,
    Normal
}

[System.Serializable]
public class TitleType {
    public int x;
    public int y;
    public TileKind tileKind;
}

[System.Serializable]
public class MatchType {
    public string type;
    public string color;
}

public class Board : MonoBehaviour
{
    [Header("Scritable Objects")]
    public World world;
    public int level;

    public GameState currentState = GameState.move;

    [Header("Board Dimensions")]
    public int width;
    public int height;
    public int offSet;

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject breakableTilePrefab;
    public GameObject lockTilePrefab;
    public GameObject icingTilePrefab;
    public GameObject chocolatePrefab;
    public GameObject[] dots;
    public GameObject[,] allDots;
    public GameObject destroyEffect;

    [Header("Layout")]
    public TitleType[] boardLayout;
    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public BackgroundTile[,] lockTiles;
    public BackgroundTile[,] icingTiles;
    private BackgroundTile[,] chocolateTiles;
    
    [Header("Match objects")]
    public MatchType matchType;
    public Dot currentDot;
    private FindMatches findMatches;
    public int basePieceValue = 20;
    private int streakValue = 1;
    private ScoreManager scoreManager;
    private SoundManager soundManager;
    private GoalManager goalManager;
    public float refillDelay = 0.5f;
    public int[] scoreGoals;
    private bool makeChocolate = true;


    //todo sound keeps making noise after win or try again displays
    //moves left or special pieces, should detonate
    //row and column bombs should be swapable to cause damage in both directions
    //tiles should not be able to fall if in a lock tile
    //icing tiles background disapears when icing removed
    //icing tiles don't get removed when match in a space that was an icing tile
    //icing tiles don't drop (they should)
    //pieces under lock tile shouldn't shuffle
    //have confirm screen when press exit game button
    //pause menu can be done better - restart game option
    //fix music playing on start

    void Awake() {
        if(PlayerPrefs.HasKey("Current Level")) {
            level = PlayerPrefs.GetInt("Current Level");
        }
        if(world != null) {
            if(level < world.levels.Length) {
                Level tempLevel = world.levels[level];
                if(tempLevel != null) {
                    width = tempLevel.width; 
                    height = tempLevel.height; 
                    dots = tempLevel.dots;
                    scoreGoals = tempLevel.scoreGoals;
                    boardLayout = tempLevel.boardLayout;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start() {
        scoreManager = FindObjectOfType<ScoreManager>();
        soundManager = FindObjectOfType<SoundManager>();
        goalManager = FindObjectOfType<GoalManager>();
        breakableTiles = new BackgroundTile[width, height];
        lockTiles = new BackgroundTile[width, height];
        icingTiles = new BackgroundTile[width, height];
        chocolateTiles = new BackgroundTile[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];
        allDots = new GameObject[width, height];
        SetUp();
        currentState = GameState.pause;
    }

    public void GenerateBlankSpaces() {
        for(int i = 0; i < boardLayout.Length; i++) {
            if(boardLayout[i].tileKind == TileKind.Blank) {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    public void GenerateBreakableTiles() {
        for(int i = 0; i < boardLayout.Length; i++) {
            if(boardLayout[i].tileKind == TileKind.Breakable) {
                //Create a "jelly" tile at the position
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakableTilePrefab, tempPosition, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateLockTiles() {
        for(int i = 0; i < boardLayout.Length; i++) {
            if(boardLayout[i].tileKind == TileKind.Lock) {
                //Create a "lock" tile at the position
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(lockTilePrefab, tempPosition, Quaternion.identity);
                lockTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateIcingTiles() {
        for(int i = 0; i < boardLayout.Length; i++) {
            if(boardLayout[i].tileKind == TileKind.Icing) {
                //Create a "icing" tile at the position
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(icingTilePrefab, tempPosition, Quaternion.identity);
                icingTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void GenerateChocolateTiles() {
        for(int i = 0; i < boardLayout.Length; i++) {
            if(boardLayout[i].tileKind == TileKind.Chocolate) {
                //Create a "chocoloate" tile at the position
                Vector2 tempPosition = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(chocolatePrefab, tempPosition, Quaternion.identity);
                chocolateTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void SetUp() {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        GenerateLockTiles();
        GenerateIcingTiles();
        GenerateChocolateTiles();
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(!blankSpaces[i,j]) {
                    Vector2 tilePosition = new Vector2(i, j);
                    GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "( " + i + ", " + j + " )";
                }

                if(!blankSpaces[i,j] && !icingTiles[i,j] && !chocolateTiles[i, j]) {
                     Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);

                    int maxIterations = 0;
                    while(MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100) {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                    }
                    maxIterations = 0;

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().column = i;
                    dot.GetComponent<Dot>().row = j;
                    dot.transform.parent = this.transform;
                    dot.name = "( " + i + ", " + j + " )";
                    allDots[i,j] = dot;
                }
            }
        }

        //Deadlocked before start playing level
        if(IsDeadlocked()) {
            StartCoroutine(ShuffleBoard());
            Debug.Log("Deadlocked!!!");
        }
    }

    private bool MatchesAt(int column, int row, GameObject piece) {
        if(column > 1 && row > 1) {
            if(allDots[column-1, row] != null && allDots[column-2, row] != null) {
                if(allDots[column-1, row].tag == piece.tag && allDots[column-2, row].tag == piece.tag) {
                    //Match
                    return true;
                }
            }
            if(allDots[column, row-1] != null && allDots[column, row-2] != null) {
                if(allDots[column, row-1].tag == piece.tag && allDots[column, row-2].tag == piece.tag) {
                    //Match
                    return true;
                }
            }
        } else if(column <= 1 || row <= 1) {
            if(row > 1) {
                if(allDots[column, row-1] != null && allDots[column, row-2] != null) {
                    if(allDots[column, row-1].tag == piece.tag && allDots[column, row-2].tag == piece.tag) {
                        return true;
                    }
                }
            }
            if(column > 1) {
                if(allDots[column-1, row] != null && allDots[column-2, row] != null) {
                    if(allDots[column-1, row].tag == piece.tag && allDots[column-2, row].tag == piece.tag) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private MatchType ColumnOrRow() {
        //Make a copy of the currentMatches
        List<GameObject> matchesCopy = findMatches.currentMatches as List<GameObject>;
        MatchType typeOfMatch = new MatchType();
        typeOfMatch.type = "";
        typeOfMatch.color = "";
        //Cycle through all of match copy and decide if a bomb needs to be made
        for(int i = 0; i < matchesCopy.Count; i++) {
            //Store this dot
            Dot thisDot = matchesCopy[i].GetComponent<Dot>();
            string color = matchesCopy[i].tag;
            int column = thisDot.column;
            int row = thisDot.row;
            int columnMatch = 0;
            int rowMatch = 0;
            //Cycle through the rest of the pieces and compare
            for(int j = 0; j < matchesCopy.Count; j++) {
                //Store the next dot
                Dot nextDot = matchesCopy[j].GetComponent<Dot>();
                if(nextDot == thisDot) {
                    continue;
                }
                if(nextDot.column == thisDot.column && nextDot.tag == color) { //.CompareTag(thisDot.tag)) {
                    columnMatch++;
                }
                if(nextDot.row == thisDot.row && nextDot.tag == color) { //.CompareTag(thisDot.tag)) {
                    rowMatch++;
                }
            }
            
            if(columnMatch == 4 || rowMatch == 4) {
                typeOfMatch.type = "color";
                typeOfMatch.color = color;
                return typeOfMatch;
            } else if(columnMatch == 2 && rowMatch == 2) {
                typeOfMatch.type = "adjacent";
                typeOfMatch.color = color;
                return typeOfMatch;
            } else if(columnMatch == 3 || rowMatch == 3) {
                typeOfMatch.type = "column or row";
                typeOfMatch.color = color;
                return typeOfMatch;
            }
        }
        // typeOfMatch.type = "";
        // typeOfMatch.color = "";
        return typeOfMatch;
    }

    private void CheckToMakeBombs() {
        //How many objects are in findMatches currentMatches
        if(findMatches.currentMatches.Count > 3) {
            matchType = ColumnOrRow();
            if(matchType.type == "color") {
                MakeColorBomb();
            } else if(matchType.type == "adjacent") {
                MakeAdjacentBomb();
            } else if(matchType.type == "column or row") {
                // if(currentMatches != null) {
                //     findMatches.CheckBombs(matchType);
                // } else {
                //     //Make column bomb as piece is falling down
                //     //the piece that falls into should be the column bomb
                // }
                findMatches.CheckBombs(matchType);
            }
        }
    }

    public void BombRow(int row) {
        for(int i = 0; i < width; i++) {
            if(icingTiles[i, row]) {
                icingTiles[i, row].TakeDamage(1);
                if(icingTiles[i, row].hitPoints <= 0) {
                    icingTiles[i, row] = null;
                }
            }
        }
    }

    public void BombColumn(int column) {
        for(int i = 0; i < width; i++) {
            if(icingTiles[column, i]) {
                icingTiles[column, i].TakeDamage(1);
                if(icingTiles[column, i].hitPoints <= 0) {
                    icingTiles[column, i] = null;
                }
            }
        }
    }

    private void MakeColorBomb() {
        //Make a color bomb
        //Is the current dot matched?
        if(currentDot != null && currentDot.isMatched && currentDot.tag == matchType.color) {
            currentDot.isMatched = false;
            currentDot.MakeColorBomb();
            Debug.Log("Make a color bomb");
        } else if(currentDot.otherDot != null) {
            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
            if(otherDot.isMatched && currentDot.tag == matchType.color) {
                otherDot.isMatched = false;
                otherDot.MakeColorBomb();
                Debug.Log("Make a color bomb");  
            }
        }
    }

    private void MakeAdjacentBomb() {
        //Make an adjacent bomb
        //Is the current dot matched?
        if(currentDot != null && currentDot.isMatched && currentDot.tag == matchType.color) {
            currentDot.isMatched = false;
            currentDot.MakeAdjacentBomb();
            Debug.Log("Make an adjacent bomb");
        } else if(currentDot.otherDot != null) {
            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
            if(otherDot.isMatched && otherDot.tag == matchType.color) {
                otherDot.isMatched = false;
                otherDot.MakeAdjacentBomb();
                Debug.Log("Make an adjacent bomb");
            }
        }
    }

    private void DestroyMatchesAt(int column, int row) {
        if(allDots[column, row].GetComponent<Dot>().isMatched) {
            //Does a tile need to break
            if(breakableTiles[column, row] != null) {
                breakableTiles[column, row].TakeDamage(1);
                if(breakableTiles[column, row].hitPoints <= 0) {
                    breakableTiles[column, row] = null;
                }
            }

            //Does a tile need to break
            if(lockTiles[column, row] != null) {
                lockTiles[column, row].TakeDamage(1);
                if(lockTiles[column, row].hitPoints <= 0) {
                    lockTiles[column, row] = null;
                }
            }

            DamageIcing(column, row);
            DamageChocolate(column, row);

            if(goalManager != null) {
                goalManager.CompareGoal(allDots[column, row].tag.ToString());
                goalManager.UpdateGoals();
            }

            //Does the sound manager exist?
            if(soundManager != null) {
                soundManager.PlayRandomDestroyNoise();
            }
            GameObject particle = Instantiate(destroyEffect, 
                                                allDots[column, row].transform.position, 
                                                Quaternion.identity);
            Destroy(particle, .5f);
            Destroy(allDots[column, row]);
            scoreManager.IncreaseScore(basePieceValue * streakValue);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches() {
        //How many elements are in the matched pieces list from findMatches?
        if(findMatches.currentMatches.Count >= 4) {
            CheckToMakeBombs();
        }
        findMatches.currentMatches.Clear();

        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(allDots[i, j] != null) {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        StartCoroutine(DecreaseRowCo());
    }

    private void DamageIcing(int column, int row) {
        if(column > 0) {
            if(icingTiles[column-1, row]) {
                icingTiles[column-1, row].TakeDamage(1);
                if(icingTiles[column-1, row].hitPoints <= 0) {
                    icingTiles[column-1, row] = null;
                }
            }
        }
        if(column < width-1) {
            if(icingTiles[column+1, row]) {
                icingTiles[column+1, row].TakeDamage(1);
                if(icingTiles[column+1, row].hitPoints <= 0) {
                    icingTiles[column+1, row] = null;
                }
            }
        }
        if(row > 0) {
            if(icingTiles[column, row-1]) {
                icingTiles[column, row-1].TakeDamage(1);
                if(icingTiles[column, row-1].hitPoints <= 0) {
                    icingTiles[column, row-1] = null;
                }
            }
        }
        if(row < height-1) {
            if(icingTiles[column, row+1]) {
                icingTiles[column, row+1].TakeDamage(1);
                if(icingTiles[column, row+1].hitPoints <= 0) {
                    icingTiles[column, row+1] = null;
                }
            }
        }
    }

     private void DamageChocolate(int column, int row) {
        if(column > 0) {
            if(chocolateTiles[column-1, row]) {
                chocolateTiles[column-1, row].TakeDamage(1);
                if(chocolateTiles[column-1, row].hitPoints <= 0) {
                    chocolateTiles[column-1, row] = null;
                }
                makeChocolate = false;
            }
        }
        if(column < width-1) {
            if(chocolateTiles[column+1, row]) {
                chocolateTiles[column+1, row].TakeDamage(1);
                if(chocolateTiles[column+1, row].hitPoints <= 0) {
                    chocolateTiles[column+1, row] = null;
                }
                makeChocolate = false;
            }
        }
        if(row > 0) {
            if(chocolateTiles[column, row-1]) {
                chocolateTiles[column, row-1].TakeDamage(1);
                if(chocolateTiles[column, row-1].hitPoints <= 0) {
                    chocolateTiles[column, row-1] = null;
                }
                makeChocolate = false;
            }
        }
        if(row < height-1) {
            if(chocolateTiles[column, row+1]) {
                chocolateTiles[column, row+1].TakeDamage(1);
                if(chocolateTiles[column, row+1].hitPoints <= 0) {
                    chocolateTiles[column, row+1] = null;
                }
                makeChocolate = false;
            }
        }
    }

    private IEnumerator DecreaseRowCo() {
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                //if the current spot isn't blank and is empty
                if(!blankSpaces[i, j] && !allDots[i, j] && !icingTiles[i, j] && !chocolateTiles[i, j]) {
                    //loop from the space above to the top of the column
                    for(int k = j+1; k < height; k++) {
                        //if a dot is found
                        if(allDots[i, k]) {
                            //move that dot to this empty space
                            allDots[i, k].GetComponent<Dot>().row = j;
                            //set that spot to be null
                            allDots[i, k] = null;
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard() {
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(!allDots[i, j] && !blankSpaces[i, j] && !icingTiles[i, j] && !chocolateTiles[i, j]) {
                    Vector2 tempPosition = new Vector2(i, j + offSet);
                    int dotToUse = Random.Range(0, dots.Length);
                    int maxIterations = 0;
					while(MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100) {
						maxIterations++;
						dotToUse = Random.Range(0, dots.Length);
                    }
                    maxIterations = 0;
                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    piece.GetComponent<Dot>().column = i;
                    piece.GetComponent<Dot>().row = j;
                    piece.name = "( " + i + ", " + j + " )";
                    allDots[i,j] = piece;
                }
            }
        }
    }

    private bool MatchesOnBoard() {
        findMatches.FindAllMatches();
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(allDots[i,j] != null) {
                    if(allDots[i,j].GetComponent<Dot>().isMatched) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo() {
        yield return new WaitForSeconds(refillDelay);
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);
        while(MatchesOnBoard()) {
            streakValue++;
            DestroyMatches();
            yield break;
        }

        currentDot = null;
        CheckToMakeChocolate();

        if(IsDeadlocked()) {
            StartCoroutine(ShuffleBoard());
            Debug.Log("Deadlocked!!!");
        }
        yield return new WaitForSeconds(refillDelay);
        if(currentState != GameState.pause) {
            currentState = GameState.move;
            makeChocolate = true;
            streakValue = 1;
        }
    }

    private void CheckToMakeChocolate() {
        //Check the slime tiles array
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(chocolateTiles[i, j] && makeChocolate) {
                    //Call another method to make a new Chocolate
                    MakeNewChocolate();
                    return;
                }
            }
        }
    }

    private Vector2 CheckForAdjacent(int column, int row) {
        if(column < width-1 && allDots[column + 1, row]) {
            return Vector2.right;
        }
        if(column > 0 && allDots[column - 1, row]) {
            return Vector2.left;
        }
        if(row < height-1 && allDots[column, row + 1]) {
            return Vector2.up;
        }
        if(row > 0 && allDots[column, row-1]) {
            return Vector2.down;
        }
        return Vector2.zero;
    }

    private void MakeNewChocolate() {
        bool chocolate = false;
        int loops = 0;
        while(!chocolate && loops < 200) {
            int newX = Random.Range(0, width);
            int newY = Random.Range(0, height);
            if(chocolateTiles[newX, newY]) {
                Vector2 adjacent = CheckForAdjacent(newX, newY);
                if(adjacent != Vector2.zero) {
                    int adjX = (int)adjacent.x;
                    int adjY = (int)adjacent.y;
                    Destroy(allDots[newX + adjX, newY + adjY]);
                    Vector2 tempPosition = new Vector2(newX + adjX, newY + adjY);
                    GameObject tile = Instantiate(chocolatePrefab, tempPosition, Quaternion.identity);
                    chocolateTiles[newX + adjX, newY + adjY] = tile.GetComponent<BackgroundTile>();
                    chocolate = true;
                }
            }
            loops++;
        }
    }

    private void SwitchPieces(int column, int row, Vector2 direction) {
        int x = (int)direction.x;
        int y = (int)direction.y;
        if(allDots[column + x, row + y]) {
            //Take second piece and save it in a holder
            GameObject holder = allDots[column + x, row + y] as GameObject;
            //switching the first dot to be the second position
            allDots[column + x, row + y] = allDots[column, row];
            //Set the first dot to be the second dot
            allDots[column, row] = holder;
        }
    }

    private bool CheckForMatches() {

        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(allDots[i, j] != null) {
                    //Make sure that one and two to the right are in the board
                    if(i < width-2) {
                        //Check if the dots to the right and two to the right exist
                        if(allDots[i+1, j] != null && allDots[i+2, j] != null) {
                            if(allDots[i+1, j].tag == allDots[i, j].tag 
                                && allDots[i+2, j].tag == allDots[i, j].tag) {
                                return true;
                            }
                        }
                    }
                    if(j < height-2) {
                        //Check if the dots above exist
                        if(allDots[i, j+1] != null && allDots[i, j+2] != null) {
                            if(allDots[i, j+1].tag == allDots[i, j].tag 
                                && allDots[i, j+2].tag == allDots[i, j].tag) {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction) {
        SwitchPieces(column, row, direction);
        if(CheckForMatches()) {
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false;
    }

    private bool IsDeadlocked() {
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(allDots[i, j] != null) {
                    if(i < width-1) {
                        if(SwitchAndCheck(i, j, Vector2.right)) {
                            return false;
                        }
                    }
                    if(j < height-1) {
                        if(SwitchAndCheck(i, j, Vector2.up)) {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private IEnumerator ShuffleBoard() {
        yield return new WaitForSeconds(refillDelay);
        //Create a list of GameObjects
        List<GameObject> newBoard = new List<GameObject>();
        //Add every piece to the list
        for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                if(allDots[i, j] != null) {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }
        yield return new WaitForSeconds(refillDelay);
        //for every spot on the board
         for(int i = 0; i < width; i++) {
            for(int j = 0; j < height; j++) {
                //If the spot shouldn't be blank
                if(!blankSpaces[i, j] && !icingTiles[i, j] && !chocolateTiles[i, j]) {
                    //Pick a random number
                    int pieceToUse = Random.Range(0, newBoard.Count);
                    int maxIterations = 0;
                    while(MatchesAt(i, j, newBoard[pieceToUse]) && maxIterations < 100) {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                        Debug.Log(maxIterations);
                    }
                    maxIterations = 0;

                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    //Assign the column and row to the piece
                    piece.column = i;
                    piece.row = j;
                    //Fill in the dots array with this new piece
                    allDots[i, j] = newBoard[pieceToUse];
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }

        if(IsDeadlocked()) {
            StartCoroutine(ShuffleBoard());
        }
    }
}
