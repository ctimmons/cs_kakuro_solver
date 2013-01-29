using System;
using System.Collections.Generic;
using System.Linq;

namespace KakuroSolver
{
  public abstract class Cell
  {
  }

  public class SumCell : Cell
  {
    public Int32 Right { get; set; }
    public Int32 Down { get; set; }
  }

  public class ValueCell : Cell
  {
    public Int32 Column { get; set; }
    public Int32 Row { get; set; }

    public Int32 Value { get; set; }
    public Boolean IsSolved { get { return this.Value > 0; } }
    public List<Int32> Candidates { get { return this.ColumnPeers.Candidates.Intersect(this.RowPeers.Candidates).ToList(); } }

    public Peers ColumnPeers { get; set; }
    public Peers RowPeers { get; set; }
  }

  public class Peers : List<ValueCell>
  {
    public Int32 Sum { get; set; }

    public List<Int32> Candidates
    {
      get
      {
        var solvedValues = this.Where(vc => vc.IsSolved).Select(vc => vc.Value).ToList();
        var numberOfEmptyValueCells = this.Count() - solvedValues.Count();
        var netSum = this.Sum - solvedValues.Sum();
        return
          Utility.GetPartitionsOfSum(netSum, numberOfEmptyValueCells, 1, 9)
          .Except(solvedValues)
          .OrderBy(n => n)
          .ToList();
      }
    }

    public new void Add(ValueCell cell)
    {
      if (cell.Value > 0)
      {
        var duplicateCells =
          this
          .Where(c => c.Value == cell.Value)
          .Select(c => String.Format("  Column: {0}, Row: {1}, Value: {2}", c.Column, c.Row, c.Value));

        if (duplicateCells.Any())
          throw new Exception(String.Format("Cannot add a value cell that has a value of {0} at column {1} and row {2}.  The following peer cells have the same value:{3}{4}",
            cell.Value, cell.Column, cell.Row, Environment.NewLine, String.Join(Environment.NewLine, duplicateCells)));
      }

      base.Add(cell);
    }
  }
}
