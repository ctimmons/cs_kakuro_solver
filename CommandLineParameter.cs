using System;
using System.Linq;

namespace KakuroSolver
{
  public class CommandLineParameter
  {
    private readonly String[] _args;
    private readonly Char[] _switchCharacters = "/-".ToCharArray();

    public String this[Int32 index]
    {
      get { return this._args[index]; }
    }

    private CommandLineParameter()
      : base()
    {
    }

    public CommandLineParameter(String[] args)
      : this()
    {
      this._args = args;
    }

    private Boolean DoesArgEqual(String arg, String value)
    {
      return arg.TrimStart(_switchCharacters).Equals(value, StringComparison.OrdinalIgnoreCase);
    }

    public Boolean Exists(String parameterName)
    {
      return this._args.Any(arg => DoesArgEqual(arg, parameterName));
    }

    private Boolean DoesArgStartWith(String arg, String value)
    {
      return arg.TrimStart(_switchCharacters).StartsWith(value, StringComparison.OrdinalIgnoreCase);
    }

    public String GetValue(String parameterName)
    {
      return
        this._args
        .Where(arg => DoesArgStartWith(arg, parameterName))
        .Select(arg => arg.Substring(arg.IndexOf(':') + 1))
        .FirstOrDefault();
    }

    public String GetValue(String parameterName, String defaultValue)
    {
      var parameterValue = GetValue(parameterName);

      return (parameterValue == default(String)) ? defaultValue : parameterValue;
    }

    public T GetValue<T>(String parameterName, T defaultValue)
      where T : struct, IComparable, IFormattable, IConvertible
    {
      if (!typeof(T).IsEnum)
        throw new ArgumentException("T must be an enumerated type");

      var parameterValue = GetValue(parameterName);

      if (parameterValue == default(String))
      {
        return defaultValue;
      }
      else
      {
        T result;
        if (Enum.TryParse(parameterValue, true /* Ignore case. */, out result))
          return result;
        else
          return defaultValue;
      }
    }
  }
}
