using System;
using System.Linq;

namespace KakuroSolver
{
  public class CommandLineParameterException : Exception
  {
    public CommandLineParameterException(String message)
      : base(message)
    {
    }

    public CommandLineParameterException(String message, Exception innerException)
      : base(message, innerException)
    {
    }
  }

  public class CommandLine
  {
    public String this[Int32 index]
    {
      get { return this._args[index]; }
    }

    /* A command line may be a mix of switches, parameters, values and response file names.
       The Values property is a string array that references only the values.
       This makes it easy to access only the desired value at its position in the
       list of command line items, ignoring the other types of items.

       For example, a program's command line might be structured like this:

         program.exe [zero or more switches] inputFile outputFile
      
       The Values property allows access to just the inputFile or outputFile items,
       skipping any switches.  Values[0] holds inputFile, and Values[1] holds outputFile. */

    private String[] _values = null;
    public String[] Values
    {
      get
      {
        if (this._values == null)
          this._values = this._args.Where(arg => this.IsValue(arg)).ToArray();

        return this._values;
      }
    }

    private readonly Char[] _parameterValueSeparators = ":=".ToCharArray();
    private readonly Char[] _switchCharacters = "@-/".ToCharArray();
    private readonly String[] _args = null;

    public CommandLine()
      : base()
    {
      /* Environment.GetCommandLineArgs() includes the app's name as the first argument in the
         array it returns.  Use Skip(1) to avoid treating the app name as a parameter. */
      this._args = Environment.GetCommandLineArgs().Skip(1).ToArray();
    }

    public CommandLine(String[] args)
      : base()
    {
      this._args = args;
    }

    private Boolean IsValue(String arg)
    {
      return (arg.Length == arg.TrimStart(this._switchCharacters).Length);
    }

    public Boolean DoesSwitchExist(String parameterName)
    {
      return this.DoesSwitchExist(parameterName, StringComparison.OrdinalIgnoreCase);
    }

    public Boolean DoesSwitchExist(String parameterName, StringComparison stringComparison)
    {
      parameterName = parameterName.TrimStart(this._switchCharacters);

      return this._args.Any(arg => arg.TrimStart(this._switchCharacters).Equals(parameterName, stringComparison));
    }

    public String GetValue(String parameterName)
    {
      return this.GetValue(parameterName, StringComparison.OrdinalIgnoreCase);
    }

    public String GetValue(String parameterName, String defaultValue)
    {
      return this.GetValue(parameterName, defaultValue, StringComparison.OrdinalIgnoreCase);
    }

    public String GetValue(String parameterName, String defaultValue, StringComparison stringComparison)
    {
      try
      {
        return this.GetValue(parameterName, stringComparison) ?? defaultValue;
      }
      catch (CommandLineParameterException)
      {
        return defaultValue;
      }
    }

    public String GetValue(String parameterName, StringComparison stringComparison)
    {
      parameterName = parameterName.TrimStart(this._switchCharacters);

      for (var i = 0; i < this._args.Length; i++)
      {
        var arg = this._args[i].TrimStart(this._switchCharacters);

        if (arg.StartsWith(parameterName, stringComparison))
        {
          if (arg.Length > parameterName.Length)
          {
            /* Returns the value for the "-paramValue", "-param=Value" or "-param:Value" formats. */
            return arg.Substring(parameterName.Length).TrimStart(this._parameterValueSeparators);
          }
          else
          {
            /* "-param Value" format.  This requires a little more logic since it involves
               getting the next parameter in this._args, which may not exist.
               Or the next parameter might not be a value (i.e. it's preceded by a - or / character). */
            if ((i == (this._args.Length - 1)) || "-/".Contains(this._args[i + 1][0]))
              return default(String);
            else
              return this._args[i + 1];
          }
        }
      }

      return default(String);
    }
  }
}

