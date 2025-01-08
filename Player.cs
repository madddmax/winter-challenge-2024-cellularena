using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

class Player
{
    static int Width;
    static int Height;

    static Dictionary<Point, Cell> Map;

    static Protein MyProtein;
    static Protein OppProtein;

    static Protein MyHarvesterGen;

    static Dictionary<int, int> OppOrganScore;

    static bool LastMoveAllWait;

    record Point(int X, int Y)
    {
        public override string ToString() => $"{X};{Y}";
    }

    record Node(Node Parent, int OrganId, Point Point, int Distance)
    {
        public int Score { get; set; }

        public string Type { get; set; }

        public string Direction { get; set; }

        public Protein MyProtein { get; set; }

        public HashSet<Point> DestroyedProtein { get; set; } = new HashSet<Point>();

        public override string ToString() => $"{Point} {Distance}";
    }

    static bool AnyOrgan(string type) =>
        type == "ROOT" 
        || type == "BASIC" 
        || type == "HARVESTER"
        || type == "TENTACLE"
        || type == "SPORER";

    static bool CheckWall(string type) => type == "WALL";

    static bool CheckRoot(string type) => type == "ROOT";

    static bool CheckHarvester(string type) => type == "HARVESTER";

    static bool CheckTentacle(string type) => type == "TENTACLE";

    static bool CheckSporer(string type) => type == "SPORER";

    static bool AllProteinGreaterOrEqual(Protein protein, int val) =>
        protein.A >= val
        && protein.B >= val 
        && protein.C >= val
        && protein.D >= val;

    static bool CheckRootAround(Point point, int owner)
    {
        // Up - North
        if (point.Y - 1 >= 0)
        {
            Map.TryGetValue(new Point(point.X, point.Y - 1), out var cell);
            if(cell != null && cell.Owner == owner && CheckRoot(cell.Type))
                return true;
        }

        // Right - East
        if (point.X + 1 < Width)
        {
            Map.TryGetValue(new Point(point.X + 1, point.Y), out var cell);
            if(cell != null && cell.Owner == owner && CheckRoot(cell.Type))
                return true;
        }

        // Down - South
        if (point.Y + 1 < Height)
        {
            Map.TryGetValue(new Point(point.X, point.Y + 1), out var cell);
            if(cell != null && cell.Owner == owner && CheckRoot(cell.Type))
                return true;
        }

        // Left - West
        if (point.X - 1 >= 0)
        {
            Map.TryGetValue(new Point(point.X - 1, point.Y), out var cell);
            if(cell != null && cell.Owner == owner && CheckRoot(cell.Type))
                return true;
        }

        return false;
    }

    static bool CheckHarvesterAround(Point point)
    {
        // Up - North
        if (point.Y - 1 >= 0)
        {
            Map.TryGetValue(new Point(point.X, point.Y - 1), out var cell);
            if(cell != null && CheckHarvester(cell.Type) && cell.OrganDir == "S")
                return true;
        }

        // Right - East
        if (point.X + 1 < Width)
        {
            Map.TryGetValue(new Point(point.X + 1, point.Y), out var cell);
            if(cell != null && CheckHarvester(cell.Type) && cell.OrganDir == "W")
                return true;
        }

        // Down - South
        if (point.Y + 1 < Height)
        {
            Map.TryGetValue(new Point(point.X, point.Y + 1), out var cell);
            if(cell != null && CheckHarvester(cell.Type) && cell.OrganDir == "N")
                return true;
        }

        // Left - West
        if (point.X - 1 >= 0)
        {
            Map.TryGetValue(new Point(point.X - 1, point.Y), out var cell);
            if(cell != null && CheckHarvester(cell.Type) && cell.OrganDir == "E")
                return true;
        }

        return false;
    }

