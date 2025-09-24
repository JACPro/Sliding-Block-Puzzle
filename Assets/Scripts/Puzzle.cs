using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;


public class Puzzle : MonoBehaviour
{
    // Tiles Setup
    [SerializeField] private int _gridSize = 10;
    [SerializeField] private Material _imageMat;
    private Block[,] _blocks;
    private Block _emptyBlock;
    
    // State
    enum PuzzleState { Solved, Shuffling, Play };
    PuzzleState _state = PuzzleState.Solved;
    
    // Moving Tiles
    private Queue<Block> _tilesToMove = new Queue<Block>();
    bool _blockIsMoving = false;
    [Tooltip("How long (in seconds) it takes the block to move 1 space when clicked")]
    [SerializeField] private float _manualTileMoveDuration = 0.1f;
    
    // Shuffling
    [SerializeField] private float _shuffleTileMoveDuration = 0.04f;
    [SerializeField] private int _numShuffleMoves = 50;
    private int _shuffleMovesRemaining = 50;
    private Vector2Int _prevShuffleOffset;
    
    private void Awake()
    {
        Camera.main.orthographicSize = _gridSize * 0.55f;

        _blocks = new Block[_gridSize, _gridSize];
        CreatePuzzle();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _state == PuzzleState.Solved)
        {
            Shuffle();
        }
    }

    private void MakeNextShuffleMove()
    {
        Vector2Int[] offsets = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        int randomIndex = UnityEngine.Random.Range(0, offsets.Length);

        for (int i = 0; i < offsets.Length; i++)
        {
            Vector2Int offset = offsets[(randomIndex + i) % offsets.Length];
            if (offset != _prevShuffleOffset * -1)
            {
                Vector2Int moveBlockCoord = _emptyBlock._coords + offset;

                if (moveBlockCoord.x >= 0 && moveBlockCoord.x < _gridSize && moveBlockCoord.y >= 0 &&
                    moveBlockCoord.y < _gridSize)
                {
                    MoveBlock(_blocks[moveBlockCoord.x, moveBlockCoord.y], _shuffleTileMoveDuration);
                    _shuffleMovesRemaining--;
                    _prevShuffleOffset = offset;
                    break;
                }
            }
        }
    }
    
    private void Shuffle()
    {
        _state = PuzzleState.Shuffling;
        _emptyBlock.gameObject.SetActive(false);
        _shuffleMovesRemaining = _numShuffleMoves;
        MakeNextShuffleMove();
    }

    private void CreatePuzzle()
    {
        for (int y = 0; y < _gridSize; y++)
        {
            for (int x = 0; x < _gridSize; x++)
            {
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                tile.transform.position = -Vector2.one * (_gridSize -1) * 0.5f + new Vector2(x, y);
                
                Block block = tile.AddComponent<Block>();
                block.OnBlockPressed += PlayerBlockMoveInput;
                block.OnFinishedMoving += OnBlockFinishedMoving;
                block.Init(new Vector2Int(x, y));
                _blocks[x, y] = block;
                
                tile.GetComponent<Renderer>().material = _imageMat;
                tile.GetComponent<Renderer>().material.mainTextureScale = new Vector2(1.0f / _gridSize, 1.0f / _gridSize);
                tile.GetComponent<Renderer>().material.mainTextureOffset = new Vector2((1.0f/_gridSize) * x, (1.0f/_gridSize) * y);
                
                if (y == 0 && x == _gridSize - 1)
                {
                    _emptyBlock = tile.GetComponent<Block>();
                }
            }
        }
    }

    private void PlayerBlockMoveInput(Block blockToMove)
    {
        if (_state != PuzzleState.Play) return;

        _tilesToMove.Enqueue(blockToMove);
        MakeNextPlayerMove();
    }

    void MakeNextPlayerMove()
    {
        while (_tilesToMove.Count > 0 && !_blockIsMoving)
        {
            MoveBlock(_tilesToMove.Dequeue(), _manualTileMoveDuration);
        }
    }
    
    void MoveBlock(Block blockToMove, float duration)
    {
        if ((blockToMove._coords - _emptyBlock._coords).sqrMagnitude == 1)
        {
            _blocks[blockToMove._coords.x, blockToMove._coords.y] = _emptyBlock;
            _blocks[_emptyBlock._coords.x, _emptyBlock._coords.y] = blockToMove;
            
            _blockIsMoving = true;
            Vector2Int targetCoords = _emptyBlock._coords;
            _emptyBlock._coords = blockToMove._coords;
            blockToMove._coords = targetCoords;

            Vector2 targetPos = _emptyBlock.transform.position;
            _emptyBlock.transform.position = blockToMove.transform.position;
            blockToMove.MoveToPosition(targetPos, duration);
        }
    }

    void OnBlockFinishedMoving()
    {
        _blockIsMoving = false;

        if (_state == PuzzleState.Play)
        {
            CheckIfSolved();
            MakeNextPlayerMove();
        }
        else if (_state == PuzzleState.Shuffling)
        {
            if (_shuffleMovesRemaining > 0)
            {
                MakeNextShuffleMove();
            }
            else
            {
                _state = PuzzleState.Play;
            }
        }
    }

    void CheckIfSolved()
    {
        foreach (Block block in _blocks)
        {
            if (!block.IsAtStartingCoordinate())
            {
                return;
            }
        }
        
        _state = PuzzleState.Solved;
        _emptyBlock.gameObject.SetActive(true);
    }
}
