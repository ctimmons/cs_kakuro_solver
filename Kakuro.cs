using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Utilities.Core;

namespace KakuroSolver
{
  public class Kakuro : IDisposable
  {
    private Cell[,] _cells { get; set; }
    private Int32 _width { get { return this._cells.GetLength(0); } }
    private Int32 _height { get { return this._cells.GetLength(1); } }

    private readonly StreamWriter _log = null;
    private readonly String _outputFilename = null;
    private readonly Boolean _shouldCreateLogEntries = false;

    private Kakuro()
      : base()
    {
    }

    public Kakuro(String inputFilename, String outputFilename, String logFilename, Boolean shouldCreateLogEntries)
      : this()
    {
      Func<String, String> getModifiedInputFilename =
        suffix => Path.ChangeExtension(inputFilename, null) + suffix + Path.GetExtension(inputFilename);

      if (String.IsNullOrWhiteSpace(outputFilename))
        this._outputFilename = getModifiedInputFilename("_SOLUTION");
      else
        this._outputFilename = outputFilename;

      if (String.IsNullOrWhiteSpace(logFilename))
        this._log = new StreamWriter(getModifiedInputFilename("_LOG"));
      else
        this._log = new StreamWriter(logFilename);

      this._shouldCreateLogEntries = shouldCreateLogEntries;

      this.Load(inputFilename);
      this.AssignRowAndColumnPeersToValueCells();
    }

    #region IDisposable Implementation
    private Boolean _isDisposed = false;