    static bool CheckHarvesterAround(Point point, int owner)
    {
        // Up - North
        if (point.Y - 1 >= 0)
        {
            Map.TryGetValue(new Point(point.X, point.Y - 1), out var cell);
            if(cell != null && cell.Owner == owner && CheckHarvester(cell.Type) && cell.OrganDir == "S")
                return true;
        }

        // Right - East
        if (point.X + 1 < Width)
        {
            Map.TryGetValue(new Point(point.X + 1, point.Y), out var cell);
            if(cell != null && cell.Owner == owner && CheckHarvester(cell.Type) && cell.OrganDir == "W")
                return true;
        }

        // Down - South
        if (point.Y + 1 < Height)
        {
            Map.TryGetValue(new Point(point.X, point.Y + 1), out var cell);
            if(cell != null && cell.Owner == owner && CheckHarvester(cell.Type) && cell.OrganDir == "N")
                return true;
        }

        // Left - West
        if (point.X - 1 >= 0)
        {
            Map.TryGetValue(new Point(point.X - 1, point.Y), out var cell);
            if(cell != null && cell.Owner == owner && CheckHarvester(cell.Type) && cell.OrganDir == "E")
                return true;
        }

        return false;
    }

    static bool CheckTentacleAround(Point point, int owner)
    {
        // Up - North
        if (point.Y - 1 >= 0)
        {
            Map.TryGetValue(new Point(point.X, point.Y - 1), out var cell);
            if(cell != null && cell.Owner == owner && CheckTentacle(cell.Type) && cell.OrganDir == "S")
                return true;
        }

        // Right - East
        if (point.X + 1 < Width)
        {
            Map.TryGetValue(new Point(point.X + 1, point.Y), out var cell);
            if(cell != null && cell.Owner == owner && CheckTentacle(cell.Type) && cell.OrganDir == "W")
                return true;
        }

        // Down - South
        if (point.Y + 1 < Height)
        {
            Map.TryGetValue(new Point(point.X, point.Y + 1), out var cell);
            if(cell != null && cell.Owner == owner && CheckTentacle(cell.Type) && cell.OrganDir == "N")
                return true;
        }

        // Left - West
        if (point.X - 1 >= 0)
        {
            Map.TryGetValue(new Point(point.X - 1, point.Y), out var cell);
            if(cell != null && cell.Owner == owner && CheckTentacle(cell.Type) && cell.OrganDir == "E")
                return true;
        }

        return false;
    }

    static Point OneMoveToDirection(Point point, string direction)
    {
        if(direction == "N" && point.Y - 1 >= 0)
        {
            return new Point(point.X, point.Y - 1);  
        }
        else if(direction == "E" && point.X + 1 < Width)
        {
            return new Point(point.X + 1, point.Y);     
        }
        else if(direction == "S" && point.Y + 1 < Height)
        {
            return new Point(point.X, point.Y + 1);   
        }
        else if(direction == "W" && point.X - 1 >= 0)
        {
            return new Point(point.X - 1, point.Y);      
        }

        return point;
    }

    static int FreeLineLength(Point point, string direction)
    {
        int totalLength = 0;

        if(direction == "N")
        {
            for(int y = point.Y; y >= 0; y--)
            {
                (int length, bool success) = CheckFreePoint(new Point(point.X, y));
                if(!success)
                    break;

                totalLength += length;
            }      
        }
        else if(direction == "E")
        {
            for(int x = point.X; x < Width; x++)
            {
                (int length, bool success) = CheckFreePoint(new Point(x, point.Y));
                if(!success)
                    break;

                totalLength += length;
            }      
        }
        else if(direction == "S")
        {
            for(int y = point.Y; y < Height; y++)
            {
                (int length, bool success) = CheckFreePoint(new Point(point.X, y));
                if(!success)
                    break;

                totalLength += length;
            }      
        }
        else if(direction == "W")
        {
            for(int x = point.X; x >= 0; x--)
            {
                (int length, bool success) = CheckFreePoint(new Point(x, point.Y));
                if(!success)
                    break;

                totalLength += length;
            }      
        }

        return totalLength;
    }

