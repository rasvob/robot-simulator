using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.DTO
{
    public class ProductionState
    {
        public Queue<ItemState> FutureProductionPlan { get; set; } = new Queue<ItemState>();
        public Queue<ItemState> ProductionHistory { get; set; } = new Queue<ItemState>();
        public ItemState[,] WarehouseState { get; set; } = new ItemState[WarehouseYDimension, WarehouseXDimension];
        public double[,] TimeMatrix { get; set; } = new double[TimeMatrixDimension, TimeMatrixDimension];

        private const int TimeMatrixDimension = 47;
        private const int WarehouseXDimension = 12;
        private const int WarehouseYDimension = 4;
        private Dictionary<PositionCodes, (int row, int col)> _warehousePositionMapping;
        private Dictionary<PositionCodes, int> _timeMatrixMapping;

        public ProductionState()
        {
            _warehousePositionMapping = BuildWarehousePositionMappingDict();
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

        private (int numericalPart, char alphabetPart) ParsePositionCode(PositionCodes code) {
            var pureCode = code.ToString().Remove(0, 1);
            var numericalPart = int.Parse(pureCode.Remove(pureCode.Length - 1));
            var alphabetPart = pureCode[pureCode.Length - 1];
            return (numericalPart, alphabetPart);
        }

        private Dictionary<PositionCodes, int> BuildTimeMatrixPositionMappingDict()
        {
            return ((PositionCodes[])Enum.GetValues(typeof(PositionCodes))).ToDictionary(code => code, code => {
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
        }

        public int GetTimeMatrixIndex(PositionCodes code) => _timeMatrixMapping[code];
        public (int row, int col) GetWarehouseIndex(PositionCodes code) => _warehousePositionMapping[code];

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

        private ItemState GetItemState(string str)
        {
            switch(str)
            {
                case "MQB": 
                    return ItemState.MQB;
                case "MEB":
                    return ItemState.MEB;
                case "Forbidden":
                    return ItemState.Forbidden;
                case "Empty":
                    return ItemState.Empty;
                default:
                    throw new Exception("Wrong item state in warehouse input state.");
            }
        }

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

        public void LoadProductionHistory(string csvPath)
        {
            ProductionHistory = new Queue<ItemState>(File.ReadLines(csvPath).Select(line => GetItemState(line)));
        }

        public void LoadFutureProductionPlan(string csvPath)
        {
            FutureProductionPlan = new Queue<ItemState>(File.ReadLines(csvPath).Select(line => GetItemState(line)));
        }
    }
}