    ~Kakuro()
    {
      this.Dispose(false);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(Boolean isDisposing)
    {
      if (this._isDisposed)
        return;

      if (isDisposing)
      {
        if (this._log != null)
          this._log.Close();
      }

      this._isDisposed = true;
    }
    #endregion

    private void AssignRowAndColumnPeersToValueCells()
    {
      for (var column = 0; column < this._width; column++)
        for (var row = 0; row < this._height; row++)
          if (this._cells[column, row] is ValueCell)
          {
            var valueCell = (ValueCell) this._cells[column, row];
            
            if (valueCell.ColumnPeers == null)
              valueCell.ColumnPeers = this.GetColumnPeersForValueCell(column, row);

            if (valueCell.RowPeers == null)
              valueCell.RowPeers = this.GetRowPeersForValueCell(column, row);
          }
    }

    private Peers GetColumnPeersForValueCell(Int32 column, Int32 row)
    {
      var peers = new Peers();

      /* Backtrack until a SumCell is hit. */
      var startRow = row;
      while (!(this._cells[column, --startRow] is SumCell))
        ;

      peers.Sum = (this._cells[column, startRow] as SumCell).Down;

      /* Go forward until another SumCell or the edge of the puzzle's column is hit. */
      while ((++startRow < this._height) && !(this._cells[column, startRow] is SumCell))
        peers.Add(this._cells[column, startRow] as ValueCell);

      return peers;
    }

    private Peers GetRowPeersForValueCell(Int32 column, Int32 row)
    {
      var peers = new Peers();

      /* Backtrack until a SumCell is hit. */
      var startColumn = column;
      while (!(this._cells[--startColumn, row] is SumCell))
        ;

      peers.Sum = (this._cells[startColumn, row] as SumCell).Right;

      /* Go forward until another SumCell or the edge of the puzzle's row is hit. */
      while ((++startColumn < this._width) && !(this._cells[startColumn, row] is SumCell))
        peers.Add(this._cells[startColumn, row] as ValueCell);

      return peers;
    }

    public void Solve()
    {
      var unsolvedValueCells = new List<ValueCell>();
      for (var column = 0; column < this._width; column++)
        for (var row = 0; row < this._height; row++)
          if (this._cells[column, row] is ValueCell)
          {
            var valueCell = (ValueCell) this._cells[column, row];
            if (!valueCell.IsSolved)
              unsolvedValueCells.Add(valueCell);
          }

      var started = DateTime.Now;

      this.SolveEx(unsolvedValueCells, 0);

      var finished = DateTime.Now;
      this._log.WriteLine("-".Repeat(100));
      this._log.WriteLine(String.Format("Started: {0}", started));
      this._log.WriteLine(String.Format("Finished: {0}", finished));
      this._log.WriteLine(String.Format("Elapsed: {0}", finished - started));
    }

    private Boolean SolveEx(List<ValueCell> valueCells, Int32 recursionLevel)
    {
      String logEntry = null;
      String logEntryPadding = null;

      if (this._shouldCreateLogEntries)
        logEntryPadding = " ".Repeat(recursionLevel);

      /* Base case. */
      if (!valueCells.Any())
      {
        if (this._shouldCreateLogEntries)
          this._log.WriteLine(logEntryPadding + "SOLVED!");

        return true;
      }
      else
      {
        /* There are value cells left to process.
        
           If one of them is "bad" (i.e. the value cell has no candidates), optionally log it and return false.

           If there aren't any bad cells, continue with the recursion. */

        var firstBadValueCell = valueCells.Where(vc => vc.Candidates.Count() == 0).FirstOrDefault();
        if (firstBadValueCell != default(ValueCell))
        {
          if (this._shouldCreateLogEntries)
          {
            /* A value cell with no candidates means either there are no row or column candidates,
               or the intersection of those two sets is empty.  Determine which one is true and
               create a suitable log message for each case. */

            var messageLines = new List<String>() { "CONTRADICTION!" };
            var columnCandidates = firstBadValueCell.ColumnPeers.Candidates;
            var rowCandidates = firstBadValueCell.RowPeers.Candidates;

            if ((columnCandidates.Count == 0) && (rowCandidates.Count == 0))
            {
              messageLines.Add("There are no column or row candidates for this value cell.");
            }
            else if (columnCandidates.Count == 0)
            {
              messageLines.Add(String.Format("The row candidates for this cell are '{0}',", String.Join(", ", rowCandidates)));
              messageLines.Add("but there are no column candidates for this value cell.");
            }
            else if (rowCandidates.Count == 0)
            {
              messageLines.Add(String.Format("The column candidates for this cell are '{0}',", String.Join(", ", columnCandidates)));
              messageLines.Add("but there are no row candidates for this value cell.");
            }
            else
            {
              messageLines.Add(String.Format("The column candidates for this cell are '{0}'.", String.Join(", ", columnCandidates)));
              messageLines.Add(String.Format("The row candidates for this cell are '{0}'.", String.Join(", ", rowCandidates)));
              messageLines.Add("However, the intersection of those two sets of candidates is empty.");
            }

            this._log.WriteLine(String.Join(Environment.NewLine, messageLines.Select(line => logEntryPadding + line)));
          }

          return false;
        }
      }

      var cellWithFewestCandidates =
        valueCells
        .Where(vc => !vc.IsSolved)
        .OrderBy(vc => vc.Candidates.Count())
        .First();

      var remainingValueCells = new List<ValueCell>(valueCells);
      remainingValueCells.Remove(cellWithFewestCandidates);

      if (this._shouldCreateLogEntries)
        logEntry = logEntryPadding +
          String.Format("C:{0,2}, R:{1,2}, # of Candidates: {2}, Candidates: {3} - ",
            cellWithFewestCandidates.Column,
            cellWithFewestCandidates.Row,
            cellWithFewestCandidates.Candidates.Count(),
            String.Join(", ", cellWithFewestCandidates.Candidates));

      /* Recursive case. */
      foreach (var candidate in cellWithFewestCandidates.Candidates)
      {
        cellWithFewestCandidates.Value = candidate;

        if (this._shouldCreateLogEntries)
          this._log.WriteLine(logEntry + String.Format("Trying candidate {0}.", candidate));

        if (this.SolveEx(remainingValueCells, recursionLevel + 1))
        {
          if (this._shouldCreateLogEntries)
            this._log.WriteLine(logEntry + String.Format("Candidate {0} SUCCEEDED.", candidate));

          return true;
        }
        else
        {
          if (this._shouldCreateLogEntries)
            this._log.WriteLine(logEntry + String.Format("Candidate {0} FAILED.", candidate));

          cellWithFewestCandidates.Value = 0;
        }
      }

      return false;
    }

    private static readonly Char[] _verticalBar = "|".ToCharArray();
    private static readonly Char[] _backslash = @"\".ToCharArray();

    private void Load(String inputFilename)
    {
      var lines =
        File
        .ReadAllLines(inputFilename)
        .Where(line => !String.IsNullOrWhiteSpace(line))
        .ToArray();

      this.CheckAllLinesForSameWidth(lines);

      var height = lines.Length;
      var width = this.GetWidthOfLine(lines[0]);
      this._cells = new Cell[width, height];

      for (var row = 0; row < lines.Length; row++)
      {
        var cells = lines[row].Split(_verticalBar, StringSplitOptions.RemoveEmptyEntries);
        for (var column = 0; column < cells.Length; column++)
        {
          var cell = cells[column].Trim();
          if (cell.Contains(@"\"))
            this.TryToAddSumCell(column, row, cell);
          else
            this.TryToAddValueCell(column, row, cell);
        }
      }

      this.CheckZeroSumCellsForNeighboringValueCells();
    }

    private Int32 GetWidthOfLine(String line)
    {
      return line.Count(c => c == '|') + 1;
    }

    private void CheckAllLinesForSameWidth(String[] lines)
    {
      /* Assume the first line's width is correct.
         Even if it's not correct, errors will be thrown for
         all of the other lines. */
      var widthOfFirstLine = this.GetWidthOfLine(lines[0]);
      var badLineWidths =
        lines
        .Select((line, index) => new { LineNumber = index + 1, Width = this.GetWidthOfLine(line) })
        .Where(line => line.Width != widthOfFirstLine)
        .Select(line => String.Format("  Line {0} has {1} cells.", line.LineNumber, line.Width));

      if (badLineWidths.Any())
        throw new Exception(String.Format("Based on the width of the first line of the puzzle ({0}), the following lines have the wrong number of cells:{1}{2}",
          widthOfFirstLine, Environment.NewLine, String.Join(Environment.NewLine, badLineWidths)));
    }

    private void TryToAddSumCell(Int32 column, Int32 row, String cell)
    {
      var values = cell.Split(_backslash, StringSplitOptions.RemoveEmptyEntries);
      if (values.Length != 2)
        throw new Exception(String.Format("The cell at column {0}, row {1} does not appear to be a valid sum cell ({2}).", column, row, cell));

      Func<String, String, Int32> getSumCellValue =
        (value, direction) =>
        {
          Int32 result;
          if (!Int32.TryParse(value, out result))
            throw new Exception(String.Format("The '{0}' value at column {1}, row {2} does not appear to be a valid integer ({3}).", direction, column, row, cell));

          if (((result < 3) || (result > 45)) && (result != 0))
            throw new Exception(String.Format("The '{0}' value at column {1}, row {2} does not have a valid value ({3}).  The value must either be 0, or between 3 and 45 (inclusive).", direction, column, row, result));

          return result;
        };

      var down = getSumCellValue(values[0], "down");
      var right = getSumCellValue(values[1], "right");

      this._cells[column, row] = new SumCell() { Right = right, Down = down };
    }

    private void TryToAddValueCell(Int32 column, Int32 row, String cell)
    {
      Int32 value;
      if (!Int32.TryParse(cell, out value))
        throw new Exception(String.Format("The value at column {0}, row {1} does not appear to be a valid integer ({2}).", column, row, cell));

      if ((value < 0) || (value > 9))
        throw new Exception(String.Format("The value at column {0}, row {1} does not have a valid value ({2}).  The value must between 0 and 9 (inclusive).", column, row, value));

      this._cells[column, row] = new ValueCell() { Value = value, Column = column, Row = row };
    }

    private void CheckZeroSumCellsForNeighboringValueCells()
    {
      /* A sum cell with a down or right value of 0 must not have
         any value cells next to it in that direction. */

      for (var column = 0; column < this._width; column++)
        for (var row = 0; row < this._height; row++)
          if (this._cells[column, row] is SumCell)
          {
            var sumCell = (this._cells[column, row] as SumCell);

            if ((sumCell.Down == 0) && (row < (this._height - 1)) && (this._cells[column, row + 1] is ValueCell))
              throw new Exception(String.Format("The sum cell at column {0} and row {1} has a 'down' value of 0, but there's a value cell immediately below it.",
                column, row));

            if ((sumCell.Right == 0) && (column < (this._width - 1)) && (this._cells[column + 1, row] is ValueCell))
              throw new Exception(String.Format("The sum cell at column {0} and row {1} has a 'right' value of 0, but there's a value cell immediately to the right.",
                column, row));
          }
    }

    public void Save()
    {
      File.WriteAllText(this._outputFilename, this.ToString());
    }

    public override String ToString()
    {
      var result = "";

      for (var row = 0; row < this._height; row++)
      {
        var rowValues = new List<String>(this._width);
        for (var column = 0; column < this._width; column++)
        {
          var cell = this._cells[column, row];
          if (cell is SumCell)
            rowValues.Add(String.Format(@"{0,2}\{1,2}", ((SumCell) cell).Down, ((SumCell) cell).Right));
          else if (cell is ValueCell)
            rowValues.Add(String.Format("  {0}  ", ((ValueCell) cell).Value));
        }
        result += String.Join("|", rowValues) + Environment.NewLine;
      }

      return result.TrimEnd();
    }
  }
}
