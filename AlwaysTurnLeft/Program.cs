using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AlwaysTurnLeft
{
    class Program
    {
        static void Main(string[] args)
        {
            string _inputFile = args[0];
            string _outputFile = Path.ChangeExtension(_inputFile, "out");

            var _result = File.ReadLines(_inputFile)
                              .Skip(1)
                              .Select((line, idx) => {
                                  var _parts = line.Split(' ');
                                  var _entranceToExit = _parts[0];
                                  var _exitToEntrance = _parts[1];

                                  var _johnnyWalker = new MazeWalker(CardinalDirection.South);
                                  _johnnyWalker.WalkTheLine(_entranceToExit);
                                  _johnnyWalker.TurnArround();
                                  _johnnyWalker.WalkTheLine(_exitToEntrance);

                                  var _bluePrint = MazeBricklayer.GetMazeBrickBlueprint(_johnnyWalker.CellsWalked);
                                  var _mazeDefinition = string.Join("\r\n", _bluePrint);
                                  return string.Format("Case #{0}:\r\n{1}", idx + 1, _mazeDefinition);
                              });

            File.WriteAllLines(_outputFile, _result);
        }
    }

    [Flags]
    enum CardinalDirection
    {
        North = 1,
        East = 2,
        South = 4,
        West = 8
    }

    class MazeCell
    {
        public int Row { get; set; }
        public  int Col { get; set; }
        public CardinalDirection PossibleDirections { get; set; }
    }

    class MazeWalker
    {
        public int CurrentRow { get; private set; }
        public int CurrentCol { get; private set; }
        public IList<MazeCell> CellsWalked { get; private set; }
        public CardinalDirection FacingDirection { get; set; }

        private MazeCell CurrentCell
        {
            get
            {
                return CellsWalked.Where(mc => mc.Row == CurrentRow && mc.Col == CurrentCol)
                                  .SingleOrDefault();
            }
        }

        public MazeWalker(CardinalDirection facing)
        {
            CurrentRow = 0;
            CurrentCol = 0;
            CellsWalked = new List<MazeCell>();
            FacingDirection = facing;
        }

        public void TurnLeft()
        {
            switch (FacingDirection)
            {
                case CardinalDirection.North:
                    FacingDirection = CardinalDirection.West;
                    break;
                case CardinalDirection.East:
                    FacingDirection = CardinalDirection.North;
                    break;
                case CardinalDirection.South:
                    FacingDirection = CardinalDirection.East;
                    break;
                case CardinalDirection.West:
                    FacingDirection = CardinalDirection.South;
                    break;
                default:
                    break;
            }
        }

        public void TurnRight()
        {
            switch (FacingDirection)
            {
                case CardinalDirection.North:
                    FacingDirection = CardinalDirection.East;
                    break;
                case CardinalDirection.East:
                    FacingDirection = CardinalDirection.South;
                    break;
                case CardinalDirection.South:
                    FacingDirection = CardinalDirection.West;
                    break;
                case CardinalDirection.West:
                    FacingDirection = CardinalDirection.North;
                    break;
                default:
                    break;
            }
        }

        public void TurnArround()
        {
            TurnRight();
            TurnRight();
        }

        public void GoStraight(bool isLastMovement)
        {
            // move the walker and record from which direction we came from
            var _cameFrom = CardinalDirection.North;
            switch (FacingDirection)
            {
                case CardinalDirection.North:
                    CurrentRow -= 1;
                    _cameFrom = CardinalDirection.South;
                    break;
                case CardinalDirection.East:
                    CurrentCol += 1;
                    _cameFrom = CardinalDirection.West;
                    break;
                case CardinalDirection.South:
                    CurrentRow += 1;
                    _cameFrom = CardinalDirection.North;
                    break;
                case CardinalDirection.West:
                    CurrentCol -= 1;
                    _cameFrom = CardinalDirection.East;
                    break;
                default:
                    break;
            }

            // if we haven't been here already and there is a next movement
            // then it's a new cell...so add it  to the list
            if (CurrentCell == null && !isLastMovement)
            {
                CellsWalked.Add(new MazeCell() { Col = CurrentCol, Row = CurrentRow });
            }

            // update the possible directions for the current cell
            // if we are not out of the maze already that is
            if (CurrentCell != null)
            { 
                CurrentCell.PossibleDirections = CurrentCell.PossibleDirections | _cameFrom;
            }
        }

        public void Move(char instruction, bool isLastMovement)
        {
            switch (instruction)
            {
                case 'W':
                    GoStraight(isLastMovement);
                    break;
                case 'R':
                    TurnRight();
                    break;
                case 'L':
                    TurnLeft();
                    break;
                default:
                    break;
            }
        }

        public void WalkTheLine(string theLine)
        {
            var _linkedLine = new LinkedList<char>(theLine);
            var _instruction = _linkedLine.First;
            
            while (_instruction != null)
            {
                Move(_instruction.Value, _instruction.Next == null);
                _instruction = _instruction.Next;
            }
        }
    }

    static class MazeBricklayer
    {
        private static IList<dynamic> _brickTypes = new List<dynamic>() {
            new { Character = "1", Directions = CardinalDirection.North  },
            new { Character = "2", Directions = CardinalDirection.South  },
            new { Character = "3", Directions = CardinalDirection.North | CardinalDirection.South  },
            new { Character = "4", Directions = CardinalDirection.West },
            new { Character = "5", Directions = CardinalDirection.North | CardinalDirection.West },
            new { Character = "6", Directions = CardinalDirection.South | CardinalDirection.West },
            new { Character = "7", Directions = CardinalDirection.North | CardinalDirection.South | CardinalDirection.West },
            new { Character = "8", Directions = CardinalDirection.East },
            new { Character = "9", Directions = CardinalDirection.North | CardinalDirection.East },
            new { Character = "a", Directions = CardinalDirection.South | CardinalDirection.East },
            new { Character = "b", Directions = CardinalDirection.North | CardinalDirection.South | CardinalDirection.East },
            new { Character = "c", Directions = CardinalDirection.West | CardinalDirection.East },
            new { Character = "d", Directions = CardinalDirection.North | CardinalDirection.West | CardinalDirection.East },
            new { Character = "e", Directions = CardinalDirection.South | CardinalDirection.West | CardinalDirection.East },
            new { Character = "f", Directions = CardinalDirection.North | CardinalDirection.South | CardinalDirection.West | CardinalDirection.East },
        };

        public static IEnumerable<string> GetMazeBrickBlueprint(IEnumerable<MazeCell> cellsWalked)
        {
            var _rowsWithCols = cellsWalked.Select(c => new
                                                {
                                                    Character = _brickTypes.Where(bt => bt.Directions == c.PossibleDirections)
                                                                           .Select(bt => bt.Character)
                                                                           .SingleOrDefault(),
                                                    Cell = c
                                                })
                                            .GroupBy(d => d.Cell.Row)
                                            .Select(grp => grp.OrderBy(a => a.Cell.Col)
                                                              .Select(a => a.Character.ToString())
                                                              .Aggregate((res, next) => res = res + next))
                                            .Cast<string>();


            return _rowsWithCols;
        }
    }
}
