using System;
using System.IO;

using Utilities.Core;

namespace KakuroSolver
{
  public class Program
  {
    public static void Main(String[] args)
    {
      var cl = new CommandLine(args);

      if ((args.Length == 0) || cl.DoesSwitchExist("help") || cl.DoesSwitchExist("?"))
      {
        DisplayHelp();
        return;
      }

      var inputFilename = cl[0];

      if (!File.Exists(inputFilename))
        throw new ArgumentException(String.Format("The input file '{0}' does not exist.", inputFilename));

      using (var kakuro = new Kakuro(inputFilename, cl.GetValue("outputfile", ""), cl.GetValue("logfile", ""), cl.DoesSwitchExist("log")))
      {
        kakuro.Solve();
        kakuro.Save();
      }
    }

    private static void DisplayHelp()
    {
      Console.WriteLine(@"
Kakuro Solver 1.0
by
Chris R. Timmons
http://www.crtimmonsinc.com/

License:

  This software and its source code are in the PUBLIC DOMAIN.
  See the UNLICENSE.txt file for details.

Source Code:

  The C# source code is freely available at https://github.com/ctimmons/cs_kakuro_solver

Usage:

  KakuroSolver <input filename> [/output:'output filename']
    [/log:'log filename'] [/log]

Optional Parameters:

  /outputfile:'output filename'

    Filename to write the solved puzzle to. If not provided, defaults
    to the input filename with '_SOLUTION' appended.

      E.g. 'c:\puzzles\kakuro_5_SOLUTION.txt'

    The output file is always created, unless an error occurs.


  /logfile:'log filename'

    Filename to write log data to. If not provided, defaults
    to the input filename with '_LOG' appended.

      E.g. 'c:\puzzles\kakuro_5_LOG.txt'

    The log file is always created.  If the /log switch is not provided,
    the log file will only contain a few lines of timing data, and any error
    messages.


  /log

    '/log' turns on logging. Omitting the switch prevents logging.

    Log entries are written for each step of the recursive solution's
    algorithm.

    NB: For hard puzzles, the log file can get very large (> 100 MB), and
    have a material impact on the time the program takes to solve the puzzle.

    The default is no logging.
");
    }
  }
}
