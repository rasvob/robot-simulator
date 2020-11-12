using OptimizationLogic.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OptimizationLogic.Extensions
{
    public static class PositionCodesExt
    {
        public static PositionCodes[] FilterPositions(this PositionCodes[] values, int rowsInWarehouse, int columnsInWarehouse)
        {
            var rowCharacterCodes = Enumerable.Range(0, rowsInWarehouse/2).Select(t => (char)('A' + t)).ToArray();
            return values
                .Where(t => t != PositionCodes.Stacker && t != PositionCodes.Service)
                .Select(t => t.ParsePositionCode())
                .Where(t => rowCharacterCodes.Contains(t.alphabetPart) && t.numericalPart < columnsInWarehouse*2 && t.numericalPart != columnsInWarehouse*2 - 2)
                .Select(t => t.ParsedPositionCodeToEnum())
                .Append(PositionCodes.Stacker)
                .Append(PositionCodes.Service)
                .ToArray();
        }

        public static (int numericalPart, char alphabetPart) ParsePositionCode(this PositionCodes code)
        {
            var pureCode = code.ToString().Remove(0, 1);
            var numericalPart = int.Parse(pureCode.Remove(pureCode.Length - 1));
            var alphabetPart = pureCode[pureCode.Length - 1];
            return (numericalPart, alphabetPart);
        }

        public static PositionCodes ParsedPositionCodeToEnum(this (int numericalPart, char alphabetPart) code)
        {
            return (PositionCodes)Enum.Parse(typeof(PositionCodes), $"_{code.numericalPart}{code.alphabetPart}");
        }
    }
}
