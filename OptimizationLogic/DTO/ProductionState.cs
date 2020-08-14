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
                throw new Exception("Incorrect input matrix shape");
            }
        }

        public void LoadTimeMatrix(string csvPath)
        {
            string[] lines = File.ReadAllLines(csvPath);
            CheckCorrectInputDimension(lines, TimeMatrixDimension);
            
            for (int i = 0; i < lines.Length; i++)
            {
                String[] row = lines[i].Split(';');
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
                String[] row = lines[i].Split(';');
                CheckCorrectInputDimension(row, WarehouseXDimension);
                for (int j = 0; j < row.Length; j++)
                {
                    WarehouseState[i, j] = GetItemState(row[j]);
                }
            }
        }

        public void LoadProductionHistory(string csvPath) => ProductionHistory = new Queue<ItemState>(File.ReadLines(csvPath).Select(GetItemState));
        public void LoadFutureProductionPlan(string csvPath) => FutureProductionPlan = new Queue<ItemState>(File.ReadLines(csvPath).Select(GetItemState));


        public object Clone()
        {
            ProductionState productionStateCopy = new ProductionState();
            productionStateCopy.FutureProductionPlan = new Queue<ItemState>(this.FutureProductionPlan);
            productionStateCopy.ProductionHistory = new Queue<ItemState>(this.ProductionHistory);
            productionStateCopy.WarehouseState = (ItemState[,])this.WarehouseState.Clone();
            productionStateCopy.TimeMatrix = this.TimeMatrix;
            productionStateCopy.ProductionStateIsOk = this.ProductionStateIsOk;
            productionStateCopy.StepCounter = this.StepCounter;
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
            String warehouseToString = "";
            foreach (KeyValuePair<ItemState, int> kvp in occurancesDict)
            {
                warehouseToString += string.Format("Key = {0}, Value = {1}\n", kvp.Key, kvp.Value);
            }

            return String.Format("StepCounter={0}, ProductionStateIsOk={1}, FutureProductionPlan len={2}, FutureProductionPlan head={3}, ProductionHistory len={4}, ProductionHistory head={5}\nWarehouse occurances:\n{6}",
                StepCounter, ProductionStateIsOk, FutureProductionPlan.Count, FutureProductionPlan.Peek(), ProductionHistory.Count, ProductionHistory.Peek(), warehouseToString);
        }
    }
}
