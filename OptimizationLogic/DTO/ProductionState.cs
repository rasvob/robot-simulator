using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.DTO
{
    public class ProductionState
    {
        public Queue<ItemState> FutureProductionPlan { get; set; } = new Queue<ItemState>();
        public Queue<ItemState> ProductionHistory { get; set; } = new Queue<ItemState>();
        public ItemState[,] WarehouseState { get; set; } = new ItemState[4, 12];
        public double[,] TimeMatrix { get; set; } = new double[47, 47];

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

        public void LoadTimeMatrix(string csvPath)
        {
            throw new NotImplementedException();
        }

        public void LoadWarehouseState(string csvPath)
        {
            throw new NotImplementedException();
        }

        public void LoadProductionHistory(string csvPath)
        {
            throw new NotImplementedException();
        }

        public void LoadFutureProductionPlan(string csvPath)
        {
            throw new NotImplementedException();
        }
    }
}
