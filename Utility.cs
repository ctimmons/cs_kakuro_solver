using System;
using System.Collections.Generic;
using System.Linq;

namespace KakuroSolver
{
  public static class Utility
  {
    /* Caching the generated partitions greatly improves the app's performance.
       See http://en.wikipedia.org/wiki/Memoization. */
    private static Dictionary<String, List<Int32>> _cache = new Dictionary<String, List<Int32>>();

    public static List<Int32> GetPartitionsOfSum(Int32 sum, Int32 numberOfCells, Int32 minimumCellValue, Int32 maximumCellValue)
    {
      var key = String.Format("{0}~{1}~{2}~{3}", sum, numberOfCells, minimumCellValue, maximumCellValue);

      if (!_cache.ContainsKey(key))
        _cache.Add(key,
          GetPartitions(sum, numberOfCells, minimumCellValue, maximumCellValue)
            .SelectMany(list => list) /* Flatten the nested List<List<Int32>> into a single List<Int32>. */
            .Distinct()               /* Remove duplicates. */
            .ToList());

      return _cache[key];
    }

    /* See http://en.wikipedia.org/wiki/Integer_partition for more info on integer partitions. */
    private static List<List<Int32>> GetPartitions(Int32 sum, Int32 numberOfIntegers, Int32 minimumValue, Int32 maximumValue)
    {
      var result = new List<List<Int32>>();

      /* Base case. */
      if ((sum <= 1) || (numberOfIntegers == 1))
      {
        if ((sum >= minimumValue) && (sum <= maximumValue))
          result.Add(new List<Int32>() { sum });

        return result;
      }

      /* Recursive case. */
      for (var y = Math.Min(sum, maximumValue); y >= minimumValue; y--)
      {
        var recursivePartitions = GetPartitions(sum - y, numberOfIntegers - 1, minimumValue, y);
        for (var i = 0; i < recursivePartitions.Count; i++)
        {
          recursivePartitions[i].Add(y);

          /* Don't add partitions that have duplicate values
             or don't fit within numberOfIntegers. */
          var count = recursivePartitions[i].Count();
          var distinctCount = recursivePartitions[i].Distinct().Count();

          if ((count == numberOfIntegers) && (count == distinctCount))
            result.Add(recursivePartitions[i]);
        }
      }

      return result;
    }
  }
}