    static (int length, bool success) CheckFreePoint(Point point)
    {
        Map.TryGetValue(point, out var cell);
        if(CheckWall(cell?.Type)
        || AnyOrgan(cell?.Type))
            return (0, false);

        return (1, true);
    }

    static Node GetPath(List<Cell> myOrganList, List<Point> myOtherOrganList, Protein myProtein, int rootCount, Stopwatch stopWatch, int turn)
    {
        int maxScore = turn <= 80 && !LastMoveAllWait ? 0 : int.MinValue;
        Node bestNode = null;

        Queue<Node> fifo = new Queue<Node>();

        foreach(var myOrgan in myOrganList)
        {
            var myOrganNode = new Node(null, myOrgan.OrganId, myOrgan.Point, 0);
            myOrganNode.Score = 0;
            myOrganNode.Type = myOrgan.Type;
            myOrganNode.Direction = myOrgan.OrganDir;
            myOrganNode.MyProtein = new Protein(myProtein.A, myProtein.B, myProtein.C, myProtein.D);
            fifo.Enqueue(myOrganNode);
        }

        while (fifo.Count > 0)
        {
            TimeSpan ts = stopWatch.Elapsed;
            if(ts.TotalMilliseconds > 45)
            {
                Console.Error.WriteLine($"TurnTime {ts.TotalMilliseconds}ms");
                break;
            }

            var currentNode = fifo.Dequeue();
            var myNewOrganSet = new HashSet<Point>(myOtherOrganList);
            var myNewOrganList = new List<Cell>();
            var myNewOrganNode = currentNode;
            while(myNewOrganNode.Parent != null)
            {
                myNewOrganSet.Add(myNewOrganNode.Point);
                myNewOrganList.Add(new Cell(myNewOrganNode.Point, myNewOrganNode.Type, 1, myNewOrganNode.OrganId, myNewOrganNode.Direction, 0, 0));
                myNewOrganNode = myNewOrganNode.Parent;
            }

            if(currentNode.Distance > 1)
            {
                var allMyOrganList = new List<Cell>(myOrganList);
                allMyOrganList.AddRange(myNewOrganList);
                foreach(var myOrgan in myOrganList)
                {
                    if(!CheckHarvester(myOrgan.Type))
                        continue;

                    var harvestingPoint = OneMoveToDirection(myOrgan.Point, myOrgan.OrganDir);
                    if(currentNode.DestroyedProtein.Contains(harvestingPoint))
                    {
                        continue;
                    }

                    Map.TryGetValue(harvestingPoint, out var harvestingCell);
                    var harvestingType = harvestingCell?.Type;
                    if(harvestingType == "A")
                    {
                        currentNode.MyProtein.A += 1;
                    }
                    else if(harvestingType == "B")
                    {
                        currentNode.MyProtein.B += 1;
                    }
                    else if(harvestingType == "C")
                    {
                        currentNode.MyProtein.C += 1;
                    }
                    else if(harvestingType == "D")
                    {
                        currentNode.MyProtein.D += 1;
                    }
                } 
            }

            var neighbours = Neighbours(currentNode.Point).ToList();
            if(CheckSporer(currentNode.Type)
            && currentNode.Distance <= 3
            && AllProteinGreaterOrEqual(currentNode.MyProtein, 1 + rootCount))
            {
                var sporePoint = OneMoveToDirection(currentNode.Point, currentNode.Direction);
                int freeLine = FreeLineLength(sporePoint, currentNode.Direction);

                if(freeLine >= 3)
                    for(int i = freeLine; i >= 3; i--)
                    {
                        if(currentNode.Direction == "N" && currentNode.Point.Y - i >= 0)
                        {
                            var freePoint = new Point(currentNode.Point.X, currentNode.Point.Y - i);
                            if(!CheckRootAround(freePoint, 1))
                                neighbours.Add(freePoint);     
                        }
                        else if(currentNode.Direction == "E" && currentNode.Point.X + i < Width)
                        {
                            var freePoint = new Point(currentNode.Point.X + i, currentNode.Point.Y);
                            if(!CheckRootAround(freePoint, 1))
                                neighbours.Add(freePoint); 
                        }
                        else if(currentNode.Direction == "S" && currentNode.Point.Y + i < Height)
                        {
                            var freePoint = new Point(currentNode.Point.X, currentNode.Point.Y + i);
                            if(!CheckRootAround(freePoint, 1))
                                neighbours.Add(freePoint);  
                        }
                        else if(currentNode.Direction == "W" && currentNode.Point.X - i >= 0)
                        {
                            var freePoint = new Point(currentNode.Point.X - i, currentNode.Point.Y);
                            if(!CheckRootAround(freePoint, 1))
                                neighbours.Add(freePoint);    
                        }
                    }
            }

            for (int i = 0; i < neighbours.Count; i++)
            {
                var neighbourPoint = neighbours[i];
                if(myNewOrganSet.Contains(neighbourPoint))
                    continue;

                // todo 5
                int distance = currentNode.Distance + 1;
                if(distance > 5)
                    continue;

                var neighbourNode = new Node(currentNode, currentNode.OrganId, neighbourPoint, distance);
                neighbourNode.MyProtein = new Protein(currentNode.MyProtein.A, currentNode.MyProtein.B, currentNode.MyProtein.C, currentNode.MyProtein.D);

                Map.TryGetValue(neighbourPoint, out var cell);
                var type = cell?.Type;

                if(type == "WALL"
                || AnyOrgan(type))
                    continue;

                int scoreCoeff = currentNode.Score;
                if(!neighbourNode.DestroyedProtein.Contains(neighbourNode.Point))
                    if(type == "A")
                    {
                        if(CheckHarvesterAround(neighbourNode.Point, 1))
                            scoreCoeff -= 5;
                        else
                            scoreCoeff += 1;

                        if(CheckHarvesterAround(neighbourNode.Point, 0))
                            scoreCoeff += 5;

                        neighbourNode.MyProtein.A += 3;
                        neighbourNode.DestroyedProtein.Add(neighbourNode.Point);
                    }
                    else if(type == "B")
                    {
                        if(CheckHarvesterAround(neighbourNode.Point, 1))
                            scoreCoeff -= 10;
                        else
                            scoreCoeff += 1;

                        if(CheckHarvesterAround(neighbourNode.Point, 0))
                            scoreCoeff += 10;

                        neighbourNode.MyProtein.B += 3;
                        neighbourNode.DestroyedProtein.Add(neighbourNode.Point);
                    }
                    else if(type == "C")
                    {
                        if(CheckHarvesterAround(neighbourNode.Point, 1))
                            scoreCoeff -= 10;
                        else
                            scoreCoeff += 1;

                        if(CheckHarvesterAround(neighbourNode.Point, 0))
                            scoreCoeff += 10;

                        neighbourNode.MyProtein.C += 3;
                        neighbourNode.DestroyedProtein.Add(neighbourNode.Point);
                    }
                    else if(type == "D")
                    {
                        if(CheckHarvesterAround(neighbourNode.Point, 1))
                            scoreCoeff -= 10;
                        else
                            scoreCoeff += 1;

                        if(CheckHarvesterAround(neighbourNode.Point, 0))
                            scoreCoeff += 10;

                        neighbourNode.MyProtein.D += 3;
                        neighbourNode.DestroyedProtein.Add(neighbourNode.Point);
                    }

                bool terminate = false;
                var aroundPoints = Neighbours(neighbourPoint);
                foreach(var aroundPoint in aroundPoints)
                {
                    if(myNewOrganSet.Contains(aroundPoint))
                        continue;

                    Map.TryGetValue(aroundPoint, out var aroundCell);
                    var aroundOwner = aroundCell?.Owner ?? -1;
                    var aroundType = aroundCell?.Type;
                    var aroundOrganId = aroundCell?.OrganId ?? 0;

                    if(aroundType == "WALL"
                    || (aroundOwner == 1 
                    && AnyOrgan(aroundType)))
                        continue;

                    if(aroundOwner == 0
                    && neighbourNode.Distance == 1
                    && aroundCell?.OrganDir == InvertDirection(Direction(neighbourNode.Point, aroundPoint))
                    && aroundType == "TENTACLE")
                    {
                        terminate = true;
                        break;
                    }

                    if(i > 3)
                        continue;

                    if(aroundOwner == 0 
                    && AnyOrgan(aroundType)
                    && !CheckTentacleAround(aroundPoint, 1))
                    {
                        var tentacleScore = (scoreCoeff + OppOrganScore[aroundOrganId] * 5) * 5 / neighbourNode.Distance;

                        if(currentNode.MyProtein.B >= 1 
                        && currentNode.MyProtein.C >= 2
                        && neighbourNode.Score < tentacleScore)
                        {
                            neighbourNode.Type = "TENTACLE";
                            neighbourNode.Score = tentacleScore;
                            neighbourNode.Direction = Direction(neighbourNode.Point, aroundPoint);
                        }
                        continue;
                    }

                    if(neighbourNode.Type != "TENTACLE")
                    {
                        var aroundPoints2 = Neighbours(aroundPoint);
                        foreach(var aroundPoint2 in aroundPoints2)
                        {
                            if(myNewOrganSet.Contains(aroundPoint2))
                                continue;

                            Map.TryGetValue(aroundPoint2, out var aroundCell2);
                            var aroundOwner2 = aroundCell2?.Owner ?? -1;
                            var aroundType2 = aroundCell2?.Type;

                            if(aroundType2 == "WALL"
                            || (aroundOwner2 == 1 
                            && AnyOrgan(aroundType2)))
                                continue;

                            if(aroundOwner2 == 0 
                            && AnyOrgan(aroundType2)
                            && !CheckTentacleAround(aroundPoint, 1))
                            {
                                if(currentNode.MyProtein.B >= 1 
                                && currentNode.MyProtein.C >= 2)
                                {
                                    neighbourNode.Type = "TENTACLE";
                                    neighbourNode.Score = (scoreCoeff + 20) * 5 / neighbourNode.Distance;
                                    neighbourNode.Direction = Direction(neighbourNode.Point, aroundPoint);
                                }
                                break;
                            }
                            ///
                            var aroundPoints3 = Neighbours(aroundPoint2);
                            foreach(var aroundPoint3 in aroundPoints3)
                            {
                                if(myNewOrganSet.Contains(aroundPoint3))
                                    continue;

                                Map.TryGetValue(aroundPoint3, out var aroundCell3);
                                var aroundOwner3 = aroundCell3?.Owner ?? -1;
                                var aroundType3 = aroundCell3?.Type;

                                if(aroundType3 == "WALL"
                                || (aroundOwner3 == 1 
                                && AnyOrgan(aroundType3)))
                                    continue;

                                if(aroundOwner3 == 0 
                                && AnyOrgan(aroundType3)
                                && !CheckTentacleAround(aroundPoint, 1))
                                {
                                    if(currentNode.MyProtein.B >= 1 
                                    && currentNode.MyProtein.C >= 2)
                                    {
                                        neighbourNode.Type = "TENTACLE";
                                        neighbourNode.Score = (scoreCoeff + 10) * 5 / neighbourNode.Distance;
                                        neighbourNode.Direction = Direction(neighbourNode.Point, aroundPoint);
                                    }
                                    break;
                                }
                            }
                            ///
                        }
                    }

                    if(!CheckHarvesterAround(aroundPoint)
                    && (aroundType == "A" 
                    || aroundType == "B" 
                    || aroundType == "C" 
                    || aroundType == "D"))
                    {
                        if(currentNode.MyProtein.C >= 1 
                        && currentNode.MyProtein.D >= 1)
                        {
                            int genCoeff = 0;
                            if(aroundType == "A")
                            {
                                if(MyHarvesterGen.A == 0)
                                    genCoeff = 5;
                                else if(MyHarvesterGen.A == 1)
                                    genCoeff = 0;
                            }
                            else if(aroundType == "B")
                            {
                                if(MyHarvesterGen.B == 0)
                                    genCoeff = 10;
                                else if(MyHarvesterGen.B == 1)
                                    genCoeff = 1;
                            }
                            else if(aroundType == "C")
                            {
                                if(MyHarvesterGen.C == 0)
                                    genCoeff = 10;
                                else if(MyHarvesterGen.C == 1)
                                    genCoeff = 1;
                            }
                            else if(aroundType == "D")
                            {
                                if(MyHarvesterGen.D == 0)
                                    genCoeff = 10;
                                else if(MyHarvesterGen.D == 1)
                                    genCoeff = 1;
                            }

                            var harvesterScore = (scoreCoeff + genCoeff) * 5 / neighbourNode.Distance;
                            if(neighbourNode.Score < harvesterScore)
                            {
                                neighbourNode.Type = "HARVESTER";
                                neighbourNode.Score = harvesterScore;
                                neighbourNode.Direction = Direction(neighbourNode.Point, aroundPoint);
                            }
                        }
                    }

                    string freeLineDirection = Direction(neighbourNode.Point, aroundPoint);
                    int freeLineLength = FreeLineLength(neighbourNode.Point, freeLineDirection);
                    var sporerScore = (scoreCoeff + freeLineLength) * 5 / neighbourNode.Distance;
                    if(freeLineLength >= 4
                    && AllProteinGreaterOrEqual(currentNode.MyProtein, 2 + rootCount)
                    && neighbourNode.Score < sporerScore)
                    {
                        neighbourNode.Type = "SPORER";
                        neighbourNode.Score = sporerScore;
                        neighbourNode.Direction = Direction(neighbourNode.Point, aroundPoint);                
                    }

                }

                if(terminate)
                    continue;

                if(i > 3)
                {
                    var halfMapPoint = new Point(Width/2, Height/2);
                    var d1 = Distance(neighbourNode.Parent.Point, halfMapPoint);
                    var d2 = Distance(neighbourNode.Point, halfMapPoint);
                    var moveScore = d2 < d1 ? 20 : 10;

                    neighbourNode.Score = (scoreCoeff + moveScore + Distance(neighbourNode.Parent.Point, neighbourNode.Point)) * 5 / neighbourNode.Distance;
                    neighbourNode.Type = "SPORE";
                }

                if(string.IsNullOrEmpty(neighbourNode.Type))
                {
                    if(currentNode.MyProtein.B >= 10
                    && currentNode.MyProtein.C >= 10)
                        neighbourNode.Type = "TENTACLE";
                    else if(currentNode.MyProtein.A >= 1)
                        neighbourNode.Type = "BASIC";                     
                    else if( currentNode.MyProtein.B >= 1 
                    && currentNode.MyProtein.C >= 1 
                    && currentNode.MyProtein.B >= currentNode.MyProtein.D
                    && currentNode.MyProtein.C >= currentNode.MyProtein.D)
                        neighbourNode.Type = "TENTACLE";
                    else if(currentNode.MyProtein.B >= 1 
                    && currentNode.MyProtein.D >= 1
                    && currentNode.MyProtein.B >= currentNode.MyProtein.C
                    && currentNode.MyProtein.D >= currentNode.MyProtein.C)
                        neighbourNode.Type = "SPORER";   
                    else if(currentNode.MyProtein.C >= 1 
                    && currentNode.MyProtein.D >= 1
                    && currentNode.MyProtein.C >= currentNode.MyProtein.B
                    && currentNode.MyProtein.D >= currentNode.MyProtein.B)
                        neighbourNode.Type = "HARVESTER";

                    if(string.IsNullOrEmpty(neighbourNode.Type))
                        continue;

                    var halfMapPoint = new Point(Width/2, Height/2);
                    var d1 = Distance(neighbourNode.Parent.Point, halfMapPoint);
                    var d2 = Distance(neighbourNode.Point, halfMapPoint);
                    var moveScore = d2 < d1 ? 1 : 0;

                    neighbourNode.Score = scoreCoeff + moveScore;
                    neighbourNode.Direction = Direction(currentNode.Point, neighbourNode.Point);
                }

                if(neighbourNode.Type == "BASIC")
                {
                    neighbourNode.MyProtein.A -= 1;
                }    
                else if(neighbourNode.Type == "HARVESTER")
                {
                    neighbourNode.MyProtein.C -= 1;
                    neighbourNode.MyProtein.D -= 1;
                }
                else if(neighbourNode.Type == "TENTACLE")
                {
                    neighbourNode.MyProtein.B -= 1;
                    neighbourNode.MyProtein.C -= 1;
                }
                else if(neighbourNode.Type == "SPORER")
                {
                    neighbourNode.MyProtein.B -= 1;
                    neighbourNode.MyProtein.D -= 1;
                }
                else if(neighbourNode.Type == "SPORE")
                {
                    neighbourNode.MyProtein.A -= 1;
                    neighbourNode.MyProtein.B -= 1;
                    neighbourNode.MyProtein.C -= 1;
                    neighbourNode.MyProtein.D -= 1;
                }

                if(neighbourNode.Score > maxScore)
                {
                    maxScore = neighbourNode.Score;
                    bestNode = neighbourNode;
                }

                fifo.Enqueue(neighbourNode);
            }
        }

        return bestNode;
    }

