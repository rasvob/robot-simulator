namespace OptimizationLogic.DTO
{
    public class StepModel
    {
        public PositionCodes InsertToCell { get; set; }
        public PositionCodes WithdrawFromCell { get; set; }
        public double InsertTime { get; set; }
        public double WithdrawTime { get; set; }
        public double MoveToDifferentCellTime { get; set; }
        public ItemState InsertType { get; set; }
        public ItemState WithdrawType { get; set; }
        public override string ToString() => $"Inserting to {InsertToCell} (time {InsertTime:f}, type {InsertType}), move to {WithdrawFromCell}, time({MoveToDifferentCellTime:f}), and going to stacker (time {WithdrawTime:f}, withdrawing type {WithdrawType}), total time {InsertTime + MoveToDifferentCellTime + WithdrawTime}";
    }
}
