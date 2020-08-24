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
        public ItemState[,] WarehouseState { get; set; } = new ItemState[WarehouseYDimension, WarehouseXDimension];
        public double[,] TimeMatrix { get; set; } = new double[TimeMatrixDimension, TimeMatrixDimension];
        public bool ProductionStateIsOk { get; set; } = true;
        public int StepCounter { get; set; } = 1;
        public double CurrentStepTime { get; set; } = 0;
        public double TimeSpentInSimulation { get; set; } = 0;

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

        public int TimeMatrixDimensionPub
        {
            get
            {
                return TimeMatrixDimension;
            }
        }

        private const int TimeMatrixDimension = 47;
        private const int WarehouseXDimension = 12;
        private const int WarehouseYDimension = 4;
        private Dictionary<PositionCodes, (int row, int col)> _warehousePositionMapping;
        private Dictionary<(int row, int col), PositionCodes> _warehousePositionMappingReverse;
        private Dictionary<PositionCodes, int> _timeMatrixMapping;

        public ProductionState()
        {
            _warehousePositionMapping = BuildWarehousePositionMappingDict();
            var s = _warehousePositionMapping.Select(t => t.Value).GroupBy(t => t);
            _warehousePositionMappingReverse = BuildWarehousePositionMappingReverseDict();
            _timeMatrixMapping = BuildTimeMatrixPositionMappingDict();
            FillForbidden();
        }

        public void FillForbidden()
        {
            WarehouseState[2, 0] = ItemState.Forbidden;
            WarehouseState[3, 0] = ItemState.Forbidden;
            WarehouseState[2, WarehouseState.GetLength(1) - 1] = ItemState.Forbidden;
            WarehouseState[3, WarehouseState.GetLength(1) - 1] = ItemState.Forbidden;
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
            return ((PositionCodes[])Enum.GetValues(typeof(PositionCodes))).ToDictionary(code => code, code => {
                if (code == PositionCodes.Stacker)
                {
                    return (2, 0);
                }
                else if (code == PositionCodes.Service)
                {
                    return (2, 11);
                }
                else
                {
                    (int numericalPart, char alphabetPart) = ParsePositionCode(code);
                    int col = (numericalPart - (numericalPart % 2 == 0 ? 0 : 1)) / 2;
                    int row = (alphabetPart == 'A' ? 1 : 0) + (numericalPart % 2 == 0 ? 2 : 0);
                    return (row, col);
                }
            });
        }

        private Dictionary<(int row, int col), PositionCodes> BuildWarehousePositionMappingReverseDict()
        {
            var res = _warehousePositionMapping.ToDictionary(x => x.Value, x => x.Key);
            foreach (char alphabetPart in new[] { 'A', 'B' })
            {
                int numericalPart = 22;
                int col = (numericalPart - (numericalPart % 2 == 0 ? 0 : 1)) / 2;
                int row = (alphabetPart == 'A' ? 1 : 0) + (numericalPart % 2 == 0 ? 2 : 0);
                res[(row, col)] = PositionCodes.Service;
            }
            res[(3, 0)] = PositionCodes.Stacker;
            return res;
        }

        private (int numericalPart, char alphabetPart) ParsePositionCode(PositionCodes code) {
            var pureCode = code.ToString().Remove(0, 1);
            var numericalPart = int.Parse(pureCode.Remove(pureCode.Length - 1));
            var alphabetPart = pureCode[pureCode.Length - 1];
            return (numericalPart, alphabetPart);
        }

        private Dictionary<PositionCodes, int> BuildTimeMatrixPositionMappingDict() => _timeMatrixMapping = ((PositionCodes[])Enum.GetValues(typeof(PositionCodes))).ToDictionary(code => code, code =>
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
        });

        public int GetTimeMatrixIndex(PositionCodes code) => _timeMatrixMapping[code];
        public (int row, int col) GetWarehouseIndex(PositionCodes code) => _warehousePositionMapping[code];
        public PositionCodes GetWarehouseCell(int row, int col) => _warehousePositionMappingReverse[(row, col)];

        private void CheckCorrectInputDimension(string[] arr, int correctLen)
        {
            if (arr.Length != correctLen)
            {
                throw new ArgumentException("Incorrect input matrix shape");
            }
        }

        public void LoadTimeMatrix(string csvPath)
        {
            string[] lines = File.ReadAllLines(csvPath);
            CheckCorrectInputDimension(lines, TimeMatrixDimension);
            
            for (int i = 0; i < lines.Length; i++)
            {
                string[] row = lines[i].Split(';');
                CheckCorrectInputDimension(row, TimeMatrixDimension); 
                for (int j = 0; j < row.Length; j++)
                {
                    TimeMatrix[i, j] = double.Parse(row[j], CultureInfo.InvariantCulture);
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
        {
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
        public void LoadFutureProductionPlan(string csvPath) => FutureProductionPlan = new Queue<ItemState>(File.ReadLines(csvPath).Select(GetItemState));

        public List<Tuple<PositionCodes, PositionCodes>> GetAvailableWarehouseSwaps()
        {
            Dictionary<ItemState, List<PositionCodes>> dict = new Dictionary<ItemState, List<PositionCodes>>();
            foreach (ItemState itemState in Enum.GetValues(typeof(ItemState)))
            {
                dict[itemState] = new List<PositionCodes>();
            }
            foreach (PositionCodes positionCode in Enum.GetValues(typeof(PositionCodes)))
            {
                if (positionCode != PositionCodes.Service && positionCode != PositionCodes.Stacker)
                {
                    (int row, int col) = GetWarehouseIndex(positionCode);
                    dict[WarehouseState[row, col]].Add(positionCode);
                }
            }

            List<Tuple<PositionCodes, PositionCodes>> availableSwaps = new List<Tuple<PositionCodes, PositionCodes>>();
            var query = dict[ItemState.Empty].SelectMany(x => dict[ItemState.MEB], (x, y) => new Tuple<PositionCodes, PositionCodes>(x, y));
            foreach (var item in query)
            {
                availableSwaps.Add(item);
            }
            query = dict[ItemState.Empty].SelectMany(x => dict[ItemState.MQB], (x, y) => new Tuple<PositionCodes, PositionCodes>(x, y));
            foreach (var item in query)
            {
                availableSwaps.Add(item);
            }
            query = dict[ItemState.MQB].SelectMany(x => dict[ItemState.MEB], (x, y) => new Tuple<PositionCodes, PositionCodes>(x, y));
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
            return TimeMatrix[GetTimeMatrixIndex(pos1), GetTimeMatrixIndex(pos2)];
        }

        public object Clone()
        {
            ProductionState productionStateCopy = new ProductionState();
            productionStateCopy.FutureProductionPlan = new Queue<ItemState>(this.FutureProductionPlan);
            productionStateCopy.ProductionHistory = new Queue<ItemState>(this.ProductionHistory);
            productionStateCopy.WarehouseState = (ItemState[,])this.WarehouseState.Clone();
            productionStateCopy.TimeMatrix = this.TimeMatrix;
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