    static int Distance(Point from, Point to) =>
        Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);

    static string Direction(Point from, Point to)
    {
        if(from.Y > to.Y)
            return "N";

        if(from.X < to.X)
            return "E";

        if(from.Y < to.Y)
            return "S";

        if(from.X > to.X)
            return "W";

        return "";
    }

    static string InvertDirection(string direction)
    {
        if(direction == "N")
            return "S";

        if(direction == "E")
            return "W";

        if(direction == "S")
            return "N";

        if(direction == "W")
            return "E";

        return "";
    }

    static IEnumerable<Point> Neighbours(Point point)
    {
        // Up - North
        if (point.Y - 1 >= 0)
        {
            yield return new Point(point.X, point.Y - 1);
        }

        // Right - East
        if (point.X + 1 < Width)
        {
            yield return new Point(point.X + 1, point.Y);
        }

        // Down - South
        if (point.Y + 1 < Height)
        {
            yield return new Point(point.X, point.Y + 1);
        }

        // Left - West
        if (point.X - 1 >= 0)
        {
            yield return new Point(point.X - 1, point.Y);
        }
    }

    record Cell(Point Point, string Type, int Owner, int OrganId, string OrganDir, int OrganParentId, int OrganRootId);

    record Protein(int a, int b, int c, int d)
    {
        public int A { get; set; } = a;
        public int B { get; set; } = b;
        public int C { get; set; } = c;
        public int D { get; set; } = d;
    };

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        Width = int.Parse(inputs[0]); // columns in the game grid
        Height = int.Parse(inputs[1]); // rows in the game grid

        int turn = 1;
        Stopwatch stopWatch = new Stopwatch();

        while (true)
        {
            int entityCount = int.Parse(Console.ReadLine());
            Console.Error.WriteLine($"entityCount = {entityCount}");

            Map = new Dictionary<Point, Cell>();
            List<Cell> list = new List<Cell>();
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int x = int.Parse(inputs[0]);
                int y = int.Parse(inputs[1]); // grid coordinate

                string type = inputs[2]; // WALL, ROOT, BASIC, TENTACLE, HARVESTER, SPORER, A, B, C, D
                int owner = int.Parse(inputs[3]); // 1 if your organ, 0 if enemy organ, -1 if neither
                int organId = int.Parse(inputs[4]); // id of this entity if it's an organ, 0 otherwise
                string organDir = inputs[5]; // N,E,S,W or X if not an organ
                int organParentId = int.Parse(inputs[6]);
                int organRootId = int.Parse(inputs[7]);

                var point = new Point(x, y);
                var cell = new Cell(point, type, owner, organId, organDir, organParentId, organRootId);

                list.Add(cell);
                Map.Add(point, cell);
            }

            inputs = Console.ReadLine().Split(' ');
            MyProtein = new Protein(int.Parse(inputs[0]), int.Parse(inputs[1]), int.Parse(inputs[2]), int.Parse(inputs[3]));

            inputs = Console.ReadLine().Split(' ');
            OppProtein = new Protein(int.Parse(inputs[0]), int.Parse(inputs[1]), int.Parse(inputs[2]), int.Parse(inputs[3]));

            var myOrganRootCount = list.Count(x => x.Owner == 1 && CheckRoot(x.Type));
            var myOrganGroups = list.Where(x => x.Owner == 1).GroupBy(x => x.OrganRootId).OrderByDescending(x => x.Key).ToList();
            int requiredActionsCount = int.Parse(Console.ReadLine()); // your number of organisms, output an action for each one in any order
            var myProtein = new Protein(MyProtein.A, MyProtein.B, MyProtein.C, MyProtein.D);

            OppOrganScore = new Dictionary<int, int>();
            var oppOrganGroups = list.Where(x => x.Owner == 0).GroupBy(x => x.OrganRootId);
            foreach(var oppOrganGroup in oppOrganGroups)
            {
                var oppOrgans = oppOrganGroup.OrderBy(x => x.OrganId).ToList();
                int score = oppOrgans.Count;
                foreach(var oppOrgan in oppOrgans)
                {
                    OppOrganScore.Add(oppOrgan.OrganId, oppOrgan.OrganId == oppOrgan.OrganRootId ? 1000 + score : score);
                    score--;
                }
            }

            var myHarvesterList = list.Where(x => x.Owner == 1 && CheckHarvester(x.Type));
            MyHarvesterGen = new Protein(0, 0, 0, 0);

            foreach(var myHarvester in myHarvesterList)
            {
                var harvestingPoint = OneMoveToDirection(myHarvester.Point, myHarvester.OrganDir);

                Map.TryGetValue(harvestingPoint, out var harvestingCell);
                var harvestingType = harvestingCell?.Type;
                if(harvestingType == "A")
                    MyHarvesterGen.A += 1;
                else if(harvestingType == "B")
                    MyHarvesterGen.B += 1;
                else if(harvestingType == "C")
                    MyHarvesterGen.C += 1;
                else if(harvestingType == "D")
                    MyHarvesterGen.D += 1;
            } 

            stopWatch.Restart();

            int waitCount = 0;
            var myOtherOrgansList = new List<Point>();
            for (int i = 0; i < requiredActionsCount; i++)
            {
                var path = GetPath(myOrganGroups[i].ToList(), myOtherOrgansList, myProtein, myOrganRootCount, stopWatch, turn);
                if(path == null)
                {
                    Console.WriteLine($"WAIT");
                    waitCount++;
                    continue;
                }

                var nextNode = path;
                while(path.Parent != null)
                {
                    Console.Error.Write($"{path} ");
                    myOtherOrgansList.Add(path.Point);
                    nextNode = path;
                    path = path.Parent;
                }
                Console.Error.WriteLine();

                myProtein = new Protein(nextNode.MyProtein.A, nextNode.MyProtein.B, nextNode.MyProtein.C, nextNode.MyProtein.D);

                if(nextNode.Type == "SPORE")
                    Console.WriteLine($"SPORE {nextNode.OrganId} {nextNode.Point.X} {nextNode.Point.Y}");
                else
                    Console.WriteLine($"GROW {nextNode.OrganId} {nextNode.Point.X} {nextNode.Point.Y} {nextNode.Type} {nextNode.Direction}");
            }

            LastMoveAllWait = waitCount == requiredActionsCount;

            turn++;
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;    
            Console.Error.WriteLine($"Turn = {turn} TotalTime {ts.TotalMilliseconds}ms");
        }
    }
}