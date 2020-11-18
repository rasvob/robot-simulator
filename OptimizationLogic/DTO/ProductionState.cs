using OptimizationLogic.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.DTO
{
    public class ProductionState : ICloneable
    {
        public Queue<ItemState> FutureProductionPlan { get; set; } = new Queue<ItemState>();
        public Queue<ItemState> ProductionHistory { get; set; } = new Queue<ItemState>();
        public ItemState[,] WarehouseState { get; set; }
        public Dictionary<PositionCodes, Dictionary<PositionCodes, double>> TimeDictionary { get; set; }
        public bool ProductionStateIsOk { get; set; } = true;
        public int StepCounter { get; set; } = 1;
        public double CurrentStepTime { get; set; } = 0;
        public double TimeSpentInSimulation { get; set; } = 0;
        public int InitialFutureProductionPlanLen { get; set; } = 0;
        public int WarehouseXDimension { get; }
        public int WarehouseYDimension { get; }

        private static readonly int _forbiddedSpotsCount = 4;

        public bool SimulationFinished { get => FutureProductionPlan.Count == 0; }

        public int WarehouseColls
        {
            get
            {
                return WarehouseXDimension;
            }
        }

        public int WarehouseRows
        {
            get
            {
                return WarehouseYDimension;
            }
        }

        private Dictionary<PositionCodes, (int row, int col)> _warehousePositionMapping;
        private Dictionary<(int row, int col), PositionCodes> _warehousePositionMappingReverse;

        public ItemState this[PositionCodes index]
        {
            get
            {
                (int r, int c) = GetWarehouseIndex(index);
                return WarehouseState[r, c];
            }
            set
            {
                (int r, int c) = GetWarehouseIndex(index);
                WarehouseState[r, c] = value;
            }
        }

        public double this[PositionCodes from, PositionCodes to]
        {
            get
            {
                return TimeDictionary[from][to];
            }
            private set
            {
                TimeDictionary[from][to] = value;
            }
        }

        public double GetDistanceFromStacker(int row, int col)
        {
            return TimeDictionary[PositionCodes.Stacker][GetWarehouseCell(row, col)];
        }

        public static int ComputeNeededColumnsInWarehouse(int rows, int dominantTypeCount, int nonDominantTypeCount, int freeSpots, int pastProductionQueueLength) => (int)Math.Ceiling((dominantTypeCount + nonDominantTypeCount + freeSpots + _forbiddedSpotsCount - pastProductionQueueLength) / (double)rows);
        //TODO: Check computations
        public static int ComputeDominantTypeItemsCount(int pastProductionQueueLength) => pastProductionQueueLength + 2;
        //TODO: Check computations
        public static int ComputeNonDominantTypeItemsCount(int pastProductionQueueLength, int maxNonDominantInARow) => maxNonDominantInARow == 0 ? ComputeDominantTypeItemsCount(pastProductionQueueLength) : (int)Math.Ceiling(ComputeDominantTypeItemsCount(pastProductionQueueLength) / (double)(maxNonDominantInARow + 1)) + 2;

        public ProductionState(int WarehouseXDimension, int WarehouseYDimension)
        {
            WarehouseState = new ItemState[WarehouseYDimension, WarehouseXDimension];
            TimeDictionary = new Dictionary<PositionCodes, Dictionary<PositionCodes, double>>();
            //TimeMatrix = new double[TimeMatrixDimension, TimeMatrixDimension];
            this.WarehouseXDimension = WarehouseXDimension;
            this.WarehouseYDimension = WarehouseYDimension;
            _warehousePositionMapping = BuildWarehousePositionMappingDict();
            var s = _warehousePositionMapping.Select(t => t.Value).GroupBy(t => t);
            _warehousePositionMappingReverse = BuildWarehousePositionMappingReverseDict();
            //_timeMatrixMapping = BuildTimeMatrixPositionMappingDict();
            BuildTimeDictionary();
            FillForbidden();
        }

        public void FillForbidden()
        {
            for (int i = WarehouseYDimension / 2; i < WarehouseYDimension; i++)
            {
                WarehouseState[i, 0] = ItemState.Forbidden;
                WarehouseState[i, WarehouseXDimension - 1] = ItemState.Forbidden;
            }
        }

        public void ResetState()
        {
            StepCounter = 0;
            ProductionStateIsOk = true;
            CurrentStepTime = 0;
            TimeSpentInSimulation = 0;
        }

        private Dictionary<PositionCodes, (int row, int col)> BuildWarehousePositionMappingDict()
        {
            return ((PositionCodes[])Enum.GetValues(typeof(PositionCodes))).FilterPositions(WarehouseRows, WarehouseColls).ToDictionary(code => code, code =>
            {
                if (code == PositionCodes.Stacker)
                {
                    return (WarehouseYDimension - 1, 0);
                }
                else if (code == PositionCodes.Service)
                {
                    return (WarehouseYDimension - 1, WarehouseXDimension - 1);
                }
                else
                {
                    (int numericalPart, char alphabetPart) = ParsePositionCode(code);
                    int col = (numericalPart - (numericalPart % 2 == 0 ? 0 : 1)) / 2;
                    int row = (numericalPart % 2 == 0 ? WarehouseYDimension - 1 : WarehouseYDimension / 2 - 1) - (alphabetPart - 'A');
                    return (row, col);
                }
            });
        }

        private Dictionary<(int row, int col), PositionCodes> BuildWarehousePositionMappingReverseDict()
        {

            var res = _warehousePositionMapping.ToDictionary(x => x.Value, x => x.Key);
            for (int i = WarehouseYDimension / 2; i < WarehouseYDimension; i++)
            {
                res[(i, WarehouseXDimension - 1)] = PositionCodes.Service;
                res[(i, 0)] = PositionCodes.Stacker;
            }

            // TODO: verify this method; nejsem si jist co tady zmenit v cem se lisi oproti pouhemu otoceni, tzn proc pridavat na num part 22 Service a na 3,0 Stacker (pravdepodobne doplneni pro kompletnost)
            /*foreach (char alphabetPart in new[] { 'A', 'B' })
            {
                int numericalPart = 22;
                int col = (numericalPart - (numericalPart % 2 == 0 ? 0 : 1)) / 2;
                int row = (alphabetPart == 'A' ? 1 : 0) + (numericalPart % 2 == 0 ? 2 : 0);
                res[(row, col)] = PositionCodes.Service;
            }
            res[(3, 0)] = PositionCodes.Stacker;*/
            return res;
        }

        private (int numericalPart, char alphabetPart) ParsePositionCode(PositionCodes code)
        {
            var pureCode = code.ToString().Remove(0, 1);
            var numericalPart = int.Parse(pureCode.Remove(pureCode.Length - 1));
            var alphabetPart = pureCode[pureCode.Length - 1];
            return (numericalPart, alphabetPart);
        }

        /*private Dictionary<PositionCodes, int> BuildTimeMatrixPositionMappingDict() => _timeMatrixMapping = ((PositionCodes[])Enum.GetValues(typeof(PositionCodes))).ToDictionary(code => code, code =>
        {
            if (code == PositionCodes.Stacker)
            {
                return 0;
            }
            else if (code == PositionCodes.Service)
            {
                return TimeMatrix.GetLength(0) - 3;
            }
            else
            {
                (int numericalPart, char alphabetPart) = ParsePositionCode(code);
                return numericalPart * 2 - (alphabetPart == 'A' ? 1 : 0);
            }
        });*/

        //public int GetTimeMatrixIndex(PositionCodes code) => _timeMatrixMapping[code];
        public (int row, int col) GetWarehouseIndex(PositionCodes code) => _warehousePositionMapping[code];
        public PositionCodes GetWarehouseCell(int row, int col) => _warehousePositionMappingReverse[(row, col)];

        private void CheckCorrectInputDimension(string[] arr, int correctLen)
        {
            if (arr.Length != correctLen)
            {
                throw new ArgumentException("Incorrect input matrix shape");
            }
        }

        private double CalculateTime(PositionCodes from, PositionCodes to)
        {
            if (from == to)
            {
                return 0;
            }

            (int fromRow, int fromCol) = GetWarehouseIndex(from);
            (int toRow, int toCol) = GetWarehouseIndex(to);
            const double operationTime = 5;
            const double xMoveTime = 2.3;
            const double yMoveTime = 3.5;

            int yMiddleSize = WarehouseYDimension / 2;
            int xMovement = Math.Abs(fromCol - toCol);
            int yMovement = Math.Abs(Math.Abs(fromRow % yMiddleSize) - Math.Abs(toRow % yMiddleSize));

            double time = operationTime + xMovement * xMoveTime;
            if (xMovement < 8 && yMovement > 1)
            {
                time += 2 * yMoveTime;
            }
            else if (xMovement < 4 && yMovement > 0)
            {
                time += yMoveTime;
            }
            return time;
        }

        public void BuildTimeDictionary()
        {
            foreach (var position1 in ((PositionCodes[])Enum.GetValues(typeof(PositionCodes))).FilterPositions(WarehouseRows, WarehouseColls))
            {
                TimeDictionary[position1] = new Dictionary<PositionCodes, double>();
                foreach (var position2 in ((PositionCodes[])Enum.GetValues(typeof(PositionCodes))).FilterPositions(WarehouseRows, WarehouseColls))
                {
                    TimeDictionary[position1][position2] = CalculateTime(position1, position2);
                }
            }
        }

        private ItemState GetItemState(string str) => str switch
        {
            "MQB" => ItemState.MQB,
            "MEB" => ItemState.MEB,
            "Forbidden" => ItemState.Forbidden,
            "Empty" => ItemState.Empty,
            _ => throw new ArgumentException("Wrong item state in warehouse input state."),
        };

        public void LoadWarehouseState(string csvPath)
        { // TODO : zkontrolovat zda je nutny refactoring
            string[] lines = File.ReadAllLines(csvPath);
            CheckCorrectInputDimension(lines, WarehouseYDimension);

            for (int i = 0; i < lines.Length; i++)
            {
                string[] row = lines[i].Split(';');
                CheckCorrectInputDimension(row, WarehouseXDimension);
                for (int j = 0; j < row.Length; j++)
                {
                    WarehouseState[i, j] = GetItemState(row[j]);
                }
            }
        }

        public void LoadProductionHistory(string csvPath) => ProductionHistory = new Queue<ItemState>(File.ReadLines(csvPath).Select(GetItemState));
        public void LoadFutureProductionPlan(string csvPath)
        {
            FutureProductionPlan = new Queue<ItemState>(File.ReadLines(csvPath).Select(GetItemState));
            InitialFutureProductionPlanLen = FutureProductionPlan.Count;
        }

        public void SaveProductionState(string path, string suffix)
        {
            Directory.CreateDirectory(Path.Combine(path, $@"generated_situation{suffix}"));
            File.WriteAllLines(Path.Combine(path, $@"generated_situation{suffix}\FutureProductionList{suffix}.txt"), FutureProductionPlan.Select(x => x.ToString()).ToList());
            File.WriteAllLines(Path.Combine(path, $@"generated_situation{suffix}\HistoricalProductionList{suffix}.txt"), ProductionHistory.Select(x => x.ToString()).ToList());

            string[] lines = new string[WarehouseYDimension];
            for (int i = 0; i < WarehouseYDimension; i++)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = 0; j < WarehouseXDimension; j++)
                {
                    if (j > 0)
                    {
                        sb.Append(';');
                    }
                    sb.Append(WarehouseState[i, j].ToString());
                }
                lines[i] = sb.ToString();
            }
            File.WriteAllLines(Path.Combine(path, $@"generated_situation{suffix}\WarehouseInitialState{suffix}.csv"), lines);
        }
        public List<Tuple<PositionCodes, PositionCodes>> GetAvailableWarehouseSwaps()
        {
            Dictionary<ItemState, List<PositionCodes>> dict = new Dictionary<ItemState, List<PositionCodes>>();
            foreach (ItemState itemState in Enum.GetValues(typeof(ItemState)))
            {
                dict[itemState] = new List<PositionCodes>();
            }
            foreach (PositionCodes positionCode in ((PositionCodes[])Enum.GetValues(typeof(PositionCodes))).FilterPositions(this.WarehouseRows, this.WarehouseColls))
            {
                if (positionCode != PositionCodes.Service && positionCode != PositionCodes.Stacker)
                {
                    (int row, int col) = GetWarehouseIndex(positionCode);
                    dict[WarehouseState[row, col]].Add(positionCode);
                }
            }

            List<Tuple<PositionCodes, PositionCodes>> availableSwaps = new List<Tuple<PositionCodes, PositionCodes>>();
            var query = dict[ItemState.MEB].SelectMany(x => dict[ItemState.Empty], (x, y) => new Tuple<PositionCodes, PositionCodes>(x, y));
            foreach (var item in query)
            {
                availableSwaps.Add(item);
            }
            query = dict[ItemState.MQB].SelectMany(x => dict[ItemState.Empty], (x, y) => new Tuple<PositionCodes, PositionCodes>(x, y));
            foreach (var item in query)
            {
                availableSwaps.Add(item);
            }

            return availableSwaps;
        }

        public double SwapWarehouseItems(PositionCodes pos1, PositionCodes pos2)
        {
            (int row1, int col1) = GetWarehouseIndex(pos1);
            var item1 = WarehouseState[row1, col1];
            (int row2, int col2) = GetWarehouseIndex(pos2);
            var item2 = WarehouseState[row2, col2];
            WarehouseState[row1, col1] = item2;
            WarehouseState[row2, col2] = item1;
            return TimeDictionary[pos1][pos2];
        }

        public object Clone()
        {
            ProductionState productionStateCopy = new ProductionState(WarehouseXDimension, WarehouseYDimension);
            productionStateCopy.FutureProductionPlan = new Queue<ItemState>(this.FutureProductionPlan);
            productionStateCopy.ProductionHistory = new Queue<ItemState>(this.ProductionHistory);
            productionStateCopy.WarehouseState = (ItemState[,])this.WarehouseState.Clone();
            productionStateCopy.TimeDictionary = this.TimeDictionary;
            productionStateCopy.ProductionStateIsOk = this.ProductionStateIsOk;
            productionStateCopy.StepCounter = this.StepCounter;
            productionStateCopy.CurrentStepTime = this.CurrentStepTime;
            productionStateCopy.TimeSpentInSimulation = this.TimeSpentInSimulation;
            return productionStateCopy;
        }

        public override string ToString()
        {
            Dictionary<ItemState, int> occurancesDict = new Dictionary<ItemState, int>();
            for (int i = 0; i < WarehouseYDimension; i++)
            {
                for (int j = 0; j < WarehouseXDimension; j++)
                {
                    if (!occurancesDict.ContainsKey(WarehouseState[i, j]))
                    {
                        occurancesDict[WarehouseState[i, j]] = 0;
                    }
                    occurancesDict[WarehouseState[i, j]]++;
                }
            }
            var sb = new StringBuilder();
            foreach (KeyValuePair<ItemState, int> kvp in occurancesDict)
            {
                sb.Append("Key = ").Append(kvp.Key).Append(", Value = ").Append(kvp.Value).Append('\n');
            }
            string warehouseToString = sb.ToString();

            return $"StepCounter={StepCounter}, ProductionStateIsOk={ProductionStateIsOk}, FutureProductionPlan len={FutureProductionPlan.Count}, FutureProductionPlan head={FutureProductionPlan.Peek()}, ProductionHistory len={ProductionHistory.Count}, ProductionHistory head={ProductionHistory.Peek()}\nWarehouse occurances:\n{warehouseToString}";
        }
    }
}
